using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{

    public partial class SecurityAlerts : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")            //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadAlerts();
        }

        private void LoadAlerts()
        {
            // TODO:
            // var alerts = SecurityService.Search(tbSearch.Text.Trim(),
            //                                     ddlSeverity.SelectedValue,
            //                                     ddlAlertStatus.SelectedValue);
            // litOpen.Text         = alerts.Count(a => a.AlertStatus == "Open").ToString();
            // litHigh.Text         = alerts.Count(a => a.Severity == "High").ToString();
            // litInvestigating.Text= alerts.Count(a => a.AlertStatus == "Investigating").ToString();
            // litResolved.Text     = SecurityService.GetResolvedTodayCount().ToString();
            // rptAlerts.DataSource = alerts; rptAlerts.DataBind();
            // pnlEmpty.Visible = alerts.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void Filter_Changed(object sender, EventArgs e) => LoadAlerts();

        protected void rptAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            int adminId = (int)Session["UserID"];
            switch (e.CommandName)
            {
                case "Investigate":
                    // TODO: SecurityService.SetStatus(id, "Investigating", adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Alert marked as under investigation.";
                    LoadAlerts(); break;
                case "Resolve":
                    // TODO: SecurityService.SetStatus(id, "Resolved", adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Alert resolved successfully.";
                    LoadAlerts(); break;
                case "BlockUser":
                    // TODO: var alert = SecurityService.GetById(id);
                    //       UserService.ToggleActive(alert.AffectedUserID);
                    //       SecurityService.SetStatus(id, "Resolved", adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "User blocked and alert resolved.";
                    LoadAlerts(); break;
            }
        }

        public string GetSeverityBadge(string s) =>
            s == "High" ? "badge-red" : s == "Medium" ? "badge-amber" : "badge-blue";

        public string GetAlertStatusBadge(string s) =>
            s == "Resolved" ? "badge-green" : s == "Investigating" ? "badge-amber" : "badge-red";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}