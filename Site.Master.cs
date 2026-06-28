using System;
using System.Web.UI;

namespace CSA
{
    public partial class Site : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["UserID"] = 999;
            Session["Role"] = "Admin";
            Session["FullName"] = "Admin Tester";

            // Show/hide nav items and avatar based on session role
            bool isLoggedIn = Session["UserID"] != null;
            string role = Session["Role"] as string ??                            

            pnlGuest.Visible = !isLoggedIn;
            pnlUser.Visible = isLoggedIn;
            pnlStudentNav.Visible = isLoggedIn && role != "Admin";
            pnlAdminNav.Visible = isLoggedIn && role == "Admin";

            // Set avatar initials from session
            if (isLoggedIn)
            {
                string name = Session["FullName"] as string ?? "User";
                string[] parts = name.Split(' ');
                string initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[parts.Length - 1][0]}"
                    : name.Substring(0, Math.Min(2, name.Length));
                lbProfile.Text = initials.ToUpper();
            }

            // Highlight active nav link
            HighlightActiveLink();
        }

        private void HighlightActiveLink()
        {
            string path = Request.AppRelativeCurrentExecutionFilePath?.ToLower() ?? "";
            // Use RegisterStartupScript to add .active class via JS
            // (simpler than server-side HtmlAnchor manipulation with runat=server)
            string script = $"document.querySelectorAll('.nav-link[href]').forEach(function(a){{" +
                            $"  if(window.location.pathname.toLowerCase().endsWith(a.getAttribute('href').replace('~','').replace('~/','').toLowerCase())){{" +
                            $"    a.classList.add('active');" +
                            $"  }}" +
                            $"}});";
            ScriptManager.RegisterStartupScript(this, GetType(), "activeNav", script, true);
        }
    }
}