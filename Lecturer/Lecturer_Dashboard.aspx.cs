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
            { Response.Redirect("~/Login.aspx"); return; }
            
            if (!IsPostBack)
            {
                string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
                if (!string.IsNullOrEmpty(instructorId))
                {
                    LoadDashboard(instructorId);
                }
                else
                {
                    litModules.Text = "0"; // Handle empty session fallback safely
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
            string userId = Session["UserID"].ToString(); //bypass Login
            litName.Text = Session["FullName"] as string ?? "Lecturer";
            litDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");


            string connString = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

            string modulesQuery = "SELECT COUNT(*) FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1";

            string studentsQuery = @"
                SELECT COUNT(DISTINCT e.StudentID) 
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1
                  AND e.Status != 'Not Started'";

            string labRateQuery = @"
                SELECT ISNULL(AVG(e.Progress), 0) AS AvgProgress
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

            string quizAvgQuery = @"
                SELECT ISNULL(AVG(qa.Score), 0) AS AvgQuizScore
                FROM QuizAttempts qa
                INNER JOIN Enrollments e ON qa.StudentID = e.StudentID
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID 
                  AND c.IsPublished = 1";

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

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(modulesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        int activeModulesCount = (int)cmd.ExecuteScalar();
                        litModules.Text = activeModulesCount.ToString();
                    }
                    catch (Exception ex) { litModules.Text = "0"; }
                }

                // litStudent count
                using (SqlCommand cmd = new SqlCommand(studentsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        int studentsCount = (int)cmd.ExecuteScalar();
                        litStudents.Text = studentsCount.ToString();
                    }
                    catch (Exception ex) { litStudents.Text = "0"; }
                }

                //Lab completion rate
                using (SqlCommand cmd = new SqlCommand(labRateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        object result = cmd.ExecuteScalar();
                        decimal labRate = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                        litLabRate.Text = Math.Round(labRate, 0).ToString() + "%";
                    }
                    catch (Exception ex)
                    {
                        litLabRate.Text = "0";
                    }
                }

                //Get avr score
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

                //Lab Completion Terminal
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
            }

            

            // TODO:
            // var stats = LecturerService.GetDashboardStats(userId);
            
            
            // litLabRate.Text  = stats.AvgLabCompletion + "%";
            // litQuizAvg.Text  = stats.AvgQuizScore + "%";
            // rptLabRates.DataSource   = stats.LabRates;   rptLabRates.DataBind();
            // rptQuizScores.DataSource = stats.QuizScores; rptQuizScores.DataBind();
            // rptModules.DataSource    = stats.Modules;    rptModules.DataBind();
            // pnlNoLabs.Visible    = stats.LabRates.Count == 0;
            // pnlNoQuiz.Visible    = stats.QuizScores.Count == 0;
            // pnlNoModules.Visible = stats.Modules.Count == 0;
            pnlNoModules.Visible = true;
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        public string GetTypeBadge(string t) =>
            t == "Chapter" ? "badge-blue" : t == "Lab" ? "badge-green" : t == "Quiz" ? "badge-amber" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    
        public class LabRateViewModel
        {
            public string CourseName { get; set; }
            public int TotalStudents { get; set; }
            public int CompletedCount { get; set; }
            public int CompletionPct { get; set; }
        }

        // quiz scores per quiz
        public class QuizScoreViewModel
        {
            public string QuizName { get; set; }
            public string CourseName { get; set; }
            public int AttemptCount { get; set; }
            public int AvgScore { get; set; }
        }
    }
}