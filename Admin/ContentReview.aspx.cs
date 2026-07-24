using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class ContentReview : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadContent();
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
                        _pager = new Pager { PageSize = 10 };
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

        private void LoadContent()
        {
            var p = PagerState;
            DataTable list = AdminService.GetPendingContent(ddlType.SelectedValue,
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            // A page that has emptied out (last item approved) should fall back a page
            // rather than showing "Page 3 of 2" with nothing on it.
            if (list.Rows.Count == 0 && p.Page > 1 && total > 0)
            {
                p.Page = Math.Max(1, p.TotalPages);
                list = AdminService.GetPendingContent(ddlType.SelectedValue,
                    p.Page, p.PageSize, out total);
                p.Total = total;
                ViewState["Pager"] = _pager;
            }

            litPending.Text = AdminService.GetPendingCount().ToString();
            litPublished.Text = AdminService.GetPublishedCount().ToString();
            litReviewedToday.Text = AdminService.GetReviewedTodayCount().ToString();

            rptContent.DataSource = list;
            rptContent.DataBind();
            pnlEmpty.Visible = list.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadContent();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadContent();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadContent();
        }

        protected void rptContent_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // Content keys are NVARCHAR ("QUZ001"), and the same key can exist in more
            // than one table, so the type travels with it as "Type|ID".
            string[] parts = Convert.ToString(e.CommandArgument).Split('|');
            if (parts.Length != 2) return;
            string type = parts[0], id = parts[1];

            string adminId = Session["UserID"].ToString();
            switch (e.CommandName)
            {
                case "Preview":
                    Response.Redirect("~/Admin/PreviewContent.aspx?type=" +
                        Server.UrlEncode(type) + "&id=" + Server.UrlEncode(id));
                    break;
                case "Approve":
                    try
                    {
                        if (AdminService.ApproveContent(type, id, adminId))
                        {
                            pnlSuccess.Visible = true;
                            litSuccess.Text = type + " published — it is now visible to enrolled students.";
                        }
                        else
                        {
                            pnlError.Visible = true;
                            litError.Text = "That content no longer exists.";
                        }
                        LoadContent();
                    }
                    catch (Exception ex)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Error publishing content: " + Server.HtmlEncode(ex.Message);
                    }
                    break;
                case "Reject":
                    try
                    {
                        if (AdminService.RejectContent(type, id, adminId))
                        {
                            pnlSuccess.Visible = true;
                            litSuccess.Text = type + " rejected — it stays a draft with its author.";
                        }
                        else
                        {
                            pnlError.Visible = true;
                            litError.Text = "That content no longer exists.";
                        }
                        LoadContent();
                    }
                    catch (Exception ex)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Error rejecting content: " + Server.HtmlEncode(ex.Message);
                    }
                    break;
            }
        }

        protected void btnSendRevision_Click(object sender, EventArgs e)
        {
            string[] parts = Convert.ToString(hfRevisionRef.Value).Split('|');
            string message = tbRevisionMessage.Text.Trim();
            if (parts.Length != 2 || string.IsNullOrEmpty(message))
            {
                pnlError.Visible = true;
                litError.Text = "Please pick an item and describe the changes the lecturer needs to make.";
                LoadContent();
                return;
            }

            string type = parts[0], id = parts[1];
            string adminId = Session["UserID"].ToString();
            try
            {
                if (AdminService.RequestRevision(type, id, adminId, message, out bool emailSent))
                {
                    pnlSuccess.Visible = true;
                    litSuccess.Text = emailSent
                        ? type + " sent back for revision — the lecturer has been emailed your requested changes."
                        : type + " sent back for revision. The email notification could not be sent (SMTP not configured or unavailable); please contact the lecturer directly.";
                }
                else
                {
                    pnlError.Visible = true;
                    litError.Text = "That content no longer exists.";
                }
                LoadContent();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error requesting revision: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
