using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Labs : Page
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
                LoadLabs();
            }
        }

        private void LoadLabs()
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
                      vl.LabID,
                      vl.LabTitle AS LabName,
                      c.CourseName,
                      vl.TimeLimitMinutes,
                      vl.Difficulty,

                      (
                          SELECT TOP 1
                              ls.Result
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @UserID
                          ORDER BY ls.SubmittedAt DESC
                      ) AS LastResult,

                      (
                          SELECT COUNT(*)
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @UserID
                      ) AS AttemptCount

                  FROM VirtualLabs vl

                  INNER JOIN Courses c
                      ON c.CourseID = vl.CourseID

                  INNER JOIN Enrollments e
                      ON e.CourseID = c.CourseID
                     AND e.StudentID = @UserID

                  WHERE vl.IsPublished = 1

                  ORDER BY
                      c.CourseName,
                      vl.LabTitle", con))
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
                    "EstimatedMinutes",
                    typeof(int)
                );

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

                int done = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["TimeLimitMinutes"] ==
                        DBNull.Value)
                    {
                        row["EstimatedMinutes"] = 15;
                    }
                    else
                    {
                        row["EstimatedMinutes"] =
                            Convert.ToInt32(
                                row["TimeLimitMinutes"]
                            );
                    }

                    string lastResult =
                        row["LastResult"] == DBNull.Value
                            ? ""
                            : Convert.ToString(
                                row["LastResult"]);

                    int attempts =
                        Convert.ToInt32(
                            row["AttemptCount"]);

                    string statusKey;

                    if (lastResult == "Passed")
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

                    if (statusKey == "done")
                    {
                        row["StatusBadgeClass"] =
                            "badge-green";

                        row["StatusLabel"] =
                            "Completed";
                    }
                    else if (statusKey ==
                             "in-progress")
                    {
                        row["StatusBadgeClass"] =
                            "badge-blue";

                        row["StatusLabel"] =
                            "In Progress";
                    }
                    else
                    {
                        row["StatusBadgeClass"] =
                            "badge-amber";

                        row["StatusLabel"] =
                            "Not Started";
                    }
                }

                litTotal.Text =
                    dt.Rows.Count.ToString();

                litDone.Text =
                    done.ToString();

                litRemaining.Text =
                    (dt.Rows.Count - done).ToString();

                rptLabs.DataSource = dt;
                rptLabs.DataBind();

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