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
        private const int MaxAttempts = 3;
        private const string QuizSessionKey = "StudentChallengeQuizID";

        private string ConnectionString =>
            ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

        private string UserId =>
            Convert.ToString(Session["UserID"]);

        private string QuizId
        {
            get { return Convert.ToString(Session[QuizSessionKey]); }
            set { Session[QuizSessionKey] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (IsPostBack && !string.IsNullOrEmpty(QuizId))
                BindQuestions();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
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
                SELECT q.QuizID, q.Title, q.PassMark, q.DurationMinutes,
                       c.CourseName,
                       COUNT(DISTINCT qq.QuestionID) AS QuestionCount,
                       COUNT(DISTINCT qa.AttemptID) AS AttemptCount,
                       CAST(CASE WHEN MAX(CAST(qa.IsPassed AS INT)) = 1
                            THEN 1 ELSE 0 END AS BIT) AS HasPassed
                FROM Quizzes q
                INNER JOIN Courses c ON q.CourseID = c.CourseID
                INNER JOIN Enrollments e
                    ON q.CourseID = e.CourseID
                   AND e.StudentID = @StudentID
                LEFT JOIN QuizQuestions qq ON q.QuizID = qq.QuizID
                LEFT JOIN QuizAttempts qa
                    ON q.QuizID = qa.QuizID
                   AND qa.StudentID = @StudentID
                WHERE q.IsPublished = 1
                GROUP BY q.QuizID, q.Title, q.PassMark,
                         q.DurationMinutes, c.CourseName
                ORDER BY q.Title",
                new SqlParameter("@StudentID", UserId));

            litTotal.Text = dt.Rows.Count.ToString();
            litPassed.Text = dt.AsEnumerable()
                .Count(r => Convert.ToBoolean(r["HasPassed"])).ToString();
            litAttempts.Text = dt.AsEnumerable()
                .Sum(r => Convert.ToInt32(r["AttemptCount"])).ToString();

            rptChallenges.DataSource = dt;
            rptChallenges.DataBind();
            pnlEmpty.Visible = dt.Rows.Count == 0;
        }

        protected void rptChallenges_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Open")
                return;

            QuizId = Convert.ToString(e.CommandArgument);
            LoadQuiz();
        }

        private void LoadQuiz()
        {
            DataTable dt = Query(@"
                SELECT q.Title, q.Description, q.PassMark,
                       q.StartDate, q.EndDate, c.CourseName
                FROM Quizzes q
                INNER JOIN Courses c ON q.CourseID = c.CourseID
                INNER JOIN Enrollments e
                    ON q.CourseID = e.CourseID
                   AND e.StudentID = @StudentID
                WHERE q.QuizID = @QuizID
                  AND q.IsPublished = 1",
                new SqlParameter("@QuizID", QuizId),
                new SqlParameter("@StudentID", UserId));

            if (dt.Rows.Count == 0)
                return;

            DataRow quiz = dt.Rows[0];
            int attempts = GetAttemptCount();

            litQuizTitle.Text =
                Server.HtmlEncode(Convert.ToString(quiz["Title"]));

            litQuizDescription.Text =
                Server.HtmlEncode(Convert.ToString(quiz["Description"]));

            litCourseName.Text =
                Server.HtmlEncode(Convert.ToString(quiz["CourseName"]));

            litPassMark.Text =
                Convert.ToDecimal(quiz["PassMark"]).ToString("0.##");

            litAttemptUsage.Text =
                attempts + " / " + MaxAttempts + " attempts";

            string notice = GetUnavailableMessage(quiz, attempts);

            pnlNotice.Visible = notice != "";
            litNotice.Text = Server.HtmlEncode(notice);
            btnSubmit.Enabled = notice == "";

            BindQuestions();
            LoadAttempts();

            pnlResult.Visible = false;
            pnlChallengeList.Visible = false;
            pnlWorkspace.Visible = true;
        }

        private void BindQuestions()
        {
            DataTable dt = Query(@"
                SELECT QuestionID, QuestionText, QuestionType,
                       OptionA, OptionB, OptionC, OptionD, Points
                FROM QuizQuestions
                WHERE QuizID = @QuizID
                ORDER BY SortOrder, QuestionID",
                new SqlParameter("@QuizID", QuizId));

            rptQuestions.DataSource = dt;
            rptQuestions.DataBind();
        }

        protected void rptQuestions_ItemDataBound(
            object sender,
            RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView row = (DataRowView)e.Item.DataItem;
            string type = Convert.ToString(row["QuestionType"]);

            ((Panel)e.Item.FindControl("pnlMCQ")).Visible =
                type == "MCQ";

            ((RadioButtonList)e.Item.FindControl("rblTrueFalse")).Visible =
                type == "TrueFalse";

            ((TextBox)e.Item.FindControl("tbStructure")).Visible =
                type == "Structure";

            if (type == "MCQ")
            {
                SetOption(e.Item, "cbA", "lblA", "A", row["OptionA"]);
                SetOption(e.Item, "cbB", "lblB", "B", row["OptionB"]);
                SetOption(e.Item, "cbC", "lblC", "C", row["OptionC"]);
                SetOption(e.Item, "cbD", "lblD", "D", row["OptionD"]);
            }
        }

        private static void SetOption(
            RepeaterItem item,
            string boxId,
            string labelId,
            string letter,
            object value)
        {
            CheckBox box = (CheckBox)item.FindControl(boxId);
            Label label = (Label)item.FindControl(labelId);
            string text = Convert.ToString(value);

            box.Visible = text != "";
            label.Visible = text != "";
            label.Text = letter + ". " + text;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int attempts = GetAttemptCount();

            if (attempts >= MaxAttempts)
            {
                ShowResult(false, "You have used all three attempts.");
                return;
            }

            DataTable quizTable = Query(@"
                SELECT PassMark, StartDate, EndDate,
                       ISNULL(TotalMarks,
                           (SELECT ISNULL(SUM(Points), 0)
                            FROM QuizQuestions
                            WHERE QuizID = @QuizID)) AS TotalMarks
                FROM Quizzes
                WHERE QuizID = @QuizID
                  AND IsPublished = 1",
                new SqlParameter("@QuizID", QuizId));

            if (quizTable.Rows.Count == 0)
                return;

            DataRow quiz = quizTable.Rows[0];
            string unavailable = GetUnavailableMessage(quiz, attempts);

            if (unavailable != "")
            {
                ShowResult(false, unavailable);
                return;
            }

            DataTable questions = Query(@"
                SELECT QuestionID, QuestionType,
                       OptionA, OptionB, OptionC, OptionD,
                       CorrectAnswer, MatchStrategy,
                       Explanation, Points
                FROM QuizQuestions
                WHERE QuizID = @QuizID
                ORDER BY SortOrder, QuestionID",
                new SqlParameter("@QuizID", QuizId));

            Dictionary<string, string> answers = ReadAnswers();
            int obtained = 0;

            foreach (DataRow question in questions.Rows)
            {
                string id = Convert.ToString(question["QuestionID"]);
                string answer = answers.ContainsKey(id) ? answers[id] : "";

                if (IsCorrect(question, answer))
                    obtained += Convert.ToInt32(question["Points"]);
            }

            int total = Convert.ToInt32(quiz["TotalMarks"]);

            decimal score = total == 0
                ? 0
                : Math.Round(obtained * 100m / total, 2);

            bool passed =
                score >= Convert.ToDecimal(quiz["PassMark"]);

            bool awardXp =
                passed && !HasPassed();

            SaveAttempt(
                questions,
                answers,
                obtained,
                total,
                score,
                passed,
                awardXp);

            string message =
                "You scored " + obtained + " / " + total +
                " marks (" + score.ToString("0.##") + "%). ";

            if (passed && awardXp)
                message += "Challenge passed. XP awarded.";
            else if (passed)
                message += "Challenge passed. No additional XP was awarded.";
            else
                message += "Challenge not passed.";

            ShowResult(passed, message);
            ShowCorrectAnswers(questions);

            attempts++;
            litAttemptUsage.Text =
                attempts + " / " + MaxAttempts + " attempts";

            btnSubmit.Enabled = attempts < MaxAttempts;

            if (!btnSubmit.Enabled)
            {
                pnlNotice.Visible = true;
                litNotice.Text = "You have used all three attempts.";
            }

            LoadAttempts();
        }

        private Dictionary<string, string> ReadAnswers()
        {
            Dictionary<string, string> answers =
                new Dictionary<string, string>();

            foreach (RepeaterItem item in rptQuestions.Items)
            {
                string id =
                    ((HiddenField)item.FindControl("hfQuestionID")).Value;

                string type =
                    ((HiddenField)item.FindControl("hfQuestionType")).Value;

                if (type == "MCQ")
                {
                    List<string> selected = new List<string>();

                    AddChecked(item, "cbA", "A", selected);
                    AddChecked(item, "cbB", "B", selected);
                    AddChecked(item, "cbC", "C", selected);
                    AddChecked(item, "cbD", "D", selected);

                    answers[id] = string.Join(",", selected);
                }
                else if (type == "TrueFalse")
                {
                    answers[id] =
                        ((RadioButtonList)item.FindControl(
                            "rblTrueFalse")).SelectedValue;
                }
                else
                {
                    answers[id] =
                        ((TextBox)item.FindControl(
                            "tbStructure")).Text.Trim();
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
                (CheckBox)item.FindControl(controlId);

            if (box.Visible && box.Checked)
                selected.Add(value);
        }

        private static bool IsCorrect(
            DataRow question,
            string submitted)
        {
            string type =
                Convert.ToString(question["QuestionType"]);

            string expected =
                Convert.ToString(question["CorrectAnswer"]);

            if (type == "MCQ")
                return Normalise(submitted) == Normalise(expected);

            if (type == "TrueFalse")
                return string.Equals(
                    submitted.Trim(),
                    expected.Trim(),
                    StringComparison.OrdinalIgnoreCase);

            string strategy =
                Convert.ToString(question["MatchStrategy"]);

            if (strategy == "Contains")
                return submitted.IndexOf(
                    expected,
                    StringComparison.OrdinalIgnoreCase) >= 0;

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
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalise(string answer)
        {
            return string.Join(",",
                (answer ?? "")
                    .Split(
                        new[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().ToUpper())
                    .OrderBy(x => x));
        }

        private void ShowCorrectAnswers(DataTable questions)
        {
            foreach (RepeaterItem item in rptQuestions.Items)
            {
                string id =
                    ((HiddenField)item.FindControl("hfQuestionID")).Value;

                DataRow question = questions.AsEnumerable()
                    .First(r =>
                        Convert.ToString(r["QuestionID"]) == id);

                string correct =
                    Convert.ToString(question["CorrectAnswer"]);

                if (Convert.ToString(question["QuestionType"]) == "MCQ")
                    correct = GetMcqAnswer(question, correct);

                ((Literal)item.FindControl("litCorrectAnswer")).Text =
                    Server.HtmlEncode(correct);

                string explanation =
                    Convert.ToString(question["Explanation"]);

                Panel explanationPanel =
                    (Panel)item.FindControl("pnlExplanation");

                explanationPanel.Visible =
                    !string.IsNullOrWhiteSpace(explanation);

                ((Literal)item.FindControl("litExplanation")).Text =
                    Server.HtmlEncode(explanation);

                ((Panel)item.FindControl("pnlAnswerReview")).Visible = true;
            }
        }

        private static string GetMcqAnswer(
            DataRow question,
            string answer)
        {
            List<string> result = new List<string>();

            foreach (string letter in Normalise(answer).Split(','))
            {
                if (letter == "")
                    continue;

                string column = "Option" + letter;
                string text = Convert.ToString(question[column]);

                result.Add(letter + ". " + text);
            }

            return string.Join(", ", result);
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
            string attemptId = IdGenerator.NewId("ATT");

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            {
                con.Open();

                using (SqlTransaction transaction =
                       con.BeginTransaction())
                {
                    try
                    {
                        Execute(con, transaction, @"
                            INSERT INTO QuizAttempts
                            (
                                AttemptID, QuizID, StudentID,
                                Score, TotalMarks,
                                ObtainedMarks, IsPassed
                            )
                            VALUES
                            (
                                @AttemptID, @QuizID, @StudentID,
                                @Score, @Total,
                                @Obtained, @Passed
                            )",
                            new SqlParameter("@AttemptID", attemptId),
                            new SqlParameter("@QuizID", QuizId),
                            new SqlParameter("@StudentID", UserId),
                            new SqlParameter("@Score", score),
                            new SqlParameter("@Total", total),
                            new SqlParameter("@Obtained", obtained),
                            new SqlParameter("@Passed", passed));

                        foreach (DataRow question in questions.Rows)
                        {
                            string id =
                                Convert.ToString(question["QuestionID"]);

                            string answer =
                                answers.ContainsKey(id) ? answers[id] : "";

                            Execute(con, transaction, @"
                                INSERT INTO QuizAnswers
                                (
                                    AttemptID, QuestionID,
                                    StudentAnswer, IsCorrect
                                )
                                VALUES
                                (
                                    @AttemptID, @QuestionID,
                                    @Answer, @Correct
                                )",
                                new SqlParameter("@AttemptID", attemptId),
                                new SqlParameter("@QuestionID", id),
                                new SqlParameter(
                                    "@Answer",
                                    answer == ""
                                        ? (object)DBNull.Value
                                        : answer),
                                new SqlParameter(
                                    "@Correct",
                                    IsCorrect(question, answer)));
                        }

                        if (awardXp)
                        {
                            Execute(con, transaction, @"
                                UPDATE Users
                                SET TotalPoints = TotalPoints + @XP
                                WHERE UserID = @StudentID",
                                new SqlParameter("@XP", obtained),
                                new SqlParameter("@StudentID", UserId));
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
            int attempts)
        {
            if (attempts >= MaxAttempts)
                return "You have used all three attempts.";

            DateTime now = DateTime.Now;

            if (quiz["StartDate"] != DBNull.Value &&
                now < Convert.ToDateTime(quiz["StartDate"]))
                return "This challenge has not opened yet.";

            if (quiz["EndDate"] != DBNull.Value &&
                now > Convert.ToDateTime(quiz["EndDate"]))
                return "This challenge has already closed.";

            return "";
        }

        private int GetAttemptCount()
        {
            return Convert.ToInt32(Scalar(@"
                SELECT COUNT(*)
                FROM QuizAttempts
                WHERE QuizID = @QuizID
                  AND StudentID = @StudentID",
                new SqlParameter("@QuizID", QuizId),
                new SqlParameter("@StudentID", UserId)));
        }

        private bool HasPassed()
        {
            return Convert.ToInt32(Scalar(@"
                SELECT COUNT(*)
                FROM QuizAttempts
                WHERE QuizID = @QuizID
                  AND StudentID = @StudentID
                  AND IsPassed = 1",
                new SqlParameter("@QuizID", QuizId),
                new SqlParameter("@StudentID", UserId))) > 0;
        }

        private void LoadAttempts()
        {
            DataTable dt = Query(@"
                SELECT ObtainedMarks, TotalMarks,
                       Score, IsPassed, AttemptedAt
                FROM QuizAttempts
                WHERE QuizID = @QuizID
                  AND StudentID = @StudentID
                ORDER BY AttemptedAt DESC",
                new SqlParameter("@QuizID", QuizId),
                new SqlParameter("@StudentID", UserId));

            gvAttempts.DataSource = dt;
            gvAttempts.DataBind();
            pnlAttempts.Visible = dt.Rows.Count > 0;
        }

        private DataTable Query(
            string sql,
            params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
            using (SqlDataAdapter da =
                   new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddRange(parameters);
                da.Fill(dt);
            }

            return dt;
        }

        private object Scalar(
            string sql,
            params SqlParameter[] parameters)
        {
            using (SqlConnection con =
                   new SqlConnection(ConnectionString))
            using (SqlCommand cmd =
                   new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(parameters);
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
                   new SqlCommand(sql, con, transaction))
            {
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowResult(bool success, string message)
        {
            pnlResult.Visible = true;
            pnlResult.CssClass =
                success ? "result-success" : "result-error";

            litResult.Text = Server.HtmlEncode(message);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            QuizId = "";
            pnlWorkspace.Visible = false;
            pnlChallengeList.Visible = true;
            LoadChallenges();
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx?msg=loggedout");
        }
    }
}