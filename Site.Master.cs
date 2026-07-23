using System;
using System.Data;
using System.Web;
using System.Web.UI;
using CSA.Services;

namespace CSA
{
    public partial class Site : MasterPage
    {
        /// <summary>
        /// Profile picture path for the signed-in user, cached in session so the nav
        /// avatar does not hit the database on every page. ProfileService refreshes the
        /// cache whenever the picture is changed or removed.
        /// </summary>
        private string CurrentProfilePicture
        {
            get
            {
                object cached = Session["ProfilePicture"];

                if (cached != null)
                {
                    return cached as string ?? "";
                }

                string picture = "";

                DataRow row =
                    ProfileService.Get(
                        Session["UserID"].ToString());

                if (row != null &&
                    row["ProfilePicture"] != DBNull.Value)
                {
                    picture =
                        Convert.ToString(
                            row["ProfilePicture"]);
                }

                // Cache "" too, so a user without a picture
                // is only looked up once.
                Session["ProfilePicture"] = picture;

                return picture;
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // Show/hide nav items and avatar
            // based on session role.
            bool isLoggedIn =
                Session["UserID"] != null;

            string role =
                Session["Role"] as string ?? "";

            pnlGuest.Visible =
                !isLoggedIn;

            pnlUser.Visible =
                isLoggedIn;

            pnlStudentNav.Visible =
                isLoggedIn &&
                role == "Student";

            pnlLecturerNav.Visible =
                isLoggedIn &&
                role == "Lecturer";

            pnlAdminNav.Visible =
                isLoggedIn &&
                role == "Admin";

            // Students go to My Courses.
            // Everyone else goes to the public Courses page.
            lnkCourses.HRef =
                isLoggedIn &&
                role == "Student"
                    ? ResolveUrl(
                        "~/Student/MyCourses.aspx")
                    : ResolveUrl(
                        "~/Courses.aspx");

            // Highlight active nav link.
            HighlightActiveLink();
        }

        /// <summary>
        /// The avatar is rendered here rather than in Page_Load because the master's
        /// Page_Load runs before the content page's control events. Uploading or
        /// removing a picture would otherwise leave the nav showing the old avatar
        /// until the next request.
        /// </summary>
        protected override void OnPreRender(
            EventArgs e)
        {
            base.OnPreRender(e);

            if (Session["UserID"] == null)
            {
                return;
            }

            string role =
                Session["Role"] as string ?? "";

            string name =
                Session["FullName"] as string ?? "User";

            string picture =
                CurrentProfilePicture;

            if (!string.IsNullOrEmpty(picture))
            {
                string src =
                    HttpUtility.HtmlAttributeEncode(
                        ResolveUrl(picture));

                lbProfile.InnerHtml =
                    $"<img src=\"{src}\" alt=\"\" class=\"nav-avatar-img\" />";
            }
            else
            {
                lbProfile.InnerHtml =
                    ProfileService.MakeInitials(name);
            }

            lbProfile.HRef =
                role == "Admin"
                    ? ResolveUrl(
                        "~/Admin/Profile.aspx")
                    : role == "Lecturer"
                        ? ResolveUrl(
                            "~/Lecturer/Profile.aspx")
                        : ResolveUrl(
                            "~/Student/Profile.aspx");
        }

        private void HighlightActiveLink()
        {
            string path =
                Request
                    .AppRelativeCurrentExecutionFilePath?
                    .ToLower() ?? "";

            // Use RegisterStartupScript to add
            // the active class through JavaScript.
            string script =
                "document.querySelectorAll('.nav-link[href]').forEach(function(a){" +
                "var href=a.getAttribute('href');" +
                "if(!href)return;" +
                "var cleanHref=href.replace('~','').replace('~/','').toLowerCase();" +
                "if(window.location.pathname.toLowerCase().endsWith(cleanHref)){" +
                "a.classList.add('active');" +
                "}" +
                "});";

            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "activeNav",
                script,
                true);
        }
    }
}