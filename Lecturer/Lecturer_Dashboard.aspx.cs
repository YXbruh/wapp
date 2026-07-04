using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class Lecturer_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Instructor")           //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadDashboard();
        }

        private void LoadDashboard()
        {
            //int userId = (int)Session["UserID"];                                              //Bypass login for testing
            litName.Text = Session["FullName"] as string ?? "Lecturer";
            litDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            // TODO:
            // var stats = LecturerService.GetDashboardStats(userId);
            // litModules.Text  = stats.ActiveModules.ToString();
            // litStudents.Text = stats.TotalStudents.ToString();
            // litLabRate.Text  = stats.AvgLabCompletion + "%";
            // litQuizAvg.Text  = stats.AvgQuizScore + "%";
            // rptLabRates.DataSource   = stats.LabRates;   rptLabRates.DataBind();
            // rptQuizScores.DataSource = stats.QuizScores; rptQuizScores.DataBind();
            // rptModules.DataSource    = stats.Modules;    rptModules.DataBind();
            // pnlNoLabs.Visible    = stats.LabRates.Count == 0;
            // pnlNoQuiz.Visible    = stats.QuizScores.Count == 0;
            // pnlNoModules.Visible = stats.Modules.Count == 0;
            pnlNoLabs.Visible = pnlNoQuiz.Visible = pnlNoModules.Visible = true;
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        public string GetTypeBadge(string t) =>
            t == "Chapter" ? "badge-blue" : t == "Lab" ? "badge-green" : t == "Quiz" ? "badge-amber" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}