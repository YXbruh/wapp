using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Dashboard : Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;
            }
        }

        private string CurrentUserId
        {
            get
            {
                return Convert.ToString(
                    Session["UserID"]);
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            string userId = CurrentUserId;

            string fullName =
                Convert.ToString(
                    Session["FullName"]);

            litName.Text =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Student"
                    : Server.HtmlEncode(fullName);

            litSubtitle.Text =
                "Continue learning and review your latest progress.";

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                litMetricCourses.Text =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM Enrollments
                          WHERE StudentID = @UserID",
                        userId
                    ).ToString();

                litMetricLabs.Text =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(DISTINCT LabID)
                          FROM LabSubmissions
                          WHERE StudentID = @UserID
                            AND Result = 'Passed'",
                        userId
                    ).ToString();

                litMetricBadges.Text =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM UserAchievements
                          WHERE UserID = @UserID",
                        userId
                    ).ToString();

                litMetricQuiz.Text =
                    GetAverageQuizScore(
                        con,
                        userId);

                LoadCourses(
                    con,
                    userId);

                LoadRecentActivity(
                    con,
                    userId);
            }
        }

        private string GetAverageQuizScore(
            SqlConnection con,
            string userId)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT AVG(Score)
                  FROM QuizAttempts
                  WHERE StudentID = @UserID",
                con))
            {
                AddUserId(
                    cmd,
                    userId);

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return "—";
                }

                return Math.Round(
                    Convert.ToDecimal(result)
                ).ToString("0") + "%";
            }
        }

        private void LoadCourses(
            SqlConnection con,
            string userId)
        {
            DataTable dt =
                new DataTable();

            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT TOP 3
                      c.CourseName,
                      e.Progress,
                      e.Status,
                      e.EnrolledAt
                  FROM Enrollments e
                  INNER JOIN Courses c
                      ON c.CourseID = e.CourseID
                  WHERE e.StudentID = @UserID
                  ORDER BY
                      CASE
                          WHEN e.Status = 'In Progress'
                              THEN 1
                          WHEN e.Status = 'Not Started'
                              THEN 2
                          ELSE 3
                      END,
                      e.EnrolledAt DESC",
                con))
            {
                AddUserId(
                    cmd,
                    userId);

                using (SqlDataAdapter da =
                       new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            rptCourses.DataSource = dt;
            rptCourses.DataBind();

            pnlNoCourses.Visible =
                dt.Rows.Count == 0;
        }

        private void LoadRecentActivity(
            SqlConnection con,
            string userId)
        {
            DataTable dt =
                new DataTable();

            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT TOP 5
                      Description,
                      CreatedAt
                  FROM
                  (
                      SELECT
                          Description,
                          CreatedAt
                      FROM ActivityLog
                      WHERE UserID = @UserID

                      UNION ALL

                      SELECT
                          'Enrolled in course: '
                              + c.CourseName,
                          e.EnrolledAt
                      FROM Enrollments e
                      INNER JOIN Courses c
                          ON c.CourseID = e.CourseID
                      WHERE e.StudentID = @UserID

                      UNION ALL

                      SELECT
                          CASE
                              WHEN ls.Result = 'Passed'
                                  THEN 'Completed lab: '
                                       + vl.LabTitle
                              ELSE 'Attempted lab: '
                                   + vl.LabTitle
                          END,
                          ls.SubmittedAt
                      FROM LabSubmissions ls
                      INNER JOIN VirtualLabs vl
                          ON vl.LabID = ls.LabID
                      WHERE ls.StudentID = @UserID

                      UNION ALL

                      SELECT
                          CASE
                              WHEN qa.IsPassed = 1
                                  THEN 'Passed quiz: '
                                       + q.Title
                              ELSE 'Attempted quiz: '
                                   + q.Title
                          END,
                          qa.AttemptedAt
                      FROM QuizAttempts qa
                      INNER JOIN Quizzes q
                          ON q.QuizID = qa.QuizID
                      WHERE qa.StudentID = @UserID
                  ) activities
                  ORDER BY CreatedAt DESC",
                con))
            {
                AddUserId(
                    cmd,
                    userId);

                using (SqlDataAdapter da =
                       new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            dt.Columns.Add(
                "TimeAgo",
                typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                DateTime createdAt =
                    Convert.ToDateTime(
                        row["CreatedAt"]);

                row["TimeAgo"] =
                    FormatTime(createdAt);
            }

            rptActivity.DataSource = dt;
            rptActivity.DataBind();

            pnlNoActivity.Visible =
                dt.Rows.Count == 0;
        }

        private static string FormatTime(
            DateTime date)
        {
            TimeSpan difference =
                DateTime.Now - date;

            if (difference.TotalMinutes < 1)
            {
                return "Just now";
            }

            if (difference.TotalMinutes < 60)
            {
                return
                    Convert.ToInt32(
                        difference.TotalMinutes
                    ) + " minute(s) ago";
            }

            if (difference.TotalHours < 24)
            {
                return
                    Convert.ToInt32(
                        difference.TotalHours
                    ) + " hour(s) ago";
            }

            if (difference.TotalDays < 7)
            {
                return
                    Convert.ToInt32(
                        difference.TotalDays
                    ) + " day(s) ago";
            }

            return date.ToString(
                "dd MMM yyyy, hh:mm tt");
        }

        private static int ExecuteCount(
            SqlConnection con,
            string sql,
            string userId)
        {
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
            {
                AddUserId(
                    cmd,
                    userId);

                object result =
                    cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
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