using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Student
{
    public partial class Student_Labs : Page
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

        private string SelectedLabId
        {
            get
            {
                return Convert.ToString(
                    ViewState["SelectedLabID"]);
            }
            set
            {
                ViewState["SelectedLabID"] =
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
                pnlLabList.Visible = true;
                pnlLabWorkspace.Visible = false;

                LoadLabs();
            }
        }

        private void LoadLabs()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      vl.LabID,
                      vl.LabTitle AS LabName,
                      c.CourseName,
                      vl.TimeLimitMinutes,
                      vl.Difficulty,
                      vl.PointsReward,

                      ISNULL(
                          (
                              SELECT TOP 1
                                  ls.Result
                              FROM LabSubmissions ls
                              WHERE ls.LabID = vl.LabID
                                AND ls.StudentID = @StudentID
                              ORDER BY ls.SubmittedAt DESC
                          ),
                          ''
                      ) AS LastResult,

                      (
                          SELECT COUNT(*)
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @StudentID
                      ) AS AttemptCount

                  FROM VirtualLabs vl

                  INNER JOIN Courses c
                      ON c.CourseID = vl.CourseID

                  INNER JOIN Enrollments e
                      ON e.CourseID = vl.CourseID
                     AND e.StudentID = @StudentID

                  WHERE vl.IsPublished = 1

                  ORDER BY
                      c.CourseName,
                      vl.LabTitle",
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

            dt.Columns.Add(
                "EstimatedMinutes",
                typeof(int));

            dt.Columns.Add(
                "StatusKey",
                typeof(string));

            dt.Columns.Add(
                "StatusBadgeClass",
                typeof(string));

            dt.Columns.Add(
                "StatusLabel",
                typeof(string));

            int completedCount = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["EstimatedMinutes"] =
                    row["TimeLimitMinutes"] == DBNull.Value
                        ? 15
                        : Convert.ToInt32(
                            row["TimeLimitMinutes"]);

                string lastResult =
                    Convert.ToString(
                        row["LastResult"]);

                int attemptCount =
                    Convert.ToInt32(
                        row["AttemptCount"]);

                if (lastResult == "Passed")
                {
                    row["StatusKey"] =
                        "done";

                    row["StatusBadgeClass"] =
                        "badge-green";

                    row["StatusLabel"] =
                        "Completed";

                    completedCount++;
                }
                else if (attemptCount > 0)
                {
                    row["StatusKey"] =
                        "in-progress";

                    row["StatusBadgeClass"] =
                        "badge-blue";

                    row["StatusLabel"] =
                        "In Progress";
                }
                else
                {
                    row["StatusKey"] =
                        "not-started";

                    row["StatusBadgeClass"] =
                        "badge-amber";

                    row["StatusLabel"] =
                        "Not Started";
                }
            }

            litTotal.Text =
                dt.Rows.Count.ToString();

            litDone.Text =
                completedCount.ToString();

            litRemaining.Text =
                (dt.Rows.Count - completedCount)
                    .ToString();

            rptLabs.DataSource = dt;
            rptLabs.DataBind();

            pnlEmpty.Visible =
                dt.Rows.Count == 0;
        }

        protected void rptLabs_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Open")
            {
                return;
            }

            string labId =
                Convert.ToString(
                    e.CommandArgument);

            if (string.IsNullOrWhiteSpace(labId))
            {
                return;
            }

            SelectedLabId = labId;

            LoadLabWorkspace();
        }

        private void LoadLabWorkspace()
        {
            if (string.IsNullOrWhiteSpace(
                SelectedLabId))
            {
                return;
            }

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      vl.LabTitle,
                      vl.Scenario,
                      vl.HintText,
                      vl.Difficulty,
                      vl.TimeLimitMinutes,
                      vl.PointsReward,
                      c.CourseName

                  FROM VirtualLabs vl

                  INNER JOIN Courses c
                      ON c.CourseID = vl.CourseID

                  INNER JOIN Enrollments e
                      ON e.CourseID = vl.CourseID
                     AND e.StudentID = @StudentID

                  WHERE vl.LabID = @LabID
                    AND vl.IsPublished = 1",
                con))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

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
                        pnlLabWorkspace.Visible =
                            false;

                        pnlLabList.Visible =
                            true;

                        return;
                    }

                    litLabTitle.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["LabTitle"]));

                    litCourseName.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["CourseName"]));

                    litScenario.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["Scenario"]));

                    litDifficulty.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["Difficulty"]));

                    litPointsReward.Text =
                        Convert.ToString(
                            reader["PointsReward"]);

                    litTimeLimit.Text =
                        reader["TimeLimitMinutes"] ==
                        DBNull.Value
                            ? "No time limit"
                            : Convert.ToString(
                                reader["TimeLimitMinutes"]) +
                              " minutes";

                    string hint =
                        Convert.ToString(
                            reader["HintText"]);

                    litHint.Text =
                        Server.HtmlEncode(hint);

                    pnlHint.Visible =
                        !string.IsNullOrWhiteSpace(
                            hint);
                }
            }

            hlOpenTerminal.NavigateUrl =
                ResolveUrl("~/StartLab.aspx") +
                "?id=" +
                Server.UrlEncode(
                    SelectedLabId);

            tbFinalCommand.Text = "";

            pnlSubmissionResult.Visible =
                false;

            pnlLabList.Visible =
                false;

            pnlLabWorkspace.Visible =
                true;

            LoadSubmissionHistory();
        }

        protected void btnSubmitLab_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("LabSubmission");

            if (!Page.IsValid ||
                string.IsNullOrWhiteSpace(
                    SelectedLabId))
            {
                return;
            }

            string submittedCommand =
                tbFinalCommand.Text.Trim();

            string expectedCommand;
            string validationType;
            int pointsReward;

            if (!GetValidationDetails(
                out expectedCommand,
                out validationType,
                out pointsReward))
            {
                ShowSubmissionResult(
                    false,
                    "The selected lab is no longer available.");

                return;
            }

            bool isCorrect =
                ValidateCommand(
                    submittedCommand,
                    expectedCommand,
                    validationType);

            bool alreadyPassed =
                HasPreviouslyPassed();

            string submissionId =
                IdGenerator.NewId("SUB");

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                using (SqlTransaction transaction =
                       con.BeginTransaction())
                {
                    try
                    {
                        InsertSubmission(
                            con,
                            transaction,
                            submissionId,
                            submittedCommand,
                            isCorrect,
                            pointsReward);

                        if (isCorrect &&
                            !alreadyPassed)
                        {
                            AwardPoints(
                                con,
                                transaction,
                                pointsReward);
                        }

                        AddActivity(
                            con,
                            transaction,
                            isCorrect);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            if (isCorrect)
            {
                string message =
                    alreadyPassed
                        ? "Correct command. This lab was already completed, so no additional XP was awarded."
                        : "Correct command. Lab completed and " +
                          pointsReward +
                          " XP awarded.";

                ShowSubmissionResult(
                    true,
                    message);
            }
            else
            {
                ShowSubmissionResult(
                    false,
                    "The submitted command did not match the lecturer's validation key. Review the scenario and hint, then try again.");
            }

            LoadSubmissionHistory();
        }

        private bool GetValidationDetails(
            out string expectedCommand,
            out string validationType,
            out int pointsReward)
        {
            expectedCommand = "";
            validationType = "";
            pointsReward = 0;

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      vl.ExpectedCommand,
                      vl.ValidationType,
                      vl.PointsReward

                  FROM VirtualLabs vl

                  INNER JOIN Enrollments e
                      ON e.CourseID = vl.CourseID
                     AND e.StudentID = @StudentID

                  WHERE vl.LabID = @LabID
                    AND vl.IsPublished = 1",
                con))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

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
                        return false;
                    }

                    expectedCommand =
                        Convert.ToString(
                            reader["ExpectedCommand"]);

                    validationType =
                        Convert.ToString(
                            reader["ValidationType"]);

                    pointsReward =
                        Convert.ToInt32(
                            reader["PointsReward"]);

                    return true;
                }
            }
        }

        private bool HasPreviouslyPassed()
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT COUNT(*)
                  FROM LabSubmissions
                  WHERE LabID = @LabID
                    AND StudentID = @StudentID
                    AND Result = 'Passed'",
                con))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                con.Open();

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }

        private void InsertSubmission(
            SqlConnection con,
            SqlTransaction transaction,
            string submissionId,
            string submittedCommand,
            bool isCorrect,
            int pointsReward)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"INSERT INTO LabSubmissions
                  (
                      SubmissionID,
                      LabID,
                      StudentID,
                      CommandSubmitted,
                      IsCorrect,
                      Result,
                      Feedback,
                      PointsEarned,
                      SubmittedAt
                  )
                  VALUES
                  (
                      @SubmissionID,
                      @LabID,
                      @StudentID,
                      @CommandSubmitted,
                      @IsCorrect,
                      @Result,
                      @Feedback,
                      @PointsEarned,
                      GETDATE()
                  )",
                con,
                transaction))
            {
                cmd.Parameters.Add(
                    "@SubmissionID",
                    SqlDbType.NVarChar,
                    10
                ).Value = submissionId;

                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@CommandSubmitted",
                    SqlDbType.NVarChar,
                    2000
                ).Value = submittedCommand;

                cmd.Parameters.Add(
                    "@IsCorrect",
                    SqlDbType.Bit
                ).Value = isCorrect;

                cmd.Parameters.Add(
                    "@Result",
                    SqlDbType.NVarChar,
                    20
                ).Value =
                    isCorrect
                        ? "Passed"
                        : "Incomplete";

                cmd.Parameters.Add(
                    "@Feedback",
                    SqlDbType.NVarChar,
                    500
                ).Value =
                    isCorrect
                        ? "The submitted command satisfied the validation rule."
                        : "The submitted command did not satisfy the validation rule.";

                cmd.Parameters.Add(
                    "@PointsEarned",
                    SqlDbType.Int
                ).Value =
                    isCorrect
                        ? pointsReward
                        : 0;

                cmd.ExecuteNonQuery();
            }
        }

        private void AwardPoints(
            SqlConnection con,
            SqlTransaction transaction,
            int pointsReward)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"UPDATE Users
                  SET TotalPoints =
                      TotalPoints + @Points
                  WHERE UserID = @StudentID",
                con,
                transaction))
            {
                cmd.Parameters.Add(
                    "@Points",
                    SqlDbType.Int
                ).Value = pointsReward;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.ExecuteNonQuery();
            }
        }

        private void AddActivity(
            SqlConnection con,
            SqlTransaction transaction,
            bool isCorrect)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"INSERT INTO ActivityLog
                  (
                      UserID,
                      Description,
                      ActivityType,
                      ReferenceID,
                      CreatedAt
                  )
                  SELECT
                      @StudentID,
                      @Description + LabTitle,
                      'Lab',
                      NULL,
                      GETDATE()
                  FROM VirtualLabs
                  WHERE LabID = @LabID",
                con,
                transaction))
            {
                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

                cmd.Parameters.Add(
                    "@Description",
                    SqlDbType.NVarChar,
                    100
                ).Value =
                    isCorrect
                        ? "Completed lab: "
                        : "Submitted lab attempt: ";

                cmd.ExecuteNonQuery();
            }
        }

        private static bool ValidateCommand(
            string submitted,
            string expected,
            string validationType)
        {
            submitted =
                (submitted ?? "").Trim();

            expected =
                (expected ?? "").Trim();

            if (validationType == "Contains")
            {
                return submitted.IndexOf(
                    expected,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
            }

            if (validationType == "Regex")
            {
                try
                {
                    return Regex.IsMatch(
                        submitted,
                        expected,
                        RegexOptions.IgnoreCase);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            return string.Equals(
                submitted,
                expected,
                StringComparison.OrdinalIgnoreCase);
        }

        private void LoadSubmissionHistory()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      CommandSubmitted,
                      Result,
                      PointsEarned,
                      SubmittedAt
                  FROM LabSubmissions
                  WHERE LabID = @LabID
                    AND StudentID = @StudentID
                  ORDER BY SubmittedAt DESC",
                con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedLabId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                da.Fill(dt);
            }

            gvAttempts.DataSource = dt;
            gvAttempts.DataBind();

            pnlAttempts.Visible =
                dt.Rows.Count > 0;
        }

        private void ShowSubmissionResult(
            bool success,
            string message)
        {
            pnlSubmissionResult.Visible =
                true;

            pnlSubmissionResult.CssClass =
                success
                    ? "result-success"
                    : "result-error";

            litSubmissionResult.Text =
                Server.HtmlEncode(message);
        }

        protected void btnBackToLabs_Click(
            object sender,
            EventArgs e)
        {
            SelectedLabId = "";

            pnlLabWorkspace.Visible =
                false;

            pnlLabList.Visible =
                true;

            LoadLabs();
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