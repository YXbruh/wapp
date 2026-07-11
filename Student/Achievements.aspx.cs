using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Achievements : Page
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
                LoadAchievements();
            }
        }

        private void LoadAchievements()
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
                      a.AchievementID,
                      a.BadgeName,
                      a.Description,
                      a.IconPath,
                      a.PointsGranted,
                      ua.EarnedAt
                  FROM Achievements a
                  LEFT JOIN UserAchievements ua
                      ON ua.AchievementID =
                         a.AchievementID
                     AND ua.UserID = @UserID
                  ORDER BY a.BadgeName", con))
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
                    "IsEarned",
                    typeof(bool)
                );

                dt.Columns.Add(
                    "IconClass",
                    typeof(string)
                );

                dt.Columns.Add(
                    "EarnedDate",
                    typeof(string)
                );

                int earned = 0;
                int totalXP = 0;

                string latestBadge = "—";
                DateTime latestDate = DateTime.MinValue;

                foreach (DataRow row in dt.Rows)
                {
                    bool isEarned =
                        row["EarnedAt"] != DBNull.Value;

                    row["IsEarned"] = isEarned;

                    if (row["IconPath"] == DBNull.Value ||
                        string.IsNullOrWhiteSpace(
                            Convert.ToString(
                                row["IconPath"])))
                    {
                        row["IconClass"] = "ti-award";
                    }
                    else
                    {
                        row["IconClass"] =
                            Convert.ToString(
                                row["IconPath"]);
                    }

                    if (isEarned)
                    {
                        DateTime earnedAt =
                            Convert.ToDateTime(
                                row["EarnedAt"]);

                        row["EarnedDate"] =
                            earnedAt.ToString(
                                "dd MMM yyyy"
                            );

                        earned++;

                        totalXP +=
                            Convert.ToInt32(
                                row["PointsGranted"]
                            );

                        if (earnedAt > latestDate)
                        {
                            latestDate = earnedAt;

                            latestBadge =
                                Convert.ToString(
                                    row["BadgeName"]);
                        }
                    }
                    else
                    {
                        row["EarnedDate"] = "";
                    }
                }

                litEarned.Text = earned.ToString();
                litTotal.Text = dt.Rows.Count.ToString();
                litXP.Text = totalXP.ToString();
                litLatest.Text = latestBadge;

                rptAchievements.DataSource = dt;
                rptAchievements.DataBind();

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