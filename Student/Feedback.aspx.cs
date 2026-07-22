using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Feedback : Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;
            }
        }

        private string UserId
        {
            get
            {
                return Convert.ToString(
                    Session["UserID"]);
            }
        }

        private string FeedbackType
        {
            get
            {
                return Convert.ToString(
                    Request.QueryString["type"])
                    .Trim()
                    .ToLower();
            }
        }

        private string ItemId
        {
            get
            {
                return Convert.ToString(
                    Request.QueryString["id"])
                    .Trim();
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadFeedbackPage();
            }
        }

        private void LoadFeedbackPage()
        {
            if (ItemId == "" ||
                (FeedbackType != "course" &&
                 FeedbackType != "lab" &&
                 FeedbackType != "quiz"))
            {
                ShowError(
                    "The selected feedback item is invalid.");

                return;
            }

            string itemName;

            if (!CanSubmitFeedback(out itemName))
            {
                string message;

                if (FeedbackType == "course")
                {
                    message =
                        "Course feedback is available after completing the course.";
                }
                else if (FeedbackType == "lab")
                {
                    message =
                        "Lab feedback is available after passing the lab.";
                }
                else
                {
                    message =
                        "Quiz feedback is available after submitting at least one attempt.";
                }

                ShowError(message);
                return;
            }

            litItemName.Text =
                Server.HtmlEncode(itemName);

            if (FeedbackType == "course")
            {
                litItemType.Text =
                    "Overall course feedback";
            }
            else if (FeedbackType == "lab")
            {
                litItemType.Text =
                    "Virtual lab feedback";
            }
            else
            {
                litItemType.Text =
                    "Post-quiz feedback";
            }

            pnlFeedback.Visible = true;

            LoadExistingFeedback();
        }

        private bool CanSubmitFeedback(
            out string itemName)
        {
            itemName = "";

            string sql;

            if (FeedbackType == "course")
            {
                sql = @"
                    SELECT c.CourseName
                    FROM Courses c
                    INNER JOIN Enrollments e
                        ON e.CourseID = c.CourseID
                    WHERE c.CourseID = @ItemID
                      AND e.StudentID = @StudentID
                      AND e.Progress >= 100";
            }
            else if (FeedbackType == "lab")
            {
                sql = @"
                    SELECT vl.LabTitle
                    FROM VirtualLabs vl
                    INNER JOIN Enrollments e
                        ON e.CourseID = vl.CourseID
                       AND e.StudentID = @StudentID
                    WHERE vl.LabID = @ItemID
                      AND EXISTS
                      (
                          SELECT 1
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @StudentID
                            AND ls.Result = 'Passed'
                      )";
            }
            else
            {
                sql = @"
                    SELECT q.Title
                    FROM Quizzes q
                    INNER JOIN Enrollments e
                        ON e.CourseID = q.CourseID
                       AND e.StudentID = @StudentID
                    WHERE q.QuizID = @ItemID
                      AND EXISTS
                      (
                          SELECT 1
                          FROM QuizAttempts qa
                          WHERE qa.QuizID = q.QuizID
                            AND qa.StudentID = @StudentID
                      )";
            }

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
            {
                cmd.Parameters.Add(
                    "@ItemID",
                    SqlDbType.NVarChar,
                    10
                ).Value = ItemId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                con.Open();

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return false;
                }

                itemName =
                    Convert.ToString(result);

                return true;
            }
        }

        private string GetTargetColumn()
        {
            if (FeedbackType == "course")
            {
                return "CourseID";
            }

            if (FeedbackType == "lab")
            {
                return "LabID";
            }

            return "QuizID";
        }

        private void LoadExistingFeedback()
        {
            string column =
                GetTargetColumn();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT TOP 1
                      StarRating,
                      Comment,
                      SubmittedAt
                  FROM Feedback
                  WHERE StudentID = @StudentID
                    AND " + column + @" = @ItemID
                  ORDER BY SubmittedAt DESC",
                con))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@ItemID",
                    SqlDbType.NVarChar,
                    10
                ).Value = ItemId;

                con.Open();

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        pnlForm.Visible = true;
                        pnlSaved.Visible = false;

                        return;
                    }

                    pnlForm.Visible = false;
                    pnlSaved.Visible = true;

                    litRating.Text =
                        Convert.ToString(
                            reader["StarRating"]);

                    string comment =
                        Convert.ToString(
                            reader["Comment"]);

                    pnlComment.Visible =
                        !string.IsNullOrWhiteSpace(
                            comment);

                    litComment.Text =
                        Server.HtmlEncode(comment);

                    litSubmittedAt.Text =
                        Convert.ToDateTime(
                            reader["SubmittedAt"])
                            .ToString(
                                "dd MMM yyyy, hh:mm tt");
                }
            }
        }

        protected void btnSubmit_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("Feedback");

            if (!Page.IsValid)
            {
                return;
            }

            string itemName;

            if (!CanSubmitFeedback(out itemName))
            {
                ShowError(
                    "You are not eligible to submit feedback for this item.");

                return;
            }

            int rating =
                Convert.ToInt32(
                    ddlRating.SelectedValue);

            string comment =
                tbComment.Text.Trim();

            string column =
                GetTargetColumn();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"IF NOT EXISTS
                  (
                      SELECT 1
                      FROM Feedback
                      WHERE StudentID = @StudentID
                        AND " + column + @" = @ItemID
                  )
                  BEGIN
                      INSERT INTO Feedback
                      (
                          StudentID,
                          " + column + @",
                          StarRating,
                          Comment,
                          SubmittedAt
                      )
                      VALUES
                      (
                          @StudentID,
                          @ItemID,
                          @Rating,
                          @Comment,
                          GETDATE()
                      )
                  END",
                con))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@ItemID",
                    SqlDbType.NVarChar,
                    10
                ).Value = ItemId;

                cmd.Parameters.Add(
                    "@Rating",
                    SqlDbType.TinyInt
                ).Value = rating;

                cmd.Parameters.Add(
                    "@Comment",
                    SqlDbType.NVarChar,
                    2000
                ).Value =
                    comment == ""
                        ? (object)DBNull.Value
                        : comment;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadExistingFeedback();
        }

        private void ShowError(
            string message)
        {
            pnlFeedback.Visible = false;
            pnlMessage.Visible = true;

            pnlMessage.CssClass =
                "feedback-message feedback-error";

            litMessage.Text =
                Server.HtmlEncode(message);
        }

        protected void btnBack_Click(
            object sender,
            EventArgs e)
        {
            if (FeedbackType == "lab")
            {
                Response.Redirect(
                    "~/Student/Labs.aspx");
            }
            else if (FeedbackType == "quiz")
            {
                Response.Redirect(
                    "~/Student/Challenges.aspx");
            }
            else
            {
                Response.Redirect(
                    "~/Student/MyCourses.aspx");
            }
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect(
                "~/Login.aspx?msg=loggedout");
        }
    }
}