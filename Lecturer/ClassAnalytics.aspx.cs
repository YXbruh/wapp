using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;

namespace CSA.Lecturer
{
    public partial class ClassAnalytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack)
            {
                string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
                if (!string.IsNullOrEmpty(instructorId))
                {
                    LoadCourseDropdown(instructorId);
                    LoadData(instructorId, null, null);
                }
            }
            else
            {
                litTotal.Text = "0";
                litAvgQuiz.Text = "0";

            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;
        }

        private void LoadCourseDropdown(string instructorId)
        {
            //string userId = Session["UserID"].ToString();                                              //Bypass login for testing
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new System.Web.UI.WebControls.ListItem("All Courses", ""));
            // TODO: foreach (var c in CourseService.GetByInstructor(userId))
            //           ddlCourse.Items.Add(new ListItem(c.CourseName, c.CourseID.ToString()));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ddlCourse.Items.Add(new ListItem(
                        reader["CourseName"].ToString(),
                        reader["CourseID"].ToString()
                    ));
                }
            }
        }

        private void LoadData(string instructorId, string courseId, string searchTerm)
        {
            if (string.IsNullOrEmpty(instructorId)) courseId = null;
            if (string.IsNullOrEmpty(courseId)) courseId = null;
            if (string.IsNullOrEmpty(searchTerm)) searchTerm = "";

            string connString = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

            // Query 2
            string studentsQuery = @"
                SELECT COUNT(DISTINCT e.StudentID) 
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

            // Query 4
            string quizAvgQuery = @"
                SELECT ISNULL(AVG(qa.Score), 0) AS AvgQuizScore
                FROM QuizAttempts qa
                INNER JOIN Enrollments e ON qa.StudentID = e.StudentID
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

            // Query 3
            string labRateQuery = @"
                SELECT ISNULL(AVG(e.Progress), 0) AS AvgProgress
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

            // Sandbox cleared
            string sandboxQuery = @"
                    SELECT COUNT(ls.SubmissionID)
            FROM LabSubmissions ls
            WHERE ls.Result = 'Passed'
              AND ls.StudentID IN (
                  SELECT DISTINCT e.StudentID
                  FROM Enrollments e
                  INNER JOIN Courses c ON e.CourseID = c.CourseID
                  WHERE c.InstructorID = @InstructorID
                    AND c.IsPublished = 1" +
                    (string.IsNullOrEmpty(courseId) ? "" : " AND c.CourseID = @CourseID") + ")";

            // Students Table Data
            string studentQuery = BuildStudentQuery(courseId);

            // Quiz Breakdown Data
            string quizBreakdownQuery = @"
            SELECT 
                q.Title AS QuizName,
                COUNT(qa.AttemptID) AS AttemptCount,
                ISNULL(AVG(qa.Score), 0) AS AvgScore,
                ISNULL(MAX(qa.Score), 0) AS HighScore,
                ISNULL(MIN(qa.Score), 0) AS LowScore,
                ISNULL(
                    (SUM(CASE WHEN qa.IsPassed = 1 THEN 1 ELSE 0 END) * 100.0) / NULLIF(COUNT(qa.AttemptID), 0), 
                    0
                ) AS PassRate
            FROM Quizzes q
            INNER JOIN Courses c ON q.CourseID = c.CourseID
            LEFT JOIN QuizAttempts qa ON q.QuizID = qa.QuizID
            WHERE c.InstructorID = @InstructorID
              AND c.IsPublished = 1
              AND q.IsPublished = 1" +
                        (string.IsNullOrEmpty(courseId) ? "" : " AND c.CourseID = @CourseID") + @"
            GROUP BY q.QuizID, q.Title
            ORDER BY q.Title";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(studentsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        int total = (int)cmd.ExecuteScalar();
                        litTotal.Text = total.ToString();
                    }
                    catch (Exception)
                    {
                        litTotal.Text = "0";
                    }
                }

                // average score
                using (SqlCommand cmd = new SqlCommand(quizAvgQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        object result = cmd.ExecuteScalar();
                        decimal avgQuizScore = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                        litAvgQuiz.Text = Math.Round(avgQuizScore, 0).ToString() + "%";
                    }
                    catch (Exception)
                    {
                        litAvgQuiz.Text = "0%";
                    }
                }

                // average progress
                using (SqlCommand cmd = new SqlCommand(labRateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        object result = cmd.ExecuteScalar();
                        decimal avgProgress = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                        litLabRate.Text = Math.Round(avgProgress, 0).ToString() + "%";
                    }
                    catch (Exception)
                    {
                        litLabRate.Text = "0%";
                    }
                }

                // sandbox cleared
                using (SqlCommand cmd = new SqlCommand(sandboxQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    if (!string.IsNullOrEmpty(courseId))
                        cmd.Parameters.AddWithValue("@CourseID", courseId);
                    litSandboxCleared.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }

                // students table
                using (SqlCommand cmd = new SqlCommand(studentQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    if (!string.IsNullOrEmpty(courseId))
                        cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");

                    SqlDataReader reader = cmd.ExecuteReader();
                    var students = ReadStudentPerformance(reader);
                    reader.Close();

                    rptStudents.DataSource = students;
                    rptStudents.DataBind();
                    pnlEmpty.Visible = students.Count == 0;
                }

                using (SqlCommand cmd = new SqlCommand(quizBreakdownQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    if (!string.IsNullOrEmpty(courseId))
                        cmd.Parameters.AddWithValue("@CourseID", courseId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    var quizBreakdown = new List<QuizBreakdownViewModel>();
                    while (reader.Read())
                    {
                        quizBreakdown.Add(new QuizBreakdownViewModel
                        {
                            QuizName = reader["QuizName"].ToString(),
                            AttemptCount = Convert.ToInt32(reader["AttemptCount"]),
                            AvgScore = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["AvgScore"]), 0)),
                            HighScore = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["HighScore"]), 0)),
                            LowScore = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["LowScore"]), 0)),
                            PassRate = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["PassRate"]), 0))
                        });
                    }

                    reader.Close();

                    rptQuizBreakdown.DataSource = quizBreakdown;
                    rptQuizBreakdown.DataBind();
                    pnlNoQuiz.Visible = quizBreakdown.Count == 0;
                }
            }
        }

        private string BuildStudentQuery(string courseId)
        {
            return @"
                    WITH StudentCourses AS (
                        SELECT DISTINCT e.StudentID, c.CourseID
                        FROM Enrollments e
                        INNER JOIN Courses c ON e.CourseID = c.CourseID
                        WHERE c.InstructorID = @InstructorID AND c.IsPublished = 1
                          " + (string.IsNullOrEmpty(courseId) ? "" : " AND c.CourseID = @CourseID") + @"
                    ),
                    LabTotals AS (
                        SELECT vl.CourseID, COUNT(*) AS TotalLabs
                        FROM VirtualLabs vl
                        WHERE vl.IsPublished = 1
                        GROUP BY vl.CourseID
                    ),
                    StudentLabs AS (
                        SELECT sc.StudentID,
                               COUNT(DISTINCT vl.LabID) AS TotalLabs,
                               COUNT(DISTINCT CASE WHEN ls.Result = 'Passed' THEN ls.LabID END) AS PassedLabs
                        FROM StudentCourses sc
                        INNER JOIN VirtualLabs vl ON sc.CourseID = vl.CourseID
                        LEFT JOIN LabSubmissions ls ON ls.LabID = vl.LabID AND ls.StudentID = sc.StudentID
                        WHERE vl.IsPublished = 1
                        GROUP BY sc.StudentID
                    ),
                    StudentQuiz AS (
                        SELECT qa.StudentID,
                               ISNULL(AVG(qa.Score), 0) AS QuizAvg
                        FROM QuizAttempts qa
                        INNER JOIN StudentCourses sc ON qa.StudentID = sc.StudentID
                        GROUP BY qa.StudentID
                    )
                    SELECT
                        u.UserID,
                        u.FullName,
                        u.Email,
                        ISNULL(sq.QuizAvg, 0) AS QuizAvg,
                        ISNULL(sl.PassedLabs, 0) AS LabsDone,
                        ISNULL(sl.TotalLabs, 0) AS LabsTotal,
                        0 AS ChallengesDone,      -- not implemented yet
                        0 AS ChallengesTotal,
                        CASE WHEN ISNULL(sl.PassedLabs, 0) > 0 THEN 1 ELSE 0 END AS SandboxCleared,
                        ISNULL(u.LastLoginDate, u.CreatedAt) AS LastActive
                    FROM Users u
                    INNER JOIN StudentCourses sc ON u.UserID = sc.StudentID
                    LEFT JOIN StudentLabs sl ON u.UserID = sl.StudentID
                    LEFT JOIN StudentQuiz sq ON u.UserID = sq.StudentID
                    WHERE u.UserID IN (SELECT StudentID FROM StudentCourses)
                      AND (u.FullName LIKE @Search OR u.Email LIKE @Search)
                    ORDER BY u.FullName";
        }

        private List<StudentPerformanceViewModel> ReadStudentPerformance(SqlDataReader reader)
        {
            var students = new List<StudentPerformanceViewModel>();
            while (reader.Read())
            {
                students.Add(new StudentPerformanceViewModel
                {
                    UserID = reader["UserID"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    QuizAvg = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["QuizAvg"]), 0)),
                    LabsDone = Convert.ToInt32(reader["LabsDone"]),
                    LabsTotal = Convert.ToInt32(reader["LabsTotal"]),
                    ChallengesDone = 0,
                    ChallengesTotal = 0,
                    SandboxCleared = Convert.ToBoolean(reader["SandboxCleared"]),
                    LastActive = Convert.ToDateTime(reader["LastActive"])
                });
            }
            return students;
        }

        private List<StudentPerformanceViewModel> GetStudentPerformance(
            string instructorId, string courseId, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) searchTerm = "";
            string connString = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;
            string studentQuery = BuildStudentQuery(courseId);

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(studentQuery, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                if (!string.IsNullOrEmpty(courseId))
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var students = ReadStudentPerformance(reader);
                reader.Close();
                return students;
            }
        }

        protected void ddlCourse_Changed(object sender, EventArgs e)
        {
            string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
            string courseId = ddlCourse.SelectedValue;
            string search = tbSearch.Text.Trim();
            LoadData(instructorId, courseId, search);
        }
        protected void tbSearch_Changed(object sender, EventArgs e)
        {
            string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
            string courseId = ddlCourse.SelectedValue;
            string search = tbSearch.Text.Trim();
            LoadData(instructorId, courseId, search);
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
            string courseId = ddlCourse.SelectedValue;
            string search = tbSearch.Text.Trim();

            var students = GetStudentPerformance(instructorId, courseId, search);

            var sb = new StringBuilder();
            sb.AppendLine("Student,Email,Quiz Avg (%),Labs Done,Labs Total,Sandbox Cleared,Last Active");
            foreach (var s in students)
            {
                sb.AppendLine(string.Join(",",
                    CsvField(s.FullName),
                    CsvField(s.Email),
                    s.QuizAvg.ToString(),
                    s.LabsDone.ToString(),
                    s.LabsTotal.ToString(),
                    s.SandboxCleared ? "Yes" : "No",
                    CsvField(s.LastActive.ToString("yyyy-MM-dd HH:mm"))));
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition",
                $"attachment; filename=ClassAnalytics_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            Response.Write(sb.ToString());
            Response.End();
        }

        private static string CsvField(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }

        public class StudentPerformanceViewModel
        {
            public string UserID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public int QuizAvg { get; set; }
            public int LabsDone { get; set; }
            public int LabsTotal { get; set; }
            public int ChallengesDone { get; set; }
            public int ChallengesTotal { get; set; }
            public bool SandboxCleared { get; set; }
            public DateTime LastActive { get; set; }

            // For display
            public string LastActiveDisplay => GetRelativeTime(LastActive);

            private string GetRelativeTime(DateTime date)
            {
                var diff = DateTime.Now - date;
                if (diff.TotalMinutes < 1) return "Just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hr ago";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
                return date.ToString("MMM dd, yyyy");
            }
        }

        public class QuizBreakdownViewModel
        {
            public string QuizName { get; set; }
            public int AttemptCount { get; set; }
            public int AvgScore { get; set; }
            public int HighScore { get; set; }
            public int LowScore { get; set; }
            public int PassRate { get; set; }
        }
    }
}