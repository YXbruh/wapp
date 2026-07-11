using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Labs : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }         //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadLabs();
            }
        }

        private void LoadLabs()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            string query = "select vl.LabID, vl.LabTitle as LabName, c.CourseName, vl.TimeLimitMinutes, vl.Difficulty "
                         + "from VirtualLabs vl "
                         + "join Courses c on vl.CourseID = c.CourseID "
                         + "join Enrollments e on e.CourseID = c.CourseID and e.StudentID = " + userId + " "
                         + "where vl.IsPublished = 1 order by c.CourseName, vl.LabTitle";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dt.Columns.Add("EstimatedMinutes", typeof(int));
            dt.Columns.Add("StatusKey", typeof(string));
            dt.Columns.Add("StatusBadgeClass", typeof(string));
            dt.Columns.Add("StatusLabel", typeof(string));

            int done = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["EstimatedMinutes"] = (row["TimeLimitMinutes"] == DBNull.Value) ? 15 : Convert.ToInt32(row["TimeLimitMinutes"]);

                int labId = Convert.ToInt32(row["LabID"]);

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
                    done++;
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
                row["StatusBadgeClass"] = (statusKey == "done") ? "badge-green" : (statusKey == "in-progress") ? "badge-blue" : "badge-amber";
                row["StatusLabel"] = (statusKey == "done") ? "Completed" : (statusKey == "in-progress") ? "In Progress" : "Not Started";
            }

            litTotal.Text = dt.Rows.Count.ToString();
            litDone.Text = done.ToString();
            litRemaining.Text = (dt.Rows.Count - done).ToString();

            rptLabs.DataSource = dt;
            rptLabs.DataBind();
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