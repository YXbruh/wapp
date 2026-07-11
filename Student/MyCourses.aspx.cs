using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_MyCourses : Page
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
                LoadCourses();
            }
        }

        private void LoadCourses()
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
                      c.CourseID,
                      c.CourseName,
                      c.Description,
                      c.Level,
                      c.DurationHours,
                      u.FullName AS InstructorName,
                      e.Progress,
                      e.Status,
                      (
                          SELECT COUNT(*)
                          FROM VirtualLabs vl
                          WHERE vl.CourseID = c.CourseID
                            AND vl.IsPublished = 1
                      ) AS LabCount
                  FROM Enrollments e
                  INNER JOIN Courses c
                      ON c.CourseID = e.CourseID
                  INNER JOIN Users u
                      ON u.UserID = c.InstructorID
                  WHERE e.StudentID = @UserID
                  ORDER BY e.EnrolledAt DESC", con))
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
                    "StatusKey",
                    typeof(string)
                );

                dt.Columns.Add(
                    "StatusBadgeClass",
                    typeof(string)
                );

                dt.Columns.Add(
                    "StatusLabel",
                    typeof(string)
                );

                dt.Columns.Add(
                    "LevelBadgeClass",
                    typeof(string)
                );

                dt.Columns.Add(
                    "IconClass",
                    typeof(string)
                );

                string[] icons =
                {
                    "ti-shield-lock",
                    "ti-network",
                    "ti-bug",
                    "ti-server-2",
                    "ti-key"
                };

                int index = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string status =
                        Convert.ToString(row["Status"]);

                    row["StatusLabel"] = status;

                    if (status == "Completed")
                    {
                        row["StatusKey"] = "completed";
                        row["StatusBadgeClass"] =
                            "badge-green";
                    }
                    else if (status == "In Progress")
                    {
                        row["StatusKey"] = "inprogress";
                        row["StatusBadgeClass"] =
                            "badge-blue";
                    }
                    else
                    {
                        row["StatusKey"] = "notstarted";
                        row["StatusBadgeClass"] =
                            "badge-amber";
                    }

                    string level =
                        Convert.ToString(row["Level"]);

                    if (level == "Advanced")
                    {
                        row["LevelBadgeClass"] =
                            "badge-red";
                    }
                    else if (level == "Intermediate")
                    {
                        row["LevelBadgeClass"] =
                            "badge-amber";
                    }
                    else
                    {
                        row["LevelBadgeClass"] =
                            "badge-green";
                    }

                    row["IconClass"] =
                        icons[index % icons.Length];

                    index++;
                }

                rptCourses.DataSource = dt;
                rptCourses.DataBind();

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