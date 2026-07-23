using System;
using System.Data;
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

            try
            {
                string userId = UserService.Create(
                    tbFullName.Text.Trim(),
                    tbEmail.Text.Trim(),
                    tbPassword.Text,
                    ddlRole.SelectedValue,
                    tbStudentID.Text.Trim(),
                    tbPhone.Text.Trim(),
                    tbDepartment.Text.Trim());

                AdminService.LogAudit(Session["UserID"].ToString(),
                    "CREATE_USER", "Users", "0", "", tbFullName.Text.Trim());

                pnlSuccess.Visible = true;
                litSuccess.Text = $"User '{tbFullName.Text.Trim()}' created successfully. <a href='Users.aspx' style='color:var(--accent3)'>Back to Users</a>";
                tbFullName.Text = tbEmail.Text = tbPassword.Text = tbStudentID.Text = tbPhone.Text = tbDepartment.Text = "";
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error creating user: " + Server.HtmlEncode(ex.Message);
                pnlSuccess.Visible = false;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
