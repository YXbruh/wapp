using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{

    public partial class ContentReview : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")            //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadContent();
        }

        private void LoadContent()
        {
            // TODO:
            // var list = ContentService.GetPending(ddlType.SelectedValue);
            // litPending.Text  = list.Count.ToString();
            // litApproved.Text = ContentService.GetApprovedTodayCount().ToString();
            // litRejected.Text = ContentService.GetRejectedTodayCount().ToString();
            // rptContent.DataSource = list; rptContent.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void Filter_Changed(object sender, EventArgs e) => LoadContent();

        protected void rptContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "Preview": Response.Redirect($"~/Admin/PreviewContent.aspx?id={id}"); break;
                case "Approve":
                    // TODO: ContentService.Approve(id, (int)Session["UserID"]);
                    pnlSuccess.Visible = true; litSuccess.Text = "Content approved and published.";
                    LoadContent(); break;
                case "Reject":
                    // TODO: ContentService.Reject(id, (int)Session["UserID"]);
                    pnlSuccess.Visible = true; litSuccess.Text = "Content rejected. Instructor notified.";
                    LoadContent(); break;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}