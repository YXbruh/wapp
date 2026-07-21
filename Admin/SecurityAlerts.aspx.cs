using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class SecurityAlerts : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadAlerts();
        }

        private Pager PagerState
        {
            get
            {
                if (_pager == null)
                {
                    if (ViewState["Pager"] is Pager p)
                        _pager = p;
                    else
                    {
                        _pager = new Pager { PageSize = 20 };
                        ViewState["Pager"] = _pager;
                    }
                }
                return _pager;
            }
        }

        private void UpdatePagerUI()
        {
            var p = PagerState;
            litPageInfo.Text = "Page " + p.Page + " of " + Math.Max(1, p.TotalPages) + " (" + p.Total + " total)";
            btnPrev.Visible = p.HasPrevious;
            btnNext.Visible = p.HasNext;
        }

        private void LoadAlerts()
        {
            var p = PagerState;
            DataTable alerts = AdminService.GetAlerts(tbSearch.Text.Trim(),
                ddlSeverity.SelectedValue, ddlAlertStatus.SelectedValue,
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            litOpen.Text = AdminService.GetAlertCountByStatus("Open").ToString();
            litHigh.Text = AdminService.GetAlertCountBySeverity("High").ToString();
            litInvestigating.Text = AdminService.GetAlertCountByStatus("Investigating").ToString();
            litResolved.Text = AdminService.GetResolvedTodayCount().ToString();

            rptAlerts.DataSource = alerts;
            rptAlerts.DataBind();
            pnlEmpty.Visible = alerts.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadAlerts();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadAlerts();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadAlerts();
        }

        protected void rptAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            string adminId = Session["UserID"].ToString();
            switch (e.CommandName)
            {
                case "Investigate":
                    AdminService.SetAlertStatus(id, "Investigating", adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Alert marked as under investigation.";
                    LoadAlerts();
                    break;
                case "Resolve":
                    AdminService.SetAlertStatus(id, "Resolved", adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Alert resolved successfully.";
                    LoadAlerts();
                    break;
                case "BlockUser":
                    DataTable alert = AdminService.GetAlertById(id);
                    if (alert.Rows.Count > 0)
                    {
                        string affectedUserId = alert.Rows[0]["AffectedUserID"].ToString();
                        if (affectedUserId == Session["UserID"].ToString())
                        {
                            ClientScript.RegisterStartupScript(GetType(), "alert",
                                "alert('You cannot block your own account.');", true);
                            break;
                        }
                        UserService.ToggleActive(affectedUserId);
                        AdminService.SetAlertStatus(id, "Resolved", adminId);
                        pnlSuccess.Visible = true;
                        litSuccess.Text = "User blocked and alert resolved.";
                    }
                    LoadAlerts();
                    break;
            }
        }

        public string GetSeverityBadge(string s) =>
            s == "High" ? "badge-red" : s == "Medium" ? "badge-amber" : "badge-blue";

        public string GetAlertStatusBadge(string s) =>
            s == "Resolved" ? "badge-green" : s == "Investigating" ? "badge-amber" : "badge-red";

        protected void lbExport_Click(object sender, EventArgs e)
        {
            string csv = AdminService.ExportAlertsCsv(tbSearch.Text.Trim(),
                ddlSeverity.SelectedValue, ddlAlertStatus.SelectedValue);
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=security-alerts.csv");
            Response.Write(csv);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
