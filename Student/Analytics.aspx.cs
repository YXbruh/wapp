using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Analytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadAnalytics();
            }
        }

        private void LoadAnalytics()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            // ---- Overall progress ----
            SqlCommand cmd1 = new SqlCommand("select avg(Progress) from Enrollments where StudentID = " + userId, con);
            object overallResult = cmd1.ExecuteScalar();
            litOverall.Text = (overallResult == DBNull.Value) ? "0%" : Math.Round(Convert.ToDouble(overallResult)) + "%";

            // ---- Average quiz score ----
            SqlCommand cmd2 = new SqlCommand("select avg(Score) from QuizAttempts where StudentID = " + userId, con);
            object quizResult = cmd2.ExecuteScalar();
            litAvgQuiz.Text = (quizResult == DBNull.Value) ? "—" : Math.Round(Convert.ToDouble(quizResult)) + "%";

            // ---- Labs done / total ----
            SqlCommand cmd3 = new SqlCommand("select count(distinct LabID) from LabSubmissions where StudentID = " + userId + " and Result = 'Passed'", con);
            int labsDone = Convert.ToInt32(cmd3.ExecuteScalar());

            SqlCommand cmd4 = new SqlCommand("select count(*) from VirtualLabs vl join Enrollments e on vl.CourseID = e.CourseID "
                                            + "where e.StudentID = " + userId + " and vl.IsPublished = 1", con);
            int labsTotal = Convert.ToInt32(cmd4.ExecuteScalar());

            litLabsDone.Text = labsDone.ToString();
            litLabsTotal.Text = labsTotal.ToString();

            // ---- Study time (estimated from the time limit of each passed lab) ----
            SqlCommand cmd5 = new SqlCommand("select isnull(sum(isnull(TimeLimitMinutes,15)),0) from VirtualLabs "
                                            + "where LabID in (select distinct LabID from LabSubmissions where StudentID = " + userId + " and Result = 'Passed')", con);
            int studyMinutes = Convert.ToInt32(cmd5.ExecuteScalar());
            litStudyTime.Text = Math.Round(studyMinutes / 60.0, 1) + "h";

            // ---- Course progress ----
            string courseQuery = "select c.CourseName, e.Progress, e.Status, "
                + "(select count(*) from ChapterProgress cp join Chapters ch on cp.ChapterID = ch.ChapterID "
                + " where ch.CourseID = c.CourseID and cp.StudentID = " + userId + " and cp.IsCompleted = 1) as CompletedModules, "
                + "(select count(*) from Chapters ch2 where ch2.CourseID = c.CourseID and ch2.IsPublished = 1) as TotalModules "
                + "from Enrollments e join Courses c on e.CourseID = c.CourseID "
                + "where e.StudentID = " + userId + " order by e.EnrolledAt desc";

            SqlDataAdapter daCourses = new SqlDataAdapter(courseQuery, con);
            DataTable dtCourses = new DataTable();
            daCourses.Fill(dtCourses);

            dtCourses.Columns.Add("StatusBadgeClass", typeof(string));
            dtCourses.Columns.Add("StatusLabel", typeof(string));

            foreach (DataRow row in dtCourses.Rows)
            {
                string status = row["Status"].ToString();
                row["StatusLabel"] = status;
                if (status == "Completed") row["StatusBadgeClass"] = "badge-green";
                else if (status == "In Progress") row["StatusBadgeClass"] = "badge-blue";
                else row["StatusBadgeClass"] = "badge-amber";
            }

            rptCourseProgress.DataSource = dtCourses;
            rptCourseProgress.DataBind();
            pnlNoCourses.Visible = (dtCourses.Rows.Count == 0);

            // ---- Quiz performance ----
            string quizQuery = "select q.Title as QuizName, qa.Score, qa.AttemptedAt "
                + "from QuizAttempts qa join Quizzes q on qa.QuizID = q.QuizID "
                + "where qa.StudentID = " + userId + " order by qa.AttemptedAt desc";

            SqlDataAdapter daQuiz = new SqlDataAdapter(quizQuery, con);
            DataTable dtQuiz = new DataTable();
            daQuiz.Fill(dtQuiz);

            dtQuiz.Columns.Add("AttemptedOn", typeof(string));
            foreach (DataRow row in dtQuiz.Rows)
            {
                row["AttemptedOn"] = Convert.ToDateTime(row["AttemptedAt"]).ToString("dd MMM yyyy");
            }

            rptQuizScores.DataSource = dtQuiz;
            rptQuizScores.DataBind();
            pnlNoQuizzes.Visible = (dtQuiz.Rows.Count == 0);

            // ---- Lab completion by course ----
            string labQuery = "select c.CourseName, "
                + "(select count(distinct ls.LabID) from LabSubmissions ls join VirtualLabs vl2 on ls.LabID = vl2.LabID "
                + " where vl2.CourseID = c.CourseID and ls.StudentID = " + userId + " and ls.Result = 'Passed') as LabsDone, "
                + "(select count(*) from VirtualLabs vl3 where vl3.CourseID = c.CourseID and vl3.IsPublished = 1) as LabsTotal "
                + "from Enrollments e join Courses c on e.CourseID = c.CourseID "
                + "where e.StudentID = " + userId + " order by c.CourseName";

            SqlDataAdapter daLabs = new SqlDataAdapter(labQuery, con);
            DataTable dtLabs = new DataTable();
            daLabs.Fill(dtLabs);

            dtLabs.Columns.Add("LabProgressPct", typeof(int));
            foreach (DataRow row in dtLabs.Rows)
            {
                int done = Convert.ToInt32(row["LabsDone"]);
                int total = Convert.ToInt32(row["LabsTotal"]);
                row["LabProgressPct"] = (total > 0) ? (done * 100 / total) : 0;
            }

            rptLabProgress.DataSource = dtLabs;
            rptLabProgress.DataBind();
            pnlNoLabs.Visible = (dtLabs.Rows.Count == 0);

            con.Close();
        }

        // Called from markup — colours bar by score
        public string GetScoreColor(int score)
        {
            if (score >= 80) return "var(--success)";
            if (score >= 60) return "var(--warning)";
            return "var(--danger)";
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}