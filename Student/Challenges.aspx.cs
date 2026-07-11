using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Challenges : Page
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
                LoadChallenges();
            }
        }

        private void LoadChallenges()
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
                      vl.LabID AS ChallengeID,
                      vl.LabTitle AS ChallengeName,
                      vl.SkillTag,
                      vl.Difficulty,
                      vl.PointsReward AS XPReward,
                      vl.TimeLimitMinutes,

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
                    "Category",
                    typeof(string)
                );

                dt.Columns.Add(
                    "DeadlineDisplay",
                    typeof(string)
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

                int totalDone = 0;
                int totalXP = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["SkillTag"] ==
                        DBNull.Value)
                    {
                        row["Category"] = "General";
                    }
                    else
                    {
                        row["Category"] =
                            Convert.ToString(
                                row["SkillTag"]);
                    }

                    if (row["TimeLimitMinutes"] ==
                        DBNull.Value)
                    {
                        row["DeadlineDisplay"] =
                            "No time limit";
                    }
                    else
                    {
                        row["DeadlineDisplay"] =
                            row["TimeLimitMinutes"] +
                            " min limit";
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

                        totalDone++;

                        totalXP +=
                            Convert.ToInt32(
                                row["XPReward"]);
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
                    totalDone.ToString();

                litXP.Text =
                    totalXP.ToString();

                rptChallenges.DataSource = dt;
                rptChallenges.DataBind();

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