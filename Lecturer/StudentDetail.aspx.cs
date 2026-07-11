using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class StudentDetail : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            string studentId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(studentId))
            { Response.Redirect("~/Lecturer/ClassAnalytics.aspx"); return; }

            if (!IsPostBack) LoadStudentData(studentId);
        }

        private void LoadStudentData(string studentId)
        {
            // TODO:
            // var student = UserService.GetById(studentId);
            // if (student == null) { Response.Redirect("~/Lecturer/ClassAnalytics.aspx"); return; }
            // litName.Text       = student.FullName;
            // litEmail.Text      = student.Email;
            // litEnrolled.Text   = student.EnrolledCount + " courses";
            // litLastActive.Text = student.LastActive.ToString("dd MMM yyyy");
            // string[] parts = student.FullName.Split(' ');
            // litInitials.Text = parts.Length >= 2
            //     ? $"{parts[0][0]}{parts[parts.Length-1][0]}" : student.FullName.Substring(0,2);

            // var stats = AnalyticsService.GetStudentDetail(studentId, Session["UserID"].ToString());
            // litQuizAvg.Text    = stats.QuizAvg + "%";
            // litLabsDone.Text   = stats.LabsDone.ToString();
            // litLabsTotal.Text  = stats.LabsTotal.ToString();
            // litChallenges.Text = stats.ChallengesDone.ToString();
            // litSandbox.Text    = stats.SandboxCleared ? "Cleared ?" : "Pending";
            // rptQuizAttempts.DataSource = stats.QuizAttempts; rptQuizAttempts.DataBind();
            // rptLabs.DataSource         = stats.Labs;         rptLabs.DataBind();
            // pnlNoQuiz.Visible = stats.QuizAttempts.Count == 0;
            // pnlNoLabs.Visible = stats.Labs.Count == 0;

            litName.Text = "Student Name";
            litEmail.Text = "student@example.com";
            pnlNoQuiz.Visible = pnlNoLabs.Visible = true;
        }

        public string GetScoreColor(int s) =>
            s >= 80 ? "var(--success)" : s >= 60 ? "var(--warning)" : "var(--danger)";

        public string GetLabStatusBadge(string s) =>
            s == "Done" ? "badge-green" : s == "In Progress" ? "badge-amber" : "badge-blue";

        public string GetLabStatusIcon(string s) =>
            s == "Done" ? "ti-circle-check" : s == "In Progress" ? "ti-player-play" : "ti-clock";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }

}