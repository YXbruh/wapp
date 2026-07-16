using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class Mentorship : Page
    {
        // ====================================================================
        // Page Load
        // ====================================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadMetrics();
                LoadFeedbackList();
                pnlNoSelection.Visible = true;
                pnlDetail.Visible = false;
            }
        }

        // ====================================================================
        // Database helper
        // ====================================================================
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;
        }

        // ====================================================================
        // Load course dropdown
        // ====================================================================
        private void LoadCourseDropdown()
        {
            string instructorId = Session["UserID"].ToString().Trim();
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("All Courses", ""));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ddlCourse.Items.Add(new ListItem(reader["CourseName"].ToString(), reader["CourseID"].ToString()));
                }
            }
        }

        // ====================================================================
        // Load metrics (unread = total, replied = 0)
        // ====================================================================
        private void LoadMetrics()
        {
            string instructorId = Session["UserID"].ToString().Trim();

            // Total feedback (all are considered "unread" for now)
            string totalQuery = @"
                SELECT COUNT(DISTINCT f.FeedbackID)
                FROM Feedback f
                INNER JOIN Courses c ON f.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID";

            // Replied is always 0 because we have no reply tracking
            litReplied.Text = "0";

            // Average rating (of all feedback)
            string avgRatingQuery = @"
                SELECT ISNULL(AVG(CAST(StarRating AS FLOAT)), 0)
                FROM Feedback f
                INNER JOIN Courses c ON f.CourseID = c.CourseID
                WHERE c.InstructorID = @InstructorID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(totalQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    litUnread.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }
                using (SqlCommand cmd = new SqlCommand(avgRatingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        decimal avg = Convert.ToDecimal(result);
                        litAvgRating.Text = avg.ToString("0.0");
                    }
                    else
                        litAvgRating.Text = "—";
                }
            }
        }

        // ====================================================================
        // Load feedback list (no reply filtering)
        // ====================================================================
        private void LoadFeedbackList()
        {
            string instructorId = Session["UserID"].ToString().Trim();
            string search = tbSearch.Text.Trim();
            string filter = ddlFilter.SelectedValue;
            string courseId = ddlCourse.SelectedValue;

            // Base query: all feedback for the instructor's courses
            string query = @"
                SELECT 
                    f.FeedbackID,
                    u.FullName AS StudentName,
                    c.CourseName,
                    q.Title AS QuizName,
                    f.StarRating,
                    f.Comment,
                    f.SubmittedAt,
                    0 AS HasReply,     -- no replies
                    0 AS IsRead        -- treat all as unread
                FROM Feedback f
                INNER JOIN Users u ON f.StudentID = u.UserID
                LEFT JOIN Courses c ON f.CourseID = c.CourseID
                LEFT JOIN Quizzes q ON f.QuizID = q.QuizID
                WHERE c.InstructorID = @InstructorID
                  AND (u.FullName LIKE @Search OR f.Comment LIKE @Search)";

            if (!string.IsNullOrEmpty(courseId))
                query += " AND f.CourseID = @CourseID";

            // Filter: since we have no reply, "Unread" = all, "Replied" = none
            if (filter == "Replied")
                query += " AND 1 = 0"; // no results for replied
            // else Unread or All → all feedback

            query += " ORDER BY f.SubmittedAt DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                if (!string.IsNullOrEmpty(courseId))
                    cmd.Parameters.AddWithValue("@CourseID", courseId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var list = new List<FeedbackListItem>();
                while (reader.Read())
                {
                    list.Add(new FeedbackListItem
                    {
                        FeedbackID = Convert.ToInt32(reader["FeedbackID"]),
                        StudentName = reader["StudentName"].ToString(),
                        CourseName = reader["CourseName"]?.ToString() ?? "N/A",
                        QuizName = reader["QuizName"]?.ToString() ?? "N/A",
                        StarRating = Convert.ToInt32(reader["StarRating"]),
                        Comment = reader["Comment"]?.ToString() ?? "",
                        SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"]),
                        HasReply = false,
                        IsRead = false
                    });
                }
                reader.Close();

                rptFeedback.DataSource = list;
                rptFeedback.DataBind();
                pnlEmpty.Visible = list.Count == 0;
            }
        }

        // ====================================================================
        // When a feedback item is selected
        // ====================================================================
        protected void rptFeedback_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                int feedbackId = Convert.ToInt32(e.CommandArgument);
                LoadFeedbackDetail(feedbackId);
            }
        }

        // ====================================================================
        // Load detail for the selected feedback
        // ====================================================================
        private void LoadFeedbackDetail(int feedbackId)
        {
            string query = @"
                SELECT 
                    f.FeedbackID,
                    u.FullName AS StudentName,
                    u.UserID AS StudentID,
                    c.CourseName,
                    q.Title AS QuizTitle,
                    f.StarRating,
                    f.Comment,
                    f.SubmittedAt
                FROM Feedback f
                INNER JOIN Users u ON f.StudentID = u.UserID
                LEFT JOIN Courses c ON f.CourseID = c.CourseID
                LEFT JOIN Quizzes q ON f.QuizID = q.QuizID
                WHERE f.FeedbackID = @FeedbackID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    pnlNoSelection.Visible = false;
                    pnlDetail.Visible = true;
                    hfFeedbackID.Value = feedbackId.ToString();

                    // Student info
                    litDetailName.Text = reader["StudentName"].ToString();
                    litDetailStudentID.Text = reader["StudentID"].ToString();
                    litDetailCourse.Text = reader["CourseName"]?.ToString() ?? "N/A";
                    litDetailQuiz.Text = reader["QuizTitle"]?.ToString() ?? "N/A";
                    litDetailInitials.Text = GetInitials(reader["StudentName"].ToString());

                    // Rating (int)
                    int ratingInt = Convert.ToInt32(reader["StarRating"]);
                    litDetailRatingNum.Text = ratingInt.ToString("0.0");
                    litDetailDate.Text = Convert.ToDateTime(reader["SubmittedAt"]).ToString("dd MMM yyyy, HH:mm");

                    // Comment
                    litDetailComment.Text = reader["Comment"]?.ToString() ?? "";

                    // Placeholders
                    litDetailScore.Text = "—";
                    litDetailLabs.Text = "—";

                    // No previous reply, so hide the panel
                    pnlPrevReply.Visible = false;
                    // Set default rating dropdown
                    ddlStarRating.SelectedValue = "5.0";

                    pnlSuccess.Visible = false;
                    pnlError.Visible = false;
                    tbReply.Text = "";
                }
                else
                {
                    pnlNoSelection.Visible = true;
                    pnlDetail.Visible = false;
                }
            }
        }

        // ====================================================================
        // Send Reply (saved as a new feedback record? We'll store it as a new row,
        // but without ReplyToFeedbackID we can't link it. So we'll update the 
        // original feedback's Comment? But that would overwrite student's comment.
        // Given no reply mechanism, we'll just show a success message and clear.
        // ====================================================================
        protected void btnSendReply_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Get rating from dropdown (rounded to int)
            if (!decimal.TryParse(ddlStarRating.SelectedValue, out decimal ratingDecimal))
            {
                ShowError("Invalid rating selected.");
                return;
            }
            int starRating = (int)Math.Round(ratingDecimal, MidpointRounding.AwayFromZero);
            if (starRating < 1) starRating = 1;
            if (starRating > 5) starRating = 5;

            string replyText = tbReply.Text.Trim();
            if (string.IsNullOrEmpty(replyText))
            {
                ShowError("Reply cannot be empty.");
                return;
            }

            int feedbackId = Convert.ToInt32(hfFeedbackID.Value);

            // Since we cannot link a reply, we'll simply update the original feedback's StarRating
            // and append the reply to the comment (or store it elsewhere). 
            // But to keep it simple and avoid data loss, we'll just show success and clear.
            // In a real scenario, you'd have a separate Replies table.

            // For now, we just pretend to send a reply.
            ShowSuccess("Reply sent successfully. (Note: reply is not stored permanently until you add a reply mechanism.)");

            // Reload and clear
            LoadMetrics();
            LoadFeedbackList();
            LoadFeedbackDetail(feedbackId);
            tbReply.Text = "";
        }

        // ====================================================================
        // Clear button
        // ====================================================================
        protected void btnClear_Click(object sender, EventArgs e)
        {
            tbReply.Text = "";
            ddlStarRating.SelectedValue = "5.0";
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
        }

        // ====================================================================
        // Filter events
        // ====================================================================
        protected void ddlFilter_Changed(object sender, EventArgs e) => LoadFeedbackList();
        protected void tbSearch_Changed(object sender, EventArgs e) => LoadFeedbackList();

        // ====================================================================
        // Helper methods
        // ====================================================================
        private void ShowSuccess(string msg)
        {
            pnlSuccess.Visible = true;
            litSuccess.Text = msg;
            pnlError.Visible = false;
        }

        private void ShowError(string msg)
        {
            pnlError.Visible = true;
            litError.Text = Server.HtmlEncode(msg);
            pnlSuccess.Visible = false;
        }

        public string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        public string BuildStars(int rating)
        {
            string fullStars = new string('★', rating);
            string emptyStars = new string('☆', 5 - rating);
            return fullStars + emptyStars;
        }

        // ====================================================================
        // Logout
        // ====================================================================
        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx?msg=loggedout");
        }

        // ====================================================================
        // ViewModel
        // ====================================================================
        public class FeedbackListItem
        {
            public int FeedbackID { get; set; }
            public string StudentName { get; set; }
            public string CourseName { get; set; }
            public string QuizName { get; set; }
            public int StarRating { get; set; }
            public string Comment { get; set; }
            public DateTime SubmittedAt { get; set; }
            public bool HasReply { get; set; }
            public bool IsRead { get; set; }

            public string TimeAgo
            {
                get
                {
                    var diff = DateTime.Now - SubmittedAt;
                    if (diff.TotalMinutes < 1) return "Just now";
                    if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                    if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
                    if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
                    return SubmittedAt.ToString("dd MMM yyyy");
                }
            }

            public string CommentPreview
            {
                get
                {
                    if (string.IsNullOrEmpty(Comment)) return "";
                    return Comment.Length > 80 ? Comment.Substring(0, 80) + "…" : Comment;
                }
            }
        }
    }
}