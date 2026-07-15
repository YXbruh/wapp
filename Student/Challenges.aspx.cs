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
    public partial class Student_Challenges : Page
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

        private string SelectedChallengeId
        {
            get
            {
                return Convert.ToString(
                    ViewState["ChallengeID"]);
            }
            set
            {
                ViewState["ChallengeID"] =
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
                LoadChallenges();
            }
        }

        private void LoadChallenges()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      vl.LabID AS ChallengeID,
                      vl.LabTitle AS ChallengeName,
                      c.CourseName,
                      vl.Difficulty,
                      vl.PointsReward AS XPReward,
                      ISNULL(
                          (
                              SELECT TOP 1
                                  ls.Result
                              FROM LabSubmissions ls
                              WHERE ls.LabID = vl.LabID
                                AND ls.StudentID = @UserID
                              ORDER BY ls.SubmittedAt DESC
                          ),
                          ''
                      ) AS LastResult,
                      (
                          SELECT COUNT(*)
                          FROM LabSubmissions ls
                          WHERE ls.LabID = vl.LabID
                            AND ls.StudentID = @UserID
                      ) AS AttemptCount
                  FROM VirtualLabs vl
                  INNER JOIN Courses c
                      ON c.CourseID = vl.CourseID
                  INNER JOIN Enrollments e
                      ON e.CourseID = vl.CourseID
                     AND e.StudentID = @UserID
                  WHERE vl.IsPublished = 1
                  ORDER BY vl.LabTitle",
                con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                da.Fill(dt);
            }

            dt.Columns.Add(
                "StatusClass",
                typeof(string));

            dt.Columns.Add(
                "StatusLabel",
                typeof(string));

            dt.Columns.Add(
                "ActionText",
                typeof(string));

            int completed = 0;
            int xp = 0;

            foreach (DataRow row in dt.Rows)
            {
                string result =
                    Convert.ToString(
                        row["LastResult"]);

                int attempts =
                    Convert.ToInt32(
                        row["AttemptCount"]);

                if (result == "Passed")
                {
                    row["StatusClass"] =
                        "badge-green";

                    row["StatusLabel"] =
                        "Completed";

                    row["ActionText"] =
                        "Review";

                    completed++;

                    xp += Convert.ToInt32(
                        row["XPReward"]);
                }
                else if (attempts > 0)
                {
                    row["StatusClass"] =
                        "badge-blue";

                    row["StatusLabel"] =
                        "In Progress";

                    row["ActionText"] =
                        "Continue";
                }
                else
                {
                    row["StatusClass"] =
                        "badge-amber";

                    row["StatusLabel"] =
                        "Not Started";

                    row["ActionText"] =
                        "Attempt";
                }
            }

            litTotal.Text =
                dt.Rows.Count.ToString();

            litDone.Text =
                completed.ToString();

            litXP.Text =
                xp.ToString();

            rptChallenges.DataSource = dt;
            rptChallenges.DataBind();

            pnlEmpty.Visible =
                dt.Rows.Count == 0;
        }

        protected void rptChallenges_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Open")
            {
                return;
            }

            SelectedChallengeId =
                Convert.ToString(
                    e.CommandArgument);

            LoadWorkspace();
        }

        private void LoadWorkspace()
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      vl.LabTitle,
                      vl.Scenario,
                      vl.HintText,
                      vl.Difficulty,
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
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedChallengeId;

                con.Open();

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    litChallengeTitle.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["LabTitle"]));

                    litScenario.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["Scenario"]));

                    string hint =
                        Convert.ToString(
                            reader["HintText"]);

                    litHint.Text =
                        Server.HtmlEncode(hint);

                    pnlHint.Visible =
                        !string.IsNullOrWhiteSpace(hint);

                    litDifficulty.Text =
                        Server.HtmlEncode(
                            Convert.ToString(
                                reader["Difficulty"]));

                    litReward.Text =
                        Convert.ToString(
                            reader["PointsReward"]);
                }
            }

            LoadAttempts();

            pnlChallengeList.Visible = false;
            pnlWorkspace.Visible = true;
            pnlResult.Visible = false;
            tbCommand.Text = "";
        }

        protected void btnSubmit_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("ChallengeGroup");

            if (!Page.IsValid ||
                string.IsNullOrWhiteSpace(
                    SelectedChallengeId))
            {
                return;
            }

            string expectedCommand;
            string validationType;
            int pointsReward;

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      ExpectedCommand,
                      ValidationType,
                      PointsReward
                  FROM VirtualLabs
                  WHERE LabID = @LabID
                    AND IsPublished = 1",
                con))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedChallengeId;

                con.Open();

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
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
                }
            }

            string submitted =
                tbCommand.Text.Trim();

            bool correct =
                ValidateCommand(
                    submitted,
                    expectedCommand,
                    validationType);

            string submissionId =
                IdGenerator.NewId("SUB");

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

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
                          @Command,
                          @IsCorrect,
                          @Result,
                          @Feedback,
                          @Points,
                          GETDATE()
                      )",
                    con))
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
                    ).Value =
                        SelectedChallengeId;

                    cmd.Parameters.Add(
                        "@StudentID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = UserId;

                    cmd.Parameters.Add(
                        "@Command",
                        SqlDbType.NVarChar,
                        2000
                    ).Value = submitted;

                    cmd.Parameters.Add(
                        "@IsCorrect",
                        SqlDbType.Bit
                    ).Value = correct;

                    cmd.Parameters.Add(
                        "@Result",
                        SqlDbType.NVarChar,
                        20
                    ).Value =
                        correct
                            ? "Passed"
                            : "Incomplete";

                    cmd.Parameters.Add(
                        "@Feedback",
                        SqlDbType.NVarChar,
                        500
                    ).Value =
                        correct
                            ? "Correct command."
                            : "The command did not match the expected answer.";

                    cmd.Parameters.Add(
                        "@Points",
                        SqlDbType.Int
                    ).Value =
                        correct
                            ? pointsReward
                            : 0;

                    cmd.ExecuteNonQuery();
                }

                if (correct &&
                    !PreviouslyPassed(
                        con,
                        submissionId))
                {
                    using (SqlCommand cmd =
                           new SqlCommand(
                        @"UPDATE Users
                          SET TotalPoints =
                              TotalPoints + @Points
                          WHERE UserID = @StudentID",
                        con))
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

                using (SqlCommand cmd =
       new SqlCommand(
    @"DECLARE @ActivityID INT;

      SELECT @ActivityID =
          ISNULL(MAX(ActivityID), 0) + 1
      FROM ActivityLog;

      INSERT INTO ActivityLog
      (
          ActivityID,
          UserID,
          Description,
          ActivityType,
          CreatedAt
      )
      SELECT
          @ActivityID,
          @StudentID,
          @Description + LabTitle,
          'Challenge',
          GETDATE()
      FROM VirtualLabs
      WHERE LabID = @LabID",
    con))
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
                    ).Value = SelectedChallengeId;

                    cmd.Parameters.Add(
                        "@Description",
                        SqlDbType.NVarChar,
                        100
                    ).Value =
                        correct
                            ? "Completed challenge: "
                            : "Attempted challenge: ";

                    cmd.ExecuteNonQuery();
                }
            }

            pnlResult.Visible = true;

            pnlResult.CssClass =
                correct
                    ? "result-success"
                    : "result-error";

            litResult.Text =
                correct
                    ? "Correct. You earned " +
                      pointsReward + " XP."
                    : "Incorrect command. Review the hint and try again.";

            LoadAttempts();
        }

        private bool PreviouslyPassed(
            SqlConnection con,
            string currentSubmissionId)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT COUNT(*)
                  FROM LabSubmissions
                  WHERE LabID = @LabID
                    AND StudentID = @StudentID
                    AND Result = 'Passed'
                    AND SubmissionID <> @SubmissionID",
                con))
            {
                cmd.Parameters.Add(
                    "@LabID",
                    SqlDbType.NVarChar,
                    10
                ).Value = SelectedChallengeId;

                cmd.Parameters.Add(
                    "@StudentID",
                    SqlDbType.NVarChar,
                    10
                ).Value = UserId;

                cmd.Parameters.Add(
                    "@SubmissionID",
                    SqlDbType.NVarChar,
                    10
                ).Value = currentSubmissionId;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool ValidateCommand(
            string submitted,
            string expected,
            string validationType)
        {
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

        private void LoadAttempts()
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
                ).Value =
                    SelectedChallengeId;

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

        protected void btnBack_Click(
            object sender,
            EventArgs e)
        {
            pnlWorkspace.Visible = false;
            pnlChallengeList.Visible = true;

            LoadChallenges();
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