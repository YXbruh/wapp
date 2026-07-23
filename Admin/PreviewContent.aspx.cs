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
                @"SELECT cf.*, u.FullName AS ReportedByName,
                         CASE cf.ContentType
                             WHEN 'Chapter' THEN ch.ChapterTitle
                             WHEN 'Quiz' THEN q.Title
                             WHEN 'Lab' THEN vl.LabTitle
                             ELSE 'Unknown'
                         END AS Title,
                         CASE cf.ContentType
                             WHEN 'Chapter' THEN LEFT(ch.Content, 100)
                             WHEN 'Quiz' THEN q.Description
                             WHEN 'Lab' THEN LEFT(vl.Scenario, 100)
                             ELSE ''
                         END AS Preview,
                         c.CourseName
                  FROM ContentFlags cf
                  JOIN Users u ON cf.ReportedByID = u.UserID
                  LEFT JOIN Chapters ch ON cf.ContentType = 'Chapter' AND CAST(cf.ContentID AS NVARCHAR(10)) = ch.ChapterID
                  LEFT JOIN Quizzes q ON cf.ContentType = 'Quiz' AND CAST(cf.ContentID AS NVARCHAR(10)) = q.QuizID
                  LEFT JOIN VirtualLabs vl ON cf.ContentType = 'Lab' AND CAST(cf.ContentID AS NVARCHAR(10)) = vl.LabID
                  LEFT JOIN Courses c ON (ch.CourseID = c.CourseID OR q.CourseID = c.CourseID OR vl.CourseID = c.CourseID)
                  WHERE cf.FlagID = @ID",
                new System.Data.SqlClient.SqlParameter("@ID", _flagId));

            if (dt.Rows.Count == 0)
            { Response.Redirect("~/Admin/ContentReview.aspx"); return; }

            DataRow row = dt.Rows[0];
            litFlagID.Text = row["FlagID"].ToString();
            litContentType.Text = row["ContentType"].ToString();
            litReason.Text = row["Reason"].ToString();
            litReportedBy.Text = row["ReportedByName"].ToString();
            litTitle.Text = row["Title"].ToString();
            litCourse.Text = row["CourseName"].ToString();
            litPreview.Text = row["Preview"].ToString();

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
            try
            {
                AdminService.ApproveContent(_flagId, Session["UserID"].ToString());
                Response.Redirect("~/Admin/ContentReview.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error approving content: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                AdminService.RejectContent(_flagId, Session["UserID"].ToString());
                Response.Redirect("~/Admin/ContentReview.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error dismissing flag: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
