using System;
using System.Web.UI;
using System.Web.Security;

namespace CSA
{

    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && User.Identity.IsAuthenticated)
                RedirectByRole();
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string email = tbEmail.Text.Trim().ToLower();
            string password = tbPassword.Text.Trim();

            // TODO: replace with your DB authentication call
            // Example: bool ok = UserService.Authenticate(email, password, out string role, out int userId, out string fullName);
            bool ok = false;
            string role = "";
            int userId = 0;
            string fullName = "";

            // ============================================================
            // HARDCODED TESTING BYPASS (No Database Required)
            // ============================================================
            if (email == "admin@test.com" && password == "admin123")
            {
                ok = true;
                role = "Admin";
                userId = 999;
                fullName = "Admin Tester";
            }
            // Add additional student profiles here if needed for testing:
            else if (email == "student@test.com" && password == "student123")
            {
                ok = true;
                role = "Student";
                userId = 888;
                fullName = "John Student";
            }
            // ============================================================

            if (ok)
            {
                Session["UserID"] = userId;
                Session["Role"] = role;
                Session["FullName"] = fullName;
                Session["Email"] = email;

                if (cbRemember.Checked)
                    FormsAuthentication.SetAuthCookie(email, true);

                RedirectByRole();
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = $"Invalid. Server received: Email='{email}' | Pass='{password}'";
            }
        }

        private void RedirectByRole()
        {
            string role = Session["Role"] as string ?? "";
            if (role == "Admin")
                Response.Redirect("~/Admin/Admin_Dashboard.aspx");
            else
                Response.Redirect("~/Student/Student_Dashboard.aspx");
        }
    }

}