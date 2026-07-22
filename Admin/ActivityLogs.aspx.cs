using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Admin_ActivityLogs : Page
    {
        private const int PageSize = 25;

        private int CurrentPage
        {
            get { return ViewState["Page"] as int? ?? 1; }
            set { ViewState["Page"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack)
            {
                tbDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                tbDateFrom.Text = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
                LoadLogs();
            }
        }

        private void LoadLogs()
        {
            DataTable logs = AdminService.GetLogs(tbSearch.Text.Trim(),
                ddlSeverity.SelectedValue, tbDateFrom.Text, tbDateTo.Text,
                CurrentPage, PageSize, out int total);

            litInfo.Text = AdminService.CountBySeverity("Info").ToString();
            litWarning.Text = AdminService.CountBySeverity("Warning").ToString();
            litCritical.Text = AdminService.CountBySeverity("Critical").ToString();

            litShowing.Text = $"{logs.Rows.Count} of {total}";
            lbPrev.Enabled = CurrentPage > 1;
            lbNext.Enabled = (CurrentPage * PageSize) < total;

            rptLogs.DataSource = logs;
            rptLogs.DataBind();
            pnlEmpty.Visible = logs.Rows.Count == 0;
        }

        protected void Filter_Changed(object sender, EventArgs e) { CurrentPage = 1; LoadLogs(); }

        protected void lbPrev_Click(object sender, EventArgs e)
        { if (CurrentPage > 1) { CurrentPage--; LoadLogs(); } }

        protected void lbNext_Click(object sender, EventArgs e)
        { CurrentPage++; LoadLogs(); }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            DataTable logs = AdminService.GetLogs(tbSearch.Text.Trim(),
                ddlSeverity.SelectedValue, tbDateFrom.Text, tbDateTo.Text,
                1, 99999, out _);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Severity,User,Action,Details,Timestamp");
            foreach (DataRow row in logs.Rows)
                sb.AppendLine($"\"{row["Severity"]}\",\"{row["UserName"]}\",\"{row["Action"]}\",\"{row["Details"]}\",\"{row["OccurredAt"]}\"");
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=audit_log.csv");
            Response.Write(sb.ToString());
            Context.ApplicationInstance.CompleteRequest();
        }

        public string GetSeverityBadge(string s) =>
            s == "Critical" ? "badge-red" : s == "Warning" ? "badge-amber" : "badge-blue";

        public string GetSeverityIcon(string s) =>
            s == "Critical" ? "ti-alert-octagon" : s == "Warning" ? "ti-alert-triangle" : "ti-info-circle";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
