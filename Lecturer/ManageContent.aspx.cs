using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Lecturer
{
    public partial class ManageContent : Page
    {
        private string CurrentInstructorId => Session["UserID"]?.ToString() ?? "";

        // ========================================================================
        // Page Load
        // ========================================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
                Response.Redirect("~/Login.aspx");

            // Required for the plain <input type="file" multiple> attachment picker
            // to actually transmit file bytes with the postback.
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadContentList();
                PendingAttachmentService.Clear(AttachBucket);
                LoadAttachments("");
            }
        }

        // ========================================================================
        // Database helper: Connection string
        // ========================================================================
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;
        }

        // ========================================================================
        // Load Course Dropdown
        // ========================================================================
        private void LoadCourseDropdown()
        {
            string userId = CurrentInstructorId;
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("— Select Course —", ""));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", userId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ddlCourse.Items.Add(new ListItem(
                        reader["CourseName"].ToString(),
                        reader["CourseID"].ToString()
                    ));
                }
            }
        }

        // ========================================================================
        // Load chapter list
        // ========================================================================
        private void LoadContentList()
        {
            string userId = CurrentInstructorId;
            string search = tbSearch.Text.Trim();

            string query = @"
                SELECT ch.ChapterID AS ContentID, ch.ChapterTitle AS Title,
                       c.CourseName, ch.IsPublished, ch.UpdatedAt
                FROM Chapters ch
                INNER JOIN Courses c ON ch.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                  AND (ch.ChapterTitle LIKE @Search OR ch.Content LIKE @Search)
                ORDER BY ch.UpdatedAt DESC";
            string countQuery = @"
                SELECT COUNT(*) FROM Chapters ch
                INNER JOIN Courses c ON ch.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                  AND (ch.ChapterTitle LIKE @Search OR ch.Content LIKE @Search)";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(countQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", userId);
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    litCount.Text = count.ToString();
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", userId);
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                    SqlDataReader reader = cmd.ExecuteReader();
                    var list = new List<ContentItem>();
                    while (reader.Read())
                    {
                        list.Add(new ContentItem
                        {
                            ContentID = reader["ContentID"].ToString(),
                            Title = reader["Title"].ToString(),
                            CourseName = reader["CourseName"].ToString(),
                            IsPublished = Convert.ToBoolean(reader["IsPublished"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                        });
                    }
                    reader.Close();

                    rptContent.DataSource = list;
                    rptContent.DataBind();
                    pnlEmpty.Visible = list.Count == 0;
                }
            }
        }

        // ========================================================================
        // Save button handler
        // ========================================================================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            SaveChapter(CurrentInstructorId, hfEditID.Value);
        }

        // ========================================================================
        // Save Chapter
        // ========================================================================
        private void SaveChapter(string userId, string editId)
        {
            if (!Regex.IsMatch(tbTitle.Text.Trim(), @"^[\w\s\-:.,]{3,200}$"))
            { ShowError("Title contains invalid characters."); return; }
            if (tbChapterBody.Text.Trim().Length > 20000)
            { ShowError("Lesson notes exceed 20,000 character limit."); return; }

            string courseId = ddlCourse.SelectedValue;
            string title = tbTitle.Text.Trim();
            int sortOrder = string.IsNullOrEmpty(tbChapterNum.Text) ? 0 : Convert.ToInt32(tbChapterNum.Text);
            string body = tbChapterBody.Text.Trim();
            string objectives = tbObjectives.Text.Trim();
            bool isPublished = cbPublishChapter.Checked;

            string fullContent = body;
            if (!string.IsNullOrEmpty(objectives))
                fullContent += $"\n\n## Learning Objectives\n{objectives}";

            if (string.IsNullOrEmpty(editId))
            {
                string newId = IdGenerator.NewId("CHP");
                string query = @"
                    INSERT INTO Chapters (ChapterID, CourseID, ChapterTitle, Content, SortOrder, IsPublished, CreatedByID, CreatedAt, UpdatedAt)
                    VALUES (@ChapterID, @CourseID, @Title, @Content, @SortOrder, @IsPublished, @CreatedByID, GETDATE(), GETDATE())";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ChapterID", newId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Content", fullContent);
                    cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                    cmd.Parameters.AddWithValue("@IsPublished", isPublished);
                    cmd.Parameters.AddWithValue("@CreatedByID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                // Flush anything staged while the chapter was still unsaved.
                int committed = PendingAttachmentService.Commit(AttachBucket, "Chapter", newId, userId);
                ShowSuccess(committed > 0
                    ? $"Chapter saved with {committed} attachment(s)."
                    : "Chapter saved. You can now attach articles, pictures, media links, and documents to it.");

                AdminService.LogAudit(userId, "CREATE_CHAPTER", "Chapters", newId, "", title);

                // Keep editing the chapter we just created so the Attachments panel opens.
                editId = newId;
                hfEditID.Value = newId;
                litFormTitle.Text = "Edit Chapter";
                lbCancelEdit.Visible = true;
                btnSave.Text = "Update Chapter";
                LoadAttachments(newId);
            }
            else
            {
                string query = @"
                    UPDATE Chapters SET
                        CourseID = @CourseID,
                        ChapterTitle = @Title,
                        Content = @Content,
                        SortOrder = @SortOrder,
                        IsPublished = @IsPublished,
                        UpdatedAt = GETDATE()
                    WHERE ChapterID = @ChapterID AND CreatedByID = @CreatedByID";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ChapterID", editId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Content", fullContent);
                    cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                    cmd.Parameters.AddWithValue("@IsPublished", isPublished);
                    cmd.Parameters.AddWithValue("@CreatedByID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                int committed = PendingAttachmentService.Commit(AttachBucket, "Chapter", editId, userId);
                ShowSuccess(committed > 0
                    ? $"Chapter updated with {committed} new attachment(s)."
                    : "Chapter updated successfully.");
                LoadAttachments(editId);

                AdminService.LogAudit(userId, "UPDATE_CHAPTER", "Chapters", editId, "", title);
            }

            LoadContentList();
        }

        // ========================================================================
        // Attachments (articles, pictures, media links, documents)
        // ========================================================================
        /// <summary>Session bucket holding attachments staged for an unsaved chapter.</summary>
        private const string AttachBucket = "Chapter";

        private void LoadAttachments(string chapterId)
        {
            // The upload controls are always available: anything chosen before the
            // chapter exists is staged and committed when the chapter is saved.
            pnlAttachments.Visible = true;

            DataTable committed = string.IsNullOrEmpty(chapterId)
                ? null
                : AttachmentService.GetByChapter(chapterId);

            DataTable dt = PendingAttachmentService.BuildDisplayTable(committed, AttachBucket);
            rptAttachments.DataSource = dt;
            rptAttachments.DataBind();
            litAttCount.Text = dt.Rows.Count.ToString();
            pnlNoAttachments.Visible = dt.Rows.Count == 0;
        }

        // Bound by the attachment repeater markup.
        public string GetAttachmentHref(object type, object filePath, object linkUrl, object isPending)
            => PendingAttachmentService.DisplayHref(type, filePath, linkUrl, isPending);

        public string GetAttachmentMeta(object type, object by, object at, object isPending)
            => PendingAttachmentService.DisplayMeta(type, by, at, isPending);

        protected void btnUploadFiles_Click(object sender, EventArgs e)
        {
            string chapterId = hfEditID.Value;

            if (!fuAttachFiles.HasFiles)
            { ShowError("Choose at least one file to upload."); LoadAttachments(chapterId); return; }

            List<string> rejected;
            int saved;
            bool staged = string.IsNullOrEmpty(chapterId);

            if (staged)
                saved = PendingAttachmentService.StageFiles(fuAttachFiles.PostedFiles, AttachBucket, out rejected);
            else
                saved = AttachmentService.SaveFiles(fuAttachFiles.PostedFiles, "Chapter", chapterId, CurrentInstructorId, out rejected);

            string verb = staged ? "file(s) ready — saved when you save the chapter." : "file(s) uploaded.";
            if (saved > 0 && rejected.Count == 0)
                ShowSuccess($"{saved} {verb}");
            else if (saved > 0)
                ShowError($"{saved} {verb} Skipped: {string.Join(", ", rejected)}");
            else
                ShowError(rejected.Count > 0 ? $"No files added. Skipped: {string.Join(", ", rejected)}" : "No files added.");

            LoadAttachments(chapterId);
        }

        protected void btnAddLink_Click(object sender, EventArgs e)
        {
            string chapterId = hfEditID.Value;

            string title = tbLinkTitle.Text.Trim();
            string url = tbLinkUrl.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
            { ShowError("Both a link title and URL are required."); LoadAttachments(chapterId); return; }

            if (!Regex.IsMatch(url, @"^https?://[^\s]{4,}$"))
            { ShowError("Enter a valid URL starting with http:// or https://"); LoadAttachments(chapterId); return; }

            if (string.IsNullOrEmpty(chapterId))
            {
                PendingAttachmentService.StageLink(AttachBucket, title, url);
                ShowSuccess("Link ready — saved when you save the chapter.");
            }
            else
            {
                AttachmentService.SaveLink("Chapter", chapterId, title, url, CurrentInstructorId);
                ShowSuccess("Link added.");
            }

            tbLinkTitle.Text = "";
            tbLinkUrl.Text = "";
            LoadAttachments(chapterId);
        }

        protected void rptAttachments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                string attachmentId = e.CommandArgument.ToString();

                bool removed = attachmentId.StartsWith("PND")
                    ? PendingAttachmentService.Remove(AttachBucket, attachmentId)
                    : AttachmentService.Delete(attachmentId, CurrentInstructorId);

                ShowSuccess(removed ? "Attachment removed." : "Attachment not found.");
                LoadAttachments(hfEditID.Value);
            }
        }

        public string GetAttachmentIcon(string type) =>
            type == "Image" ? "ti-photo" : type == "Link" ? "ti-link" : "ti-file-text";

        // ========================================================================
        // Repeater Item Command (Edit / Delete)
        // ========================================================================
        protected void rptContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string userId = CurrentInstructorId;

            if (e.CommandName == "Edit")
            {
                string query = @"
                    SELECT ChapterID, CourseID, ChapterTitle AS Title, Content, SortOrder, IsPublished
                    FROM Chapters WHERE ChapterID = @ID AND CreatedByID = @UserID";

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        hfEditID.Value = id;
                        litFormTitle.Text = "Edit Chapter";
                        lbCancelEdit.Visible = true;
                        btnSave.Text = "Update Chapter";

                        ddlCourse.SelectedValue = reader["CourseID"].ToString();
                        tbTitle.Text = reader["Title"].ToString();
                        tbChapterBody.Text = reader["Content"].ToString();
                        tbObjectives.Text = "";
                        tbChapterNum.Text = reader["SortOrder"].ToString();
                        cbPublishChapter.Checked = Convert.ToBoolean(reader["IsPublished"]);

                        reader.Close();
                        // Switching to an existing chapter abandons anything staged for
                        // the new-chapter form, so it is not silently attached here.
                        PendingAttachmentService.Clear(AttachBucket);
                        LoadAttachments(id);
                    }
                }
            }
            else if (e.CommandName == "Delete")
            {
                // Attachments reference the chapter, so they must go first.
                AttachmentService.DeleteByParent("Chapter", id, userId);

                string deleteQuery = "DELETE FROM Chapters WHERE ChapterID = @ID AND CreatedByID = @UserID";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowSuccess("Chapter deleted.");
                if (hfEditID.Value == id) ResetForm();
                LoadContentList();

                AdminService.LogAudit(userId, "DELETE_CHAPTER", "Chapters", id, "", "");
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e) => ResetForm();
        protected void tbSearch_TextChanged(object sender, EventArgs e) => LoadContentList();

        // ========================================================================
        // Helpers
        // ========================================================================
        private void ResetForm()
        {
            tbTitle.Text = "";
            tbChapterBody.Text = "";
            tbObjectives.Text = "";
            tbChapterNum.Text = "";
            hfEditID.Value = "";
            litFormTitle.Text = "New Chapter";
            lbCancelEdit.Visible = false;
            btnSave.Text = "Save Chapter";
            pnlSuccess.Visible = false;
            pnlError.Visible = false;

            // Abandoning the form drops anything staged for it.
            PendingAttachmentService.Clear(AttachBucket);
            LoadAttachments("");
        }

        private void ShowSuccess(string msg)
        { pnlSuccess.Visible = true; litSuccess.Text = msg; pnlError.Visible = false; }

        private void ShowError(string msg)
        { pnlError.Visible = true; litError.Text = Server.HtmlEncode(msg); pnlSuccess.Visible = false; }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }

        // ========================================================================
        // ViewModel for content list
        // ========================================================================
        public class ContentItem
        {
            public string ContentID { get; set; }
            public string Title { get; set; }
            public string CourseName { get; set; }
            public bool IsPublished { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string UpdatedDisplay => GetRelativeTime(UpdatedAt);

            private string GetRelativeTime(DateTime date)
            {
                var diff = DateTime.Now - date;
                if (diff.TotalMinutes < 1) return "Just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hr ago";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
                return date.ToString("MMM dd, yyyy");
            }
        }
    }
}
