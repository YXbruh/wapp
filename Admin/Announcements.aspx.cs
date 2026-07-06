using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{
    public partial class Announcements : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")        //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadAnnouncements();
        }

        private void LoadAnnouncements()
        {
            // TODO:
            // var list = AnnouncementService.Search(tbSearchAnn.Text.Trim());
            // rptAnnouncements.DataSource = list; rptAnnouncements.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int adminId = (int)Session["UserID"];
            int editId = Convert.ToInt32(hfEditID.Value);

            DateTime? expiry = null;
            if (!string.IsNullOrWhiteSpace(tbExpiry.Text))
                expiry = DateTime.Parse(tbExpiry.Text);

            if (editId == 0)
            {
                // TODO: AnnouncementService.Create(tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                //           ddlAudience.SelectedValue, ddlPriority.SelectedValue, expiry, adminId);
                pnlSuccess.Visible = true;
                litSuccess.Text = "Announcement published successfully.";
            }
            else
            {
                // TODO: AnnouncementService.Update(editId, tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                //           ddlAudience.SelectedValue, ddlPriority.SelectedValue, expiry);
                pnlSuccess.Visible = true;
                litSuccess.Text = "Announcement updated.";
                ResetForm();
            }
            LoadAnnouncements();
        }

        protected void rptAnnouncements_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Edit")
            {
                // TODO: var ann = AnnouncementService.GetById(id);
                // tbTitle.Text              = ann.Title;
                // tbMessage.Text            = ann.Message;
                // ddlAudience.SelectedValue = ann.Audience;
                // ddlPriority.SelectedValue = ann.Priority;
                // tbExpiry.Text             = ann.Expiry?.ToString("yyyy-MM-dd") ?? "";
                hfEditID.Value = id.ToString();
                litFormTitle.Text = "Edit Announcement";
                lbCancelEdit.Visible = true;
                btnPublish.Text = "Update Announcement";
            }
            else if (e.CommandName == "Delete")
            {
                // TODO: AnnouncementService.Delete(id);
                pnlSuccess.Visible = true; litSuccess.Text = "Announcement deleted.";
                LoadAnnouncements();
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e) => ResetForm();

        protected void tbSearchAnn_TextChanged(object sender, EventArgs e) => LoadAnnouncements();

        private void ResetForm()
        {
            tbTitle.Text = tbMessage.Text = tbExpiry.Text = "";
            ddlAudience.SelectedIndex = 0;
            ddlPriority.SelectedIndex = 0;
            hfEditID.Value = "0";
            litFormTitle.Text = "New Announcement";
            lbCancelEdit.Visible = false;
            btnPublish.Text = "Publish Announcement";
        }

        public string GetPriorityBadge(string p) =>
            p == "Urgent" ? "badge-red" : p == "Important" ? "badge-amber" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}