using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Certificates : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCertificates();
            }
        }

        private void LoadCertificates()
        {
            string userId =
                Convert.ToString(Session["UserID"]);

            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT
                      e.EnrollmentID AS CertificateID,
                      c.CourseName,
                      e.CompletedAt
                  FROM Enrollments e
                  INNER JOIN Courses c
                      ON c.CourseID = e.CourseID
                  WHERE e.StudentID = @UserID
                    AND e.Status = 'Completed'
                  ORDER BY e.CompletedAt DESC", con))
            {
                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    10
                ).Value = userId;

                DataTable dt = new DataTable();

                using (SqlDataAdapter da =
                       new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                dt.Columns.Add(
                    "IssuedDate",
                    typeof(string)
                );

                foreach (DataRow row in dt.Rows)
                {
                    if (row["CompletedAt"] ==
                        DBNull.Value)
                    {
                        row["IssuedDate"] = "—";
                    }
                    else
                    {
                        row["IssuedDate"] =
                            Convert.ToDateTime(
                                row["CompletedAt"]
                            ).ToString("dd MMM yyyy");
                    }
                }

                litCount.Text =
                    dt.Rows.Count.ToString();

                if (dt.Rows.Count > 0)
                {
                    litLatest.Text =
                        Convert.ToString(
                            dt.Rows[0]["IssuedDate"]);
                }
                else
                {
                    litLatest.Text = "—";
                }

                rptCerts.DataSource = dt;
                rptCerts.DataBind();

                pnlEmpty.Visible =
                    dt.Rows.Count == 0;
            }
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("~/Login.aspx");
        }
    }
}