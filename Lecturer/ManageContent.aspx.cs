using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace CSA.Lecturer
{
    public partial class ManageContent : Page
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".txt" };
        private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                ActivateTab("Chapter");
                LoadContentList();
            }
        }

        private void LoadCourseDropdown()
        {
            //string userId = Session["UserID"].ToString();                                                  //Bypass login for testing
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("� Select Course �", ""));
            // TODO: foreach (var c in CourseService.GetByInstructor(userId))
            //           ddlCourse.Items.Add(new ListItem(c.CourseName, c.CourseID.ToString()));
        }

        // -- Tab switching --------------------------------------
        protected void tabChapter_Click(object sender, EventArgs e) { ActivateTab("Chapter"); LoadContentList(); }
        protected void tabArticle_Click(object sender, EventArgs e) { ActivateTab("Article"); LoadContentList(); }
        protected void tabMedia_Click(object sender, EventArgs e) { ActivateTab("Media"); LoadContentList(); }

        private void ActivateTab(string tab)
        {
            hfActiveTab.Value = tab;

            pnlChapter.Visible = tab == "Chapter";
            pnlArticle.Visible = tab == "Article";
            pnlMedia.Visible = tab == "Media";

            // Enable only the validators for the active tab
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

        private void LoadContentList()
        {
            //string userId = Session["UserID"].ToString();                                                  //Bypass login for testing
            string tab = hfActiveTab.Value;
            // TODO:
            // var list = ContentService.GetByInstructorAndType(userId, tab, tbSearch.Text.Trim());
            // litCount.Text          = list.Count.ToString();
            // rptContent.DataSource  = list;
            // rptContent.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            litCount.Text = "0";
            pnlEmpty.Visible = true;
        }

        // -- Save ----------------------------------------------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            //string userId = Session["UserID"]?.ToString() ?? "";                                                  //Bypass login for testing
            string editId = hfEditID.Value;
            string tab = hfActiveTab.Value;

            switch (tab)
            {
                //case "Chapter": SaveChapter(userId, editId); break;                               //Bypass login for testing
                //case "Article": SaveArticle(userId, editId); break;
                //case "Media": SaveMedia(userId); break;
            }
        }

        private void SaveChapter(int userId, int editId)
        {
            // Regex: title must be 3�200 printable chars
            if (!Regex.IsMatch(tbTitle.Text.Trim(), @"^[\w\s\-\:\.\,]{3,200}$"))
            { ShowError("Title contains invalid characters."); return; }

            if (tbChapterBody.Text.Trim().Length > 20000)
            { ShowError("Lesson notes exceed 20,000 character limit."); return; }

            // TODO: ContentService.SaveChapter(editId, userId,
            //   Convert.ToInt32(ddlCourse.SelectedValue),
            //   tbTitle.Text.Trim(), Convert.ToInt32(tbChapterNum.Text),
            //   tbChapterBody.Text.Trim(), tbObjectives.Text.Trim(),
            //   cbPublishChapter.Checked);

            ShowSuccess(editId == 0 ? "Chapter saved." : "Chapter updated.");
            ResetForm();
            LoadContentList();
        }

        private void SaveArticle(int userId, int editId)
        {
            string body = tbArticleBody.Text.Trim();
            if (body.Length < 100) { ShowError("Article must be at least 100 characters."); return; }
            if (body.Length > 50000) { ShowError("Article exceeds 50,000 character limit."); return; }

            // TODO: ContentService.SaveArticle(editId, userId,
            //   Convert.ToInt32(ddlCourse.SelectedValue),
            //   tbTitle.Text.Trim(), tbArticleTag.Text.Trim(),
            //   body, tbArticleUrl.Text.Trim(), cbPublishArticle.Checked);

            ShowSuccess(editId == 0 ? "Article saved." : "Article updated.");
            ResetForm();
            LoadContentList();
        }

        private void SaveMedia(int userId)
        {
            if (!fuMedia.HasFile) { ShowError("Please select a file to upload."); return; }

            string ext = Path.GetExtension(fuMedia.FileName).ToLower();
            if (Array.IndexOf(AllowedExtensions, ext) < 0)
            { ShowError($"File type '{ext}' not allowed. Use: PDF, PNG, JPG, TXT."); return; }

            if (fuMedia.PostedFile.ContentLength > MaxBytes)
            { ShowError("File exceeds 10 MB limit."); return; }

            // Sanitise filename � alphanumeric + dash/underscore/dot only
            string safeName = Regex.Replace(Path.GetFileNameWithoutExtension(fuMedia.FileName), @"[^\w\-]", "_");
            string fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{safeName}{ext}";
            string savePath = Server.MapPath($"~/App_Data/Uploads/{fileName}");

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            fuMedia.SaveAs(savePath);

            // TODO: ContentService.SaveMedia(userId,
            //   Convert.ToInt32(ddlCourse.SelectedValue),
            //   tbTitle.Text.Trim(), tbMediaDesc.Text.Trim(),
            //   ddlMediaType.SelectedValue, fileName);

            ShowSuccess($"File '{fuMedia.FileName}' uploaded successfully.");
            ResetForm();
            LoadContentList();
        }

        protected void rptContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            if (e.CommandName == "Edit")
            {
                // TODO: populate form fields from ContentService.GetById(id)
                hfEditID.Value = id;
                litFormTitle.Text = $"Edit {hfActiveTab.Value}";
                lbCancelEdit.Visible = true;
                btnSave.Text = $"Update {hfActiveTab.Value}";
            }
            else if (e.CommandName == "Delete")
            {
                // TODO: ContentService.Delete(id);
                ShowSuccess("Content deleted.");
                LoadContentList();
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e) => ResetForm();
        protected void tbSearch_TextChanged(object sender, EventArgs e) => LoadContentList();

        private void ResetForm()
        {
            tbTitle.Text = tbChapterBody.Text = tbObjectives.Text = "";
            tbArticleBody.Text = tbArticleTag.Text = tbArticleUrl.Text = "";
            tbMediaDesc.Text = "";
            tbChapterNum.Text = "";
            hfEditID.Value = "0";
            litFormTitle.Text = $"New {hfActiveTab.Value}";
            lbCancelEdit.Visible = false;
            btnSave.Text = $"Save {hfActiveTab.Value}";
        }

        private void ShowSuccess(string msg)
        { pnlSuccess.Visible = true; litSuccess.Text = msg; pnlError.Visible = false; }
        private void ShowError(string msg)
        { pnlError.Visible = true; litError.Text = Server.HtmlEncode(msg); pnlSuccess.Visible = false; }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }

}