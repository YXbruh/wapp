using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Achievements : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }         //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadAchievements();
            }
        }

        private void LoadAchievements()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            string query = "select a.AchievementID, a.BadgeName, a.Description, a.IconPath, a.PointsGranted, ua.EarnedAt "
                         + "from Achievements a left join UserAchievements ua "
                         + "on ua.AchievementID = a.AchievementID and ua.UserID = " + userId + " "
                         + "order by a.BadgeName";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dt.Columns.Add("IsEarned", typeof(bool));
            dt.Columns.Add("IconClass", typeof(string));
            dt.Columns.Add("EarnedDate", typeof(string));

            int earned = 0;
            int totalXP = 0;
            string latestBadge = "—";
            DateTime latestDate = DateTime.MinValue;

            foreach (DataRow row in dt.Rows)
            {
                bool isEarned = row["EarnedAt"] != DBNull.Value;
                row["IsEarned"] = isEarned;
                row["IconClass"] = (row["IconPath"] == DBNull.Value) ? "ti-award" : row["IconPath"].ToString();

                if (isEarned)
                {
                    DateTime earnedAt = Convert.ToDateTime(row["EarnedAt"]);
                    row["EarnedDate"] = earnedAt.ToString("dd MMM yyyy");

                    earned++;
                    totalXP += Convert.ToInt32(row["PointsGranted"]);

                    if (earnedAt > latestDate)
                    {
                        latestDate = earnedAt;
                        latestBadge = row["BadgeName"].ToString();
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