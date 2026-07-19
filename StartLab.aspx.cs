using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using CSA.DataAccess;

namespace CSA
{
    /// <summary>
    /// Virtual lab page. Its only feature is a self-contained terminal that runs
    /// entirely in the browser (the JavaScript lives in Scripts/browser-terminal.js) —
    /// nothing is executed on the server. When opened from a specific lab it also shows
    /// that lab's brief.
    /// </summary>
    public partial class StartLab : Page
    {
        // Values handed to the in-browser terminal (rendered into data-* attributes).
        public string TermUser { get; private set; } = "student";
        public string TermHost { get; private set; } = "cybershield-lab";
        public string TermFlag { get; private set; } = "CSA{explore_the_box}";
        public string TermMotd { get; private set; } = "CyberShield practice shell";

        // Key under which the browser stores this user's saved machine state
        // (IndexedDB, client-side). Per user and per lab so sessions don't mix.
        public string TermSaveKey { get; private set; } = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only signed-in users may open the lab.
            if (Session["UserID"] == null)
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack) LoadContext();
        }

        /// <summary>
        /// Sets the terminal's username from the signed-in student and, when opened
        /// from a specific lab ("StartLab.aspx?id=LAB..."), shows that lab's brief and
        /// derives a per-lab hostname and flag. The lab's expected command is the
        /// grading key, so it is never shown here.
        /// </summary>
        private void LoadContext()
        {
            // A Linux-safe handle from the student's first name.
            string name = Session["FullName"] as string ?? "student";
            string first = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "student";
            string handle = Regex.Replace(first.ToLowerInvariant(), "[^a-z0-9]", "");
            TermUser = handle.Length > 0 ? handle : "student";

            string labId = Request.QueryString["id"];
            TermSaveKey = ("u" + Session["UserID"] + "-" +
                (string.IsNullOrWhiteSpace(labId) ? "free" : labId)).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(labId)) return;

            DataRow lab = LabService.GetById(labId);
            if (lab == null) return;

            pnlLab.Visible = true;
            pnlBack.Visible = true;
            lnkBack.NavigateUrl = ResolveUrl("~/Student/Labs.aspx");

            string title = Convert.ToString(lab["LabTitle"]);
            litHeading.Text = Server.HtmlEncode(title);
            litScenario.Text = Server.HtmlEncode(Convert.ToString(lab["Scenario"]));
            lblDifficulty.Text = Server.HtmlEncode(Convert.ToString(lab["Difficulty"]));

            lblTimeLimit.Text = lab["TimeLimitMinutes"] == DBNull.Value
                ? ""
                : $"~{Convert.ToInt32(lab["TimeLimitMinutes"])} min";

            string hint = lab["HintText"] == DBNull.Value ? "" : Convert.ToString(lab["HintText"]);
            if (!string.IsNullOrWhiteSpace(hint))
            {
                pnlHint.Visible = true;
                litHint.Text = Server.HtmlEncode(hint);
            }

            // Give the terminal a lab-flavoured hostname and a findable flag.
            TermHost = Slug(title, "lab");
            TermFlag = "CSA{" + labId.ToLowerInvariant() + "}";
            TermMotd = title + " — practice shell";
        }

        /// <summary>Turns text into a short, Linux-hostname-safe slug.</summary>
        private static string Slug(string text, string fallback)
        {
            string slug = Regex.Replace((text ?? "").ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            if (slug.Length > 24) slug = slug.Substring(0, 24).Trim('-');
            return slug.Length > 0 ? slug : fallback;
        }
    }
}
