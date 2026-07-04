using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class Mentorship : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Instructor")               //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadFeedbackList();

                // Pre-select if studentId passed from StudentDetail
                if (int.TryParse(Request.QueryString["studentId"], out int sid))
                    PreSelectStudent(sid);
            }
        }

        private void LoadCourseDropdown()
        {
            //int userId = (int)Session["UserID"];                                                      //Bypass login for testing
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("All Courses", ""));
            // TODO: foreach (var c in CourseService.GetByInstructor(userId))
            //           ddlCourse.Items.Add(new ListItem(c.CourseName, c.CourseID.ToString()));
        }

        private void LoadFeedbackList()
        {
            //int userId = (int)Session["UserID"];                                                      //Bypass login for testing
            // TODO:
            // var list = FeedbackService.GetForInstructor(userId,
            //                tbSearch.Text.Trim(),
            //                ddlFilter.SelectedValue,
            //                ddlCourse.SelectedValue);
            // litUnread.Text    = list.Count(f => !f.IsRead).ToString();
            // litReplied.Text   = FeedbackService.GetRepliedThisMonthCount(userId).ToString();
            // litAvgRating.Text = list.Count > 0
            //     ? list.Average(f => f.StarRating).ToString("F1") : "—";
            // rptFeedback.DataSource = list;
            // rptFeedback.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
            pnlNoSelection.Visible = true;
            pnlDetail.Visible = false;
        }

        private void PreSelectStudent(int studentId)
        {
            // TODO: auto-open the first unread feedback for this student
            // var fb = FeedbackService.GetFirstUnreadByStudent(studentId, (int)Session["UserID"]);
            // if (fb != null) OpenFeedback(fb.FeedbackID);
        }

        protected void rptFeedback_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Open")
                OpenFeedback(Convert.ToInt32(e.CommandArgument));
        }

        private void OpenFeedback(int feedbackId)
        {
            //int userId = (int)Session["UserID"];                                          //Bypass login for testing
            hfFeedbackID.Value = feedbackId.ToString();

            // TODO:
            // var fb = FeedbackService.GetById(feedbackId, userId);
            // if (fb == null) return;
            // FeedbackService.MarkRead(feedbackId);

            // litDetailInitials.Text  = GetInitials(fb.StudentName);
            // litDetailName.Text      = fb.StudentName;
            // litDetailCourse.Text    = fb.CourseName;
            // litDetailQuiz.Text      = fb.QuizName;
            // litDetailStudentID.Text = fb.StudentID.ToString();
            // litDetailStars.Text     = BuildStars(fb.StarRating);
            // litDetailRatingNum.Text = fb.StarRating.ToString();
            // litDetailDate.Text      = fb.SubmittedAt.ToString("dd MMM yyyy, HH:mm");
            // litDetailComment.Text   = Server.HtmlEncode(fb.Comment);
            // litDetailScore.Text     = fb.QuizScore + "%";
            // litDetailLabs.Text      = fb.LabsDone + "/" + fb.LabsTotal;

            // pnlPrevReply.Visible = !string.IsNullOrEmpty(fb.LecturerReply);
            // if (pnlPrevReply.Visible)
            // {
            //     litPrevReply.Text  = Server.HtmlEncode(fb.LecturerReply);
            //     litReplyDate.Text  = fb.RepliedAt?.ToString("dd MMM yyyy, HH:mm");
            // }

            // Demo placeholder
            litDetailName.Text = "Student Name";
            litDetailInitials.Text = "SN";
            litDetailCourse.Text = "Network Security";
            litDetailQuiz.Text = "Quiz 1";
            litDetailComment.Text = "The quiz was challenging. I struggled with the subnetting section.";
            litDetailStars.Text = BuildStars(3);
            litDetailRatingNum.Text = "3";
            litDetailDate.Text = DateTime.Now.ToString("dd MMM yyyy");
            litDetailScore.Text = "62%";
            litDetailLabs.Text = "4/8";
            pnlPrevReply.Visible = false;

            pnlNoSelection.Visible = false;
            pnlDetail.Visible = true;
            pnlSuccess.Visible = false;
            tbReply.Text = "";

            LoadFeedbackList();
        }

        protected void btnSendReply_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            int feedbackId = Convert.ToInt32(hfFeedbackID.Value);
            //int userId = (int)Session["UserID"];                                          //Bypass login for testing

            string replyText = tbReply.Text.Trim();
            if (replyText.Length < 10)
            { pnlSuccess.Visible = false; /* show inline error */ return; }

            // TODO: FeedbackService.SendReply(feedbackId, userId, replyText);
            // This pushes the reply to the student's dashboard

            pnlSuccess.Visible = true;
            litSuccess.Text = "Reply sent. The student will see it on their dashboard.";

            // Refresh previous reply section
            litPrevReply.Text = Server.HtmlEncode(replyText);
            litReplyDate.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");
            pnlPrevReply.Visible = true;
            tbReply.Text = "";

            LoadFeedbackList();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            tbReply.Text = "";
            pnlSuccess.Visible = false;
        }

        protected void tbSearch_Changed(object sender, EventArgs e) => LoadFeedbackList();
        protected void ddlFilter_Changed(object sender, EventArgs e) => LoadFeedbackList();

        // ── Helpers called from markup ───────────────────────
        public string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ');
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper()
                : name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }

        public string BuildStars(int rating)
        {
            string s = "";
            for (int i = 1; i <= 5; i++)
                s += i <= rating ? "&#9733;" : "&#9734;";
            return s;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}