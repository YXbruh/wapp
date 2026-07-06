using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.DataAccess;

namespace CSA.Lecturer
{
    public partial class TerminalSandbox : Page
    {
        // ---- Temporary test instructor while login is bypassed ----
        // Replace with (int)Session["UserID"] once authentication is wired up.
        private int CurrentInstructorId
        {
            get
            {
                if (Session["UserID"] != null) return (int)Session["UserID"];
                return 1; // <-- test lecturer UserID (see seed script)
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")   // Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack) { LoadCourseDropdown(); LoadLabs(); }
        }

        private void LoadCourseDropdown()
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("— Select Course —", ""));

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

            int labId = Convert.ToInt32(hfLabID.Value);

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
                int savedId = LabService.Save(
                    labId,
                    CurrentInstructorId,
                    Convert.ToInt32(ddlCourse.SelectedValue),
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
                litSuccess.Text = labId == 0
                    ? "Lab scenario saved."
                    : "Lab scenario updated.";

                ResetForm();
                LoadLabs();

                ScriptManager.RegisterStartupScript(this, GetType(), "closeEditor",
                    "toggleEditor(false);", true);
            }
            catch (Exception ex)
            {
                ShowError("Could not save the lab. " + ex.Message);
            }
        }

        protected void rptLabs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "Edit":
                    PopulateForm(id);
                    break;

                case "Preview":
                    Response.Redirect($"~/Lecturer/LabPreview.aspx?id={id}");
                    break;

                case "Delete":
                    LabService.Delete(id, CurrentInstructorId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Lab deleted.";
                    LoadLabs();
                    break;
            }
        }

        /// <summary>Loads a lab into the editor form for editing.</summary>
        private void PopulateForm(int labId)
        {
            DataRow lab = LabService.GetById(labId);
            if (lab == null) { ShowError("Lab not found."); return; }

            hfLabID.Value = labId.ToString();
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

            ScriptManager.RegisterStartupScript(this, GetType(), "openEditor",
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
        }

        private void ShowError(string msg)
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;
            litError.Text = msg;
        }

        public string GetDiffBadge(string d) =>
            d == "Beginner" ? "badge-blue" : d == "Intermediate" ? "badge-amber" : "badge-red";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }
}