using System;
using System.Data;
using System.Web.UI;
using CSA.DataAccess;
using CSA.Services;

namespace CSA.Admin
{
    public partial class PreviewContent : Page
    {
        private int _flagId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!int.TryParse(Request.QueryString["id"], out _flagId))
            { Response.Redirect("~/Admin/ContentReview.aspx"); return; }

            if (!IsPostBack) LoadFlag();
        }

        private void LoadFlag()
        {
            DataTable dt = DBHelper.ExecuteQuery(
                "SELECT * FROM ContentFlags WHERE FlagID = @ID",
                new System.Data.SqlClient.SqlParameter("@ID", _flagId));

            if (dt.Rows.Count == 0)
            { Response.Redirect("~/Admin/ContentReview.aspx"); return; }

            DataRow row = dt.Rows[0];
            litFlagID.Text = row["FlagID"].ToString();
            litContentType.Text = row["ContentType"].ToString();
            litReason.Text = row["Reason"].ToString();
            litReportedBy.Text = row["ReportedByID"].ToString();

            DataTable detail = AdminService.GetPendingContent("");
            DataRow[] matches = detail.Select("FlagID = " + _flagId);
            if (matches.Length > 0)
            {
                litTitle.Text = matches[0]["Title"].ToString();
                litCourse.Text = matches[0]["CourseName"].ToString();
                litPreview.Text = matches[0]["Preview"].ToString();
            }

            btnApprove.Enabled = row["Status"].ToString() == "Pending";
            btnReject.Enabled = row["Status"].ToString() == "Pending";

            if (row["Status"].ToString() != "Pending")
            {
                pnlError.Visible = true;
                litError.Text = "This flag has already been " + row["Status"].ToString().ToLower() + ".";
            }
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            AdminService.ApproveContent(_flagId, Session["UserID"].ToString());
            Response.Redirect("~/Admin/ContentReview.aspx");
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            AdminService.RejectContent(_flagId, Session["UserID"].ToString());
            Response.Redirect("~/Admin/ContentReview.aspx");
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
