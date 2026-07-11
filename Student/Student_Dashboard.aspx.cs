using System;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Student")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack)
                LoadDashboard();
        }

        private void LoadDashboard()
        {
            //string userId = Session["UserID"].ToString();                              //remove to bypass login for testing
            litName.Text = Session["FullName"] as string ?? "Student";

            // TODO: replace with real DB calls
            // var stats = DashboardService.GetStudentStats(userId);
            // litMetricCourses.Text = stats.CourseCount.ToString();
            // litMetricLabs.Text    = stats.LabsCompleted.ToString();
            // litMetricQuiz.Text    = stats.AvgQuizScore + "%";
            // litMetricBadges.Text  = stats.BadgeCount.ToString();
            // litCourseCount.Text   = stats.CourseCount.ToString();

            // TODO: bind repeaters
            // rptCourses.DataSource  = DashboardService.GetActiveCourses(userId);
            // rptCourses.DataBind();
            // rptActivity.DataSource = DashboardService.GetRecentActivity(userId, 5);
            // rptActivity.DataBind();

            litSubtitle.Text = "Check your courses and activity below.";
            pnlNoCourses.Visible = true;  // flip to false when data exists
            pnlNoActivity.Visible = true;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout");
        }
    }
}