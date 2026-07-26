using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Lecturer
{
    public partial class Mentorship : Page
    {
        private string CurrentLecturerId => Session["UserID"]?.ToString().Trim() ?? "";

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

                string studentId = Request.QueryString["studentId"];
                if (!string.IsNullOrEmpty(studentId) && StudentDetailService.Owns(CurrentLecturerId, studentId))
                {
                    LoadComposeMode(studentId);
                }
                else
                {
                    pnlNoSelection.Visible = true;
                    pnlDetail.Visible = false;
                }
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
            string lecturerId = Session["UserID"].ToString().Trim();
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("All Courses", ""));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE LecturerID = @LecturerID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ddlCourse.Items.Add(new ListItem(reader["CourseName"].ToString(), reader["CourseID"].ToString()));
                }
            }
        }

        // ====================================================================
        // Load metrics
        // ====================================================================
        private void LoadMetrics()
        {
            string lecturerId = CurrentLecturerId;

            // Feedback the lecturer hasn't opened yet
            string unreadQuery = @"
                SELECT COUNT(DISTINCT f.FeedbackID)
                FROM Feedback f
                LEFT JOIN Courses c ON f.CourseID = c.CourseID
                WHERE (c.LecturerID = @LecturerID OR f.LecturerID = @LecturerID)
                  AND f.InstReadAt IS NULL";

            // Feedback this lecturer has replied to / messages they sent
            string repliedQuery = @"
                SELECT COUNT(DISTINCT f.FeedbackID)
                FROM Feedback f
                LEFT JOIN Courses c ON f.CourseID = c.CourseID
                WHERE (c.LecturerID = @LecturerID OR f.LecturerID = @LecturerID)
                  AND f.RepText IS NOT NULL";

            // Average rating (of actual student reviews only)
            string avgRatingQuery = @"
                SELECT ISNULL(AVG(CAST(StarRating AS FLOAT)), 0)
                FROM Feedback f
                INNER JOIN Courses c ON f.CourseID = c.CourseID
                WHERE c.LecturerID = @LecturerID AND f.StarRating IS NOT NULL";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(unreadQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                    litUnread.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }
                using (SqlCommand cmd = new SqlCommand(repliedQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                    litReplied.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                }
                using (SqlCommand cmd = new SqlCommand(avgRatingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
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
        // Load feedback list (student reviews + lecturer-initiated messages)
        // ====================================================================
        private void LoadFeedbackList()
        {
            string lecturerId = CurrentLecturerId;
            string search = tbSearch.Text.Trim();
            string filter = ddlFilter.SelectedValue;
            string courseId = ddlCourse.SelectedValue;

            // Base query: feedback tied to the lecturer's courses, plus any
            // message this lecturer sent directly (which may have no CourseID).
            string query = @"
                SELECT
                    f.FeedbackID,
                    u.FullName AS StudentName,
                    c.CourseName,
                    q.Title AS QuizName,
                    f.StarRating,
                    f.Comment,
                    f.RepText,
                    f.SubmittedAt,
                    CASE WHEN f.RepText IS NOT NULL THEN 1 ELSE 0 END AS HasReply,
                    CASE WHEN f.InstReadAt IS NOT NULL THEN 1 ELSE 0 END AS IsRead
                FROM Feedback f
                INNER JOIN Users u ON f.StudentID = u.UserID
                LEFT JOIN Courses c ON f.CourseID = c.CourseID
                LEFT JOIN Quizzes q ON f.QuizID = q.QuizID
                WHERE (c.LecturerID = @LecturerID OR f.LecturerID = @LecturerID)
                  AND (u.FullName LIKE @Search OR f.Comment LIKE @Search OR f.RepText LIKE @Search)";

            if (!string.IsNullOrEmpty(courseId))
                query += " AND f.CourseID = @CourseID";

            if (filter == "Replied")
                query += " AND f.RepText IS NOT NULL";
            else if (filter == "Unread")
                query += " AND f.InstReadAt IS NULL";
            // else "All" → no extra filter

            query += " ORDER BY f.SubmittedAt DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                if (!string.IsNullOrEmpty(courseId))
                    cmd.Parameters.AddWithValue("@CourseID", courseId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var list = new List<FeedbackListItem>();
                while (reader.Read())
                {
                    string comment = reader["Comment"]?.ToString() ?? "";
                    string repText = reader["RepText"] == DBNull.Value ? "" : reader["RepText"].ToString();
                    bool hasReply = Convert.ToInt32(reader["HasReply"]) == 1;
                    bool isRead = Convert.ToInt32(reader["IsRead"]) == 1;

                    list.Add(new FeedbackListItem
                    {
                        FeedbackID = Convert.ToInt32(reader["FeedbackID"]),
                        StudentName = reader["StudentName"].ToString(),
                        CourseName = reader["CourseName"]?.ToString() ?? "Direct Message",
                        QuizName = reader["QuizName"]?.ToString() ?? "N/A",
                        StarRating = reader["StarRating"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StarRating"]),
                        // Lecturer-initiated rows have no student Comment - preview the sent message instead.
                        Comment = string.IsNullOrEmpty(comment) ? "You: " + repText : comment,
                        SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"]),
                        HasReply = hasReply,
                        IsRead = isRead
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
                MarkAsRead(feedbackId);
                LoadFeedbackDetail(feedbackId);
                LoadMetrics();
                LoadFeedbackList();
            }
        }

        // ====================================================================
        // Mark a feedback item as opened (first view only - doesn't overwrite
        // an existing InstReadAt).
        // ====================================================================
        private void MarkAsRead(int feedbackId)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Feedback SET InstReadAt = ISNULL(InstReadAt, GETDATE()) WHERE FeedbackID = @FeedbackID;", conn))
            {
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ====================================================================
        // Load detail for the selected feedback (reply mode)
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
                    f.SubmittedAt,
                    f.RepText,
                    f.RepAt
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
                    hfComposeStudentID.Value = "";

                    string studentId = reader["StudentID"].ToString();
                    string studentName = reader["StudentName"].ToString();

                    // Student info
                    litDetailName.Text = studentName;
                    litDetailCourse.Text = reader["CourseName"]?.ToString() ?? "Direct Message";
                    litDetailQuiz.Text = reader["QuizTitle"]?.ToString() ?? "N/A";
                    litDetailInitials.Text = GetInitials(studentName);
                    hrefAnalytics.HRef = ResolveUrl("~/Lecturer/StudentDetail.aspx?id=" + Server.UrlEncode(studentId));

                    // Rating (student's own rating - display only, may be absent for
                    // a lecturer-initiated message with no underlying student review)
                    bool hasRating = reader["StarRating"] != DBNull.Value;
                    pnlRatingBlock.Visible = hasRating;
                    if (hasRating)
                    {
                        int ratingInt = Convert.ToInt32(reader["StarRating"]);
                        ddlStarRating.SelectedValue = ratingInt.ToString("0.0");
                        litDetailRatingNum.Text = ratingInt.ToString("0.0");
                    }
                    litDetailDate.Text = Convert.ToDateTime(reader["SubmittedAt"]).ToString("dd MMM yyyy, HH:mm");

                    // Student comment (absent for lecturer-initiated messages)
                    string comment = reader["Comment"]?.ToString() ?? "";
                    pnlStudentComment.Visible = !string.IsNullOrEmpty(comment);
                    litDetailComment.Text = comment;

                    pnlQuizContext.Visible = true;
                    litDetailScore.Text = "—";
                    litDetailLabs.Text = "—";

                    // Previous reply, if any
                    string repText = reader["RepText"] == DBNull.Value ? "" : reader["RepText"].ToString();
                    pnlPrevReply.Visible = !string.IsNullOrEmpty(repText);
                    if (pnlPrevReply.Visible)
                    {
                        litPrevReply.Text = Server.HtmlEncode(repText);
                        litReplyDate.Text = reader["RepAt"] == DBNull.Value
                            ? "" : Convert.ToDateTime(reader["RepAt"]).ToString("dd MMM yyyy, HH:mm");
                    }

                    litReplyLabel.Text = "Your Response / Remediation Guidance";
                    btnSendReply.Text = "Send Reply";

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
        // Load the "send a new message to this student" compose panel
        // ====================================================================
        private void LoadComposeMode(string studentId)
        {
            DataRow profile = StudentDetailService.GetProfile(studentId, CurrentLecturerId);
            if (profile == null)
            {
                pnlNoSelection.Visible = true;
                pnlDetail.Visible = false;
                return;
            }

            pnlNoSelection.Visible = false;
            pnlDetail.Visible = true;

            hfFeedbackID.Value = "";
            hfComposeStudentID.Value = studentId;

            string fullName = profile["FullName"].ToString();
            litDetailName.Text = fullName;
            litDetailInitials.Text = GetInitials(fullName);
            litDetailCourse.Text = "New message";
            litDetailQuiz.Text = "";
            hrefAnalytics.HRef = ResolveUrl("~/Lecturer/StudentDetail.aspx?id=" + Server.UrlEncode(studentId));

            // None of these apply when composing a brand-new message.
            pnlRatingBlock.Visible = false;
            pnlStudentComment.Visible = false;
            pnlQuizContext.Visible = false;
            pnlPrevReply.Visible = false;

            litReplyLabel.Text = "Your Message";
            btnSendReply.Text = "Send Feedback";

            pnlSuccess.Visible = false;
            pnlError.Visible = false;
            tbReply.Text = "";
        }

        // ====================================================================
        // Send Reply (to existing student feedback) OR Send Feedback
        // (new lecturer-initiated message, compose mode)
        // ====================================================================
        protected void btnSendReply_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string replyText = tbReply.Text.Trim();
            if (string.IsNullOrEmpty(replyText))
            {
                ShowError("Message cannot be empty.");
                return;
            }

            string lecturerId = CurrentLecturerId;
            string feedbackIdRaw = hfFeedbackID.Value;
            string composeStudentId = hfComposeStudentID.Value;

            if (!string.IsNullOrEmpty(feedbackIdRaw))
            {
                // Replying to an existing student-submitted feedback item.
                int feedbackId = Convert.ToInt32(feedbackIdRaw);

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Feedback
                    SET RepText = @RepText, RepAt = GETDATE(), LecturerID = @LecturerID,
                        InstReadAt = ISNULL(InstReadAt, GETDATE())
                    WHERE FeedbackID = @FeedbackID;", conn))
                {
                    cmd.Parameters.AddWithValue("@RepText", replyText);
                    cmd.Parameters.AddWithValue("@LecturerID", lecturerId);
                    cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Reply sent to the student.");
                LoadMetrics();
                LoadFeedbackList();
                LoadFeedbackDetail(feedbackId);
                tbReply.Text = "";
            }
            else if (!string.IsNullOrEmpty(composeStudentId))
            {
                // New lecturer-initiated message to a specific student.
                if (!StudentDetailService.Owns(lecturerId, composeStudentId))
                {
                    ShowError("You can only message students enrolled in your own courses.");
                    return;
                }

                StudentDetailService.SendFeedback(lecturerId, composeStudentId, replyText);

                ShowSuccess("Feedback sent to the student.");
                LoadMetrics();
                LoadFeedbackList();
                LoadComposeMode(composeStudentId);
                tbReply.Text = "";
            }
            else
            {
                ShowError("Nothing selected to reply to.");
            }
        }

        // ====================================================================
        // Clear button
        // ====================================================================
        protected void btnClear_Click(object sender, EventArgs e)
        {
            tbReply.Text = "";
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