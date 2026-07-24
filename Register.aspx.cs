using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string fullName = tbFullName.Text.Trim();
            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text;

            bool success = UserService.Register(fullName, email, password, out string error);

            if (success)
            {
                Response.Redirect("~/Login.aspx?msg=registered");
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = error;
            }
        }
    }
}
