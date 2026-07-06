using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace CSA.Lecturer
{
    public partial class TerminalSandbox : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Instructor")                       //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) { LoadCourseDropdown(); LoadLabs(); }
        }

        private void LoadCourseDropdown()
        {
            //int userId = (int)Session["UserID"];                                                          //Bypass login for testing
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("— Select Course —", ""));
            // TODO: foreach (var c in CourseService.GetByInstructor(userId))
            //           ddlCourse.Items.Add(new ListItem(c.CourseName, c.CourseID.ToString()));
        }

        private void LoadLabs()
        {
            //int userId = (int)Session["UserID"];                                                      //Bypass login for testing
            // TODO:
            // var labs = LabService.GetByInstructor(userId);
            // rptLabs.DataSource = labs; rptLabs.DataBind();
            // pnlEmpty.Visible = labs.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void btnSaveLab_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            //int userId = (int)Session["UserID"];                                                          //Bypass login for testing
            int labId = Convert.ToInt32(hfLabID.Value);

            // Validate: title 3–200 printable chars
            if (!Regex.IsMatch(tbLabTitle.Text.Trim(), @"^[\w\s\-\:\.\,\(\)\/]{3,200}$"))
            { pnlError.Visible = true; litError.Text = "Lab title contains invalid characters."; return; }

            // Validate: validation key — printable ASCII only, 1–500 chars
            if (!Regex.IsMatch(tbValidationKey.Text.Trim(), @"^[\x20-\x7E]{1,500}$"))
            { pnlError.Visible = true; litError.Text = "Validation key contains invalid characters."; return; }

            int? timeLimit = null;
            if (!string.IsNullOrWhiteSpace(tbTimeLimit.Text) && int.TryParse(tbTimeLimit.Text, out int tl))
                timeLimit = tl;

            // TODO: LabService.Save(labId, userId,
            //   Convert.ToInt32(ddlCourse.SelectedValue),
            //   tbLabTitle.Text.Trim(), tbInstructions.Text.Trim(),
            //   tbHint.Text.Trim(), tbValidationKey.Text.Trim(),
            //   ddlValidationType.SelectedValue, ddlDifficulty.SelectedValue,
            //   timeLimit, cbActive.Checked);

            pnlSuccess.Visible = true;
            litSuccess.Text = labId == 0 ? "Lab scenario saved." : "Lab scenario updated.";
            pnlError.Visible = false;
            ResetForm();
            LoadLabs();

            // Close editor panel via script
            ScriptManager.RegisterStartupScript(this, GetType(), "closeEditor",
                "toggleEditor(false);", true);
        }

        protected void rptLabs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "Edit":
                    // TODO: populate form from LabService.GetById(id)
                    hfLabID.Value = id.ToString();
                    litEditorTitle.Text = "Edit Lab Scenario";
                    btnSaveLab.Text = "Update Lab Scenario";
                    ScriptManager.RegisterStartupScript(this, GetType(), "openEditor",
                        "toggleEditor(true);", true);
                    break;
                case "Preview":
                    Response.Redirect($"~/Lecturer/LabPreview.aspx?id={id}");
                    break;
                case "Delete":
                    // TODO: LabService.Delete(id);
                    pnlSuccess.Visible = true; litSuccess.Text = "Lab deleted.";
                    LoadLabs(); break;
            }
        }

        private void ResetForm()
        {
            tbLabTitle.Text = tbInstructions.Text = tbHint.Text = tbValidationKey.Text = tbTimeLimit.Text = "";
            hfLabID.Value = "0";
            litEditorTitle.Text = "New Lab Scenario";
            btnSaveLab.Text = "Save Lab Scenario";
            cbActive.Checked = false;
        }

        public string GetDiffBadge(string d) =>
            d == "Beginner" ? "badge-blue" : d == "Intermediate" ? "badge-amber" : "badge-red";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}