using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class EditUser : Page
    {
        private string _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            _userId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(_userId))
            { Response.Redirect("~/Admin/Users.aspx"); return; }

            if (!IsPostBack)
            {
                LoadRoles();
                LoadUser();
            }
        }

        private void LoadRoles()
        {
            DataTable roles = UserService.GetAllRoles();
            ddlRole.DataSource = roles;
            ddlRole.DataTextField = "RoleName";
            ddlRole.DataValueField = "RoleID";
            ddlRole.DataBind();
        }

        private void LoadUser()
        {
            DataTable dt = UserService.GetById(_userId);
            if (dt.Rows.Count == 0)
            { Response.Redirect("~/Admin/Users.aspx"); return; }

            DataRow row = dt.Rows[0];
            hfUserID.Value = _userId.ToString();
            tbFullName.Text = row["FullName"].ToString();
            tbEmail.Text = row["Email"].ToString();
            tbPhone.Text = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"].ToString() : "";
            tbDepartment.Text = row["Department"] != DBNull.Value ? row["Department"].ToString() : "";
            tbStudentID.Text = row["StudentID"] != DBNull.Value ? row["StudentID"].ToString() : "";
            cbActive.Checked = Convert.ToBoolean(row["IsActive"]);

            if (ddlRole.Items.FindByValue(row["RoleID"].ToString()) != null)
                ddlRole.SelectedValue = row["RoleID"].ToString();
        }

        protected void btnGeneratePw_Click(object sender, EventArgs e)
        {
            string temp = PasswordHelper.GenerateTempPassword();
            tbNewPassword.Text = temp;
            litGeneratedPw.Text = "<div style='margin-top:8px;font-size:12px;color:var(--warning)'><i class='ti ti-copy'></i> Generated: <code style='padding:2px 6px;background:var(--bg2);border-radius:4px;user-select:all'>"
                + System.Web.HttpUtility.HtmlEncode(temp) + "</code></div>";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                UserService.Update(_userId,
                    tbFullName.Text.Trim(),
                    tbEmail.Text.Trim(),
                    ddlRole.SelectedValue,
                    cbActive.Checked,
                    tbPhone.Text.Trim(),
                    tbDepartment.Text.Trim(),
                    tbStudentID.Text.Trim());

                string pw = tbNewPassword.Text.Trim();
                if (!string.IsNullOrEmpty(pw))
                {
                    UserService.UpdatePassword(_userId, pw);
                    AdminService.LogAudit(Session["UserID"].ToString(),
                        "RESET_PASSWORD", "Users", "0", "", "Password reset by admin");
                }

                AdminService.LogAudit(Session["UserID"].ToString(),
                    "UPDATE_USER", "Users", "0", "", tbFullName.Text.Trim());

                pnlSuccess.Visible = true;
                litSuccess.Text = "User updated successfully.";
                litGeneratedPw.Text = "";
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error updating user: " + ex.Message;
                pnlSuccess.Visible = false;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
