using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Achievements : Page
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

        private string CurrentUserId
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
            if (Session["UserID"] == null)
            {
            Response.Redirect("~/Login.aspx?msg=loggedout");
                return;
            }

            if (!IsPostBack)
            {
                CheckAndAwardAchievements();
                LoadAchievementPage();
            }
        }

        private void LoadAchievementPage()
        {
            LoadStudentStatistics();
            LoadAchievements();
        }

        private void LoadStudentStatistics()
        {
            int totalXP = 0;
            int streakDays = 0;
            int totalBadges = 0;
            int earnedBadges = 0;

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"SELECT
                          ISNULL(TotalPoints, 0),
                          ISNULL(StreakDays, 0)
                      FROM Users
                      WHERE UserID = @UserID",
                    con))
                {
                    AddUserId(cmd);

                    using (SqlDataReader reader =
                           cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalXP =
                                Convert.ToInt32(
                                    reader[0]);

                            streakDays =
                                Convert.ToInt32(
                                    reader[1]);
                        }
                    }
                }

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"SELECT COUNT(*)
                      FROM Achievements",
                    con))
                {
                    totalBadges =
                        Convert.ToInt32(
                            cmd.ExecuteScalar());
                }

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"SELECT COUNT(*)
                      FROM UserAchievements
                      WHERE UserID = @UserID",
                    con))
                {
                    AddUserId(cmd);

                    earnedBadges =
                        Convert.ToInt32(
                            cmd.ExecuteScalar());
                }
            }

            int lockedBadges =
                Math.Max(
                    totalBadges - earnedBadges,
                    0);

            decimal progress = 0;

            if (totalBadges > 0)
            {
                progress =
                    earnedBadges * 100m /
                    totalBadges;
            }

            litTotalXP.Text =
                totalXP.ToString();

            litStreak.Text =
                streakDays.ToString();

            litEarnedCount.Text =
                earnedBadges.ToString();

            litLockedCount.Text =
                lockedBadges.ToString();

            litBadgeProgress.Text =
                progress.ToString("0") + "%";

            badgeProgressFill.Style["width"] =
                progress.ToString("0") + "%";
        }

        private void LoadAchievements()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      a.AchievementID,
                      a.BadgeName,
                      a.Description,
                      a.IconPath,
                      a.PointsGranted,
                      a.TriggerType,
                      ua.EarnedAt,

                      CAST(
                          CASE
                              WHEN ua.UserAchievementID
                                   IS NULL
                                  THEN 0
                              ELSE 1
                          END
                          AS BIT
                      ) AS IsEarned

                  FROM Achievements a

                  LEFT JOIN UserAchievements ua
                      ON ua.AchievementID =
                         a.AchievementID
                     AND ua.UserID = @UserID

                  ORDER BY
                      CASE
                          WHEN ua.UserAchievementID
                               IS NULL
                              THEN 1
                          ELSE 0
                      END,
                      ua.EarnedAt DESC,
                      a.BadgeName",
                    con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                AddUserId(cmd);
                da.Fill(dt);
            }

            dt.Columns.Add(
                "CardClass",
                typeof(string));

            dt.Columns.Add(
                "StatusClass",
                typeof(string));

            dt.Columns.Add(
                "StatusText",
                typeof(string));

            dt.Columns.Add(
                "EarnedDisplay",
                typeof(string));

            dt.Columns.Add(
                "IconClass",
                typeof(string));

            dt.Columns.Add(
                "CornerIcon",
                typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                bool isEarned =
                    Convert.ToBoolean(
                        row["IsEarned"]);

                string badgeName =
                    Convert.ToString(
                        row["BadgeName"]);

                row["CardClass"] =
                    isEarned
                        ? ""
                        : "locked";

                row["StatusClass"] =
                    isEarned
                        ? "badge-green"
                        : "badge-gray";

                row["StatusText"] =
                    isEarned
                        ? "Earned"
                        : "Locked";

                row["CornerIcon"] =
                    isEarned
                        ? "ti-circle-check"
                        : "ti-lock";

                row["IconClass"] =
                    GetBadgeIcon(
                        badgeName);

                if (isEarned &&
                    row["EarnedAt"] != DBNull.Value)
                {
                    row["EarnedDisplay"] =
                        "Earned on " +
                        Convert.ToDateTime(
                            row["EarnedAt"]
                        ).ToString(
                            "dd MMM yyyy");
                }
                else
                {
                    row["EarnedDisplay"] =
                        GetUnlockRequirement(
                            badgeName);
                }
            }

            rptAchievements.DataSource = dt;
            rptAchievements.DataBind();

            pnlEmpty.Visible =
                dt.Rows.Count == 0;
        }

        private void CheckAndAwardAchievements()
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                AwardByBadgeName(
                    con,
                    "First Login",
                    true);

                int enrolmentCount =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM Enrollments
                          WHERE StudentID = @UserID");

                AwardByBadgeName(
                    con,
                    "First Enrollment",
                    enrolmentCount >= 1);

                int passedQuizCount =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM QuizAttempts
                          WHERE StudentID = @UserID
                            AND IsPassed = 1");

                AwardByBadgeName(
                    con,
                    "First Quiz Passed",
                    passedQuizCount >= 1);

                int completedLabs =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(
                              DISTINCT LabID)
                          FROM LabSubmissions
                          WHERE StudentID = @UserID
                            AND Result = 'Passed'");

                AwardByBadgeName(
                    con,
                    "First Lab Completed",
                    completedLabs >= 1);

                AwardByBadgeName(
                    con,
                    "Lab Master",
                    completedLabs >= 10);

                int perfectQuizCount =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM QuizAttempts
                          WHERE StudentID = @UserID
                            AND Score >= 100");

                AwardByBadgeName(
                    con,
                    "Quiz Ace",
                    perfectQuizCount >= 1);

                int completedCourses =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(*)
                          FROM Enrollments
                          WHERE StudentID = @UserID
                            AND
                            (
                                Status = 'Completed'
                                OR Progress >= 100
                            )");

                AwardByBadgeName(
                    con,
                    "Course Graduate",
                    completedCourses >= 1);

                int streakDays =
                    ExecuteScalarInt(
                        con,
                        @"SELECT
                              ISNULL(StreakDays, 0)
                          FROM Users
                          WHERE UserID = @UserID");

                AwardByBadgeName(
                    con,
                    "7-Day Streak",
                    streakDays >= 7);

                AwardByBadgeName(
                    con,
                    "30-Day Streak",
                    streakDays >= 30);

                int scanningLabs =
                    ExecuteCount(
                        con,
                        @"SELECT COUNT(
                              DISTINCT ls.LabID)
                          FROM LabSubmissions ls

                          INNER JOIN VirtualLabs vl
                              ON vl.LabID =
                                 ls.LabID

                          WHERE ls.StudentID =
                                @UserID
                            AND ls.Result =
                                'Passed'
                            AND
                            (
                                vl.SkillTag =
                                    'Network Scanning'

                                OR vl.LabTitle LIKE
                                    '%Nmap%'

                                OR vl.LabTitle LIKE
                                    '%Scan%'
                            )");

                AwardByBadgeName(
                    con,
                    "Network Scanner",
                    scanningLabs >= 1);
            }
        }

        private void AwardByBadgeName(
    SqlConnection con,
    string badgeName,
    bool conditionMet)
        {
            if (!conditionMet)
            {
                return;
            }

            string achievementId;
            int pointsGranted;

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT
              AchievementID,
              ISNULL(PointsGranted, 0) AS PointsGranted
          FROM Achievements
          WHERE BadgeName = @BadgeName",
                con))
            {
                cmd.Parameters.Add(
                    "@BadgeName",
                    SqlDbType.NVarChar,
                    100
                ).Value = badgeName;

                using (SqlDataReader reader =
                       cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    achievementId =
                        Convert.ToString(
                            reader["AchievementID"]);

                    pointsGranted =
                        Convert.ToInt32(
                            reader["PointsGranted"]);
                }
            }

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(*)
          FROM UserAchievements
          WHERE UserID = @UserID
            AND AchievementID = @AchievementID",
                con))
            {
                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    CurrentUserId.Length
                ).Value = CurrentUserId;

                cmd.Parameters.Add(
                    "@AchievementID",
                    SqlDbType.NVarChar,
                    achievementId.Length
                ).Value = achievementId;

                int alreadyEarned =
                    Convert.ToInt32(
                        cmd.ExecuteScalar());

                if (alreadyEarned > 0)
                {
                    return;
                }
            }

            string userAchievementId =
                CreateUserAchievementId(con);

            using (SqlTransaction transaction =
                   con.BeginTransaction())
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO UserAchievements
                  (
                      UserAchievementID,
                      UserID,
                      AchievementID,
                      EarnedAt
                  )
                  VALUES
                  (
                      @UserAchievementID,
                      @UserID,
                      @AchievementID,
                      GETDATE()
                  )",
                        con,
                        transaction))
                    {
                        cmd.Parameters.Add(
                            "@UserAchievementID",
                            SqlDbType.NVarChar,
                            userAchievementId.Length
                        ).Value = userAchievementId;

                        cmd.Parameters.Add(
                            "@UserID",
                            SqlDbType.NVarChar,
                            CurrentUserId.Length
                        ).Value = CurrentUserId;

                        cmd.Parameters.Add(
                            "@AchievementID",
                            SqlDbType.NVarChar,
                            achievementId.Length
                        ).Value = achievementId;

                        cmd.ExecuteNonQuery();
                    }

                    if (pointsGranted > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE Users
                      SET TotalPoints =
                          ISNULL(TotalPoints, 0) + @Points
                      WHERE UserID = @UserID",
                            con,
                            transaction))
                        {
                            cmd.Parameters.Add(
                                "@Points",
                                SqlDbType.Int
                            ).Value = pointsGranted;

                            cmd.Parameters.Add(
                                "@UserID",
                                SqlDbType.NVarChar,
                                CurrentUserId.Length
                            ).Value = CurrentUserId;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private string CreateUserAchievementId(
            SqlConnection con)
        {
            int maximumLength =
                GetColumnMaximumLength(
                    con,
                    "UserAchievements",
                    "UserAchievementID");

            if (maximumLength <= 0)
            {
                maximumLength = 8;
            }

            string prefix =
                maximumLength >= 3
                    ? "UA"
                    : "U";

            int randomLength =
                maximumLength -
                prefix.Length;

            if (randomLength < 1)
            {
                throw new InvalidOperationException(
                    "UserAchievementID column is too short.");
            }

            for (int attempt = 0;
                 attempt < 20;
                 attempt++)
            {
                string randomText =
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(
                            0,
                            randomLength)
                        .ToUpper();

                string id =
                    prefix + randomText;

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"SELECT COUNT(*)
                      FROM UserAchievements
                      WHERE UserAchievementID =
                            @UserAchievementID",
                        con))
                {
                    cmd.Parameters.Add(
                        "@UserAchievementID",
                        SqlDbType.NVarChar,
                        maximumLength
                    ).Value = id;

                    int exists =
                        Convert.ToInt32(
                            cmd.ExecuteScalar());

                    if (exists == 0)
                    {
                        return id;
                    }
                }
            }

            throw new InvalidOperationException(
                "Unable to generate a unique achievement ID.");
        }

        private int GetColumnMaximumLength(
            SqlConnection con,
            string tableName,
            string columnName)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                @"SELECT
                      CHARACTER_MAXIMUM_LENGTH
                  FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_NAME = @TableName
                    AND COLUMN_NAME = @ColumnName",
                    con))
            {
                cmd.Parameters.Add(
                    "@TableName",
                    SqlDbType.NVarChar,
                    128
                ).Value = tableName;

                cmd.Parameters.Add(
                    "@ColumnName",
                    SqlDbType.NVarChar,
                    128
                ).Value = columnName;

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return 0;
                }

                int length =
                    Convert.ToInt32(result);

                return length == -1
                    ? 50
                    : length;
            }
        }

        private int ExecuteCount(
            SqlConnection con,
            string sql)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                       sql,
                       con))
            {
                AddUserId(cmd);

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToInt32(
                    result);
            }
        }

        private int ExecuteScalarInt(
            SqlConnection con,
            string sql)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                       sql,
                       con))
            {
                AddUserId(cmd);

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToInt32(
                    result);
            }
        }

        private void AddUserId(
            SqlCommand cmd)
        {
            if (cmd.Parameters.Contains(
                    "@UserID"))
            {
                return;
            }

            cmd.Parameters.Add(
                "@UserID",
                SqlDbType.NVarChar,
                Math.Max(
                    CurrentUserId.Length,
                    1)
            ).Value = CurrentUserId;
        }

        private static string GetBadgeIcon(
            string badgeName)
        {
            switch (badgeName)
            {
                case "First Login":
                    return "ti-login";

                case "First Enrollment":
                    return "ti-book-2";

                case "First Quiz Passed":
                    return "ti-checkbox";

                case "First Lab Completed":
                    return "ti-terminal-2";

                case "Lab Master":
                    return "ti-device-desktop-analytics";

                case "Quiz Ace":
                    return "ti-brain";

                case "Course Graduate":
                    return "ti-school";

                case "7-Day Streak":
                    return "ti-flame";

                case "30-Day Streak":
                    return "ti-flame";

                case "Network Scanner":
                    return "ti-radar";

                default:
                    return "ti-award";
            }
        }

        private static string GetUnlockRequirement(
            string badgeName)
        {
            switch (badgeName)
            {
                case "First Login":
                    return "Log in to your account.";

                case "First Enrollment":
                    return "Enrol in your first course.";

                case "First Quiz Passed":
                    return "Pass your first quiz.";

                case "First Lab Completed":
                    return "Complete your first lab.";

                case "Lab Master":
                    return "Complete 10 different labs.";

                case "Quiz Ace":
                    return "Score 100% on a quiz.";

                case "Course Graduate":
                    return "Complete an entire course.";

                case "7-Day Streak":
                    return "Reach a 7-day login streak.";

                case "30-Day Streak":
                    return "Reach a 30-day login streak.";

                case "Network Scanner":
                    return "Complete a network scanning lab.";

                default:
                    return "Complete the required activity.";
            }
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