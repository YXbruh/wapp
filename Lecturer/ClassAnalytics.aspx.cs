using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class ClassAnalytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) { LoadCourseDropdown(); LoadData(); }
        }

        private void LoadCourseDropdown()
        {
            //string userId = Session["UserID"].ToString();                                              //Bypass login for testing
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new System.Web.UI.WebControls.ListItem("All Courses", ""));
            // TODO: foreach (var c in CourseService.GetByInstructor(userId))
            //           ddlCourse.Items.Add(new ListItem(c.CourseName, c.CourseID.ToString()));
        }

        private void LoadData()
        {
            //string userId = Session["UserID"].ToString();                                              //Bypass login for testing
            int? courseId = string.IsNullOrEmpty(ddlCourse.SelectedValue)
                            ? (int?)null
                            : Convert.ToInt32(ddlCourse.SelectedValue);
            // TODO:
            // var stats = AnalyticsService.GetClassStats(userId, courseId, tbSearch.Text.Trim());
            // litTotal.Text          = stats.TotalStudents.ToString();
            // litAvgQuiz.Text        = stats.AvgQuizScore + "%";
            // litLabRate.Text        = stats.LabCompletionRate + "%";
            // litSandboxCleared.Text = stats.SandboxClearedCount.ToString();
            // rptStudents.DataSource      = stats.Students;      rptStudents.DataBind();
            // rptQuizBreakdown.DataSource = stats.QuizBreakdown; rptQuizBreakdown.DataBind();
            // pnlEmpty.Visible  = stats.Students.Count == 0;
            // pnlNoQuiz.Visible = stats.QuizBreakdown.Count == 0;
            pnlEmpty.Visible = pnlNoQuiz.Visible = true;
        }

        protected void ddlCourse_Changed(object sender, EventArgs e) => LoadData();
        protected void tbSearch_Changed(object sender, EventArgs e) => LoadData();

        protected void lbExport_Click(object sender, EventArgs e)
        {
            // TODO: CSV export
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }

}