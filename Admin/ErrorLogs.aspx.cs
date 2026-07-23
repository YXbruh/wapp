using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class ErrorLogs : Page
    {
        private const int PageSize = 10;

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
            DataTable logs = AdminService.GetErrorLogs(tbSearch.Text.Trim(),
                ddlSeverity.SelectedValue, tbDateFrom.Text, tbDateTo.Text,
                CurrentPage, PageSize, out int total);

            litErrors.Text = AdminService.GetErrorLogCountBySeverity("Error").ToString();
            litWarnings.Text = AdminService.GetErrorLogCountBySeverity("Warning").ToString();
            litInfo.Text = AdminService.GetErrorLogCountBySeverity("Info").ToString();
            litUnresolved.Text = AdminService.GetErrorLogUnresolvedCount().ToString();

            litShowing.Text = $"{logs.Rows.Count} of {total}";
            lbPrev.Enabled = CurrentPage > 1;
            lbNext.Enabled = (CurrentPage * PageSize) < total;

            rptErrors.DataSource = logs;
            rptErrors.DataBind();
            pnlEmpty.Visible = logs.Rows.Count == 0;
        }

        protected void Filter_Changed(object sender, EventArgs e) { CurrentPage = 1; LoadLogs(); }

        protected void lbPrev_Click(object sender, EventArgs e)
        { if (CurrentPage > 1) { CurrentPage--; LoadLogs(); } }

        protected void lbNext_Click(object sender, EventArgs e)
        { CurrentPage++; LoadLogs(); }

        protected void rptErrors_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Resolve")
            {
                if (!int.TryParse(e.CommandArgument.ToString(), out int errorId)) return;
                string adminId = Session["UserID"].ToString();
                try
                {
                    AdminService.MarkErrorResolved(errorId, adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Error marked as resolved.";
                    LoadLogs();
                }
                catch (Exception)
                {
                    pnlError.Visible = true;
                    litError.Text = "Error marking as resolved.";
                }
            }
        }

        public string GetRowClass(string severity) =>
            severity == "Critical" || severity == "Error" ? "error-row-critical" :
            severity == "Warning" ? "error-row-warning" : "error-row-info";

        public string GetSeverityBadge(string s) =>
            s == "Critical" || s == "Error" ? "badge-red" :
            s == "Warning" ? "badge-amber" : "badge-blue";

        public string GetSeverityIcon(string s) =>
            s == "Critical" || s == "Error" ? "ti-alert-octagon" :
            s == "Warning" ? "ti-alert-triangle" : "ti-info-circle";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
