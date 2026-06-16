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

            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text;

            // TODO: replace with your DB authentication call
            // Example: bool ok = UserService.Authenticate(email, password, out string role, out int userId, out string fullName);
            bool ok = false;
            string role = "";
            int userId = 0;
            string fullName = "";

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
                litError.Text = "Invalid email or password. Please try again.";
            }
        }

        private void RedirectByRole()
        {
            string role = Session["Role"] as string ?? "";
            if (role == "Admin")
                Response.Redirect("~/Admin/Dashboard.aspx");
            else
                Response.Redirect("~/Student/Dashboard.aspx");
        }
    }

}