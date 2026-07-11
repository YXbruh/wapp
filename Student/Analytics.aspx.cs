using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Analytics : Page
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
                LoadAnalytics();
            }
        }

        private void LoadAnalytics()
        {
            string userId =
                Convert.ToString(Session["UserID"]);

            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                litOverall.Text = ReadAverage(
                    con,
                    @"SELECT AVG(Progress)
                      FROM Enrollments
                      WHERE StudentID = @UserID",
                    userId,
                    "0%"
                );

                litAvgQuiz.Text = ReadAverage(
                    con,
                    @"SELECT AVG(Score)
                      FROM QuizAttempts
                      WHERE StudentID = @UserID",
                    userId,
                    "—"
                );

                int labsDone = ExecuteInt(
                    con,
                    @"SELECT COUNT(DISTINCT LabID)
                      FROM LabSubmissions
                      WHERE StudentID = @UserID
                        AND Result = 'Passed'",
                    userId
                );

                int labsTotal = ExecuteInt(
                    con,
                    @"SELECT COUNT(*)
                      FROM VirtualLabs vl
                      INNER JOIN Enrollments e
                          ON e.CourseID = vl.CourseID
                      WHERE e.StudentID = @UserID
                        AND vl.IsPublished = 1",
                    userId
                );

                litLabsDone.Text =
                    labsDone.ToString();

                litLabsTotal.Text =
                    labsTotal.ToString();

                int studyMinutes = ExecuteInt(
                    con,
                    @"SELECT ISNULL(
                          SUM(
                              ISNULL(
                                  vl.TimeLimitMinutes,
                                  15
                              )
                          ),
                          0
                      )
                      FROM VirtualLabs vl
                      WHERE EXISTS
                      (
                          SELECT 1
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @UserID
                            AND ls.Result = 'Passed'
                      )",
                    userId
                );

                litStudyTime.Text =
                    Math.Round(
                        studyMinutes / 60.0,
                        1
                    ) + "h";

                DataTable dtCourses = FillTable(
                    con,
                    @"SELECT
                          c.CourseName,
                          e.Progress,
                          e.Status,

                          (
                              SELECT COUNT(*)
                              FROM ChapterProgress cp
                              INNER JOIN Chapters ch
                                  ON ch.ChapterID =
                                     cp.ChapterID
                              WHERE ch.CourseID =
                                    c.CourseID
                                AND cp.StudentID =
                                    @UserID
                                AND cp.IsCompleted = 1
                          ) AS CompletedModules,

                          (
                              SELECT COUNT(*)
                              FROM Chapters ch2
                              WHERE ch2.CourseID =
                                    c.CourseID
                                AND ch2.IsPublished = 1
                          ) AS TotalModules

                      FROM Enrollments e
                      INNER JOIN Courses c
                          ON c.CourseID = e.CourseID
                      WHERE e.StudentID = @UserID
                      ORDER BY e.EnrolledAt DESC",
                    userId
                );

                dtCourses.Columns.Add(
                    "StatusBadgeClass",
                    typeof(string)
                );

                dtCourses.Columns.Add(
                    "StatusLabel",
                    typeof(string)
                );

                foreach (DataRow row in dtCourses.Rows)
                {
                    string status =
                        Convert.ToString(
                            row["Status"]);

                    row["StatusLabel"] = status;

                    if (status == "Completed")
                    {
                        row["StatusBadgeClass"] =
                            "badge-green";
                    }
                    else if (status == "In Progress")
                    {
                        row["StatusBadgeClass"] =
                            "badge-blue";
                    }
                    else
                    {
                        row["StatusBadgeClass"] =
                            "badge-amber";
                    }
                }

                rptCourseProgress.DataSource =
                    dtCourses;

                rptCourseProgress.DataBind();

                pnlNoCourses.Visible =
                    dtCourses.Rows.Count == 0;

                DataTable dtQuiz = FillTable(
                    con,
                    @"SELECT
                          q.Title AS QuizName,
                          qa.Score,
                          qa.AttemptedAt
                      FROM QuizAttempts qa
                      INNER JOIN Quizzes q
                          ON q.QuizID = qa.QuizID
                      WHERE qa.StudentID = @UserID
                      ORDER BY qa.AttemptedAt DESC",
                    userId
                );

                dtQuiz.Columns.Add(
                    "AttemptedOn",
                    typeof(string)
                );

                foreach (DataRow row in dtQuiz.Rows)
                {
                    row["AttemptedOn"] =
                        Convert.ToDateTime(
                            row["AttemptedAt"]
                        ).ToString("dd MMM yyyy");
                }

                rptQuizScores.DataSource = dtQuiz;
                rptQuizScores.DataBind();

                pnlNoQuizzes.Visible =
                    dtQuiz.Rows.Count == 0;

                DataTable dtLabs = FillTable(
                    con,
                    @"SELECT
                          c.CourseName,

                          (
                              SELECT COUNT(
                                  DISTINCT ls.LabID
                              )
                              FROM LabSubmissions ls
                              INNER JOIN VirtualLabs vl2
                                  ON vl2.LabID =
                                     ls.LabID
                              WHERE vl2.CourseID =
                                    c.CourseID
                                AND ls.StudentID =
                                    @UserID
                                AND ls.Result =
                                    'Passed'
                          ) AS LabsDone,

                          (
                              SELECT COUNT(*)
                              FROM VirtualLabs vl3
                              WHERE vl3.CourseID =
                                    c.CourseID
                                AND vl3.IsPublished = 1
                          ) AS LabsTotal

                      FROM Enrollments e
                      INNER JOIN Courses c
                          ON c.CourseID = e.CourseID
                      WHERE e.StudentID = @UserID
                      ORDER BY c.CourseName",
                    userId
                );

                dtLabs.Columns.Add(
                    "LabProgressPct",
                    typeof(int)
                );

                foreach (DataRow row in dtLabs.Rows)
                {
                    int done =
                        Convert.ToInt32(
                            row["LabsDone"]);

                    int total =
                        Convert.ToInt32(
                            row["LabsTotal"]);

                    if (total > 0)
                    {
                        row["LabProgressPct"] =
                            done * 100 / total;
                    }
                    else
                    {
                        row["LabProgressPct"] = 0;
                    }
                }

                rptLabProgress.DataSource = dtLabs;
                rptLabProgress.DataBind();

                pnlNoLabs.Visible =
                    dtLabs.Rows.Count == 0;
            }
        }

        private static string ReadAverage(
            SqlConnection con,
            string sql,
            string userId,
            string emptyText)
        {
            using (SqlCommand cmd =
                   CreateUserCommand(
                       con,
                       sql,
                       userId))
            {
                object result = cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return emptyText;
                }

                return Math.Round(
                    Convert.ToDecimal(result)
                ) + "%";
            }
        }

        private static int ExecuteInt(
            SqlConnection con,
            string sql,
            string userId)
        {
            using (SqlCommand cmd =
                   CreateUserCommand(
                       con,
                       sql,
                       userId))
            {
                return Convert.ToInt32(
                    cmd.ExecuteScalar()
                );
            }
        }

        private static DataTable FillTable(
            SqlConnection con,
            string sql,
            string userId)
        {
            using (SqlCommand cmd =
                   CreateUserCommand(
                       con,
                       sql,
                       userId))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private static SqlCommand CreateUserCommand(
            SqlConnection con,
            string sql,
            string userId)
        {
            SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add(
                "@UserID",
                SqlDbType.NVarChar,
                10
            ).Value = userId;

            return cmd;
        }

        public string GetScoreColor(int score)
        {
            if (score >= 80)
            {
                return "var(--success)";
            }

            if (score >= 60)
            {
                return "var(--warning)";
            }

            return "var(--danger)";
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