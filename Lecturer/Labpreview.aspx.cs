using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using CSA.DataAccess;

namespace CSA.Lecturer
{
    public partial class LabPreview : Page
    {
        private string CurrentLecturerId => Session["UserID"]?.ToString() ?? "";
        private string LabId => Request.QueryString["id"] ?? "";

        // Values handed to the shared in-browser terminal (data-* attributes).
        public string TermUser { get; private set; } = "lecturer";
        public string TermHost { get; private set; } = "cybershield-lab";
        public string TermFlag { get; private set; } = "CSA{preview}";
        public string TermMotd { get; private set; } = "Lab preview shell";

        // Key under which this lecturer's preview machine is saved on the server
        // (per lecturer and per lab, distinct from the student session keys).
        public string TermSaveKey { get; private set; } = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            if (string.IsNullOrEmpty(LabId))
            { Response.Redirect("~/Lecturer/TerminalSandbox.aspx"); return; }

            // Lab must exist and belong to this lecturer.
            DataRow lab = LabService.GetById(LabId);
            if (lab == null || !OwnsLab())
            { Response.Redirect("~/Lecturer/TerminalSandbox.aspx?err=access"); return; }

            TermSaveKey = ("u" + Session["UserID"] + "-preview-" + LabId).ToLowerInvariant();

            if (!IsPostBack) BindLab(lab);
        }

        /// <summary>Confirms the lab's CreatedByID matches the current lecturer.</summary>
        private bool OwnsLab()
        {
            object owner = DBHelper.ExecuteScalar(
                "SELECT CreatedByID FROM VirtualLabs WHERE LabID = @LabID;",
                new System.Data.SqlClient.SqlParameter("@LabID", LabId));
            return owner != null && owner.ToString() == CurrentLecturerId;
        }

        private void BindLab(DataRow lab)
        {
            string title = lab["LabTitle"].ToString();

            litTitle.Text = Server.HtmlEncode(title);
            litInstructions.Text = Server.HtmlEncode(
                lab["Scenario"] == DBNull.Value ? "" : lab["Scenario"].ToString());
            litDifficulty.Text = Server.HtmlEncode(lab["Difficulty"].ToString());
            litValType.Text = Server.HtmlEncode(lab["ValidationType"].ToString());
            litExpected.Text = Server.HtmlEncode(lab["ExpectedCommand"].ToString());

            // Course name for the briefing header.
            object courseName = DBHelper.ExecuteScalar(@"
                SELECT c.CourseName FROM Courses c
                INNER JOIN VirtualLabs vl ON vl.CourseID = c.CourseID
                WHERE vl.LabID = @LabID;",
                new System.Data.SqlClient.SqlParameter("@LabID", LabId));
            litCourse.Text = Server.HtmlEncode(courseName?.ToString() ?? "");

            string hint = lab["HintText"] == DBNull.Value ? "" : lab["HintText"].ToString();
            pnlHint.Visible = !string.IsNullOrWhiteSpace(hint);
            litHint.Text = Server.HtmlEncode(hint);

            bool published = Convert.ToBoolean(lab["IsPublished"]);
            spanStatusBadge.Attributes["class"] = "badge " + (published ? "badge-green" : "badge-amber");
            litStatusText.Text = published ? "Published" : "Draft";

            // Flavour the browser terminal for this lab.
            TermUser = SlugUser(Session["FullName"] as string);
            TermHost = Slug(title, "lab");
            TermFlag = "CSA{" + LabId.ToLowerInvariant() + "}";
            TermMotd = title + " — preview shell";
        }

        /// <summary>
        /// Linux-safe handle from the lecturer's name, skipping a leading title so
        /// "Dr. Farah Aziz" becomes "farah" rather than "dr".
        /// </summary>
        private static string SlugUser(string fullName)
        {
            var titles = new[] { "dr", "mr", "mrs", "ms", "prof", "miss" };
            string[] parts = (fullName ?? "lecturer")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string handle = Regex.Replace(part.ToLowerInvariant(), "[^a-z0-9]", "");
                if (handle.Length > 0 && Array.IndexOf(titles, handle) < 0)
                    return handle;
            }
            return "lecturer";
        }

        /// <summary>Turns text into a short, Linux-hostname-safe slug.</summary>
        private static string Slug(string text, string fallback)
        {
            string slug = Regex.Replace((text ?? "").ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            if (slug.Length > 24) slug = slug.Substring(0, 24).Trim('-');
            return slug.Length > 0 ? slug : fallback;
        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            DataRow lab = LabService.GetById(LabId);
            if (lab == null || !OwnsLab()) { Response.Redirect("~/Lecturer/TerminalSandbox.aspx"); return; }

            BindLab(lab);   // re-populate the read-only briefing on postback

            string expected = lab["ExpectedCommand"].ToString();
            string valType = lab["ValidationType"].ToString();
            string submitted = tbCommand.Text.Trim();

            if (submitted.Length == 0)
            {
                ShowResult(false, "No command entered",
                    "Type a command above, then press Run to test it against the key.");
                return;
            }

            bool correct = ValidateCommand(submitted, expected, valType);

            if (correct)
                ShowResult(true, "Correct — the student would pass",
                    "This command satisfies the validation key using the '" + valType + "' rule.");
            else
                ShowResult(false, "Incorrect — the student would not pass",
                    "This command does not match the key under the '" + valType + "' rule. "
                    + "Adjust the command, or revise the key on the Terminal Sandbox page.");
        }

        /// <summary>
        /// Identical matching logic to the student Challenges page, so the
        /// preview result is faithful to what students will experience.
        /// </summary>
        private static bool ValidateCommand(string submitted, string expected, string validationType)
        {
            if (validationType == "Contains")
                return submitted.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;

            if (validationType == "Regex")
            {
                try { return Regex.IsMatch(submitted, expected, RegexOptions.IgnoreCase); }
                catch (ArgumentException) { return false; }
            }

            // ExactMatch (default)
            return string.Equals(submitted, expected, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowResult(bool ok, string title, string detail)
        {
            pnlResult.Visible = true;
            iResultIcon.Attributes["class"] = "ti " + (ok ? "ti-circle-check" : "ti-circle-x");
            litResultTitle.Text = title;
            litResultDetail.Text = detail;
            pnlResult.Style["border-color"] = ok ? "var(--success)" : "var(--danger)";
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
