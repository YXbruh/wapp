using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{
    public partial class Student_Analytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack) LoadAnalytics();
        }

        private void LoadAnalytics()
        {
            //int userId = (int)Session["UserID"];                                                  //remove to bypass login for testing
            // TODO: replace with DB calls
            // var stats = AnalyticsService.GetStudentStats(userId);
            // litOverall.Text   = stats.OverallProgress + "%";
            // litAvgQuiz.Text   = stats.AvgQuizScore + "%";
            // litLabsDone.Text  = stats.LabsDone.ToString();
            // litLabsTotal.Text = stats.LabsTotal.ToString();
            // litStudyTime.Text = stats.StudyHours + "h";
            // rptCourseProgress.DataSource = stats.CourseProgress; rptCourseProgress.DataBind();
            // rptQuizScores.DataSource     = stats.QuizScores;     rptQuizScores.DataBind();
            // rptLabProgress.DataSource    = stats.LabProgress;    rptLabProgress.DataBind();
            pnlNoCourses.Visible = true;
            pnlNoQuizzes.Visible = true;
            pnlNoLabs.Visible = true;
        }

        // Called from markup — colours bar by score
        public string GetScoreColor(int score)
        {
            if (score >= 80) return "var(--success)";
            if (score >= 60) return "var(--warning)";
            return "var(--danger)";
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }

}