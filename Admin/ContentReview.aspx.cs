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

        private void LoadContent()
        {
            var p = PagerState;
            DataTable list = AdminService.GetPendingContent(ddlType.SelectedValue,
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            int pending = 0, approved = 0, rejected = 0;
            foreach (DataRow row in list.Rows)
            {
                string status = row["Status"].ToString();
                if (status == "Pending") pending++;
                else if (status == "Removed") approved++;
                else if (status == "Dismissed") rejected++;
            }

            litPending.Text = pending.ToString();
            litApproved.Text = approved.ToString();
            litRejected.Text = rejected.ToString();

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
            int id = Convert.ToInt32(e.CommandArgument);
            string adminId = Session["UserID"].ToString();
            switch (e.CommandName)
            {
                case "Preview":
                    Response.Redirect($"~/Admin/PreviewContent.aspx?id={id}");
                    break;
                case "Approve":
                    AdminService.ApproveContent(id, adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Flag resolved � content removal action recorded.";
                    LoadContent();
                    break;
                case "Reject":
                    AdminService.RejectContent(id, adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Flag dismissed � no action taken.";
                    LoadContent();
                    break;
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
