using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA.Student
{
    public partial class Student_Profile : Page
    {
        private string CurrentUserId => Session["UserID"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
            Response.Redirect("~/Login.aspx?msg=loggedout");
                return;
            }

            // Required for the profile picture picker to transmit file bytes.
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack) LoadProfile();
        }

        private void LoadProfile()
        {
            DataRow row = ProfileService.Get(CurrentUserId);
            if (row == null) { ShowError("Could not load your profile."); return; }

            string name = Convert.ToString(row["FullName"]);
            string email = Convert.ToString(row["Email"]);

            tbFullName.Text = name;
            tbEmail.Text = email;
            tbBio.Text = Convert.ToString(Session["Bio"]);

            litDisplayName.Text = name;
            litJoined.Text = row["CreatedAt"] == DBNull.Value
                ? "—"
                : Convert.ToDateTime(row["CreatedAt"]).ToString("MMMM yyyy");

            ShowAvatar(row["ProfilePicture"] == DBNull.Value ? "" : Convert.ToString(row["ProfilePicture"]), name);

            Session["FullName"] = name;
            Session["Email"] = email;
        }

        /// <summary>Shows the uploaded picture when present, otherwise initials.</summary>
        private void ShowAvatar(string picturePath, string name)
        {
            bool hasPicture = !string.IsNullOrEmpty(picturePath);
            pnlPicture.Visible = hasPicture;
            pnlInitials.Visible = !hasPicture;
            btnRemovePicture.Visible = hasPicture;

            if (hasPicture) imgAvatar.ImageUrl = picturePath;
            else litAvatarInitials.Text = ProfileService.MakeInitials(name);
        }

        protected void btnSaveInfo_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            bool ok = ProfileService.UpdateDetails(
                CurrentUserId,
                tbFullName.Text.Trim(),
                tbEmail.Text.Trim(),
                null, null,
                out string error);

            if (!ok) { ShowError(error); return; }

            Session["Bio"] = tbBio.Text.Trim();
            ShowSuccess("Profile updated successfully.");
            LoadProfile();
        }

        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            bool ok = ProfileService.ChangePassword(
                CurrentUserId, tbCurrentPwd.Text, tbNewPwd.Text, out string error);

            if (!ok) { ShowError(error); return; }

            tbCurrentPwd.Text = tbNewPwd.Text = tbConfirmPwd.Text = "";
            ShowSuccess("Password changed successfully.");
        }

        protected void btnUploadPicture_Click(object sender, EventArgs e)
        {
            string path = ProfileService.SavePicture(fuPicture.PostedFile, CurrentUserId, out string error);

            if (path == null) { ShowError(error); LoadProfile(); return; }

            ShowSuccess("Profile picture updated.");
            LoadProfile();
        }

        protected void btnRemovePicture_Click(object sender, EventArgs e)
        {
            ProfileService.RemovePicture(CurrentUserId);
            ShowSuccess("Profile picture removed.");
            LoadProfile();
        }

        private void ShowSuccess(string msg)
        {
            pnlError.Visible = false;
            pnlSuccess.Visible = true;
            litSuccess.Text = msg;
        }

        private void ShowError(string msg)
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;
            litError.Text = Server.HtmlEncode(msg);
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}
