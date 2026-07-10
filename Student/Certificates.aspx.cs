using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Certificates : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }     //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadCertificates();
            }
        }

        private void LoadCertificates()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            // NOTE: there is no separate Certificates table in the database — a completed
            // Enrollment (Status = 'Completed') is treated as an earned certificate here.
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            string query = "select e.EnrollmentID as CertificateID, c.CourseName, e.CompletedAt "
                         + "from Enrollments e join Courses c on e.CourseID = c.CourseID "
                         + "where e.StudentID = " + userId + " and e.Status = 'Completed' "
                         + "order by e.CompletedAt desc";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dt.Columns.Add("IssuedDate", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                row["IssuedDate"] = Convert.ToDateTime(row["CompletedAt"]).ToString("dd MMM yyyy");
            }

            litCount.Text = dt.Rows.Count.ToString();
            litLatest.Text = (dt.Rows.Count > 0) ? dt.Rows[0]["IssuedDate"].ToString() : "—";

            rptCerts.DataSource = dt;
            rptCerts.DataBind();
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