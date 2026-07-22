using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.DataAccess;
using CSA.Services;

namespace CSA.Lecturer
{
    public partial class TerminalSandbox : Page
    {
        private string CurrentInstructorId => Session["UserID"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            // Required for the plain <input type="file" multiple> attachment picker
            // to actually transmit file bytes with the postback.
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadLabs();
                PendingAttachmentService.Clear(AttachBucket);
                LoadAttachments("");
            }
        }

        private void LoadCourseDropdown()
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("� Select Course �", ""));

            DataTable courses = LabService.GetCoursesForInstructor(CurrentInstructorId);
            foreach (DataRow row in courses.Rows)
            {
                ddlCourse.Items.Add(new ListItem(
                    row["CourseName"].ToString(),
                    row["CourseID"].ToString()));
            }
        }

        private void LoadLabs()
        {
            DataTable labs = LabService.GetByInstructor(CurrentInstructorId);
            rptLabs.DataSource = labs;
            rptLabs.DataBind();
            pnlEmpty.Visible = labs.Rows.Count == 0;
        }

        protected void btnSaveLab_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string labId = hfLabID.Value;
            if (labId == "0") labId = "";

            // ---- Server-side validation ----
            if (string.IsNullOrEmpty(ddlCourse.SelectedValue))
            { ShowError("Please select a course."); return; }

            if (!Regex.IsMatch(tbLabTitle.Text.Trim(), @"^[\w\s\-\:\.\,\(\)\/]{3,200}$"))
            { ShowError("Lab title contains invalid characters."); return; }

            if (!Regex.IsMatch(tbValidationKey.Text.Trim(), @"^[\x20-\x7E]{1,500}$"))
            { ShowError("Validation key contains invalid characters."); return; }

            int? timeLimit = null;
            if (!string.IsNullOrWhiteSpace(tbTimeLimit.Text) &&
                int.TryParse(tbTimeLimit.Text, out int tl))
            {
                timeLimit = tl;
            }

            try
            {
                bool isNew = string.IsNullOrEmpty(labId);
                string savedId = LabService.Save(
                    labId,
                    CurrentInstructorId,
                    ddlCourse.SelectedValue,
                    tbLabTitle.Text.Trim(),
                    tbInstructions.Text.Trim(),
                    tbHint.Text.Trim(),
                    tbValidationKey.Text.Trim(),
                    ddlValidationType.SelectedValue,
                    ddlDifficulty.SelectedValue,
                    timeLimit,
                    cbActive.Checked);

                pnlError.Visible = false;
                pnlSuccess.Visible = true;
                LoadLabs();

                // Flush anything staged while the lab was still unsaved.
                int committed = PendingAttachmentService.Commit(AttachBucket, "Lab", savedId, CurrentInstructorId);

                if (isNew)
                {
                    // Stay in the editor (now in Edit mode) so attachments can be added right away.
                    litSuccess.Text = committed > 0
                        ? $"Lab scenario saved with {committed} attachment(s)."
                        : "Lab scenario saved. You can now attach articles, pictures, media links, and documents to it.";
                    hfLabID.Value = savedId;
                    litEditorTitle.Text = "Edit Lab Scenario";
                    btnSaveLab.Text = "Update Lab Scenario";
                    LoadAttachments(savedId);
                    ClientScript.RegisterStartupScript(GetType(), "openEditor",
                        "toggleEditor(true);", true);
                }
                else
                {
                    litSuccess.Text = committed > 0
                        ? $"Lab scenario updated with {committed} new attachment(s)."
                        : "Lab scenario updated.";
                    ResetForm();
                    ClientScript.RegisterStartupScript(GetType(), "closeEditor",
                        "toggleEditor(false);", true);
                }
            }
            catch (Exception ex)
            {
                ShowError("Could not save the lab. " + ex.Message);
            }
        }

        protected void btnNewScenario_Click(object sender, EventArgs e)
        {
            ResetForm();
            ClientScript.RegisterStartupScript(GetType(), "openEditor",
                "toggleEditor(true);", true);
        }

        protected void rptLabs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();

            switch (e.CommandName)
            {
                case "Edit":
                    PopulateForm(id);
                    break;

                case "Preview":
                    Response.Redirect($"~/Lecturer/LabPreview.aspx?id={id}");
                    break;

                case "Delete":
                    // Attachments reference the lab, so they must go first.
                    AttachmentService.DeleteByParent("Lab", id, CurrentInstructorId);
                    LabService.Delete(id, CurrentInstructorId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Lab deleted.";
                    LoadLabs();
                    break;
            }
        }

        /// <summary>Loads a lab into the editor form for editing.</summary>
        private void PopulateForm(string labId)
        {
            DataRow lab = LabService.GetById(labId);
            if (lab == null) { ShowError("Lab not found."); return; }

            hfLabID.Value = labId;
            ddlCourse.SelectedValue = lab["CourseID"].ToString();
            tbLabTitle.Text = lab["LabTitle"].ToString();
            tbInstructions.Text = lab["Scenario"].ToString();
            tbHint.Text = lab["HintText"] == DBNull.Value ? "" : lab["HintText"].ToString();
            tbValidationKey.Text = lab["ExpectedCommand"].ToString();
            ddlValidationType.SelectedValue = lab["ValidationType"].ToString();
            ddlDifficulty.SelectedValue = lab["Difficulty"].ToString();
            tbTimeLimit.Text = lab["TimeLimitMinutes"] == DBNull.Value ? "" : lab["TimeLimitMinutes"].ToString();
            cbActive.Checked = Convert.ToBoolean(lab["IsPublished"]);

            litEditorTitle.Text = "Edit Lab Scenario";
            btnSaveLab.Text = "Update Lab Scenario";
            // Switching to an existing lab abandons anything staged for the new-lab
            // form, so it is not silently attached here.
            PendingAttachmentService.Clear(AttachBucket);
            LoadAttachments(labId);

            ClientScript.RegisterStartupScript(GetType(), "openEditor",
                "toggleEditor(true);", true);
        }

        private void ResetForm()
        {
            tbLabTitle.Text = tbInstructions.Text = tbHint.Text =
                tbValidationKey.Text = tbTimeLimit.Text = "";
            ddlCourse.SelectedIndex = 0;
            ddlValidationType.SelectedIndex = 0;
            ddlDifficulty.SelectedIndex = 0;
            hfLabID.Value = "0";
            cbActive.Checked = false;
            litEditorTitle.Text = "New Lab Scenario";
            btnSaveLab.Text = "Save Lab Scenario";

            // Abandoning the form drops anything staged for it.
            PendingAttachmentService.Clear(AttachBucket);
            LoadAttachments("");
        }

        // ========================================================================
        // Attachments (articles, pictures, media links, documents)
        // ========================================================================
        /// <summary>Session bucket holding attachments staged for an unsaved lab.</summary>
        private const string AttachBucket = "Lab";

        /// <summary>Current lab id, or "" while the scenario is still unsaved.</summary>
        private string CurrentLabId => hfLabID.Value == "0" ? "" : hfLabID.Value;

        private void LoadAttachments(string labId)
        {
            // The upload controls are always available: anything chosen before the
            // lab exists is staged and committed when the lab is saved.
            pnlAttachments.Visible = true;

            DataTable committed = string.IsNullOrEmpty(labId)
                ? null
                : AttachmentService.GetByLab(labId);

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
            string labId = CurrentLabId;

            if (!fuAttachFiles.HasFiles)
            { ShowError("Choose at least one file to upload."); LoadAttachments(labId); return; }

            List<string> rejected;
            int saved;
            bool staged = string.IsNullOrEmpty(labId);

            if (staged)
                saved = PendingAttachmentService.StageFiles(fuAttachFiles.PostedFiles, AttachBucket, out rejected);
            else
                saved = AttachmentService.SaveFiles(fuAttachFiles.PostedFiles, "Lab", labId, CurrentInstructorId, out rejected);

            string verb = staged ? "file(s) ready — saved when you save the lab." : "file(s) uploaded.";
            if (saved > 0 && rejected.Count == 0)
                ShowSuccess($"{saved} {verb}");
            else if (saved > 0)
                ShowError($"{saved} {verb} Skipped: {string.Join(", ", rejected)}");
            else
                ShowError(rejected.Count > 0 ? $"No files added. Skipped: {string.Join(", ", rejected)}" : "No files added.");

            LoadAttachments(labId);
            ClientScript.RegisterStartupScript(GetType(), "openEditorAfterUpload", "toggleEditor(true);", true);
        }

        protected void btnAddLink_Click(object sender, EventArgs e)
        {
            string labId = CurrentLabId;

            string title = tbLinkTitle.Text.Trim();
            string url = tbLinkUrl.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
            { ShowError("Both a link title and URL are required."); LoadAttachments(labId); return; }

            if (!Regex.IsMatch(url, @"^https?://[^\s]{4,}$"))
            { ShowError("Enter a valid URL starting with http:// or https://"); LoadAttachments(labId); return; }

            if (string.IsNullOrEmpty(labId))
            {
                PendingAttachmentService.StageLink(AttachBucket, title, url);
                ShowSuccess("Link ready — saved when you save the lab.");
            }
            else
            {
                AttachmentService.SaveLink("Lab", labId, title, url, CurrentInstructorId);
                ShowSuccess("Link added.");
            }

            tbLinkTitle.Text = "";
            tbLinkUrl.Text = "";
            LoadAttachments(labId);
            ClientScript.RegisterStartupScript(GetType(), "openEditorAfterLink", "toggleEditor(true);", true);
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
                LoadAttachments(CurrentLabId);
                ClientScript.RegisterStartupScript(GetType(), "openEditorAfterDelete", "toggleEditor(true);", true);
            }
        }

        public string GetAttachmentIcon(string type) =>
            type == "Image" ? "ti-photo" : type == "Link" ? "ti-link" : "ti-file-text";

        private void ShowError(string msg)
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;
            litError.Text = msg;
        }

        private void ShowSuccess(string msg)
        {
            pnlError.Visible = false;
            pnlSuccess.Visible = true;
            litSuccess.Text = msg;
        }

        public string GetDiffBadge(string d) =>
            d == "Beginner" ? "badge-blue" : d == "Intermediate" ? "badge-amber" : "badge-red";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}