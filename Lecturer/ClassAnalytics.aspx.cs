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
                    LoadData(instructorId, null);
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

        private void LoadData(string instructorId, string courseId)
        {
            if (string.IsNullOrEmpty(instructorId)) courseId = null;

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
                    SELECT COUNT(DISTINCT e.StudentID)
                FROM Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                INNER JOIN VirtualLabs vl ON c.CourseID = vl.CourseID
                INNER JOIN LabSubmissions ls ON vl.LabID = ls.LabID AND ls.StudentID = e.StudentID
                WHERE c.InstructorID = @InstructorID
                  AND c.IsPublished = 1
                  AND ls.IsCorrect = 1" +
                (string.IsNullOrEmpty(courseId) ? "" : " AND c.CourseID = @CourseID");

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
                    catch (Exception ex)
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
                    catch (Exception ex)
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
                    catch (Exception ex)
                    {
                        litLabRate.Text = "0%";
                    }
                }

                using (SqlCommand cmd = new SqlCommand(sandboxQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    if (!string.IsNullOrEmpty(courseId))
                        cmd.Parameters.AddWithValue("@CourseID", courseId);
                    litSandboxCleared.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }

                pnlEmpty.Visible = pnlNoQuiz.Visible = true;
            }

            

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

        }

        protected void ddlCourse_Changed(object sender, EventArgs e)
        { 
            string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
            string courseId = ddlCourse.SelectedValue;
            LoadData(instructorId, courseId);
        }
        protected void tbSearch_Changed(object sender, EventArgs e)
        {
            string instructorId = Session["UserID"] != null ? Session["UserID"].ToString().Trim() : "";
            string courseId = ddlCourse.SelectedValue;
            LoadData(instructorId, courseId);
        }

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