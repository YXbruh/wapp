using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Admin_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack)
                LoadDashboard();
        }

        private void LoadDashboard()
        {
            litLastUpdated.Text = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            DataTable stats = AdminService.GetPlatformStats();
            if (stats.Rows.Count > 0)
            {
                DataRow row = stats.Rows[0];
                litTotalUsers.Text = Convert.ToInt32(row["UserCount"]).ToString("N0");
                litActiveCourses.Text = row["CourseCount"].ToString();
                litLabsOnline.Text = row["LabCount"].ToString();
                int alertCount = Convert.ToInt32(row["AlertCount"]);
                litAlerts.Text = alertCount.ToString();
                litAlertCount.Text = alertCount.ToString();
                litAlertStatus.Text = alertCount > 0 ? "Needs attention" : "All clear";
            }

            rptUserChart.DataSource = GetChartPercent("Users by Role");
            rptUserChart.DataBind();

            rptCourseChart.DataSource = GetChartPercent("Courses by Level");
            rptCourseChart.DataBind();

            rptLabChart.DataSource = GetChartPercent("Labs by Status");
            rptLabChart.DataBind();

            DataTable users = UserService.Search("", "", "");
            rptUsers.DataSource = users;
            rptUsers.DataBind();
            pnlNoUsers.Visible = users.Rows.Count == 0;
        }

        private DataTable GetChartPercent(string group)
        {
            DataTable all = AdminService.GetChartData();
            DataTable dt = all.Clone();
            dt.Columns.Add("Percent", typeof(int));
            DataRow[] rows = all.Select("ChartGroup = '" + group.Replace("'", "''") + "'");
            int maxVal = 1;
            foreach (DataRow r in rows) maxVal = Math.Max(maxVal, Convert.ToInt32(r["Value"]));
            foreach (DataRow r in rows)
            {
                DataRow nr = dt.NewRow();
                nr["ChartGroup"] = r["ChartGroup"];
                nr["Label"] = r["Label"];
                nr["Value"] = r["Value"];
                nr["Percent"] = (int)Math.Round(Convert.ToDouble(r["Value"]) / maxVal * 100);
                dt.Rows.Add(nr);
            }
            return dt;
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e)
        {
            DataTable users = UserService.Search(tbSearch.Text.Trim(), "", "");
            rptUsers.DataSource = users;
            rptUsers.DataBind();
            pnlNoUsers.Visible = users.Rows.Count == 0;
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string userId = e.CommandArgument.ToString();
            if (e.CommandName == "Edit")
                Response.Redirect($"~/Admin/EditUser.aspx?id={userId}");
            else if (e.CommandName == "Delete")
            {
                UserService.Delete(userId);
                AdminService.LogAudit(Session["UserID"].ToString(),
                    "DELETE_USER", "Users", userId, "", "");
                LoadDashboard();
            }
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            string csv = UserService.ExportCsv("", "", "");
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=users.csv");
            Response.Write(csv);
            Response.End();
        }

        public string GetRoleBadge(string role) =>
            role == "Admin" ? "badge-red" : role == "Lecturer" ? "badge-green" : "badge-blue";

        public string GetStatusBadge(string isActive) =>
            isActive == "True" ? "badge-green" : "badge-amber";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
