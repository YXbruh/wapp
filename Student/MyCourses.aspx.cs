using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_MyCourses : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadCourses();
            }
        }

        private void LoadCourses()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            string query = "select c.CourseID, c.CourseName, c.Description, c.Level, c.DurationHours, "
                         + "u.FullName as InstructorName, e.Progress, e.Status, "
                         + "(select count(*) from VirtualLabs vl where vl.CourseID = c.CourseID and vl.IsPublished = 1) as LabCount "
                         + "from Enrollments e "
                         + "join Courses c on e.CourseID = c.CourseID "
                         + "join Users u on c.InstructorID = u.UserID "
                         + "where e.StudentID = " + userId + " order by e.EnrolledAt desc";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dt.Columns.Add("StatusKey", typeof(string));
            dt.Columns.Add("StatusBadgeClass", typeof(string));
            dt.Columns.Add("StatusLabel", typeof(string));
            dt.Columns.Add("LevelBadgeClass", typeof(string));
            dt.Columns.Add("IconClass", typeof(string));

            string[] icons = { "ti-shield-lock", "ti-network", "ti-bug", "ti-server-2", "ti-key" };
            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                string status = row["Status"].ToString();
                if (status == "Completed")
                {
                    row["StatusKey"] = "completed";
                    row["StatusBadgeClass"] = "badge-green";
                }
                else if (status == "In Progress")
                {
                    row["StatusKey"] = "inprogress";
                    row["StatusBadgeClass"] = "badge-blue";
                }
                else
                {
                    row["StatusKey"] = "notstarted";
                    row["StatusBadgeClass"] = "badge-amber";
                }
                row["StatusLabel"] = status;

                string level = row["Level"].ToString();
                if (level == "Advanced") row["LevelBadgeClass"] = "badge-red";
                else if (level == "Intermediate") row["LevelBadgeClass"] = "badge-amber";
                else row["LevelBadgeClass"] = "badge-green";

                row["IconClass"] = icons[i % icons.Length];
                i++;
            }

            rptCourses.DataSource = dt;
            rptCourses.DataBind();
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