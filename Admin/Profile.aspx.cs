using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Admin_Profile : Page
    {
        private string _userId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            _userId = Session["UserID"].ToString();

            // Required for the profile picture picker to transmit file bytes.
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
                LoadProfile();
        }

        private void LoadProfile()
        {
            DataTable dt = UserService.GetById(_userId);
            if (dt.Rows.Count == 0)
            { Response.Redirect("~/Login.aspx"); return; }

            DataRow row = dt.Rows[0];

            string name = row["FullName"].ToString();
            string email = row["Email"].ToString();
            string role = row["Role"].ToString();
            DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

            tbFullName.Text = name;
            tbEmail.Text = email;
            litRole.Text = role;
            litDisplayName.Text = name;
            litJoined.Text = createdAt.ToString("MMMM yyyy");

            string picturePath = !dt.Columns.Contains("ProfilePicture") || row["ProfilePicture"] == DBNull.Value
                ? ""
                : Convert.ToString(row["ProfilePicture"]);

            bool hasPicture = !string.IsNullOrEmpty(picturePath);
            pnlPicture.Visible = hasPicture;
            pnlInitials.Visible = !hasPicture;
            btnRemovePicture.Visible = hasPicture;

            if (hasPicture) imgAvatar.ImageUrl = picturePath;
            else litAvatarInitials.Text = ProfileService.MakeInitials(name);
        }

        protected void btnUploadPicture_Click(object sender, EventArgs e)
        {
            string path = ProfileService.SavePicture(fuPicture.PostedFile, _userId, out string error);

            if (path == null)
            {
                pnlSuccess.Visible = false;
                pnlError.Visible = true;
                litError.Text = Server.HtmlEncode(error);
                LoadProfile();
                return;
            }

            AdminService.LogAudit(_userId, "UPDATE_PROFILE", "Users", _userId, "", "Profile picture updated");

            pnlError.Visible = false;
            pnlSuccess.Visible = true;
            litSuccess.Text = "Profile picture updated.";
            LoadProfile();
        }

        protected void btnRemovePicture_Click(object sender, EventArgs e)
        {
            ProfileService.RemovePicture(_userId);
            AdminService.LogAudit(_userId, "UPDATE_PROFILE", "Users", _userId, "", "Profile picture removed");

            pnlError.Visible = false;
            pnlSuccess.Visible = true;
            litSuccess.Text = "Profile picture removed.";
            LoadProfile();
        }

        protected void btnSaveInfo_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                string roleId = Session["RoleID"]?.ToString() ?? UserService.GetRoleIdByName("Admin");
                UserService.Update(_userId, tbFullName.Text.Trim(), tbEmail.Text.Trim(), roleId, true);

                Session["FullName"] = tbFullName.Text.Trim();
                Session["Email"] = tbEmail.Text.Trim();

                AdminService.LogAudit(_userId, "UPDATE_PROFILE", "Users", _userId, "", "Profile updated");

                pnlSuccess.Visible = true;
                litSuccess.Text = "Profile updated successfully.";
                pnlError.Visible = false;
                LoadProfile();
            }
            catch (Exception ex)
            {
                litError.Text = "Error: " + Server.HtmlEncode(ex.Message);
                pnlError.Visible = true;
            }
        }

        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                DataTable dt = UserService.GetById(_userId);
                if (dt.Rows.Count == 0)
                {
                    pnlError.Visible = true;
                    litError.Text = "Account not found. Please log in again.";
                    pnlSuccess.Visible = false;
                    return;
                }
                string storedHash = dt.Rows[0]["PasswordHash"].ToString();
                if (!PasswordHelper.Verify(tbCurrentPwd.Text.Trim(), storedHash))
                {
                    pnlError.Visible = true;
                    litError.Text = "Current password is incorrect.";
                    pnlSuccess.Visible = false;
                    return;
                }

                UserService.UpdatePassword(_userId, tbNewPwd.Text.Trim());

                AdminService.LogAudit(_userId, "CHANGE_PASSWORD", "Users", _userId, "", "Password changed");

                pnlSuccess.Visible = true;
                litSuccess.Text = "Password changed successfully.";
                pnlError.Visible = false;
                tbCurrentPwd.Text = "";
                tbNewPwd.Text = "";
                tbConfirmPwd.Text = "";
            }
            catch (Exception ex)
            {
                litError.Text = "Error: " + Server.HtmlEncode(ex.Message);
                pnlError.Visible = true;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
