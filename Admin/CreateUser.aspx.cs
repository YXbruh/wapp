using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class CreateUser : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadRoles();
        }

        private void LoadRoles()
        {
            DataTable roles = UserService.GetAllRoles();
            ddlRole.DataSource = roles;
            ddlRole.DataTextField = "RoleName";
            ddlRole.DataValueField = "RoleID";
            ddlRole.DataBind();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string fullName = tbFullName.Text.Trim();
            string email = tbEmail.Text.Trim();
            string studentId = tbStudentID.Text.Trim();

            // Check the rules up front so the user gets a plain message instead of a
            // raw constraint violation surfacing from the INSERT.
            if (!UserService.ValidateFields(fullName, email, tbPhone.Text,
                    tbDepartment.Text, studentId, out string validationError))
            { ShowError(validationError); return; }

            if (UserService.EmailExists(email))
            { ShowError("An account with that email already exists."); return; }

            if (UserService.StudentIdExists(studentId))
            { ShowError("That Student ID is already assigned to another user."); return; }

            try
            {
                string userId = UserService.Create(
                    fullName,
                    email,
                    tbPassword.Text,
                    ddlRole.SelectedValue,
                    studentId,
                    tbPhone.Text.Trim(),
                    tbDepartment.Text.Trim());

                // Record the id that was actually created; "0" made the audit trail
                // impossible to trace back to a row.
                AdminService.LogAudit(Session["UserID"].ToString(),
                    "CREATE_USER", "Users", userId, "", fullName);

                pnlSuccess.Visible = true;
                pnlError.Visible = false;
                litSuccess.Text = $"User '{Server.HtmlEncode(fullName)}' created successfully. <a href='Users.aspx' style='color:var(--accent3)'>Back to Users</a>";
                tbFullName.Text = tbEmail.Text = tbPassword.Text = tbStudentID.Text = tbPhone.Text = tbDepartment.Text = "";
            }
            catch (SqlException ex)
            {
                ShowError(UserService.DescribeSqlError(ex, "create the user"));
            }
            catch (Exception)
            {
                ShowError("Could not create the user. Please try again or contact an administrator.");
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
