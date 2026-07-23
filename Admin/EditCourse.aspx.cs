using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class EditCourse : Page
    {
        private string _courseId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            _courseId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(_courseId))
            { Response.Redirect("~/Admin/Courses.aspx"); return; }

            if (!IsPostBack) LoadForm();
        }

        private void LoadForm()
        {
            DataTable dt = CourseService.GetById(_courseId);
            if (dt.Rows.Count == 0)
            { Response.Redirect("~/Admin/Courses.aspx"); return; }

            DataRow row = dt.Rows[0];
            tbName.Text = row["CourseName"].ToString();
            tbDescription.Text = row["Description"].ToString();
            tbDuration.Text = row["DurationHours"].ToString();
            ddlLevel.SelectedValue = row["Level"].ToString();

            ddlCategory.DataSource = CourseService.GetCategories();
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select --", ""));
            if (row["CategoryID"] != DBNull.Value)
                ddlCategory.SelectedValue = row["CategoryID"].ToString();

            ddlInstructor.DataSource = CourseService.GetInstructors();
            ddlInstructor.DataTextField = "FullName";
            ddlInstructor.DataValueField = "UserID";
            ddlInstructor.DataBind();
            ddlInstructor.SelectedValue = row["InstructorID"].ToString();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(tbDuration.Text, out int duration))
            {
                pnlError.Visible = true;
                litError.Text = "Duration must be a valid number.";
                pnlSuccess.Visible = false;
                return;
            }

            try
            {
                CourseService.Update(_courseId,
                    tbName.Text.Trim(),
                    tbDescription.Text.Trim(),
                    ddlCategory.SelectedValue,
                    ddlInstructor.SelectedValue,
                    ddlLevel.SelectedValue,
                    duration);

                AdminService.LogAudit(Session["UserID"].ToString(),
                    "UPDATE_COURSE", "Courses", "0", "", tbName.Text.Trim());

                pnlSuccess.Visible = true;
                litSuccess.Text = "Course updated successfully.";
                pnlError.Visible = false;
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error: " + Server.HtmlEncode(ex.Message);
                pnlSuccess.Visible = false;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
