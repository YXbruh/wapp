using System;
using System.Web.UI;
using System.Web.Security;
using CSA.Services;

namespace CSA
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string msg = Request.QueryString["msg"];
                if (msg == "registered")
                {
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Account created successfully! You can now sign in.";
                }
                else if (msg == "loggedout")
                {
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "You have been signed out successfully.";
                }

                if (Session["UserID"] != null)
                    RedirectByRole();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string email = tbEmail.Text.Trim().ToLower();
            string password = tbPassword.Text.Trim();

            bool ok = UserService.Authenticate(email, password,
                out string role, out string userId, out string fullName, out string errorMsg);

            if (ok)
            {
                Session["UserID"] = userId;
                Session["Role"] = role;
                Session["FullName"] = fullName;
                Session["Email"] = email;

                if (cbRemember.Checked)
                    FormsAuthentication.SetAuthCookie(email, true);

                string dest = role == "Admin" ? "~/Admin/Admin_Dashboard.aspx"
                           : role == "Lecturer" ? "~/Lecturer/Lecturer_Dashboard.aspx"
                           : "~/Student/Student_Dashboard.aspx";
                Response.Redirect(dest + "?login=success");
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = errorMsg;
            }
        }

        private void RedirectByRole()
        {
            string role = Session["Role"] as string ?? "";
            if (role == "Admin")
                Response.Redirect("~/Admin/Admin_Dashboard.aspx");
            else if (role == "Lecturer")
                Response.Redirect("~/Lecturer/Lecturer_Dashboard.aspx");
            else
                Response.Redirect("~/Student/Student_Dashboard.aspx");
        }
    }
}
