using System;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class CreateCourse : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadDropdowns();
        }

        private void LoadDropdowns()
        {
            ddlCategory.DataSource = CourseService.GetCategories();
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select --", ""));

            ddlInstructor.DataSource = CourseService.GetInstructors();
            ddlInstructor.DataTextField = "FullName";
            ddlInstructor.DataValueField = "UserID";
            ddlInstructor.DataBind();
            ddlInstructor.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select --", ""));
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            if (!int.TryParse(tbDuration.Text, out int duration))
            {
                ShowError("Duration must be a valid number.");
                return;
            }

            string name = tbName.Text.Trim();
            if (!CourseService.ValidateCourse(name, tbDescription.Text, duration, out string validationError))
            { ShowError(validationError); return; }

            try
            {
                string courseId = CourseService.Create(
                    name,
                    tbDescription.Text.Trim(),
                    ddlCategory.SelectedValue,
                    ddlInstructor.SelectedValue,
                    ddlLevel.SelectedValue,
                    duration);

                // Log the real course id so the audit row points at an actual record.
                AdminService.LogAudit(Session["UserID"].ToString(),
                    "CREATE_COURSE", "Courses", courseId, "", name);

                pnlSuccess.Visible = true;
                pnlError.Visible = false;
                litSuccess.Text = $"Course '{Server.HtmlEncode(name)}' created successfully. <a href='Courses.aspx' style='color:var(--accent3)'>Back to Courses</a>";
                tbName.Text = tbDescription.Text = "";
                ddlCategory.SelectedIndex = 0;
                ddlInstructor.SelectedIndex = 0;
                ddlLevel.SelectedIndex = 0;
                tbDuration.Text = "0";
            }
            catch (Exception)
            {
                ShowError("Could not create the course. Please try again or contact an administrator.");
            }
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litError.Text = Server.HtmlEncode(message);
            pnlSuccess.Visible = false;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
