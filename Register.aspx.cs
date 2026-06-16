using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA
{

    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void cvTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = cbTerms.Checked;
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string fullName = tbFullName.Text.Trim();
            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text;

            // TODO: Hash password (e.g. BCrypt) and insert into DB
            // Example:
            // bool success = UserService.Register(fullName, email, password, out string error);
            bool success = false;
            string error = "Registration is not yet connected to the database.";

            if (success)
            {
                Response.Redirect("~/Login.aspx?registered=1");
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = Server.HtmlEncode(error);
            }
        }
    }
}