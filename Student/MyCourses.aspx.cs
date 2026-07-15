using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Student
{
    public partial class Student_MyCourses : Page
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

        private string SelectedCourseId
        {
            get
            {
                return Convert.ToString(
                    ViewState["SelectedCourseId"]);
            }
            set
            {
                ViewState["SelectedCourseId"] =
                    value;
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
                LoadCoursePage();
            }
        }

        private void LoadCoursePage()
        {
            LoadEnrolledCourses();
            LoadAvailableCourses();

            pnlCoursePage.Visible = true;
            pnlCourseDetails.Visible = false;
        }

        private DataTable GetCourses(
            bool enrolled)
        {
            DataTable dt =
                new DataTable();

            string sql = enrolled
                ? @"SELECT
                        c.CourseID,
                        c.CourseName,
                        c.Description,
                        c.Level,
                        c.DurationHours,
                        u.FullName AS InstructorName,
                        e.Progress,
                        e.Status,
                        (
                            SELECT COUNT(*)
                            FROM Chapters ch
                            WHERE ch.CourseID = c.CourseID
                              AND ch.IsPublished = 1
                        ) AS ChapterCount,
                        (
                            SELECT COUNT(*)
                            FROM VirtualLabs vl
                            WHERE vl.CourseID = c.CourseID
                              AND vl.IsPublished = 1
                        ) AS LabCount
                    FROM Enrollments e
                    INNER JOIN Courses c
                        ON c.CourseID = e.CourseID
                    INNER JOIN Users u
                        ON u.UserID = c.InstructorID
                    WHERE e.StudentID = @StudentID
                    ORDER BY e.EnrolledAt DESC"

                : @"SELECT
                        c.CourseID,
                        c.CourseName,
                        c.Description,
                        c.Level,
                        c.DurationHours,
                        u.FullName AS InstructorName,
                        CAST(0 AS DECIMAL(5,2)) AS Progress,
                        'Not Started' AS Status,
                        (
                            SELECT COUNT(*)
                            FROM Chapters ch
                            WHERE ch.CourseID = c.CourseID
                              AND ch.IsPublished = 1
                        ) AS ChapterCount,
                        (
                            SELECT COUNT(*)
                            FROM VirtualLabs vl
                            WHERE vl.CourseID = c.CourseID
                              AND vl.IsPublished = 1
                        ) AS LabCount
                    FROM Courses c
                    INNER JOIN Users u
                        ON u.UserID = c.InstructorID
                    WHERE c.IsPublished = 1
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM Enrollments e
                          WHERE e.CourseID = c.CourseID
                            AND e.StudentID = @StudentID
                      )
                    ORDER BY c.CreatedAt DESC";

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
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

            AddDisplayColumns(dt);

            return dt;
        }

        private static void AddDisplayColumns(
            DataTable dt)
        {
            dt.Columns.Add(
                "LevelClass",
                typeof(string));

            dt.Columns.Add(
                "StatusClass",
                typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                string level =
                    Convert.ToString(
                        row["Level"]);

                row["LevelClass"] =
                    level == "Advanced"
                        ? "badge-red"
                        : level == "Intermediate"
                            ? "badge-amber"
                            : "badge-green";

                string status =
                    Convert.ToString(
                        row["Status"]);

                row["StatusClass"] =
                    status == "Completed"
                        ? "badge-green"
                        : status == "In Progress"
                            ? "badge-blue"
                            : "badge-amber";
            }
        }

        private void LoadEnrolledCourses()
        {
            DataTable dt =
                GetCourses(true);

            rptCourses.DataSource = dt;
            rptCourses.DataBind();

            pnlNoEnrolled.Visible =
                dt.Rows.Count == 0;
        }

        private void LoadAvailableCourses()
        {
            DataTable dt =
                GetCourses(false);

            rptAvailable.DataSource = dt;
            rptAvailable.DataBind();

            pnlNoAvailable.Visible =
                dt.Rows.Count == 0;
        }

        protected void rptAvailable_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Enroll")
            {
                return;
            }

            string courseId =
                Convert.ToString(
                    e.CommandArgument);

            if (string.IsNullOrWhiteSpace(courseId))
            {
                return;
            }

            EnrollStudent(courseId);

            litMessage.Text =
                "Course enrolled successfully.";

            pnlMessage.Visible = true;

            LoadEnrolledCourses();
            LoadAvailableCourses();
        }

        private void EnrollStudent(
            string courseId)
        {
            string enrollmentId =
                IdGenerator.NewId("ENR");

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"IF NOT EXISTS
                      (
                          SELECT 1
                          FROM Enrollments
                          WHERE StudentID = @StudentID
                            AND CourseID = @CourseID
                      )
                      BEGIN
                          INSERT INTO Enrollments
                          (
                              EnrollmentID,
                              StudentID,
                              CourseID,
                              EnrolledAt,
                              Progress,
                              Status,
                              CompletedAt
                          )
                          VALUES
                          (
                              @EnrollmentID,
                              @StudentID,
                              @CourseID,
                              GETDATE(),
                              0,
                              'Not Started',
                              NULL
                          )
                      END",
                    con))
                {
                    cmd.Parameters.Add(
                        "@EnrollmentID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = enrollmentId;

                    cmd.Parameters.Add(
                        "@StudentID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = UserId;

                    cmd.Parameters.Add(
                        "@CourseID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = courseId;

                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"INSERT INTO ActivityLog
                      (
                          UserID,
                          Description,
                          ActivityType,
                          CreatedAt
                      )
                      SELECT
                          @StudentID,
                          'Enrolled in course: '
                              + CourseName,
                          'Course Enrolment',
                          GETDATE()
                      FROM Courses
                      WHERE CourseID = @CourseID",
                    con))
                {
                    cmd.Parameters.Add(
                        "@StudentID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = UserId;

                    cmd.Parameters.Add(
                        "@CourseID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = courseId;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void rptCourses_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "OpenCourse")
            {
                return;
            }

            SelectedCourseId =
                Convert.ToString(
                    e.CommandArgument);

            LoadCourseDetails();
        }

        private void LoadCourseDetails()
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      c.CourseName,
                      c.Description,
                      e.Progress
                  FROM Courses c
                  INNER JOIN Enrollments e
                      ON e.CourseID = c.CourseID
                  WHERE c.CourseID = @CourseID
                    AND e.StudentID = @StudentID",
                con))
            {
                cmd.Parameters.Add(
                    "@CourseID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedCourseId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                con.Open();

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    litCourseName.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["CourseName"]));

                    litCourseDescription.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["Description"]));

                    decimal progress =
                        Convert.ToDecimal(
                            reader["Progress"]);

                    litProgress.Text =
                        progress.ToString("0");

                    progressFill.Style["width"] =
                        progress.ToString("0") + "%";
                }
            }

            LoadChapters();

            pnlCoursePage.Visible = false;
            pnlCourseDetails.Visible = true;
        }

        private void LoadChapters()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      ch.ChapterID,
                      ch.ChapterTitle,
                      ch.Content,
                      ch.FilePath,
                      ch.SortOrder,
                      CAST(
                          ISNULL(cp.IsCompleted, 0)
                          AS BIT
                      ) AS IsCompleted
                  FROM Chapters ch
                  LEFT JOIN ChapterProgress cp
                      ON cp.ChapterID = ch.ChapterID
                     AND cp.StudentID = @StudentID
                  WHERE ch.CourseID = @CourseID
                    AND ch.IsPublished = 1
                  ORDER BY ch.SortOrder",
                con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@CourseID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedCourseId;

                da.Fill(dt);
            }

            rptChapters.DataSource = dt;
            rptChapters.DataBind();

            pnlNoChapters.Visible =
                dt.Rows.Count == 0;
        }

        protected void rptChapters_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Complete")
            {
                return;
            }

            string chapterId =
                Convert.ToString(
                    e.CommandArgument);

            MarkChapterComplete(chapterId);
            UpdateCourseProgress();
            LoadCourseDetails();
        }

        private void MarkChapterComplete(
            string chapterId)
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"IF EXISTS
                  (
                      SELECT 1
                      FROM ChapterProgress
                      WHERE StudentID = @StudentID
                        AND ChapterID = @ChapterID
                  )
                  BEGIN
                      UPDATE ChapterProgress
                      SET
                          IsCompleted = 1,
                          CompletedAt = GETDATE()
                      WHERE StudentID = @StudentID
                        AND ChapterID = @ChapterID
                  END
                  ELSE
                  BEGIN
                      DECLARE @ProgressID INT;

                      SELECT @ProgressID =
                          ISNULL(MAX(ProgressID), 0) + 1
                      FROM ChapterProgress;

                      INSERT INTO ChapterProgress
                      (
                          ProgressID,
                          StudentID,
                          ChapterID,
                          IsCompleted,
                          CompletedAt
                      )
                      VALUES
                      (
                          @ProgressID,
                          @StudentID,
                          @ChapterID,
                          1,
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
                    "@ChapterID",
                    SqlDbType.NVarChar,
                    10
                ).Value = chapterId;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateCourseProgress()
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"DECLARE @Total INT;
                  DECLARE @Completed INT;
                  DECLARE @Progress DECIMAL(5,2);

                  SELECT @Total = COUNT(*)
                  FROM Chapters
                  WHERE CourseID = @CourseID
                    AND IsPublished = 1;

                  SELECT @Completed = COUNT(*)
                  FROM ChapterProgress cp
                  INNER JOIN Chapters ch
                      ON ch.ChapterID = cp.ChapterID
                  WHERE cp.StudentID = @StudentID
                    AND ch.CourseID = @CourseID
                    AND ch.IsPublished = 1
                    AND cp.IsCompleted = 1;

                  SET @Progress =
                      CASE
                          WHEN @Total = 0 THEN 0
                          ELSE CAST(
                              @Completed * 100.0 /
                              @Total
                              AS DECIMAL(5,2))
                      END;

                  UPDATE Enrollments
                  SET
                      Progress = @Progress,
                      Status =
                          CASE
                              WHEN @Progress >= 100
                                  THEN 'Completed'
                              WHEN @Progress > 0
                                  THEN 'In Progress'
                              ELSE 'Not Started'
                          END,
                      CompletedAt =
                          CASE
                              WHEN @Progress >= 100
                                  THEN GETDATE()
                              ELSE NULL
                          END
                  WHERE StudentID = @StudentID
                    AND CourseID = @CourseID",
                con))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@CourseID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedCourseId;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsCompleted(
            object value)
        {
            return value != null &&
                   value != DBNull.Value &&
                   Convert.ToBoolean(value);
        }

        public bool ShowCompleteButton(
            object value)
        {
            return !IsCompleted(value);
        }

        public string GetChapterStatus(
            object value)
        {
            return IsCompleted(value)
                ? "Completed"
                : "Not completed";
        }

        public bool HasAttachment(
            object value)
        {
            return value != null &&
                   value != DBNull.Value &&
                   !string.IsNullOrWhiteSpace(
                       Convert.ToString(value));
        }

        protected void btnBack_Click(
            object sender,
            EventArgs e)
        {
            SelectedCourseId = "";

            LoadCoursePage();
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("~/Login.aspx");
        }
    }
}