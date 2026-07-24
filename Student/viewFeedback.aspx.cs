using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_ViewFeedback : Page
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

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadFeedback();
            }
        }

        private void LoadFeedback()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(@"
                SELECT
                    f.FeedbackID,
                    f.StarRating,
                    f.Comment,
                    f.SubmittedAt,
                    f.RepText,
                    f.RepAt,

                    CASE
                        WHEN f.QuizID IS NOT NULL
                            THEN 'Quiz'
                        WHEN f.LabID IS NOT NULL
                            THEN 'Virtual Lab'
                        WHEN f.ChapterID IS NOT NULL
                            THEN 'Chapter'
                        WHEN f.CourseID IS NOT NULL
                            THEN 'Course'
                        ELSE 'General'
                    END AS FeedbackType,

                    COALESCE
                    (
                        q.Title,
                        vl.LabTitle,
                        ch.ChapterTitle,
                        directCourse.CourseName,
                        relatedCourse.CourseName,
                        'Learning Activity'
                    ) AS ItemName,

                    lecturer.FullName AS LecturerName

                FROM Feedback f

                LEFT JOIN Quizzes q
                    ON q.QuizID = f.QuizID

                LEFT JOIN VirtualLabs vl
                    ON vl.LabID = f.LabID

                LEFT JOIN Chapters ch
                    ON ch.ChapterID = f.ChapterID

                LEFT JOIN Courses directCourse
                    ON directCourse.CourseID = f.CourseID

                LEFT JOIN Courses relatedCourse
                    ON relatedCourse.CourseID =
                       COALESCE
                       (
                           q.CourseID,
                           vl.CourseID,
                           ch.CourseID
                       )

                LEFT JOIN Users lecturer
                    ON lecturer.UserID = f.LecturerID

                WHERE f.StudentID = @StudentID

                ORDER BY f.SubmittedAt DESC",
                        con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                da.Fill(dt);
            }

            rptFeedback.DataSource = dt;
            rptFeedback.DataBind();

            int replied = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (!string.IsNullOrWhiteSpace(
                    Convert.ToString(row["RepText"])))
                {
                    replied++;
                }
            }

            litTotal.Text =
                dt.Rows.Count.ToString();

            litReplied.Text =
                replied.ToString();

            litPending.Text =
                (dt.Rows.Count - replied).ToString();

            pnlEmpty.Visible =
                dt.Rows.Count == 0;
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