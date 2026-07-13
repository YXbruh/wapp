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
                }
            }

            
        }

        private void LoadDashboard(string instructorId)
        {
            string userId = Session["UserID"].ToString(); //bypass Login
            litName.Text = Session["FullName"] as string ?? "Lecturer";
            litDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");


            string connString = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

            string query = "SELECT COUNT(*) FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    try
                    {
                        conn.Open();
                        int activeModulesCount = (int)cmd.ExecuteScalar();
                        litModules.Text = activeModulesCount.ToString();
                    }
                    catch (Exception ex) { litModules.Text = "0"; }
                }
            }

            // TODO:
            // var stats = LecturerService.GetDashboardStats(userId);
            //litModules.Text  = stats.ActiveModules.ToString();
            // litStudents.Text = stats.TotalStudents.ToString();
            // litLabRate.Text  = stats.AvgLabCompletion + "%";
            // litQuizAvg.Text  = stats.AvgQuizScore + "%";
            // rptLabRates.DataSource   = stats.LabRates;   rptLabRates.DataBind();
            // rptQuizScores.DataSource = stats.QuizScores; rptQuizScores.DataBind();
            // rptModules.DataSource    = stats.Modules;    rptModules.DataBind();
            // pnlNoLabs.Visible    = stats.LabRates.Count == 0;
            // pnlNoQuiz.Visible    = stats.QuizScores.Count == 0;
            // pnlNoModules.Visible = stats.Modules.Count == 0;
            pnlNoLabs.Visible = pnlNoQuiz.Visible = pnlNoModules.Visible = true;
        }

        public string GetScoreColor(int score) =>
            score >= 80 ? "var(--success)" : score >= 60 ? "var(--warning)" : "var(--danger)";

        public string GetTypeBadge(string t) =>
            t == "Chapter" ? "badge-blue" : t == "Lab" ? "badge-green" : t == "Quiz" ? "badge-amber" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }

}