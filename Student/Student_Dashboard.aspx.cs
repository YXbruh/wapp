using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        private string CurrentUserId
        {
            get { return Convert.ToString(Session["UserID"]); }
        }

        private void LoadDashboard()
        {
            string userId = CurrentUserId;

            litName.Text =
                Convert.ToString(Session["FullName"]) ?? "Student";

            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                int courseCount = ExecuteCount(
                    con,
                    @"SELECT COUNT(*)
                      FROM Enrollments
                      WHERE StudentID = @UserID",
                    userId
                );

                int labsDone = ExecuteCount(
                    con,
                    @"SELECT COUNT(DISTINCT LabID)
                      FROM LabSubmissions
                      WHERE StudentID = @UserID
                        AND Result = 'Passed'",
                    userId
                );

                int badgeCount = ExecuteCount(
                    con,
                    @"SELECT COUNT(*)
                      FROM UserAchievements
                      WHERE UserID = @UserID",
                    userId
                );

                string avgQuizText;

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT AVG(Score)
                      FROM QuizAttempts
                      WHERE StudentID = @UserID", con))
                {
                    AddUserId(cmd, userId);

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        avgQuizText = "—";
                    }
                    else
                    {
                        avgQuizText =
                            Math.Round(Convert.ToDecimal(result)) + "%";
                    }
                }

                int openChallenges = ExecuteCount(
                    con,
                    @"SELECT COUNT(*)
                      FROM VirtualLabs vl
                      INNER JOIN Enrollments e
                          ON e.CourseID = vl.CourseID
                      WHERE e.StudentID = @UserID
                        AND vl.IsPublished = 1
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM LabSubmissions ls
                            WHERE ls.LabID = vl.LabID
                              AND ls.StudentID = @UserID
                              AND ls.Result = 'Passed'
                        )",
                    userId
                );

                litMetricCourses.Text = courseCount.ToString();
                litMetricLabs.Text = labsDone.ToString();
                litMetricQuiz.Text = avgQuizText;
                litMetricBadges.Text = badgeCount.ToString();
                litCourseCount.Text = courseCount.ToString();
                litChallengeCount.Text = openChallenges.ToString();

                litSubtitle.Text =
                    "Check your courses and activity below.";

                DataTable dtCourses = new DataTable();

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 3
                          c.CourseName,
                          e.Progress
                      FROM Enrollments e
                      INNER JOIN Courses c
                          ON c.CourseID = e.CourseID
                      WHERE e.StudentID = @UserID
                        AND e.Status <> 'Completed'
                      ORDER BY e.EnrolledAt DESC", con))
                {
                    AddUserId(cmd, userId);

                    using (SqlDataAdapter da =
                           new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtCourses);
                    }
                }

                rptCourses.DataSource = dtCourses;
                rptCourses.DataBind();

                pnlNoCourses.Visible =
                    dtCourses.Rows.Count == 0;

                DataTable dtActivity = new DataTable();

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 5
                          Description,
                          CreatedAt
                      FROM ActivityLog
                      WHERE UserID = @UserID
                      ORDER BY CreatedAt DESC", con))
                {
                    AddUserId(cmd, userId);

                    using (SqlDataAdapter da =
                           new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtActivity);
                    }
                }

                dtActivity.Columns.Add(
                    "TimeAgo",
                    typeof(string)
                );

                foreach (DataRow row in dtActivity.Rows)
                {
                    DateTime createdAt =
                        Convert.ToDateTime(row["CreatedAt"]);

                    row["TimeAgo"] =
                        createdAt.ToString(
                            "dd MMM yyyy, hh:mm tt"
                        );
                }

                rptActivity.DataSource = dtActivity;
                rptActivity.DataBind();

                pnlNoActivity.Visible =
                    dtActivity.Rows.Count == 0;
            }
        }

        private static int ExecuteCount(
            SqlConnection con,
            string sql,
            string userId)
        {
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
            {
                AddUserId(cmd, userId);

                return Convert.ToInt32(
                    cmd.ExecuteScalar()
                );
            }
        }

        private static void AddUserId(
            SqlCommand cmd,
            string userId)
        {
            cmd.Parameters.Add(
                "@UserID",
                SqlDbType.NVarChar,
                10
            ).Value = userId;
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("~/Login.aspx");
        }
    }
}