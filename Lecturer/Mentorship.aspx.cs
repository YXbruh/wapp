using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Lecturer
{
    public partial class Mentorship : Page
    {
        // ========================================================================
        // Page Load
        // ========================================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadFeedbackList();

                if (int.TryParse(Request.QueryString["studentId"], out int sid))
                    PreSelectStudent(sid);
            }
        }

        // ========================================================================
        // Helpers
        // ========================================================================
        private string GetInstructorId() => Session["UserID"]?.ToString().Trim() ?? "";
        private string GetConnectionString() => ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

        private string GenerateId(string prefix)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomPart = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            return prefix + randomPart;
        }

        // ========================================================================
        // Load Course Dropdown
        // ========================================================================
        private void LoadCourseDropdown()
        {
            string userId = GetInstructorId();
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("All Courses", ""));

            string query = "SELECT CourseID, CourseName FROM Courses WHERE InstructorID = @InstructorID AND IsPublished = 1 ORDER BY CourseName";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", userId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ddlCourse.Items.Add(new ListItem(reader["CourseName"].ToString(), reader["CourseID"].ToString()));
                }
            }
        }

        // ========================================================================
        // Load Feedback List from LabSubmissions
        // ========================================================================
        private void LoadFeedbackList()
        {
            string userId = GetInstructorId();
            string search = tbSearch.Text.Trim();
            string filter = ddlFilter.SelectedValue;
            string courseId = ddlCourse.SelectedValue;

            string whereClause = @"
                WHERE vl.IsPublished = 1
                  AND c.InstructorID = @InstructorID
                  AND c.IsPublished = 1
                  AND ls.Feedback IS NOT NULL
                  AND ls.Feedback != ''";

            if (!string.IsNullOrEmpty(courseId))
                whereClause += " AND c.CourseID = @CourseID";
            if (!string.IsNullOrEmpty(search))
                whereClause += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search OR ls.Feedback LIKE @Search)";

            if (filter == "Unread")
                whereClause += " AND NOT EXISTS (SELECT 1 FROM Feedback f WHERE f.StudentID = ls.StudentID AND f.LabID = ls.LabID)";
            else if (filter == "Replied")
                whereClause += " AND EXISTS (SELECT 1 FROM Feedback f WHERE f.StudentID = ls.StudentID AND f.LabID = ls.LabID)";

            string query = $@"
                SELECT 
                    ls.SubmissionID AS FeedbackID,
                    u.FullName AS StudentName,
                    c.CourseName,
                    vl.LabTitle AS LabName,
                    ls.Feedback AS Comment,
                    ls.SubmittedAt AS SubmittedAt,
                    CASE WHEN f.FeedbackID IS NOT NULL THEN 1 ELSE 0 END AS HasReply,
                    f.Comment AS ReplyText,
                    f.SubmittedAt AS RepliedAt
                FROM LabSubmissions ls
                INNER JOIN VirtualLabs vl ON ls.LabID = vl.LabID
                INNER JOIN Courses c ON vl.CourseID = c.CourseID
                INNER JOIN Users u ON ls.StudentID = u.UserID
                LEFT JOIN Feedback f ON f.StudentID = ls.StudentID AND f.LabID = ls.LabID
                {whereClause}
                ORDER BY 
                    CASE WHEN f.FeedbackID IS NULL THEN 0 ELSE 1 END,
                    ls.SubmittedAt DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@InstructorID", userId);
                if (!string.IsNullOrEmpty(courseId))
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var feedbackList = new List<FeedbackListItem>();
                while (reader.Read())
                {
                    bool hasReply = Convert.ToBoolean(reader["HasReply"]);
                    string comment = reader["Comment"].ToString();
                    DateTime submitted = Convert.ToDateTime(reader["SubmittedAt"]);
                    feedbackList.Add(new FeedbackListItem
                    {
                        FeedbackID = reader["FeedbackID"].ToString(),
                        StudentName = reader["StudentName"].ToString(),
                        CourseName = reader["CourseName"].ToString(),
                        QuizName = reader["LabName"].ToString(),
                        Comment = comment,
                        SubmittedAt = submitted,
                        HasReply = hasReply,
                        ReplyText = reader["ReplyText"]?.ToString(),
                        RepliedAt = reader["RepliedAt"] != DBNull.Value ? (DateTime?)reader["RepliedAt"] : null,
                        Initials = GetInitials(reader["StudentName"].ToString()),
                        RelativeTime = GetRelativeTime(submitted),
                        // Computed for data-binding
                        IsRead = hasReply,   // read if replied
                        TimeAgo = GetRelativeTime(submitted),
                        CommentPreview = comment.Length > 100 ? comment.Substring(0, 100) + "..." : comment,
                        StarRating = 0
                    });
                }
                reader.Close();

                rptFeedback.DataSource = feedbackList;
                rptFeedback.DataBind();
                pnlEmpty.Visible = feedbackList.Count == 0;

                int unreadCount = feedbackList.Count(f => !f.HasReply);
                litUnread.Text = unreadCount.ToString();

                int repliedThisMonth = feedbackList.Count(f => f.HasReply && f.RepliedAt.HasValue &&
                    f.RepliedAt.Value.Month == DateTime.Now.Month &&
                    f.RepliedAt.Value.Year == DateTime.Now.Year);
                litReplied.Text = repliedThisMonth.ToString();

                litAvgRating.Text = "—";
            }
        }

        // ========================================================================
        // Open Feedback
        // ========================================================================
        private void OpenFeedback(string feedbackId)
        {
            string query = @"
                SELECT 
                    ls.SubmissionID AS FeedbackID,
                    u.FullName AS StudentName,
                    u.UserID AS StudentID,
                    c.CourseName,
                    vl.LabTitle AS LabName,
                    ls.Feedback AS Comment,
                    ls.SubmittedAt AS SubmittedAt,
                    f.Comment AS ReplyText,
                    f.SubmittedAt AS RepliedAt,
                    f.FeedbackID AS ReplyFeedbackID
                FROM LabSubmissions ls
                INNER JOIN VirtualLabs vl ON ls.LabID = vl.LabID
                INNER JOIN Courses c ON vl.CourseID = c.CourseID
                INNER JOIN Users u ON ls.StudentID = u.UserID
                LEFT JOIN Feedback f ON f.StudentID = ls.StudentID AND f.LabID = ls.LabID
                WHERE ls.SubmissionID = @SubmissionID";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SubmissionID", feedbackId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    hfFeedbackID.Value = feedbackId;
                    litDetailName.Text = reader["StudentName"].ToString();
                    litDetailInitials.Text = GetInitials(reader["StudentName"].ToString());
                    litDetailCourse.Text = reader["CourseName"].ToString();
                    litDetailQuiz.Text = reader["LabName"].ToString();
                    litDetailStudentID.Text = reader["StudentID"].ToString();
                    litDetailStars.Text = "—";
                    litDetailRatingNum.Text = "—";
                    litDetailDate.Text = Convert.ToDateTime(reader["SubmittedAt"]).ToString("dd MMM yyyy, HH:mm");
                    litDetailComment.Text = Server.HtmlEncode(reader["Comment"].ToString());
                    litDetailScore.Text = "—";
                    litDetailLabs.Text = "—";

                    string reply = reader["ReplyText"]?.ToString();
                    if (!string.IsNullOrEmpty(reply))
                    {
                        pnlPrevReply.Visible = true;
                        litPrevReply.Text = Server.HtmlEncode(reply);
                        litReplyDate.Text = reader["RepliedAt"] != DBNull.Value
                            ? Convert.ToDateTime(reader["RepliedAt"]).ToString("dd MMM yyyy, HH:mm")
                            : "—";
                    }
                    else
                    {
                        pnlPrevReply.Visible = false;
                    }

                    pnlNoSelection.Visible = false;
                    pnlDetail.Visible = true;
                    pnlSuccess.Visible = false;
                    tbReply.Text = "";
                }
                else
                {
                    pnlDetail.Visible = false;
                    pnlNoSelection.Visible = true;
                }
            }

            LoadFeedbackList();
        }

        // ========================================================================
        // Send Reply – upsert into Feedback
        // ========================================================================
        protected void btnSendReply_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            string submissionId = hfFeedbackID.Value;
            string replyText = tbReply.Text.Trim();

            if (replyText.Length < 10)
            {
                pnlSuccess.Visible = false;
                return;
            }

            string getDataQuery = @"
                SELECT ls.StudentID, ls.LabID, vl.CourseID
                FROM LabSubmissions ls
                INNER JOIN VirtualLabs vl ON ls.LabID = vl.LabID
                WHERE ls.SubmissionID = @SubmissionID";

            string studentId = "", labId = "", courseId = "";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(getDataQuery, conn))
            {
                cmd.Parameters.AddWithValue("@SubmissionID", submissionId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    studentId = reader["StudentID"].ToString();
                    labId = reader["LabID"].ToString();
                    courseId = reader["CourseID"].ToString();
                }
                else
                {
                    pnlSuccess.Visible = false;
                    return;
                }
            }

            // Check existing reply
            string checkQuery = "SELECT FeedbackID FROM Feedback WHERE StudentID = @StudentID AND LabID = @LabID";
            string existingId = null;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                cmd.Parameters.AddWithValue("@LabID", labId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    existingId = result.ToString();
            }

            if (string.IsNullOrEmpty(existingId))
            {
                string newId = GenerateId("FB");
                string insertReply = @"
                    INSERT INTO Feedback (FeedbackID, StudentID, CourseID, LabID, Comment, SubmittedAt, StarRating)
                    VALUES (@FeedbackID, @StudentID, @CourseID, @LabID, @Comment, GETDATE(), 0)";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(insertReply, conn))
                {
                    cmd.Parameters.AddWithValue("@FeedbackID", newId);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.Parameters.AddWithValue("@CourseID", courseId);
                    cmd.Parameters.AddWithValue("@LabID", labId);
                    cmd.Parameters.AddWithValue("@Comment", replyText);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                string updateReply = "UPDATE Feedback SET Comment = @Comment, SubmittedAt = GETDATE() WHERE FeedbackID = @FeedbackID";
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand(updateReply, conn))
                {
                    cmd.Parameters.AddWithValue("@FeedbackID", existingId);
                    cmd.Parameters.AddWithValue("@Comment", replyText);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            pnlSuccess.Visible = true;
            litSuccess.Text = "Reply sent. The student will see it on their dashboard.";
            OpenFeedback(submissionId);
        }

        // ========================================================================
        // Clear reply text
        // ========================================================================
        protected void btnClear_Click(object sender, EventArgs e)
        {
            tbReply.Text = "";
            pnlSuccess.Visible = false;
        }

        // ========================================================================
        // Event Handlers
        // ========================================================================
        protected void rptFeedback_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Open")
                OpenFeedback(e.CommandArgument.ToString());
        }

        protected void tbSearch_Changed(object sender, EventArgs e) => LoadFeedbackList();
        protected void ddlFilter_Changed(object sender, EventArgs e) => LoadFeedbackList();
        protected void ddlCourse_Changed(object sender, EventArgs e) => LoadFeedbackList();

        private void PreSelectStudent(int studentId)
        {
            string query = @"
                SELECT TOP 1 ls.SubmissionID
                FROM LabSubmissions ls
                INNER JOIN VirtualLabs vl ON ls.LabID = vl.LabID
                INNER JOIN Courses c ON vl.CourseID = c.CourseID
                WHERE ls.StudentID = @StudentID
                  AND ls.Feedback IS NOT NULL
                  AND ls.Feedback != ''
                  AND NOT EXISTS (SELECT 1 FROM Feedback f WHERE f.StudentID = ls.StudentID AND f.LabID = ls.LabID)
                  AND c.InstructorID = @InstructorID
                ORDER BY ls.SubmittedAt DESC";

            string instructorId = GetInstructorId();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentId.ToString());
                cmd.Parameters.AddWithValue("@InstructorID", instructorId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    OpenFeedback(result.ToString());
            }
        }

        // ========================================================================
        // Helpers for UI
        // ========================================================================
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
            // If rating is 0, return placeholder
            if (rating == 0) return "—";
            string s = "";
            for (int i = 1; i <= 5; i++)
                s += i <= rating ? "&#9733;" : "&#9734;";
            return s;
        }

        private string GetRelativeTime(DateTime date)
        {
            var diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return date.ToString("MMM dd");
        }

        // ========================================================================
        // Logout
        // ========================================================================
        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx?msg=loggedout");
        }

        // ========================================================================
        // ViewModel
        // ========================================================================
        public class FeedbackListItem
        {
            public string FeedbackID { get; set; }
            public string StudentName { get; set; }
            public string CourseName { get; set; }
            public string QuizName { get; set; }
            public string Comment { get; set; }
            public DateTime SubmittedAt { get; set; }
            public bool HasReply { get; set; }
            public string ReplyText { get; set; }
            public DateTime? RepliedAt { get; set; }
            public string Initials { get; set; }
            public string RelativeTime { get; set; }

            // Data-binding properties
            public bool IsRead { get; set; }
            public string TimeAgo { get; set; }
            public string CommentPreview { get; set; }
            public int StarRating { get; set; }
        }
    }
}