using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Auth guard                                                       //remove to bypass login for testing
            //if (Session["UserID"] == null)
            //{
            //    Response.Redirect("~/Login.aspx");
            //    return;
            //}

            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up
            litName.Text = Session["FullName"] as string ?? "Student";

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            SqlCommand cmd1 = new SqlCommand("select count(*) from Enrollments where StudentID = " + userId, con);
            int courseCount = Convert.ToInt32(cmd1.ExecuteScalar());

            SqlCommand cmd2 = new SqlCommand("select count(distinct LabID) from LabSubmissions where StudentID = " + userId + " and Result = 'Passed'", con);
            int labsDone = Convert.ToInt32(cmd2.ExecuteScalar());

            SqlCommand cmd3 = new SqlCommand("select count(*) from UserAchievements where UserID = " + userId, con);
            int badgeCount = Convert.ToInt32(cmd3.ExecuteScalar());

            SqlCommand cmd4 = new SqlCommand("select avg(Score) from QuizAttempts where StudentID = " + userId, con);
            object avgObj = cmd4.ExecuteScalar();
            string avgQuizText = (avgObj == DBNull.Value) ? "—" : Math.Round(Convert.ToDouble(avgObj)) + "%";

            // "Challenges" reuses VirtualLabs (see Challenges.aspx.cs) — count of published
            // labs in enrolled courses the student has not passed yet.
            SqlCommand cmd5 = new SqlCommand("select count(*) from VirtualLabs vl join Enrollments e on vl.CourseID = e.CourseID "
                                            + "where e.StudentID = " + userId + " and vl.IsPublished = 1 "
                                            + "and vl.LabID not in (select LabID from LabSubmissions where StudentID = " + userId + " and Result = 'Passed')", con);
            int openChallenges = Convert.ToInt32(cmd5.ExecuteScalar());

            litMetricCourses.Text = courseCount.ToString();
            litMetricLabs.Text = labsDone.ToString();
            litMetricQuiz.Text = avgQuizText;
            litMetricBadges.Text = badgeCount.ToString();
            litCourseCount.Text = courseCount.ToString();
            litChallengeCount.Text = openChallenges.ToString();
            litSubtitle.Text = "Check your courses and activity below.";

            // ---- Active courses (top 3, not completed) ----
            string courseQuery = "select top 3 c.CourseName, e.Progress from Enrollments e "
                                + "join Courses c on e.CourseID = c.CourseID "
                                + "where e.StudentID = " + userId + " and e.Status <> 'Completed' "
                                + "order by e.EnrolledAt desc";

            SqlDataAdapter daCourses = new SqlDataAdapter(courseQuery, con);
            DataTable dtCourses = new DataTable();
            daCourses.Fill(dtCourses);

            rptCourses.DataSource = dtCourses;
            rptCourses.DataBind();
            pnlNoCourses.Visible = (dtCourses.Rows.Count == 0);

            // ---- Recent activity (last 5) ----
            string activityQuery = "select top 5 Description, CreatedAt from ActivityLog "
                                  + "where UserID = " + userId + " order by CreatedAt desc";

            SqlDataAdapter daActivity = new SqlDataAdapter(activityQuery, con);
            DataTable dtActivity = new DataTable();
            daActivity.Fill(dtActivity);

            dtActivity.Columns.Add("TimeAgo", typeof(string));
            foreach (DataRow row in dtActivity.Rows)
            {
                DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);
                row["TimeAgo"] = createdAt.ToString("dd MMM yyyy, hh:mm tt");
            }

            rptActivity.DataSource = dtActivity;
            rptActivity.DataBind();
            pnlNoActivity.Visible = (dtActivity.Rows.Count == 0);

            con.Close();
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}