using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Announcements : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadAnnouncements();
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

        private void LoadAnnouncements()
        {
            var p = PagerState;
            DataTable list = AdminService.SearchAnnouncements(tbSearchAnn.Text.Trim(),
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            rptAnnouncements.DataSource = list;
            rptAnnouncements.DataBind();
            pnlEmpty.Visible = list.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void tbSearchAnn_TextChanged(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadAnnouncements();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadAnnouncements();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadAnnouncements();
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            string adminId = Session["UserID"].ToString();
            int editId = Convert.ToInt32(hfEditID.Value);

            DateTime? expiry = null;
            if (!string.IsNullOrWhiteSpace(tbExpiry.Text))
                expiry = DateTime.Parse(tbExpiry.Text);

            try
            {
                if (editId == 0)
                {
                    AdminService.CreateAnnouncement(
                        tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                        ddlAudience.SelectedValue, ddlPriority.SelectedValue,
                        expiry, adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement published successfully.";
                }
                else
                {
                    AdminService.UpdateAnnouncement(editId,
                        tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                        ddlAudience.SelectedValue, ddlPriority.SelectedValue, expiry);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement updated.";
                    ResetForm();
                }
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error saving announcement: " + ex.Message;
                pnlSuccess.Visible = false;
            }
        }

        protected void rptAnnouncements_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Edit")
            {
                DataTable dt = AdminService.GetAnnouncementById(id);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    tbTitle.Text = row["Title"].ToString();
                    tbMessage.Text = row["Body"].ToString();
                    tbExpiry.Text = row["ExpiresAt"] != DBNull.Value
                        ? Convert.ToDateTime(row["ExpiresAt"]).ToString("yyyy-MM-dd") : "";
                    hfEditID.Value = id.ToString();
                    litFormTitle.Text = "Edit Announcement";
                    lbCancelEdit.Visible = true;
                    btnPublish.Text = "Update Announcement";
                }
            }
            else if (e.CommandName == "Delete")
            {
                try
                {
                    AdminService.DeleteAnnouncement(id);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement deleted.";
                    LoadAnnouncements();
                }
                catch (Exception ex)
                {
                    pnlError.Visible = true;
                    litError.Text = "Error deleting announcement: " + ex.Message;
                    pnlSuccess.Visible = false;
                }
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e) => ResetForm();

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
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
