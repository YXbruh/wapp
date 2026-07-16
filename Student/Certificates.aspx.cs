using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{
    public partial class Student_Certificates : Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;
            }
        }

        private string UserId
        {
            get
            {
                return Convert.ToString(
                    Session["UserID"]);
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                pnlCertificateList.Visible = true;
                pnlCertificatePreview.Visible = false;

                LoadCertificates();
            }
        }

        private void LoadCertificates()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      e.EnrollmentID AS CertificateID,
                      c.CourseName,
                      e.CompletedAt
                  FROM Enrollments e
                  INNER JOIN Courses c
                      ON c.CourseID = e.CourseID
                  WHERE e.StudentID = @UserID
                    AND e.Status = 'Completed'
                    AND e.Progress >= 100
                  ORDER BY e.CompletedAt DESC",
                con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                da.Fill(dt);
            }

            dt.Columns.Add(
                "IssuedDate",
                typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                if (row["CompletedAt"] ==
                    DBNull.Value)
                {
                    row["IssuedDate"] = "—";
                }
                else
                {
                    DateTime completedAt =
                        Convert.ToDateTime(
                            row["CompletedAt"]);

                    row["IssuedDate"] =
                        completedAt.ToString(
                            "dd MMM yyyy");
                }
            }

            litCount.Text =
                dt.Rows.Count.ToString();

            if (dt.Rows.Count == 0)
            {
                litLatest.Text = "—";
            }
            else
            {
                litLatest.Text =
                    Convert.ToString(
                        dt.Rows[0]["IssuedDate"]);
            }

            rptCerts.DataSource = dt;
            rptCerts.DataBind();

            pnlEmpty.Visible =
                dt.Rows.Count == 0;
        }

        protected void rptCerts_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            string certificateId =
                Convert.ToString(
                    e.CommandArgument);

            if (string.IsNullOrWhiteSpace(
                certificateId))
            {
                return;
            }

            if (e.CommandName == "View")
            {
                LoadPreview(certificateId);
            }
            else if (e.CommandName == "Download")
            {
                LoadPreview(certificateId);

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "DownloadCertificatePdf",
                    "setTimeout(function () " +
                    "{ downloadCertificatePdf(); }, 500);",
                    true
                );
            }
        }

        private bool GetCertificate(
            string certificateId,
            out string studentName,
            out string courseName,
            out DateTime completedAt)
        {
            studentName = "";
            courseName = "";
            completedAt = DateTime.MinValue;

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      u.FullName,
                      c.CourseName,
                      e.CompletedAt
                  FROM Enrollments e
                  INNER JOIN Users u
                      ON u.UserID = e.StudentID
                  INNER JOIN Courses c
                      ON c.CourseID = e.CourseID
                  WHERE e.EnrollmentID =
                        @CertificateID
                    AND e.StudentID =
                        @UserID
                    AND e.Status =
                        'Completed'
                    AND e.Progress >= 100",
                con))
            {
                cmd.Parameters.Add(
                    "@CertificateID",
                    SqlDbType.NVarChar,
                    10
                ).Value = certificateId;

                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                con.Open();

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    studentName =
                        Convert.ToString(
                            reader["FullName"]);

                    courseName =
                        Convert.ToString(
                            reader["CourseName"]);

                    if (reader["CompletedAt"] ==
                        DBNull.Value)
                    {
                        completedAt =
                            DateTime.Now;
                    }
                    else
                    {
                        completedAt =
                            Convert.ToDateTime(
                                reader["CompletedAt"]);
                    }

                    return true;
                }
            }
        }

        private void LoadPreview(
            string certificateId)
        {
            string studentName;
            string courseName;
            DateTime completedAt;

            bool certificateFound =
                GetCertificate(
                    certificateId,
                    out studentName,
                    out courseName,
                    out completedAt);

            if (!certificateFound)
            {
                pnlCertificatePreview.Visible =
                    false;

                pnlCertificateList.Visible =
                    true;

                return;
            }

            litPreviewStudent.Text =
                Server.HtmlEncode(
                    studentName);

            litPreviewCourse.Text =
                Server.HtmlEncode(
                    courseName);

            litPreviewDate.Text =
                completedAt.ToString(
                    "dd MMMM yyyy");

            litPreviewId.Text =
                Server.HtmlEncode(
                    certificateId);

            pnlCertificateList.Visible =
                false;

            pnlCertificatePreview.Visible =
                true;
        }

        protected void btnBackToCertificates_Click(
            object sender,
            EventArgs e)
        {
            pnlCertificatePreview.Visible =
                false;

            pnlCertificateList.Visible =
                true;

            LoadCertificates();
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