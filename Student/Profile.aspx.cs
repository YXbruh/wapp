using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using WebGrease.Activities;

namespace CSA.Student
{
    public partial class Student_Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack) LoadProfile();
        }

        private void LoadProfile()
        {
            //int userId = (int)Session["UserID"];                  //remove to bypass login for testing        
            // TODO: replace with DB call
            // var user = UserService.GetById(userId);
            // tbFullName.Text = user.FullName;
            // tbEmail.Text    = user.Email;
            // tbBio.Text      = user.Bio;
            // litJoined.Text  = user.CreatedAt.ToString("MMMM yyyy");

            // Populate from session as fallback
            string name = Session["FullName"] as string ?? "";
            tbFullName.Text = name;
            tbEmail.Text = Session["Email"] as string ?? "";
            litDisplayName.Text = name;
            litJoined.Text = "—";

            string[] parts = name.Split(' ');
            litAvatarInitials.Text = parts.Length >= 2
                ? $"{parts[0][0]}{parts[parts.Length - 1][0]}"
                : name.Substring(0, Math.Min(2, name.Length));
        }

        protected void btnSaveInfo_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int userId = (int)Session["UserID"];
            // TODO: UserService.UpdateProfile(userId, tbFullName.Text.Trim(), tbEmail.Text.Trim(), tbBio.Text.Trim());
            // Update session
            Session["FullName"] = tbFullName.Text.Trim();
            Session["Email"] = tbEmail.Text.Trim();

            pnlSuccess.Visible = true;
            litSuccess.Text = "Profile updated successfully.";
            pnlError.Visible = false;
            LoadProfile();
        }

        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int userId = (int)Session["UserID"];
            // TODO:
            // bool ok = UserService.ChangePassword(userId, tbCurrentPwd.Text, tbNewPwd.Text, out string err);
            bool ok = false;
            string err = "Password change not yet connected to the database.";

            if (ok)
            {
                pnlSuccess.Visible = true;
                litSuccess.Text = "Password changed successfully.";
                pnlError.Visible = false;
                tbCurrentPwd.Text = "";
                tbNewPwd.Text = "";
                tbConfirmPwd.Text = "";
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = Server.HtmlEncode(err);
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }

}