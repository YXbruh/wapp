using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{
    public partial class Admin_ActivityLogs : Page
    {
        private int _page = 1;
        private const int PageSize = 25;

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            //{ Response.Redirect("~/Login.aspx"); return; }                                     //remove to bypass login for testing
            if (!IsPostBack)
            {
                // Default date range: last 7 days
                tbDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                tbDateFrom.Text = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
                LoadLogs();
            }
        }

        private void LoadLogs()
        {
            // TODO:
            // var logs = LogService.Search(tbSearch.Text.Trim(), ddlSeverity.SelectedValue,
            //                              tbDateFrom.Text, tbDateTo.Text, _page, PageSize,
            //                              out int total);
            // litInfo.Text     = LogService.CountBySeverity("Info").ToString();
            // litWarning.Text  = LogService.CountBySeverity("Warning").ToString();
            // litCritical.Text = LogService.CountBySeverity("Critical").ToString();
            // litShowing.Text  = $"{logs.Count} of {total}";
            // lbPrev.Enabled   = _page > 1;
            // lbNext.Enabled   = (_page * PageSize) < total;
            // rptLogs.DataSource = logs; rptLogs.DataBind();
            // pnlEmpty.Visible = logs.Count == 0;
            pnlEmpty.Visible = true;
            litShowing.Text = "0";
        }

        protected void Filter_Changed(object sender, EventArgs e) { _page = 1; LoadLogs(); }

        protected void lbPrev_Click(object sender, EventArgs e)
        { if (_page > 1) { _page--; LoadLogs(); } }

        protected void lbNext_Click(object sender, EventArgs e)
        { _page++; LoadLogs(); }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            // TODO: CSV export
        }

        public string GetSeverityBadge(string s) =>
            s == "Critical" ? "badge-red" : s == "Warning" ? "badge-amber" : "badge-blue";

        public string GetSeverityIcon(string s) =>
            s == "Critical" ? "ti-alert-octagon" : s == "Warning" ? "ti-alert-triangle" : "ti-info-circle";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}