using System;
using System.Data;
using System.Web;
using System.Web.UI;
using CSA.Services;

namespace CSA.Admin
{
    public partial class PreviewContent : Page
    {
        private string _contentType;
        private string _contentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            _contentType = Request.QueryString["type"];
            _contentId = Request.QueryString["id"];

            if (string.IsNullOrWhiteSpace(_contentType) || string.IsNullOrWhiteSpace(_contentId))
            { Response.Redirect("~/Admin/ContentReview.aspx"); return; }

            if (!IsPostBack) LoadContent();
        }

        private void LoadContent()
        {
            DataRow row = AdminService.GetContentDetail(_contentType, _contentId);
            if (row == null)
            { Response.Redirect("~/Admin/ContentReview.aspx"); return; }

            // These land in non-encoding Literals, so encode the author-supplied values here.
            litContentID.Text = Server.HtmlEncode(row["ContentID"].ToString());
            litContentType.Text = Server.HtmlEncode(row["ContentType"].ToString());
            litSubmittedBy.Text = Server.HtmlEncode(row["SubmittedBy"].ToString());
            litTitle.Text = Server.HtmlEncode(row["Title"].ToString());
            litCourse.Text = Server.HtmlEncode(row["CourseName"].ToString());
            litPreview.Text = row["Body"].ToString();   // litPreview uses Mode="Encode"
            litSubmittedAt.Text = row["SubmittedAt"] == DBNull.Value
                ? "—"
                : Convert.ToDateTime(row["SubmittedAt"]).ToString("dd MMM yyyy HH:mm");

            bool isPublished = Convert.ToBoolean(row["IsPublished"]);
            litStatus.Text = isPublished ? "Published" : "Draft — awaiting review";

            // Publishing twice is a no-op, so only offer the action that changes something.
            btnApprove.Enabled = !isPublished;
            btnReject.Enabled = true;

            switch (_contentType)
            {
                case "Chapter": LoadChapterDetail(row); break;
                case "Quiz":    LoadQuizDetail(row);    break;
                case "Lab":     LoadLabDetail(row);     break;
            }

            LoadAttachments();
        }

        private void LoadChapterDetail(DataRow row)
        {
            litBodyLabel.Text = "Chapter Content";
            litChSortOrder.Text = Server.HtmlEncode(Col(row, "SortOrder"));
            pnlChapterMeta.Visible = true;
        }

        private void LoadQuizDetail(DataRow row)
        {
            litBodyLabel.Text = "Description";
            litQQuestionCount.Text = Col(row, "QuestionCount");
            litQTotalMarks.Text = row.Table.Columns.Contains("TotalMarks") && row["TotalMarks"] != DBNull.Value
                ? Col(row, "TotalMarks") : "—";
            litQPassMark.Text = row.Table.Columns.Contains("PassMark") && row["PassMark"] != DBNull.Value
                ? Convert.ToDecimal(row["PassMark"]).ToString("0.#") + "%" : "—";
            litQMaxAttempts.Text = Col(row, "MaxAttempts");
            litQDuration.Text = row.Table.Columns.Contains("DurationMinutes") && row["DurationMinutes"] != DBNull.Value
                ? Col(row, "DurationMinutes") + " min" : "No limit";
            pnlQuizMeta.Visible = true;

            DataTable questions = AdminService.GetQuizQuestionsForReview(_contentId);
            litQuestionsHeading.Text = questions.Rows.Count.ToString();
            rptQuestions.DataSource = questions;
            rptQuestions.DataBind();
            pnlQuestions.Visible = questions.Rows.Count > 0;
        }

        private void LoadLabDetail(DataRow row)
        {
            litBodyLabel.Text = "Scenario";
            litLDifficulty.Text = Server.HtmlEncode(Col(row, "Difficulty"));
            string skill = Col(row, "SkillTag");
            litLSkillTag.Text = string.IsNullOrWhiteSpace(skill) ? "—" : Server.HtmlEncode(skill);
            litLPoints.Text = Col(row, "PointsReward");
            litLTimeLimit.Text = row.Table.Columns.Contains("TimeLimitMinutes") && row["TimeLimitMinutes"] != DBNull.Value
                ? Col(row, "TimeLimitMinutes") + " min" : "No limit";
            litLValidation.Text = Server.HtmlEncode(Col(row, "ValidationType"));
            litLExpected.Text = Server.HtmlEncode(Col(row, "ExpectedCommand"));

            string hint = Col(row, "HintText");
            if (!string.IsNullOrWhiteSpace(hint))
            {
                litLHint.Text = Server.HtmlEncode(hint);
                pnlLabHint.Visible = true;
            }
            pnlLabMeta.Visible = true;
        }

        private void LoadAttachments()
        {
            DataTable attachments;
            switch (_contentType)
            {
                case "Chapter": attachments = AttachmentService.GetByChapter(_contentId); break;
                case "Lab":     attachments = AttachmentService.GetByLab(_contentId);     break;
                case "Quiz":    attachments = AttachmentService.GetByQuiz(_contentId);    break;
                default:        return;
            }

            rptAttachments.DataSource = attachments;
            rptAttachments.DataBind();
            pnlAttachments.Visible = attachments.Rows.Count > 0;
        }

        /// <summary>Reads a column as string, tolerating a missing column or NULL.</summary>
        private static string Col(DataRow row, string name)
        {
            return row.Table.Columns.Contains(name) && row[name] != DBNull.Value
                ? row[name].ToString() : "";
        }

        /// <summary>Builds the encoded, line-separated A–D option list for a question row.</summary>
        public string RenderOptions(object a, object b, object c, object d)
        {
            var opts = new System.Collections.Generic.List<string>();
            AddOption(opts, "A", a);
            AddOption(opts, "B", b);
            AddOption(opts, "C", c);
            AddOption(opts, "D", d);
            return string.Join("<br />", opts);
        }

        private static void AddOption(System.Collections.Generic.List<string> opts, string letter, object value)
        {
            string text = Convert.ToString(value);
            if (!string.IsNullOrWhiteSpace(text))
                opts.Add(letter + ". " + HttpUtility.HtmlEncode(text));
        }

        public string GetAttachmentIcon(object attachmentType)
        {
            switch (Convert.ToString(attachmentType))
            {
                case "Link":  return "ti-link";
                case "Image": return "ti-photo";
                default:      return "ti-file";
            }
        }

        public string GetAttachmentUrl(object attachmentType, object filePath, object linkUrl)
        {
            if (Convert.ToString(attachmentType) == "Link")
                return HttpUtility.HtmlAttributeEncode(Convert.ToString(linkUrl));

            string path = Convert.ToString(filePath);
            return string.IsNullOrWhiteSpace(path)
                ? "#"
                : HttpUtility.HtmlAttributeEncode(ResolveUrl(path));
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                AdminService.ApproveContent(_contentType, _contentId, Session["UserID"].ToString());
                Response.Redirect("~/Admin/ContentReview.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error publishing content: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                AdminService.RejectContent(_contentType, _contentId, Session["UserID"].ToString());
                Response.Redirect("~/Admin/ContentReview.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                litError.Text = "Error rejecting content: " + Server.HtmlEncode(ex.Message);
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
