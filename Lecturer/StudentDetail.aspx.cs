using System;
using System.Data;
using System.Web.UI;
using CSA.DataAccess;
using CSA.Services;

namespace CSA.Lecturer
{
    public partial class StudentDetail : Page
    {
        private string CurrentLecturerId => Session["UserID"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            string studentId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(studentId))
            { Response.Redirect("~/Lecturer/ClassAnalytics.aspx"); return; }

            // A lecturer may only view students enrolled in their own courses.
            if (!StudentDetailService.Owns(CurrentLecturerId, studentId))
            { Response.Redirect("~/Lecturer/ClassAnalytics.aspx?err=access"); return; }

            if (!IsPostBack) LoadStudentData(studentId);
        }

        private void LoadStudentData(string studentId)
        {
            // ---- Profile header ----
            DataRow profile = StudentDetailService.GetProfile(studentId, CurrentLecturerId);
            if (profile == null)
            { Response.Redirect("~/Lecturer/ClassAnalytics.aspx"); return; }

            string fullName = profile["FullName"].ToString();
            litName.Text = Server.HtmlEncode(fullName);
            litEmail.Text = Server.HtmlEncode(profile["Email"].ToString());
            litInitials.Text = Server.HtmlEncode(GetInitials(fullName));
            litModalStudentName.Text = Server.HtmlEncode(fullName);
            litEnrolled.Text = profile["EnrolledCount"] + " course(s)";
            litLastActive.Text = profile["LastLoginDate"] == DBNull.Value
                ? "Never"
                : Convert.ToDateTime(profile["LastLoginDate"]).ToString("dd MMM yyyy");

            // ---- Metric cards ----
            DataRow m = StudentDetailService.GetMetrics(studentId, CurrentLecturerId);
            if (m != null)
            {
                litQuizAvg.Text = Math.Round(Convert.ToDecimal(m["QuizAvg"])) + "%";
                litLabsDone.Text = m["LabsDone"].ToString();
                litLabsTotal.Text = m["LabsTotal"].ToString();

                int labsDone = Convert.ToInt32(m["LabsDone"]);
                litSandbox.Text = labsDone > 0 ? "Cleared ✓" : "Pending";
            }

            // ---- Quiz attempts table ----
            DataTable quizzes = StudentDetailService.GetQuizAttempts(studentId, CurrentLecturerId);
            rptQuizAttempts.DataSource = quizzes;
            rptQuizAttempts.DataBind();
            pnlNoQuiz.Visible = quizzes.Rows.Count == 0;

            // ---- Lab completion list ----
            DataTable labs = StudentDetailService.GetLabStatus(studentId, CurrentLecturerId);
            rptLabs.DataSource = labs;
            rptLabs.DataBind();
            pnlNoLabs.Visible = labs.Rows.Count == 0;
        }

        protected void btnSendFeedback_Click(object sender, EventArgs e)
        {
            string studentId = Request.QueryString["id"];
            string message = tbFeedbackMessage.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                pnlFeedbackError.Visible = true;
                litFeedbackError.Text = "Message cannot be empty.";
                pnlFeedbackSuccess.Visible = false;
            }
            else
            {
                StudentDetailService.SendFeedback(CurrentLecturerId, studentId, message);
                pnlFeedbackSuccess.Visible = true;
                litFeedbackSuccess.Text = "Feedback sent to the student.";
                pnlFeedbackError.Visible = false;
                tbFeedbackMessage.Text = "";
            }

            ClientScript.RegisterStartupScript(GetType(), "reopenFeedbackModal", "showFeedbackModal();", true);
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "S";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return ("" + parts[0][0] + parts[parts.Length - 1][0]).ToUpper();
            return fullName.Length >= 2 ? fullName.Substring(0, 2).ToUpper() : fullName.ToUpper();
        }

        // ---- Repeater display helpers (called from markup) ----
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