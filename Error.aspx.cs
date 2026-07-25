using System;
using System.Web.UI;

namespace CSA
{
    /// <summary>
    /// Friendly landing page for unhandled errors. Global.asax classifies the exception
    /// and passes a short reason through the query string; nothing about the exception
    /// itself (type, stack, SQL text, file paths) is ever rendered here.
    /// </summary>
    public partial class ErrorPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            switch (Request.QueryString["reason"])
            {
                case "input":
                    litMessage.Text =
                        "Some of the text submitted contains characters that are not allowed " +
                        "(for example &lt; or &gt;). Please remove them and try again.";
                    break;
                case "notfound":
                    litMessage.Text = "The page you asked for does not exist.";
                    break;
                default:
                    litMessage.Text =
                        "An unexpected error occurred and has been logged. " +
                        "Please try again, or contact an administrator if it keeps happening.";
                    break;
            }
        }
    }
}
