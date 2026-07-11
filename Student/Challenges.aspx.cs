using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Challenges : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }     //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadChallenges();
            }
        }

        private void LoadChallenges()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            // NOTE: the database has no separate Challenges table, so VirtualLabs is
            // reused here as "Challenges" (both are hands-on, validated exercises).
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            string query = "select vl.LabID as ChallengeID, vl.LabTitle as ChallengeName, vl.SkillTag, "
                         + "vl.Difficulty, vl.PointsReward as XPReward, vl.TimeLimitMinutes "
                         + "from VirtualLabs vl "
                         + "join Courses c on vl.CourseID = c.CourseID "
                         + "join Enrollments e on e.CourseID = c.CourseID and e.StudentID = " + userId + " "
                         + "where vl.IsPublished = 1 order by c.CourseName, vl.LabTitle";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("DeadlineDisplay", typeof(string));
            dt.Columns.Add("StatusKey", typeof(string));
            dt.Columns.Add("StatusBadgeClass", typeof(string));
            dt.Columns.Add("StatusLabel", typeof(string));

            int totalDone = 0;
            int totalXP = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["Category"] = (row["SkillTag"] == DBNull.Value) ? "General" : row["SkillTag"].ToString();
                row["DeadlineDisplay"] = (row["TimeLimitMinutes"] == DBNull.Value) ? "No time limit" : row["TimeLimitMinutes"] + " min limit";

                int labId = Convert.ToInt32(row["ChallengeID"]);

                SqlCommand cmdLast = new SqlCommand("select top 1 Result from LabSubmissions where LabID = " + labId
                                                   + " and StudentID = " + userId + " order by SubmittedAt desc", con);
                object lastResultObj = cmdLast.ExecuteScalar();

                SqlCommand cmdCount = new SqlCommand("select count(*) from LabSubmissions where LabID = " + labId
                                                    + " and StudentID = " + userId, con);
                int attempts = Convert.ToInt32(cmdCount.ExecuteScalar());

                string statusKey;
                if (lastResultObj != null && lastResultObj.ToString() == "Passed")
                {
                    statusKey = "done";
                }
                else if (attempts > 0)
                {
                    statusKey = "in-progress";
                }
                else
                {
                    statusKey = "not-started";
                }

                row["StatusKey"] = statusKey;

                if (statusKey == "done")
                {
                    row["StatusBadgeClass"] = "badge-green";
                    row["StatusLabel"] = "Completed";
                    totalDone++;
                    totalXP += Convert.ToInt32(row["XPReward"]);
                }
                else if (statusKey == "in-progress")
                {
                    row["StatusBadgeClass"] = "badge-blue";
                    row["StatusLabel"] = "In Progress";
                }
                else
                {
                    row["StatusBadgeClass"] = "badge-amber";
                    row["StatusLabel"] = "Not Started";
                }
            }

            litTotal.Text = dt.Rows.Count.ToString();
            litDone.Text = totalDone.ToString();
            litXP.Text = totalXP.ToString();

            rptChallenges.DataSource = dt;
            rptChallenges.DataBind();
            pnlEmpty.Visible = (dt.Rows.Count == 0);

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