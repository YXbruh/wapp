using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
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

        private string SelectedCertificateId
        {
            get
            {
                return Convert.ToString(
                    ViewState["CertificateID"]);
            }
            set
            {
                ViewState["CertificateID"] =
                    value;
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
                row["IssuedDate"] =
                    row["CompletedAt"] == DBNull.Value
                        ? "—"
                        : Convert.ToDateTime(
                            row["CompletedAt"]
                          ).ToString("dd MMM yyyy");
            }

            litCount.Text =
                dt.Rows.Count.ToString();

            litLatest.Text =
                dt.Rows.Count == 0
                    ? "—"
                    : Convert.ToString(
                        dt.Rows[0]["IssuedDate"]);

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

            if (e.CommandName == "View")
            {
                LoadPreview(certificateId);
            }
            else if (e.CommandName == "Download")
            {
                DownloadCertificate(
                    certificateId);
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
                    AND e.StudentID = @UserID
                    AND e.Status = 'Completed'
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

                    completedAt =
                        Convert.ToDateTime(
                            reader["CompletedAt"]);

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

            if (!GetCertificate(
                    certificateId,
                    out studentName,
                    out courseName,
                    out completedAt))
            {
                return;
            }

            SelectedCertificateId =
                certificateId;

            litPreviewStudent.Text =
                Server.HtmlEncode(studentName);

            litPreviewCourse.Text =
                Server.HtmlEncode(courseName);

            litPreviewDate.Text =
                completedAt.ToString(
                    "dd MMMM yyyy");

            litPreviewId.Text =
                Server.HtmlEncode(
                    certificateId);

            pnlCertificateList.Visible = false;
            pnlCertificatePreview.Visible = true;
        }

        protected void btnBackToCertificates_Click(
            object sender,
            EventArgs e)
        {
            pnlCertificatePreview.Visible = false;
            pnlCertificateList.Visible = true;
        }

        protected void btnDownloadPreview_Click(
            object sender,
            EventArgs e)
        {
            DownloadCertificate(
                SelectedCertificateId);
        }

        private void DownloadCertificate(
            string certificateId)
        {
            string studentName;
            string courseName;
            DateTime completedAt;

            if (!GetCertificate(
                    certificateId,
                    out studentName,
                    out courseName,
                    out completedAt))
            {
                return;
            }

            string text =
                "CYBERSHIELD ACADEMY\r\n\r\n" +
                "CERTIFICATE OF COMPLETION\r\n\r\n" +
                "This certificate is presented to\r\n" +
                studentName + "\r\n\r\n" +
                "for successfully completing\r\n" +
                courseName + "\r\n\r\n" +
                "Issued: " +
                completedAt.ToString(
                    "dd MMMM yyyy") +
                "\r\nCertificate ID: " +
                certificateId;

            byte[] bytes =
                Encoding.UTF8.GetBytes(text);

            Response.Clear();
            Response.ContentType =
                "application/octet-stream";

            Response.AddHeader(
                "Content-Disposition",
                "attachment; filename=Certificate-" +
                certificateId + ".txt");

            Response.OutputStream.Write(
                bytes,
                0,
                bytes.Length);

            Response.Flush();

            Context.ApplicationInstance
                .CompleteRequest();
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