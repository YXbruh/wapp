using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace CSA.Lecturer
{
    public partial class ManageContent : Page
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".txt" };
        private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

        // ========================================================================
        // Page Load
        // ========================================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
            {
                EnsureTablesExist();          // Create missing tables if needed
                LoadCourseDropdown();
                ActivateTab("Chapter");
                LoadContentList();
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
        // Ensure Articles and Media tables exist (run once)
        // ========================================================================
        private void EnsureTablesExist()
        {
            string connStr = GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Create Articles table if missing
                string createArticles = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Articles]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Articles (
                            ArticleID    NVARCHAR(10)  NOT NULL PRIMARY KEY,
                            CourseID     NVARCHAR(10)  NOT NULL,
                            Title        NVARCHAR(200) NOT NULL,
                            Tag          NVARCHAR(100) NULL,
                            Body         NVARCHAR(MAX) NOT NULL,
                            Url          NVARCHAR(500) NULL,
                            IsPublished  BIT           NOT NULL DEFAULT 0,
                            CreatedByID  NVARCHAR(10)  NOT NULL,
                            CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
                            UpdatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT FK_Articles_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
                            CONSTRAINT FK_Articles_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
                        );
                    END";

                // Create Media table if missing
                string createMedia = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Media]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Media (
                            MediaID      NVARCHAR(10)  NOT NULL PRIMARY KEY,
                            CourseID     NVARCHAR(10)  NOT NULL,
                            Title        NVARCHAR(200) NOT NULL,
                            Description  NVARCHAR(500) NULL,
                            FileName     NVARCHAR(500) NOT NULL,
                            MediaType    NVARCHAR(50)  NOT NULL,
                            CreatedByID  NVARCHAR(10)  NOT NULL,
                            CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
                            UpdatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT FK_Media_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
                            CONSTRAINT FK_Media_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
                        );
                    END";

                //using (SqlCommand cmd = new SqlCommand(createArticles, conn))
                //    cmd.ExecuteNonQuery();
                //using (SqlCommand cmd = new SqlCommand(createMedia, conn))
                //    cmd.ExecuteNonQuery();
            }
        }

        // ========================================================================
        // Load Course Dropdown
        // ========================================================================
        private void LoadCourseDropdown()
        {
            string userId = Session["UserID"].ToString();
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("— Select Course —", ""));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
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
        }

        // ========================================================================
        // Tab switching
        // ========================================================================
        protected void tabChapter_Click(object sender, EventArgs e) { ActivateTab("Chapter"); LoadContentList(); }
        protected void tabArticle_Click(object sender, EventArgs e) { ActivateTab("Article"); LoadContentList(); }
        protected void tabMedia_Click(object sender, EventArgs e) { ActivateTab("Media"); LoadContentList(); }

        private void ActivateTab(string tab)
        {
            hfActiveTab.Value = tab;

            pnlChapter.Visible = tab == "Chapter";
            pnlArticle.Visible = tab == "Article";
            pnlMedia.Visible = tab == "Media";

            // Enable/disable validators for the active tab
            rfvChapterBody.Enabled = tab == "Chapter";
            rfvArticleBody.Enabled = tab == "Article";
            revArticleUrl.Enabled = tab == "Article";
            rfvMediaDesc.Enabled = tab == "Media";

            tabChapter.CssClass = tab == "Chapter" ? "auth-tab active" : "auth-tab";
            tabArticle.CssClass = tab == "Article" ? "auth-tab active" : "auth-tab";
            tabMedia.CssClass = tab == "Media" ? "auth-tab active" : "auth-tab";

            litFormTitle.Text = $"New {tab}";
            litListTitle.Text = tab == "Chapter" ? "Chapters" : tab == "Article" ? "Articles" : "Media Files";
            btnSave.Text = $"Save {tab}";
        }

        // ========================================================================
        // Load content list based on active tab and search filter
        // ========================================================================
        private void LoadContentList()
        {
            string userId = Session["UserID"].ToString();
            string tab = hfActiveTab.Value;
            string search = tbSearch.Text.Trim();

            string query = "";
            string countQuery = "";
            string table = "";

            switch (tab)
            {
                case "Chapter":
                    table = "Chapters";
                    query = @"
                        SELECT ch.ChapterID AS ContentID, ch.ChapterTitle AS Title, '' AS Subtitle,
                               c.CourseName, ch.IsPublished, ch.UpdatedAt
                        FROM Chapters ch
                        INNER JOIN Courses c ON ch.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (ch.ChapterTitle LIKE @Search OR ch.Content LIKE @Search)
                        ORDER BY ch.UpdatedAt DESC";
                    countQuery = @"
                        SELECT COUNT(*) FROM Chapters ch
                        INNER JOIN Courses c ON ch.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (ch.ChapterTitle LIKE @Search OR ch.Content LIKE @Search)";
                    break;
                case "Article":
                    table = "Articles";
                    query = @"
                        SELECT a.ArticleID AS ContentID, a.Title, '' AS Subtitle,
                               c.CourseName, a.IsPublished, a.UpdatedAt
                        FROM Articles a
                        INNER JOIN Courses c ON a.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (a.Title LIKE @Search OR a.Body LIKE @Search OR a.Tag LIKE @Search)
                        ORDER BY a.UpdatedAt DESC";
                    countQuery = @"
                        SELECT COUNT(*) FROM Articles a
                        INNER JOIN Courses c ON a.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (a.Title LIKE @Search OR a.Body LIKE @Search OR a.Tag LIKE @Search)";
                    break;
                case "Media":
                    table = "Media";
                    query = @"
                        SELECT m.MediaID AS ContentID, m.Title, m.Description AS Subtitle,
                               c.CourseName, 1 AS IsPublished, m.UpdatedAt
                        FROM Media m
                        INNER JOIN Courses c ON m.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (m.Title LIKE @Search OR m.Description LIKE @Search)
                        ORDER BY m.UpdatedAt DESC";
                    countQuery = @"
                        SELECT COUNT(*) FROM Media m
                        INNER JOIN Courses c ON m.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          AND (m.Title LIKE @Search OR m.Description LIKE @Search)";
                    break;
            }

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
                            Subtitle = reader["Subtitle"].ToString(),
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
            string userId = Session["UserID"].ToString();
            string editId = hfEditID.Value;
            string tab = hfActiveTab.Value;

            switch (tab)
            {
                case "Chapter": SaveChapter(userId, editId); break;
                case "Article": SaveArticle(userId, editId); break;
                case "Media": SaveMedia(userId); break;
            }
        }

        // ========================================================================
        // Save Chapter
        // ========================================================================
        private void SaveChapter(string userId, string editId)
        {
            // Validate input
            if (!Regex.IsMatch(tbTitle.Text.Trim(), @"^[\w\s\-:.,]{3,200}$"))
            { ShowError("Title contains invalid characters."); return; }
            if (tbChapterBody.Text.Trim().Length > 20000)
            { ShowError("Lesson notes exceed 20,000 character limit."); return; }

            string courseId = ddlCourse.SelectedValue;
            string title = tbTitle.Text.Trim();
            int sortOrder = string.IsNullOrEmpty(tbChapterNum.Text) ? 0 : Convert.ToInt32(tbChapterNum.Text);
            string body = tbChapterBody.Text.Trim();
            string objectives = tbObjectives.Text.Trim(); // stored in Content field? We'll append to body or store separately? We'll combine with body.
            bool isPublished = cbPublishChapter.Checked;

            // Combine body and objectives (or store objectives in a separate column? We'll store as part of body with a marker)
            string fullContent = body;
            if (!string.IsNullOrEmpty(objectives))
                fullContent += $"\n\n## Learning Objectives\n{objectives}";

            string query = "";
            if (string.IsNullOrEmpty(editId) || editId == "0")
            {
                // Insert
                string newId = GenerateId("CH");
                query = @"
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
                ShowSuccess("Chapter saved successfully.");
            }
            else
            {
                // Update
                query = @"
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
                ShowSuccess("Chapter updated successfully.");
            }

            ResetForm();
            LoadContentList();
        }

        // ========================================================================
        // Save Article
        // ========================================================================
        private void SaveArticle(string userId, string editId)
        {
            string body = tbArticleBody.Text.Trim();
            if (body.Length < 100) { ShowError("Article must be at least 100 characters."); return; }
            if (body.Length > 50000) { ShowError("Article exceeds 50,000 character limit."); return; }

            string courseId = ddlCourse.SelectedValue;
            string title = tbTitle.Text.Trim();
            string tag = tbArticleTag.Text.Trim();
            string url = tbArticleUrl.Text.Trim();
            bool isPublished = cbPublishArticle.Checked;

            string query = "";
            if (string.IsNullOrEmpty(editId) || editId == "0")
            {
                string newId = GenerateId("AR");
                query = @"
                    INSERT INTO Articles (ArticleID, CourseID, Title, Tag, Body, Url, IsPublished, CreatedByID, CreatedAt, UpdatedAt)
                    VALUES (@ArticleID, @CourseID, @Title, @Tag, @Body, @Url, @IsPublished, @CreatedByID, GETDATE(), GETDATE())";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", newId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Tag", tag);
                    cmd.Parameters.AddWithValue("@Body", body);
                    cmd.Parameters.AddWithValue("@Url", url);
                    cmd.Parameters.AddWithValue("@IsPublished", isPublished);
                    cmd.Parameters.AddWithValue("@CreatedByID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowSuccess("Article saved successfully.");
            }
            else
            {
                query = @"
                    UPDATE Articles SET
                        CourseID = @CourseID,
                        Title = @Title,
                        Tag = @Tag,
                        Body = @Body,
                        Url = @Url,
                        IsPublished = @IsPublished,
                        UpdatedAt = GETDATE()
                    WHERE ArticleID = @ArticleID AND CreatedByID = @CreatedByID";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ArticleID", editId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Tag", tag);
                    cmd.Parameters.AddWithValue("@Body", body);
                    cmd.Parameters.AddWithValue("@Url", url);
                    cmd.Parameters.AddWithValue("@IsPublished", isPublished);
                    cmd.Parameters.AddWithValue("@CreatedByID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowSuccess("Article updated successfully.");
            }

            ResetForm();
            LoadContentList();
        }

        // ========================================================================
        // Save Media
        // ========================================================================
        private void SaveMedia(string userId)
        {
            if (!fuMedia.HasFile) { ShowError("Please select a file to upload."); return; }

            string ext = Path.GetExtension(fuMedia.FileName).ToLower();
            if (Array.IndexOf(AllowedExtensions, ext) < 0)
            { ShowError($"File type '{ext}' not allowed. Use: PDF, PNG, JPG, TXT."); return; }

            if (fuMedia.PostedFile.ContentLength > MaxBytes)
            { ShowError("File exceeds 10 MB limit."); return; }

            // Sanitize filename
            string safeName = Regex.Replace(Path.GetFileNameWithoutExtension(fuMedia.FileName), @"[^\w\-]", "_");
            string fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{safeName}{ext}";
            string savePath = Server.MapPath($"~/App_Data/Uploads/{fileName}");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            fuMedia.SaveAs(savePath);

            string courseId = ddlCourse.SelectedValue;
            string title = tbTitle.Text.Trim();
            string desc = tbMediaDesc.Text.Trim();
            string mediaType = ddlMediaType.SelectedValue;

            string newId = GenerateId("MD");
            string query = @"
                INSERT INTO Media (MediaID, CourseID, Title, Description, FileName, MediaType, CreatedByID, CreatedAt, UpdatedAt)
                VALUES (@MediaID, @CourseID, @Title, @Description, @FileName, @MediaType, @CreatedByID, GETDATE(), GETDATE())";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MediaID", newId);
                cmd.Parameters.AddWithValue("@CourseID", courseId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Description", desc);
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@MediaType", mediaType);
                cmd.Parameters.AddWithValue("@CreatedByID", userId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowSuccess($"File '{fuMedia.FileName}' uploaded successfully.");
            ResetForm();
            LoadContentList();
        }

        // ========================================================================
        // Repeater Item Command (Edit / Delete)
        // ========================================================================
        protected void rptContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string tab = hfActiveTab.Value;

            if (e.CommandName == "Edit")
            {
                // Populate form with existing data
                string userId = Session["UserID"].ToString();
                string table = "";
                string query = "";
                switch (tab)
                {
                    case "Chapter":
                        table = "Chapters";
                        query = @"
                            SELECT ChapterID, CourseID, ChapterTitle AS Title, Content, SortOrder, IsPublished,
                                   CreatedByID
                            FROM Chapters WHERE ChapterID = @ID AND CreatedByID = @UserID";
                        break;
                    case "Article":
                        table = "Articles";
                        query = @"
                            SELECT ArticleID AS ID, CourseID, Title, Tag, Body, Url, IsPublished, CreatedByID
                            FROM Articles WHERE ArticleID = @ID AND CreatedByID = @UserID";
                        break;
                    case "Media":
                        table = "Media";
                        query = @"
                            SELECT MediaID AS ID, CourseID, Title, Description, FileName, MediaType, CreatedByID
                            FROM Media WHERE MediaID = @ID AND CreatedByID = @UserID";
                        break;
                }

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
                        litFormTitle.Text = $"Edit {tab}";
                        lbCancelEdit.Visible = true;
                        btnSave.Text = $"Update {tab}";

                        // Common fields
                        ddlCourse.SelectedValue = reader["CourseID"].ToString();
                        tbTitle.Text = reader["Title"].ToString();

                        // Tab-specific
                        if (tab == "Chapter")
                        {
                            string content = reader["Content"].ToString();
                            // Extract objectives if stored with marker? We stored combined. To edit, we need to separate? We'll just put body and objectives in separate fields if possible.
                            // For simplicity, we'll put full content in body, and leave objectives empty.
                            tbChapterBody.Text = content;
                            tbObjectives.Text = ""; // We could parse but we'll keep it simple
                            tbChapterNum.Text = reader["SortOrder"].ToString();
                            cbPublishChapter.Checked = Convert.ToBoolean(reader["IsPublished"]);
                            ActivateTab("Chapter"); // ensure correct panel visible
                        }
                        else if (tab == "Article")
                        {
                            tbArticleBody.Text = reader["Body"].ToString();
                            tbArticleTag.Text = reader["Tag"].ToString();
                            tbArticleUrl.Text = reader["Url"].ToString();
                            cbPublishArticle.Checked = Convert.ToBoolean(reader["IsPublished"]);
                            ActivateTab("Article");
                        }
                        else if (tab == "Media")
                        {
                            tbMediaDesc.Text = reader["Description"].ToString();
                            ddlMediaType.SelectedValue = reader["MediaType"].ToString();
                            // FileName is stored but we don't display it in edit; we could show it.
                            ActivateTab("Media");
                        }
                    }
                    reader.Close();
                }
            }
            else if (e.CommandName == "Delete")
            {
                // Delete content
                string userId = Session["UserID"].ToString();
                string table = "";
                string idColumn = "";
                switch (tab)
                {
                    case "Chapter": table = "Chapters"; idColumn = "ChapterID"; break;
                    case "Article": table = "Articles"; idColumn = "ArticleID"; break;
                    case "Media": table = "Media"; idColumn = "MediaID"; break;
                }

                string deleteQuery = $"DELETE FROM {table} WHERE {idColumn} = @ID AND CreatedByID = @UserID";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowSuccess("Content deleted.");
                LoadContentList();
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
            tbArticleBody.Text = "";
            tbArticleTag.Text = "";
            tbArticleUrl.Text = "";
            tbMediaDesc.Text = "";
            tbChapterNum.Text = "";
            hfEditID.Value = "0";
            litFormTitle.Text = $"New {hfActiveTab.Value}";
            lbCancelEdit.Visible = false;
            btnSave.Text = $"Save {hfActiveTab.Value}";
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
            // Clear file upload if any
            fuMedia.Attributes.Clear();
        }

        private void ShowSuccess(string msg)
        { pnlSuccess.Visible = true; litSuccess.Text = msg; pnlError.Visible = false; }

        private void ShowError(string msg)
        { pnlError.Visible = true; litError.Text = Server.HtmlEncode(msg); pnlSuccess.Visible = false; }

        private string GenerateId(string prefix)
        {
            // Simple ID generation: prefix + timestamp + random
            return prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }

        // ========================================================================
        // ViewModel for content list
        // ========================================================================
        public class ContentItem
        {
            public string ContentID { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
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