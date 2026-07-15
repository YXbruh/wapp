using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;

namespace CSA.Lecturer
{
    public partial class Lecturer_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
                if (!string.IsNullOrEmpty(instructorId))
                {
                    LoadDashboard(instructorId);
                }
                else
                {
                    litModules.Text = "0";
                    litStudents.Text = "0";
                    litLabRate.Text = "0%";
                    litQuizAvg.Text = "0%";
                    pnlNoLabs.Visible = true;
                    pnlNoQuiz.Visible = true;
                    pnlNoModules.Visible = true;
                }
            }
        }

        private void LoadDashboard(string instructorId)
        {
            // Set user info
            litName.Text = Session["FullName"] as string ?? "Lecturer";
            litDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            string connString = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

            // Query 1
            string modulesQuery = "SELECT COUNT(*) FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1";

            // Query 2
            string studentsQuery = @"
                SELECT COUNT(DISTINCT e.StudentID) 
                FROM Enrollments e
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

            // Query 4
            string quizAvgQuery = @"
                SELECT ISNULL(AVG(qa.Score), 0) AS AvgQuizScore
                FROM QuizAttempts qa
                INNER JOIN Enrollments e ON qa.StudentID = e.StudentID
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

            // Query 5
            string labRatesQuery = @"
                SELECT 
                    c.CourseID,
                    c.CourseName,
                    COUNT(DISTINCT e.StudentID) AS TotalStudents,
                    COUNT(DISTINCT CASE 
                        WHEN e.Status = 'Completed' THEN e.StudentID 
                        ELSE NULL 
                    END) AS CompletedCount,
                    CASE 
                        WHEN COUNT(DISTINCT e.StudentID) > 0 THEN 
                            (COUNT(DISTINCT CASE 
                                WHEN e.Status = 'Completed' THEN e.StudentID 
                                ELSE NULL 
                            END) * 100.0) / COUNT(DISTINCT e.StudentID)
                        ELSE 0
                    END AS CompletionPct
                FROM Courses c
                INNER JOIN Enrollments e ON c.CourseID = e.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                GROUP BY c.CourseID, c.CourseName
                ORDER BY CompletionPct DESC";

            // Query 6
            string quizScoresQuery = @"
                SELECT 
                    q.QuizID,
                    q.Title AS QuizName,
                    c.CourseName,
                    COUNT(DISTINCT qa.AttemptID) AS AttemptCount,
                    ISNULL(AVG(qa.Score), 0) AS AvgScore
                FROM Quizzes q
                INNER JOIN Courses c ON q.CourseID = c.CourseID
                LEFT JOIN QuizAttempts qa ON q.QuizID = qa.QuizID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                  AND q.IsPublished = 1
                GROUP BY q.QuizID, q.Title, c.CourseName
                ORDER BY AvgScore DESC";

            // Query 7
            string modulesListQuery = @"
                -- Chapters
                SELECT 
                    ch.ChapterTitle AS Title,
                    '' AS Subtitle,
                    c.CourseName,
                    'Chapter' AS ContentType,
                    ch.IsPublished,
                    (SELECT COUNT(DISTINCT cp.StudentID) 
                     FROM ChapterProgress cp 
                     WHERE cp.ChapterID = ch.ChapterID) AS ViewCount,
                    ch.UpdatedAt,
                    ch.ChapterID AS ContentID
                FROM Chapters ch
                INNER JOIN Courses c ON ch.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                
                UNION ALL
                
                -- Labs
                SELECT 
                    vl.LabTitle AS Title,
                    '' AS Subtitle,
                    c.CourseName,
                    'Lab' AS ContentType,
                    vl.IsPublished,
                    (SELECT COUNT(DISTINCT ls.StudentID) 
                     FROM LabSubmissions ls 
                     WHERE ls.LabID = vl.LabID) AS ViewCount,
                    vl.UpdatedAt,
                    vl.LabID AS ContentID
                FROM VirtualLabs vl
                INNER JOIN Courses c ON vl.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                
                UNION ALL
                
                -- Quizzes
                SELECT 
                    q.Title AS Title,
                    '' AS Subtitle,
                    c.CourseName,
                    'Quiz' AS ContentType,
                    q.IsPublished,
                    (SELECT COUNT(DISTINCT qa.StudentID) 
                     FROM QuizAttempts qa 
                     WHERE qa.QuizID = q.QuizID) AS ViewCount,
                    q.UpdatedAt,
                    q.QuizID AS ContentID
                FROM Quizzes q
                INNER JOIN Courses c ON q.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                
                ORDER BY UpdatedAt DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // active modules count
                using (SqlCommand cmd = new SqlCommand(modulesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        int activeModulesCount = (int)cmd.ExecuteScalar();
                        litModules.Text = activeModulesCount.ToString();
                    }
                    catch (Exception ex)
                    {
                        litModules.Text = "0";
                    }
                }

                // enrolled count
                using (SqlCommand cmd = new SqlCommand(studentsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        int studentsCount = (int)cmd.ExecuteScalar();
                        litStudents.Text = studentsCount.ToString();
                    }
                    catch (Exception ex)
                    {
                        litStudents.Text = "0";
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
                    catch (Exception ex)
                    {
                        litLabRate.Text = "0%";
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
                        litQuizAvg.Text = Math.Round(avgQuizScore, 0).ToString() + "%";
                    }
                    catch (Exception ex)
                    {
                        litQuizAvg.Text = "0%";
                    }
                }

                //lab completion rates
                using (SqlCommand cmd = new SqlCommand(labRatesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        var labRates = new List<LabRateViewModel>();

                        while (reader.Read())
                        {
                            labRates.Add(new LabRateViewModel
                            {
                                CourseName = reader["CourseName"].ToString(),
                                TotalStudents = Convert.ToInt32(reader["TotalStudents"]),
                                CompletedCount = Convert.ToInt32(reader["CompletedCount"]),
                                CompletionPct = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["CompletionPct"]), 0))
                            });
                        }
                        reader.Close();

                        rptLabRates.DataSource = labRates;
                        rptLabRates.DataBind();

                        pnlNoLabs.Visible = labRates.Count == 0;
                    }
                    catch (Exception ex)
                    {
                        pnlNoLabs.Visible = true;
                        rptLabRates.DataSource = null;
                        rptLabRates.DataBind();
                    }
                }

                // quiz scores per quiz
                using (SqlCommand cmd = new SqlCommand(quizScoresQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        var quizScores = new List<QuizScoreViewModel>();

                        while (reader.Read())
                        {
                            quizScores.Add(new QuizScoreViewModel
                            {
                                QuizName = reader["QuizName"].ToString(),
                                CourseName = reader["CourseName"].ToString(),
                                AttemptCount = Convert.ToInt32(reader["AttemptCount"]),
                                AvgScore = Convert.ToInt32(Math.Round(Convert.ToDecimal(reader["AvgScore"]), 0))
                            });
                        }
                        reader.Close();

                        rptQuizScores.DataSource = quizScores;
                        rptQuizScores.DataBind();

                        pnlNoQuiz.Visible = quizScores.Count == 0;
                    }
                    catch (Exception ex)
                    {
                        pnlNoQuiz.Visible = true;
                        rptQuizScores.DataSource = null;
                        rptQuizScores.DataBind();
                    }
                }

                // modules list
                using (SqlCommand cmd = new SqlCommand(modulesListQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        var modules = new List<ModuleViewModel>();

                        while (reader.Read())
                        {
                            modules.Add(new ModuleViewModel
                            {
                                Title = reader["Title"].ToString(),
                                Subtitle = reader["Subtitle"].ToString(),
                                CourseName = reader["CourseName"].ToString(),
                                ContentType = reader["ContentType"].ToString(),
                                IsPublished = Convert.ToBoolean(reader["IsPublished"]),
                                ViewCount = Convert.ToInt32(reader["ViewCount"]),
                                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                                ContentID = reader["ContentID"].ToString()
                            });
                        }
                        reader.Close();

                        rptModules.DataSource = modules;
                        rptModules.DataBind();
                        pnlNoModules.Visible = modules.Count == 0;
                    }
                    catch (Exception ex)
                    {
                        pnlNoModules.Visible = true;
                        rptModules.DataSource = null;
                        rptModules.DataBind();
                    }
                }
            }
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        public string GetTypeBadge(string t) =>
            t == "Chapter" ? "badge-blue" : t == "Lab" ? "badge-green" : t == "Quiz" ? "badge-amber" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx?msg=loggedout");
        }
    }

    // lab rates
    public class LabRateViewModel
    {
        public string CourseName { get; set; }
        public int TotalStudents { get; set; }
        public int CompletedCount { get; set; }
        public int CompletionPct { get; set; }
    }

    // quiz scores
    public class QuizScoreViewModel
    {
        public string QuizName { get; set; }
        public string CourseName { get; set; }
        public int AttemptCount { get; set; }
        public int AvgScore { get; set; }
    }

    // modules list
    public class ModuleViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string CourseName { get; set; }
        public string ContentType { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ContentID { get; set; }

        // property for display
        public string UpdatedDisplay => GetRelativeTime(UpdatedAt);

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
}