using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Student
{
    public partial class Student_Challenges : Page
    {
        private const string QuizSessionKey =
            "StudentChallengeQuizID";

        private const string TimerStartPrefix =
            "StudentQuizTimerStart_";

        private const string TimerDurationPrefix =
            "StudentQuizTimerDuration_";

        private const string AttemptSubmittedPrefix =
            "StudentQuizAttemptSubmitted_";

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

        private string QuizId
        {
            get
            {
                return Convert.ToString(
                    Session[QuizSessionKey]);
            }
            set
            {
                Session[QuizSessionKey] = value;
            }
        }

        private string TimerStartKey
        {
            get
            {
                return TimerStartPrefix +
                       UserId + "_" +
                       QuizId;
            }
        }

        private string TimerDurationKey
        {
            get
            {
                return TimerDurationPrefix +
                       UserId + "_" +
                       QuizId;
            }
        }

        private string AttemptSubmittedKey
        {
            get
            {
                return AttemptSubmittedPrefix +
                       UserId + "_" +
                       QuizId;
            }
        }

        private bool CurrentAttemptSubmitted
        {
            get
            {
                object value =
                    Session[AttemptSubmittedKey];

                return value != null &&
                       Convert.ToBoolean(value);
            }
            set
            {
                Session[AttemptSubmittedKey] =
                    value;
            }
        }

        protected override void OnInit(
            EventArgs e)
        {
            base.OnInit(e);

            if (IsPostBack &&
                !string.IsNullOrWhiteSpace(
                    QuizId))
            {
                BindQuestions();
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect(
                    "~/Login.aspx");

                return;
            }

            if (!IsPostBack)
            {
                QuizId = "";
                LoadChallenges();
            }
        }

        private void LoadChallenges()
        {
            DataTable dt = Query(@"
                SELECT
                    q.QuizID,
                    q.Title,
                    q.PassMark,
                    q.DurationMinutes,
                    ISNULL(q.MaxAttempts, 3)
                        AS MaxAttempts,
                    c.CourseName,

                    COUNT(
                        DISTINCT qq.QuestionID
                    ) AS QuestionCount,

                    COUNT(
                        DISTINCT qa.AttemptID
                    ) AS AttemptCount,

                    CAST(
                        CASE
                            WHEN MAX(
                                CAST(
                                    qa.IsPassed AS INT
                                )
                            ) = 1
                            THEN 1
                            ELSE 0
                        END
                        AS BIT
                    ) AS HasPassed

                FROM Quizzes q

                INNER JOIN Courses c
                    ON c.CourseID =
                       q.CourseID

                INNER JOIN Enrollments e
                    ON e.CourseID =
                       q.CourseID
                   AND e.StudentID =
                       @StudentID

                LEFT JOIN QuizQuestions qq
                    ON qq.QuizID =
                       q.QuizID

                LEFT JOIN QuizAttempts qa
                    ON qa.QuizID =
                       q.QuizID
                   AND qa.StudentID =
                       @StudentID

                WHERE q.IsPublished = 1

                GROUP BY
                    q.QuizID,
                    q.Title,
                    q.PassMark,
                    q.DurationMinutes,
                    q.MaxAttempts,
                    c.CourseName

                ORDER BY
                    q.Title",
                new SqlParameter(
                    "@StudentID",
                    UserId));

            // Debug: ensure MaxAttempts column exists
            System.Diagnostics.Debug.WriteLine($"[LoadChallenges] Columns: {string.Join(", ", dt.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName))}");
            
            // Case-insensitive check for MaxAttempts column
            var maxAttemptsCol = dt.Columns.Cast<System.Data.DataColumn>().FirstOrDefault(c => c.ColumnName.Equals("MaxAttempts", StringComparison.OrdinalIgnoreCase));
            if (maxAttemptsCol == null)
            {
                dt.Columns.Add("MaxAttempts", typeof(int));
                foreach (DataRow row in dt.Rows)
                    row["MaxAttempts"] = 3;
            }
            else if (maxAttemptsCol.ColumnName != "MaxAttempts")
            {
                // Rename column to standard casing
                maxAttemptsCol.ColumnName = "MaxAttempts";
            }

            litTotal.Text =
                dt.Rows.Count.ToString();

            litPassed.Text =
                dt.AsEnumerable()
                    .Count(row =>
                        Convert.ToBoolean(
                            row["HasPassed"]))
                    .ToString();

            litAttempts.Text =
                dt.AsEnumerable()
                    .Sum(row =>
                        Convert.ToInt32(
                            row["AttemptCount"]))
                    .ToString();

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

            QuizId =
                Convert.ToString(
                    e.CommandArgument);

            LoadQuiz();
        }

        private void LoadQuiz()
        {
            DataTable dt = Query(@"
                SELECT
                    q.Title,
                    q.Description,
                    q.PassMark,
                    q.StartDate,
                    q.EndDate,
                    q.DurationMinutes,
                    ISNULL(q.MaxAttempts, 3)
                        AS MaxAttempts,
                    c.CourseName

                FROM Quizzes q

                INNER JOIN Courses c
                    ON c.CourseID =
                       q.CourseID

                INNER JOIN Enrollments e
                    ON e.CourseID =
                       q.CourseID
                   AND e.StudentID =
                       @StudentID

                WHERE q.QuizID =
                      @QuizID
                  AND q.IsPublished = 1",
                new SqlParameter(
                    "@QuizID",
                    QuizId),
                new SqlParameter(
                    "@StudentID",
                    UserId));

            // Debug
            System.Diagnostics.Debug.WriteLine($"[LoadQuiz] Columns: {string.Join(", ", dt.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName))}");

            // Case-insensitive check for MaxAttempts column
            var maxAttemptsCol = dt.Columns.Cast<System.Data.DataColumn>().FirstOrDefault(c => c.ColumnName.Equals("MaxAttempts", StringComparison.OrdinalIgnoreCase));
            if (maxAttemptsCol == null)
            {
                dt.Columns.Add("MaxAttempts", typeof(int));
                foreach (DataRow row in dt.Rows)
                    row["MaxAttempts"] = 3;
            }
            else if (maxAttemptsCol.ColumnName != "MaxAttempts")
            {
                maxAttemptsCol.ColumnName = "MaxAttempts";
            }

            if (dt.Rows.Count == 0)
            {
                return;
            }

            DataRow quiz =
                dt.Rows[0];

            int attempts =
                GetAttemptCount();

            int maxAttempts =
                GetMaxAttempts(quiz);

            litQuizTitle.Text =
                Server.HtmlEncode(
                    Convert.ToString(
                        quiz["Title"]));

            litQuizDescription.Text =
                Server.HtmlEncode(
                    Convert.ToString(
                        quiz["Description"]));

            litCourseName.Text =
                Server.HtmlEncode(
                    Convert.ToString(
                        quiz["CourseName"]));

            litPassMark.Text =
                Convert.ToDecimal(
                    quiz["PassMark"])
                    .ToString("0.##");

            litAttemptUsage.Text =
                attempts + " / " +
                maxAttempts +
                " attempts";

            string notice =
                GetUnavailableMessage(
                    quiz,
                    attempts,
                    maxAttempts);

            pnlNotice.Visible =
                notice != "";

            litNotice.Text =
                Server.HtmlEncode(
                    notice);

            btnSubmit.Enabled =
                notice == "" &&
                !CurrentAttemptSubmitted;

            btnNextAttempt.Visible =
                notice == "" &&
                CurrentAttemptSubmitted &&
                attempts < maxAttempts;

            hlQuizFeedback.Visible =
                attempts > 0;

            hlQuizFeedback.NavigateUrl =
                "Feedback.aspx?type=quiz&id=" +
                Server.UrlEncode(
                    QuizId);

            BindQuestions();

            if (CurrentAttemptSubmitted)
            {
                SetAnswerControlsEnabled(
                    false);

                if (attempts < maxAttempts)
                {
                    pnlNotice.Visible = true;

                    litNotice.Text =
                        "This attempt has already been submitted. " +
                        "Click Start Next Attempt to try again.";
                }
            }

            LoadAttempts();

            ConfigureTimer(
                quiz,
                attempts,
                maxAttempts);

            pnlResult.Visible = false;
            pnlChallengeList.Visible = false;
            pnlWorkspace.Visible = true;
        }

        private static int GetMaxAttempts(
            DataRow quiz)
        {
            if (quiz["MaxAttempts"] ==
                DBNull.Value)
            {
                return 3;
            }

            int maxAttempts =
                Convert.ToInt32(
                    quiz["MaxAttempts"]);

            return maxAttempts < 1
                ? 1
                : maxAttempts;
        }

        private void ConfigureTimer(
            DataRow quiz,
            int attempts,
            int maxAttempts)
        {
            pnlTimer.Visible = false;
            hfRemainingSeconds.Value = "";

            if (!btnSubmit.Enabled ||
                CurrentAttemptSubmitted ||
                attempts >= maxAttempts ||
                quiz["DurationMinutes"] ==
                    DBNull.Value)
            {
                ClearTimer();
                return;
            }

            int durationMinutes =
                Convert.ToInt32(
                    quiz["DurationMinutes"]);

            if (durationMinutes <= 0)
            {
                ClearTimer();
                return;
            }

            DateTime startedAt;

            if (Session[TimerStartKey] ==
                null)
            {
                startedAt =
                    DateTime.UtcNow;

                Session[TimerStartKey] =
                    startedAt;

                Session[TimerDurationKey] =
                    durationMinutes;
            }
            else
            {
                startedAt =
                    Convert.ToDateTime(
                        Session[TimerStartKey]);
            }

            int storedDuration =
                durationMinutes;

            if (Session[TimerDurationKey] !=
                null)
            {
                storedDuration =
                    Convert.ToInt32(
                        Session[
                            TimerDurationKey]);
            }

            DateTime expiresAt =
                startedAt.AddMinutes(
                    storedDuration);

            int remainingSeconds =
                Convert.ToInt32(
                    Math.Ceiling(
                        (expiresAt -
                         DateTime.UtcNow)
                        .TotalSeconds));

            if (remainingSeconds < 0)
            {
                remainingSeconds = 0;
            }

            pnlTimer.Visible = true;

            hfRemainingSeconds.Value =
                remainingSeconds.ToString();
        }

        private bool HasTimeExpired()
        {
            if (Session[TimerStartKey] ==
                    null ||
                Session[TimerDurationKey] ==
                    null)
            {
                return false;
            }

            DateTime startedAt =
                Convert.ToDateTime(
                    Session[TimerStartKey]);

            int durationMinutes =
                Convert.ToInt32(
                    Session[
                        TimerDurationKey]);

            return DateTime.UtcNow >=
                   startedAt.AddMinutes(
                       durationMinutes);
        }

        private void ClearTimer()
        {
            if (!string.IsNullOrWhiteSpace(
                QuizId))
            {
                Session.Remove(
                    TimerStartKey);

                Session.Remove(
                    TimerDurationKey);
            }

            pnlTimer.Visible = false;
            hfRemainingSeconds.Value = "";
        }

        private void BindQuestions()
        {
            DataTable dt = Query(@"
                SELECT
                    QuestionID,
                    QuestionText,
                    QuestionType,
                    OptionA,
                    OptionB,
                    OptionC,
                    OptionD,
                    Points

                FROM QuizQuestions

                WHERE QuizID =
                      @QuizID

                ORDER BY
                    SortOrder,
                    QuestionID",
                new SqlParameter(
                    "@QuizID",
                    QuizId));

            rptQuestions.DataSource = dt;
            rptQuestions.DataBind();
        }

        protected void rptQuestions_ItemDataBound(
            object sender,
            RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType !=
                    ListItemType.Item &&
                e.Item.ItemType !=
                    ListItemType.AlternatingItem)
            {
                return;
            }

            DataRowView row =
                (DataRowView)e.Item.DataItem;

            string type =
                Convert.ToString(
                    row["QuestionType"]);

            Panel mcqPanel =
                (Panel)e.Item.FindControl(
                    "pnlMCQ");

            RadioButtonList trueFalse =
                (RadioButtonList)
                    e.Item.FindControl(
                        "rblTrueFalse");

            TextBox structure =
                (TextBox)e.Item.FindControl(
                    "tbStructure");

            mcqPanel.Visible =
                type == "MCQ";

            trueFalse.Visible =
                type == "TrueFalse";

            structure.Visible =
                type == "Structure";

            if (type == "MCQ")
            {
                SetOption(
                    e.Item,
                    "cbA",
                    "lblA",
                    "A",
                    row["OptionA"]);

                SetOption(
                    e.Item,
                    "cbB",
                    "lblB",
                    "B",
                    row["OptionB"]);

                SetOption(
                    e.Item,
                    "cbC",
                    "lblC",
                    "C",
                    row["OptionC"]);

                SetOption(
                    e.Item,
                    "cbD",
                    "lblD",
                    "D",
                    row["OptionD"]);
            }
        }

        private static void SetOption(
            RepeaterItem item,
            string boxId,
            string labelId,
            string letter,
            object value)
        {
            CheckBox box =
                (CheckBox)item.FindControl(
                    boxId);

            Label label =
                (Label)item.FindControl(
                    labelId);

            string text =
                Convert.ToString(
                    value);

            box.Visible =
                text != "";

            label.Visible =
                text != "";

            label.Text =
                letter + ". " +
                text;
        }

        protected void btnSubmit_Click(
            object sender,
            EventArgs e)
        {
            Application.Lock();

            try
            {
                if (CurrentAttemptSubmitted)
                {
                    btnSubmit.Enabled = false;

                    btnNextAttempt.Visible =
                        GetAttemptCount() <
                        GetQuizMaxAttempts();

                    ShowResult(
                        false,
                        "This attempt has already been submitted.");

                    return;
                }

                CurrentAttemptSubmitted = true;
            }
            finally
            {
                Application.UnLock();
            }

            btnSubmit.Enabled = false;

            try
            {
                DataTable quizTable =
                    Query(@"
                        SELECT
                            PassMark,
                            StartDate,
                            EndDate,
                            DurationMinutes,

                            ISNULL(
                                MaxAttempts,
                                3
                            ) AS MaxAttempts,

                            ISNULL(
                                TotalMarks,
                                (
                                    SELECT
                                        ISNULL(
                                            SUM(Points),
                                            0
                                        )
                                    FROM QuizQuestions
                                    WHERE QuizID =
                                          @QuizID
                                )
                            ) AS TotalMarks

                        FROM Quizzes

                        WHERE QuizID =
                              @QuizID
                          AND IsPublished = 1",
                        new SqlParameter(
                            "@QuizID",
                            QuizId));

                if (quizTable.Rows.Count == 0)
                {
                    CurrentAttemptSubmitted =
                        false;

                    btnSubmit.Enabled = true;
                    return;
                }

                DataRow quiz =
                    quizTable.Rows[0];

                int attempts =
                    GetAttemptCount();

                int maxAttempts =
                    GetMaxAttempts(quiz);

                if (attempts >= maxAttempts)
                {
                    btnSubmit.Enabled = false;
                    btnNextAttempt.Visible = false;

                    ShowResult(
                        false,
                        "You have used all available attempts.");

                    ClearTimer();
                    return;
                }

                string unavailable =
                    GetUnavailableMessage(
                        quiz,
                        attempts,
                        maxAttempts);

                bool timeExpired =
                    HasTimeExpired() ||
                    hfRemainingSeconds.Value == "0";

                if (unavailable != "" &&
                    !timeExpired)
                {
                    CurrentAttemptSubmitted =
                        false;

                    btnSubmit.Enabled = true;

                    ShowResult(
                        false,
                        unavailable);

                    return;
                }

                DataTable questions =
                    Query(@"
                        SELECT
                            QuestionID,
                            QuestionType,
                            OptionA,
                            OptionB,
                            OptionC,
                            OptionD,
                            CorrectAnswer,
                            MatchStrategy,
                            Explanation,
                            Points

                        FROM QuizQuestions

                        WHERE QuizID =
                              @QuizID

                        ORDER BY
                            SortOrder,
                            QuestionID",
                        new SqlParameter(
                            "@QuizID",
                            QuizId));

                Dictionary<string, string>
                    answers =
                        ReadAnswers();

                int obtained = 0;

                foreach (DataRow question
                         in questions.Rows)
                {
                    string questionId =
                        Convert.ToString(
                            question[
                                "QuestionID"]);

                    string answer =
                        answers.ContainsKey(
                            questionId)
                            ? answers[
                                questionId]
                            : "";

                    if (IsCorrect(
                        question,
                        answer))
                    {
                        obtained +=
                            Convert.ToInt32(
                                question[
                                    "Points"]);
                    }
                }

                int total =
                    Convert.ToInt32(
                        quiz["TotalMarks"]);

                decimal score =
                    total == 0
                        ? 0
                        : Math.Round(
                            obtained *
                            100m /
                            total,
                            2);

                bool passed =
                    score >=
                    Convert.ToDecimal(
                        quiz["PassMark"]);

                bool awardXp =
                    passed &&
                    !HasPassed();

                SaveAttempt(
                    questions,
                    answers,
                    obtained,
                    total,
                    score,
                    passed,
                    awardXp);

                AdminService.LogAudit(UserId, "SUBMIT_QUIZ", "QuizAttempts", QuizId, "",
                    score.ToString("0.##") + "% - " + (passed ? "Passed" : "Failed"));

                ClearTimer();

                string message =
                    "You scored " +
                    obtained + " / " +
                    total + " marks (" +
                    score.ToString("0.##") +
                    "%). ";

                if (passed && awardXp)
                {
                    message +=
                        "Challenge passed. XP awarded.";
                }
                else if (passed)
                {
                    message +=
                        "Challenge passed. " +
                        "No additional XP was awarded.";
                }
                else
                {
                    message +=
                        "Challenge not passed.";
                }

                if (timeExpired)
                {
                    message +=
                        " The quiz was submitted because " +
                        "the time limit ended.";
                }

                ShowResult(
                    passed,
                    message);

                ShowAnswerReview(
                    questions,
                    answers);

                SetAnswerControlsEnabled(
                    false);

                attempts++;

                litAttemptUsage.Text =
                    attempts + " / " +
                    maxAttempts +
                    " attempts";

                btnSubmit.Enabled = false;

                btnNextAttempt.Visible =
                    attempts < maxAttempts;

                hlQuizFeedback.Visible = true;

                hlQuizFeedback.NavigateUrl =
                    "Feedback.aspx?type=quiz&id=" +
                    Server.UrlEncode(
                        QuizId);

                pnlNotice.Visible = true;

                if (attempts >= maxAttempts)
                {
                    litNotice.Text =
                        "You have used all available attempts. " +
                        "You may still review the quiz.";
                }
                else
                {
                    litNotice.Text =
                        "Attempt submitted. Click Start Next Attempt " +
                        "when you are ready to try again.";
                }

                LoadAttempts();
            }
            catch
            {
                CurrentAttemptSubmitted =
                    false;

                btnSubmit.Enabled = true;
                btnNextAttempt.Visible = false;

                throw;
            }
        }

        protected void btnNextAttempt_Click(
            object sender,
            EventArgs e)
        {
            DataTable quizTable =
                Query(@"
                    SELECT
                        StartDate,
                        EndDate,
                        DurationMinutes,
                        ISNULL(
                            MaxAttempts,
                            3
                        ) AS MaxAttempts

                    FROM Quizzes

                    WHERE QuizID =
                          @QuizID
                      AND IsPublished = 1",
                    new SqlParameter(
                        "@QuizID",
                        QuizId));

            // Debug
            System.Diagnostics.Debug.WriteLine($"[btnSubmit] Columns: {string.Join(", ", quizTable.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName))}");

            // Case-insensitive check for MaxAttempts column
            var maxAttemptsCol = quizTable.Columns.Cast<System.Data.DataColumn>().FirstOrDefault(c => c.ColumnName.Equals("MaxAttempts", StringComparison.OrdinalIgnoreCase));
            if (maxAttemptsCol == null)
            {
                quizTable.Columns.Add("MaxAttempts", typeof(int));
                foreach (DataRow row in quizTable.Rows)
                    row["MaxAttempts"] = 3;
            }
            else if (maxAttemptsCol.ColumnName != "MaxAttempts")
            {
                maxAttemptsCol.ColumnName = "MaxAttempts";
            }

            if (quizTable.Rows.Count == 0)
            {
                return;
            }

            DataRow quiz =
                quizTable.Rows[0];

            int attempts =
                GetAttemptCount();

            int maxAttempts =
                GetMaxAttempts(quiz);

            if (attempts >= maxAttempts)
            {
                CurrentAttemptSubmitted = true;

                btnSubmit.Enabled = false;
                btnNextAttempt.Visible = false;

                pnlNotice.Visible = true;

                litNotice.Text =
                    "You have used all available attempts.";

                return;
            }

            string unavailable =
                GetUnavailableMessage(
                    quiz,
                    attempts,
                    maxAttempts);

            if (unavailable != "")
            {
                btnSubmit.Enabled = false;
                btnNextAttempt.Visible = false;

                pnlNotice.Visible = true;

                litNotice.Text =
                    Server.HtmlEncode(
                        unavailable);

                return;
            }

            CurrentAttemptSubmitted = false;

            ClearTimer();

            BindQuestions();

            SetAnswerControlsEnabled(
                true);

            btnSubmit.Enabled = true;
            btnNextAttempt.Visible = false;

            pnlResult.Visible = false;
            pnlNotice.Visible = false;

            litAttemptUsage.Text =
                attempts + " / " +
                maxAttempts +
                " attempts";

            ConfigureTimer(
                quiz,
                attempts,
                maxAttempts);
        }

        private int GetQuizMaxAttempts()
        {
            object value =
                Scalar(@"
                    SELECT
                        ISNULL(
                            MaxAttempts,
                            3
                        )
                    FROM Quizzes
                    WHERE QuizID =
                          @QuizID",
                    new SqlParameter(
                        "@QuizID",
                        QuizId));

            if (value == null ||
                value == DBNull.Value)
            {
                return 3;
            }

            int maxAttempts =
                Convert.ToInt32(value);

            return maxAttempts < 1
                ? 1
                : maxAttempts;
        }

        private void SetAnswerControlsEnabled(
            bool enabled)
        {
            foreach (RepeaterItem item
                     in rptQuestions.Items)
            {
                CheckBox cbA =
                    item.FindControl(
                        "cbA") as CheckBox;

                CheckBox cbB =
                    item.FindControl(
                        "cbB") as CheckBox;

                CheckBox cbC =
                    item.FindControl(
                        "cbC") as CheckBox;

                CheckBox cbD =
                    item.FindControl(
                        "cbD") as CheckBox;

                RadioButtonList trueFalse =
                    item.FindControl(
                        "rblTrueFalse")
                    as RadioButtonList;

                TextBox structure =
                    item.FindControl(
                        "tbStructure")
                    as TextBox;

                if (cbA != null)
                {
                    cbA.Enabled = enabled;
                }

                if (cbB != null)
                {
                    cbB.Enabled = enabled;
                }

                if (cbC != null)
                {
                    cbC.Enabled = enabled;
                }

                if (cbD != null)
                {
                    cbD.Enabled = enabled;
                }

                if (trueFalse != null)
                {
                    trueFalse.Enabled = enabled;
                }

                if (structure != null)
                {
                    structure.Enabled = enabled;
                }
            }
        }

        private Dictionary<string, string>
            ReadAnswers()
        {
            Dictionary<string, string> answers =
                new Dictionary<string, string>();

            foreach (RepeaterItem item
                     in rptQuestions.Items)
            {
                string questionId =
                    ((HiddenField)
                        item.FindControl(
                            "hfQuestionID"))
                        .Value;

                string type =
                    ((HiddenField)
                        item.FindControl(
                            "hfQuestionType"))
                        .Value;

                if (type == "MCQ")
                {
                    List<string> selected =
                        new List<string>();

                    AddChecked(
                        item,
                        "cbA",
                        "A",
                        selected);

                    AddChecked(
                        item,
                        "cbB",
                        "B",
                        selected);

                    AddChecked(
                        item,
                        "cbC",
                        "C",
                        selected);

                    AddChecked(
                        item,
                        "cbD",
                        "D",
                        selected);

                    answers[questionId] =
                        string.Join(
                            ",",
                            selected);
                }
                else if (type ==
                         "TrueFalse")
                {
                    answers[questionId] =
                        ((RadioButtonList)
                            item.FindControl(
                                "rblTrueFalse"))
                            .SelectedValue;
                }
                else
                {
                    answers[questionId] =
                        ((TextBox)
                            item.FindControl(
                                "tbStructure"))
                            .Text.Trim();
                }
            }

            return answers;
        }

        private static void AddChecked(
            RepeaterItem item,
            string controlId,
            string value,
            List<string> selected)
        {
            CheckBox box =
                (CheckBox)item.FindControl(
                    controlId);

            if (box.Visible &&
                box.Checked)
            {
                selected.Add(value);
            }
        }

        private static bool IsCorrect(
            DataRow question,
            string submitted)
        {
            string type =
                Convert.ToString(
                    question[
                        "QuestionType"]);

            string expected =
                Convert.ToString(
                    question[
                        "CorrectAnswer"]);

            if (type == "MCQ")
            {
                return Normalise(
                    submitted) ==
                    Normalise(
                        expected);
            }

            if (type == "TrueFalse")
            {
                return string.Equals(
                    submitted.Trim(),
                    expected.Trim(),
                    StringComparison
                        .OrdinalIgnoreCase);
            }

            string strategy =
                Convert.ToString(
                    question[
                        "MatchStrategy"]);

            if (strategy == "Contains")
            {
                return submitted.IndexOf(
                    expected,
                    StringComparison
                        .OrdinalIgnoreCase) >= 0;
            }

            if (strategy == "Regex")
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
                submitted.Trim(),
                expected.Trim(),
                StringComparison
                    .OrdinalIgnoreCase);
        }

        private static string Normalise(
            string answer)
        {
            return string.Join(
                ",",
                (answer ?? "")
                    .Split(
                        new[] { ',' },
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .Select(value =>
                        value.Trim()
                            .ToUpperInvariant())
                    .OrderBy(value =>
                        value));
        }

        private void ShowAnswerReview(
            DataTable questions,
            Dictionary<string, string> answers)
        {
            foreach (RepeaterItem item
                     in rptQuestions.Items)
            {
                string questionId =
                    ((HiddenField)
                        item.FindControl(
                            "hfQuestionID"))
                        .Value;

                DataRow question =
                    questions.AsEnumerable()
                        .First(row =>
                            Convert.ToString(
                                row[
                                    "QuestionID"]) ==
                            questionId);

                string answer =
                    answers.ContainsKey(
                        questionId)
                        ? answers[
                            questionId]
                        : "";

                bool correct =
                    IsCorrect(
                        question,
                        answer);

                Label result =
                    (Label)item.FindControl(
                        "lblAnswerResult");

                result.Text =
                    correct
                        ? "Correct"
                        : "Incorrect";

                result.CssClass =
                    correct
                        ? "badge badge-green"
                        : "badge badge-red";

                string studentAnswer =
                    string.IsNullOrWhiteSpace(
                        answer)
                        ? "No answer"
                        : DisplayAnswer(
                            question,
                            answer);

                string correctAnswer =
                    DisplayAnswer(
                        question,
                        Convert.ToString(
                            question[
                                "CorrectAnswer"]));

                ((Literal)
                    item.FindControl(
                        "litStudentAnswer"))
                    .Text =
                        Server.HtmlEncode(
                            studentAnswer);

                ((Literal)
                    item.FindControl(
                        "litCorrectAnswer"))
                    .Text =
                        Server.HtmlEncode(
                            correctAnswer);

                string explanation =
                    Convert.ToString(
                        question[
                            "Explanation"]);

                Panel explanationPanel =
                    (Panel)item.FindControl(
                        "pnlExplanation");

                explanationPanel.Visible =
                    !string.IsNullOrWhiteSpace(
                        explanation);

                ((Literal)
                    item.FindControl(
                        "litExplanation"))
                    .Text =
                        Server.HtmlEncode(
                            explanation);

                ((Panel)
                    item.FindControl(
                        "pnlAnswerReview"))
                    .Visible = true;
            }
        }

        private static string DisplayAnswer(
            DataRow question,
            string answer)
        {
            if (Convert.ToString(
                question[
                    "QuestionType"]) !=
                "MCQ")
            {
                return answer;
            }

            List<string> result =
                new List<string>();

            foreach (string letter
                     in Normalise(
                         answer)
                         .Split(','))
            {
                if (letter == "")
                {
                    continue;
                }

                string option =
                    Convert.ToString(
                        question[
                            "Option" +
                            letter]);

                result.Add(
                    letter + ". " +
                    option);
            }

            return string.Join(
                ", ",
                result);
        }

        private void SaveAttempt(
            DataTable questions,
            Dictionary<string, string> answers,
            int obtained,
            int total,
            decimal score,
            bool passed,
            bool awardXp)
        {
            string attemptId =
                IdGenerator.NewId(
                    "ATT");

            using (SqlConnection con =
                   new SqlConnection(
                       ConnectionString))
            {
                con.Open();

                using (SqlTransaction transaction =
                       con.BeginTransaction())
                {
                    try
                    {
                        Execute(
                            con,
                            transaction,
                            @"
                            INSERT INTO QuizAttempts
                            (
                                AttemptID,
                                QuizID,
                                StudentID,
                                Score,
                                TotalMarks,
                                ObtainedMarks,
                                IsPassed
                            )
                            VALUES
                            (
                                @AttemptID,
                                @QuizID,
                                @StudentID,
                                @Score,
                                @TotalMarks,
                                @ObtainedMarks,
                                @IsPassed
                            )",
                            new SqlParameter(
                                "@AttemptID",
                                attemptId),
                            new SqlParameter(
                                "@QuizID",
                                QuizId),
                            new SqlParameter(
                                "@StudentID",
                                UserId),
                            new SqlParameter(
                                "@Score",
                                score),
                            new SqlParameter(
                                "@TotalMarks",
                                total),
                            new SqlParameter(
                                "@ObtainedMarks",
                                obtained),
                            new SqlParameter(
                                "@IsPassed",
                                passed));

                        foreach (DataRow question
                                 in questions.Rows)
                        {
                            string questionId =
                                Convert.ToString(
                                    question[
                                        "QuestionID"]);

                            string answer =
                                answers.ContainsKey(
                                    questionId)
                                    ? answers[
                                        questionId]
                                    : "";

                            Execute(
                                con,
                                transaction,
                                @"
                                INSERT INTO QuizAnswers
                                (
                                    AttemptID,
                                    QuestionID,
                                    StudentAnswer,
                                    IsCorrect
                                )
                                VALUES
                                (
                                    @AttemptID,
                                    @QuestionID,
                                    @StudentAnswer,
                                    @IsCorrect
                                )",
                                new SqlParameter(
                                    "@AttemptID",
                                    attemptId),
                                new SqlParameter(
                                    "@QuestionID",
                                    questionId),
                                new SqlParameter(
                                    "@StudentAnswer",
                                    string.IsNullOrWhiteSpace(
                                        answer)
                                        ? (object)
                                            DBNull.Value
                                        : answer),
                                new SqlParameter(
                                    "@IsCorrect",
                                    IsCorrect(
                                        question,
                                        answer)));
                        }

                        if (awardXp)
                        {
                            Execute(
                                con,
                                transaction,
                                @"
                                UPDATE Users
                                SET TotalPoints =
                                    TotalPoints + @Points
                                WHERE UserID =
                                      @StudentID",
                                new SqlParameter(
                                    "@Points",
                                    obtained),
                                new SqlParameter(
                                    "@StudentID",
                                    UserId));
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
        }

        private string GetUnavailableMessage(
            DataRow quiz,
            int attempts,
            int maxAttempts)
        {
            if (attempts >= maxAttempts)
            {
                return
                    "You have used all available attempts. " +
                    "You may still review the quiz.";
            }

            DateTime now =
                DateTime.Now;

            if (quiz["StartDate"] !=
                    DBNull.Value &&
                now <
                Convert.ToDateTime(
                    quiz["StartDate"]))
            {
                return
                    "This challenge has not opened yet.";
            }

            if (quiz["EndDate"] !=
                    DBNull.Value &&
                now >
                Convert.ToDateTime(
                    quiz["EndDate"]))
            {
                return
                    "This challenge has already closed.";
            }

            return "";
        }

        private int GetAttemptCount()
        {
            return Convert.ToInt32(
                Scalar(@"
                    SELECT COUNT(*)
                    FROM QuizAttempts
                    WHERE QuizID =
                          @QuizID
                      AND StudentID =
                          @StudentID",
                    new SqlParameter(
                        "@QuizID",
                        QuizId),
                    new SqlParameter(
                        "@StudentID",
                        UserId)));
        }

        private bool HasPassed()
        {
            return Convert.ToInt32(
                Scalar(@"
                    SELECT COUNT(*)
                    FROM QuizAttempts
                    WHERE QuizID =
                          @QuizID
                      AND StudentID =
                          @StudentID
                      AND IsPassed = 1",
                    new SqlParameter(
                        "@QuizID",
                        QuizId),
                    new SqlParameter(
                        "@StudentID",
                        UserId))) > 0;
        }

        private void LoadAttempts()
        {
            DataTable dt =
                Query(@"
                    SELECT
                        ObtainedMarks,
                        TotalMarks,
                        Score,
                        IsPassed,
                        AttemptedAt

                    FROM QuizAttempts

                    WHERE QuizID =
                          @QuizID
                      AND StudentID =
                          @StudentID

                    ORDER BY
                        AttemptedAt DESC",
                    new SqlParameter(
                        "@QuizID",
                        QuizId),
                    new SqlParameter(
                        "@StudentID",
                        UserId));

            gvAttempts.DataSource = dt;
            gvAttempts.DataBind();

            pnlAttempts.Visible =
                dt.Rows.Count > 0;
        }

        private DataTable Query(
            string sql,
            params SqlParameter[] parameters)
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                   new SqlConnection(
                       ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                       sql,
                       con))
            using (SqlDataAdapter adapter =
                   new SqlDataAdapter(
                       cmd))
            {
                cmd.Parameters.AddRange(
                    parameters);

                adapter.Fill(dt);
            }

            return dt;
        }

        private object Scalar(
            string sql,
            params SqlParameter[] parameters)
        {
            using (SqlConnection con =
                   new SqlConnection(
                       ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(
                       sql,
                       con))
            {
                cmd.Parameters.AddRange(
                    parameters);

                con.Open();

                return cmd.ExecuteScalar();
            }
        }

        private static void Execute(
            SqlConnection con,
            SqlTransaction transaction,
            string sql,
            params SqlParameter[] parameters)
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                       sql,
                       con,
                       transaction))
            {
                cmd.Parameters.AddRange(
                    parameters);

                cmd.ExecuteNonQuery();
            }
        }

        private void ShowResult(
            bool success,
            string message)
        {
            pnlResult.Visible = true;

            pnlResult.CssClass =
                success
                    ? "result-success"
                    : "result-error";

            litResult.Text =
                Server.HtmlEncode(
                    message);
        }

        protected void btnBack_Click(
            object sender,
            EventArgs e)
        {
            QuizId = "";

            pnlWorkspace.Visible = false;
            pnlChallengeList.Visible = true;

            LoadChallenges();
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            ClearTimer();

            Session.Clear();
            Session.Abandon();

            Response.Redirect(
                "~/Login.aspx?msg=loggedout");
        }
    }
}