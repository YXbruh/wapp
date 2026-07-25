using System;
using System.Data;
using System.Data.SqlClient;
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

            string fullName = tbFullName.Text.Trim();
            string email = tbEmail.Text.Trim();
            string studentId = tbStudentID.Text.Trim();

            if (!UserService.ValidateFields(fullName, email, tbPhone.Text,
                    tbDepartment.Text, studentId, out string validationError))
            { ShowError(validationError); return; }

            // Exclude this user so saving the form unchanged is not treated as a clash.
            if (UserService.EmailExists(email, _userId))
            { ShowError("Another account already uses that email."); return; }

            if (UserService.StudentIdExists(studentId, _userId))
            { ShowError("That Student ID is already assigned to another user."); return; }

            try
            {
                UserService.Update(_userId,
                    fullName,
                    email,
                    ddlRole.SelectedValue,
                    cbActive.Checked,
                    tbPhone.Text.Trim(),
                    tbDepartment.Text.Trim(),
                    studentId);

                string pw = tbNewPassword.Text.Trim();
                bool pwReset = false;
                bool pwEmailed = false;
                if (!string.IsNullOrEmpty(pw))
                {
                    UserService.UpdatePassword(_userId, pw);
                    AdminService.LogAudit(Session["UserID"].ToString(),
                        "RESET_PASSWORD", "Users", _userId, "", "Password reset by admin");
                    pwReset = true;
                    pwEmailed = SendNewPasswordEmail(email, fullName, pw);
                }

                AdminService.LogAudit(Session["UserID"].ToString(),
                    "UPDATE_USER", "Users", _userId, "", fullName);

                pnlError.Visible = false;
                pnlSuccess.Visible = true;
                if (pwReset)
                    litSuccess.Text = pwEmailed
                        ? "User updated successfully. The new password was emailed to " + Server.HtmlEncode(tbEmail.Text.Trim()) + "."
                        : "User updated and password reset, but the email could not be sent (SMTP not configured or unavailable). Share the new password with the user manually.";
                else
                    litSuccess.Text = "User updated successfully.";
                litGeneratedPw.Text = "";
            }
            catch (SqlException ex)
            {
                ShowError(UserService.DescribeSqlError(ex, "update the user"));
            }
            catch (Exception)
            {
                ShowError("Could not update the user. Please try again or contact an administrator.");
            }
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litError.Text = Server.HtmlEncode(message);
            pnlSuccess.Visible = false;
        }

        // Sends the newly reset password to the user, using the same SMTP setup and
        // HTML style as the announcement emails (EmailService reads the same Web.config keys).
        private bool SendNewPasswordEmail(string toEmail, string fullName, string newPassword)
        {
            string title = "Your CyberShield Academy password has been reset";
            string body =
                $"<h2>{System.Net.WebUtility.HtmlEncode(title)}</h2>" +
                $"<p>Hi {System.Net.WebUtility.HtmlEncode(fullName)},</p>" +
                "<p>An administrator has reset your password. Your new password is:</p>" +
                $"<p style='font-size:16px;font-weight:bold;letter-spacing:1px'>{System.Net.WebUtility.HtmlEncode(newPassword)}</p>" +
                "<p>Please sign in and change it as soon as possible.</p>" +
                "<hr><p><small>Sent from CyberShield Academy</small></p>";
            return EmailService.Send(toEmail, title, body);
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
