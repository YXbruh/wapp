using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.DataAccess;
using CSA.Services;

namespace CSA.Lecturer
{
    public partial class QuizEditor : Page
    {
        private string CurrentInstructorId => Session["UserID"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }

            // Required for the plain <input type="file" multiple> attachment picker
            // to actually transmit file bytes with the postback.
            Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                LoadCourseDropdown();
                LoadQuizDropdown();

                // Start with no quiz selected so files and links can be attached while
                // composing a new quiz; they are staged and committed by Create Quiz.
                PendingAttachmentService.Clear(AttachBucket);

                LoadQuestions();
                LoadAttachments(ddlQuiz.SelectedValue);
                LoadQuizDetails(ddlQuiz.SelectedValue);
                ClearBulkRows();
            }
        }

        // ---------------------------------------------------------
        // Dropdowns
        // ---------------------------------------------------------
        private void LoadCourseDropdown()
        {
            DataTable courses = LabService.GetCoursesForInstructor(CurrentInstructorId);

            ddlNewQuizCourse.Items.Clear();
            ddlNewQuizCourse.Items.Add(new ListItem("— Select Course —", ""));

            ddlFilterCourse.Items.Clear();
            ddlFilterCourse.Items.Add(new ListItem("— Select Course —", ""));

            foreach (DataRow row in courses.Rows)
            {
                string name = row["CourseName"].ToString();
                string id = row["CourseID"].ToString();
                ddlNewQuizCourse.Items.Add(new ListItem(name, id));
                ddlFilterCourse.Items.Add(new ListItem(name, id));
            }
        }

        private void LoadQuizDropdown()
        {
            string previous = ddlQuiz.SelectedValue;
            string courseId = ddlFilterCourse.SelectedValue;

            ddlQuiz.Items.Clear();
            ddlQuiz.Items.Add(new ListItem(
                string.IsNullOrEmpty(courseId) ? "— Select a course first —" : "— Select Quiz —", ""));

            // Only list quizzes once a course is chosen, so the picker stays short
            // instead of dumping every quiz the instructor owns.
            if (string.IsNullOrEmpty(courseId)) return;

            DataTable quizzes = QuizService.GetQuizzesByInstructor(CurrentInstructorId, courseId);
            foreach (DataRow row in quizzes.Rows)
            {
                ddlQuiz.Items.Add(new ListItem(
                    row["QuizTitle"].ToString(),
                    row["QuizID"].ToString()));
            }

            // Preserve the previously-selected quiz across postbacks where possible.
            if (!string.IsNullOrEmpty(previous) &&
                ddlQuiz.Items.FindByValue(previous) != null)
            {
                ddlQuiz.SelectedValue = previous;
            }
        }

        // ---------------------------------------------------------
        // Question bank
        // ---------------------------------------------------------
        private void LoadQuestions()
        {
            DataTable list = QuizService.GetQuestions(
                CurrentInstructorId,
                ddlQuiz.SelectedValue,
                tbSearch.Text.Trim(),
                ddlFilterType.SelectedValue);

            litCount.Text = list.Rows.Count.ToString();
            rptQuestions.DataSource = list;
            rptQuestions.DataBind();
            pnlEmpty.Visible = list.Rows.Count == 0;
        }

        /// <summary>
        /// Reads a schedule field. Blank means "not set" and is fine; anything that
        /// cannot be understood is reported rather than silently dropped, which is what
        /// used to make start/end dates look like they did nothing.
        /// </summary>
        private static bool TryReadDate(string text, string label, out DateTime? value, out string error)
        {
            value = null;
            error = "";

            if (string.IsNullOrWhiteSpace(text)) return true;
            text = text.Trim();

            // <input type="datetime-local"> posts ISO ("2026-08-01T09:00"). When the
            // browser falls back to a plain text box the lecturer may type a local
            // format instead, so accept the common ones too.
            string[] formats =
            {
                "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd",
                "dd/MM/yyyy HH:mm", "dd/MM/yyyy", "d/M/yyyy HH:mm", "d/M/yyyy",
                "MM/dd/yyyy HH:mm", "MM/dd/yyyy"
            };

            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime exact))
            { value = exact; return true; }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime inv))
            { value = inv; return true; }

            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime cur))
            { value = cur; return true; }

            error = $"{label} '{text}' is not a valid date. Use the date picker, or type it as dd/MM/yyyy.";
            return false;
        }

        // ---------------------------------------------------------
        // Create a new quiz
        // ---------------------------------------------------------
        protected void btnCreateQuiz_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            decimal passMark = 50m;
            if (!string.IsNullOrWhiteSpace(tbNewPassMark.Text))
                decimal.TryParse(tbNewPassMark.Text, out passMark);
            if (passMark < 0 || passMark > 100)
            { ShowError("Pass mark must be between 0 and 100."); return; }

            int maxAttempts = 3;
            if (!string.IsNullOrWhiteSpace(tbNewMaxAttempts.Text))
                int.TryParse(tbNewMaxAttempts.Text, out maxAttempts);
            if (maxAttempts < 1) maxAttempts = 1;

            // Schedule and duration are optional; only validate what was filled in.
            DateTime? startDate = null, endDate = null;
            if (!TryReadDate(tbNewStartDate.Text, "Start date", out startDate, out string dateError))
            { ShowError(dateError); return; }
            if (!TryReadDate(tbNewEndDate.Text, "End date", out endDate, out dateError))
            { ShowError(dateError); return; }

            if (startDate.HasValue && endDate.HasValue && endDate.Value <= startDate.Value)
            { ShowError("End date must be after the start date."); return; }

            int? duration = null;
            if (!string.IsNullOrWhiteSpace(tbNewDuration.Text) &&
                int.TryParse(tbNewDuration.Text, out int parsedDuration))
            {
                if (parsedDuration < 1)
                { ShowError("Duration must be at least 1 minute."); return; }
                duration = parsedDuration;
            }

            int? totalMarks = null;
            if (!string.IsNullOrWhiteSpace(tbNewTotalMarks.Text) &&
                int.TryParse(tbNewTotalMarks.Text, out int parsedTotal))
            {
                if (parsedTotal < 0)
                { ShowError("Total marks cannot be negative."); return; }
                totalMarks = parsedTotal;
            }

            try
            {
                string newCourseId = ddlNewQuizCourse.SelectedValue;
                string newId = QuizService.CreateQuiz(
                    CurrentInstructorId,
                    newCourseId,
                    tbNewQuizTitle.Text.Trim(),
                    passMark, maxAttempts,
                    tbNewQuizDescription.Text.Trim(),
                    startDate, endDate, duration, totalMarks);

                // Point the course filter at the new quiz's course, otherwise the quiz
                // picker (which lists one course at a time) would not contain it.
                if (ddlFilterCourse.Items.FindByValue(newCourseId) != null)
                    ddlFilterCourse.SelectedValue = newCourseId;

                LoadQuizDropdown();
                ddlQuiz.SelectedValue = newId;

                tbNewQuizTitle.Text = "";
                tbNewQuizDescription.Text = "";
                tbNewStartDate.Text = "";
                tbNewEndDate.Text = "";
                tbNewDuration.Text = "";
                tbNewTotalMarks.Text = "";
                tbNewPassMark.Text = "50";
                tbNewMaxAttempts.Text = "3";
                ddlNewQuizCourse.SelectedIndex = 0;

                // Flush anything staged before the quiz existed.
                int committed = PendingAttachmentService.Commit(AttachBucket, "Quiz", newId, CurrentInstructorId);
                ShowSuccess(committed > 0
                    ? $"Quiz created with {committed} attachment(s)."
                    : "Quiz created. You can now add questions and attachments to it.");

                LoadQuestions();
                LoadAttachments(newId);
                LoadQuizDetails(newId);
                ShowMarksSummary();
            }
            catch (Exception ex)
            {
                ShowError("Could not create the quiz. " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // Edit the selected quiz's details
        // ---------------------------------------------------------

        /// <summary>Fills the edit panel from the selected quiz, or hides it.</summary>
        private void LoadQuizDetails(string quizId)
        {
            if (string.IsNullOrEmpty(quizId))
            { pnlEditQuiz.Visible = false; return; }

            DataRow quiz = QuizService.GetQuizById(quizId, CurrentInstructorId);
            if (quiz == null)
            { pnlEditQuiz.Visible = false; return; }

            pnlEditQuiz.Visible = true;

            tbEditTitle.Text = Convert.ToString(quiz["Title"]);
            tbEditDescription.Text = quiz["Description"] == DBNull.Value ? "" : Convert.ToString(quiz["Description"]);
            tbEditPassMark.Text = Convert.ToString(quiz["PassMark"]);
            tbEditMaxAttempts.Text = Convert.ToString(quiz["MaxAttempts"]);
            tbEditDuration.Text = quiz["DurationMinutes"] == DBNull.Value ? "" : Convert.ToString(quiz["DurationMinutes"]);
            tbEditTotalMarks.Text = quiz["TotalMarks"] == DBNull.Value ? "" : Convert.ToString(quiz["TotalMarks"]);

            // Shown in the same shape the placeholder asks for, so it can be edited by hand.
            tbEditStartDate.Text = quiz["StartDate"] == DBNull.Value
                ? "" : Convert.ToDateTime(quiz["StartDate"]).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            tbEditEndDate.Text = quiz["EndDate"] == DBNull.Value
                ? "" : Convert.ToDateTime(quiz["EndDate"]).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        protected void btnUpdateQuiz_Click(object sender, EventArgs e)
        {
            string quizId = ddlQuiz.SelectedValue;
            if (string.IsNullOrEmpty(quizId))
            { ShowError("Select a quiz to edit."); return; }

            string title = tbEditTitle.Text.Trim();
            if (title.Length == 0)
            { ShowError("Quiz title is required."); return; }

            decimal passMark = 50m;
            if (!string.IsNullOrWhiteSpace(tbEditPassMark.Text))
                decimal.TryParse(tbEditPassMark.Text, out passMark);
            if (passMark < 0 || passMark > 100)
            { ShowError("Pass mark must be between 0 and 100."); return; }

            int maxAttempts = 3;
            if (!string.IsNullOrWhiteSpace(tbEditMaxAttempts.Text))
                int.TryParse(tbEditMaxAttempts.Text, out maxAttempts);
            if (maxAttempts < 1) maxAttempts = 1;

            if (!TryReadDate(tbEditStartDate.Text, "Start date", out DateTime? startDate, out string dateError))
            { ShowError(dateError); return; }
            if (!TryReadDate(tbEditEndDate.Text, "End date", out DateTime? endDate, out dateError))
            { ShowError(dateError); return; }

            if (startDate.HasValue && endDate.HasValue && endDate.Value <= startDate.Value)
            { ShowError("End date must be after the start date."); return; }

            int? duration = null;
            if (!string.IsNullOrWhiteSpace(tbEditDuration.Text) &&
                int.TryParse(tbEditDuration.Text, out int parsedDuration))
            {
                if (parsedDuration < 1)
                { ShowError("Duration must be at least 1 minute."); return; }
                duration = parsedDuration;
            }

            int? totalMarks = null;
            if (!string.IsNullOrWhiteSpace(tbEditTotalMarks.Text) &&
                int.TryParse(tbEditTotalMarks.Text, out int parsedTotal))
            {
                if (parsedTotal < 0)
                { ShowError("Total marks cannot be negative."); return; }
                totalMarks = parsedTotal;
            }

            // Lowering the total below what the questions already use would leave the
            // quiz permanently unbalanced, so block it rather than silently allow it.
            QuizService.MarkSummary marks = QuizService.GetMarkSummary(quizId);
            if (totalMarks.HasValue && totalMarks.Value < marks.AllocatedMarks)
            {
                ShowError($"This quiz's questions already use {marks.AllocatedMarks} marks, " +
                          $"so the total cannot be set to {totalMarks.Value}.");
                return;
            }

            int rows = QuizService.UpdateQuiz(quizId, CurrentInstructorId, title,
                passMark, maxAttempts, tbEditDescription.Text.Trim(),
                startDate, endDate, duration, totalMarks);

            if (rows == 0)
            { ShowError("Quiz not found, or it doesn't belong to you."); return; }

            ShowSuccess("Quiz details updated.");
            LoadQuizDropdown();
            ddlQuiz.SelectedValue = quizId;
            LoadQuizDetails(quizId);
            LoadQuestions();
            ShowMarksSummary();
        }

        // ---------------------------------------------------------
        // Delete the selected quiz
        // ---------------------------------------------------------
        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            string quizId = ddlQuiz.SelectedValue;
            if (string.IsNullOrEmpty(quizId))
            { ShowError("Please select a quiz to delete."); return; }

            int result = QuizService.DeleteQuiz(quizId, CurrentInstructorId);
            if (result == 1)
            {
                ShowSuccess("Quiz and its questions deleted.");
                ClearBulkRows();
                LoadQuizDropdown();
                LoadQuestions();
                LoadAttachments(ddlQuiz.SelectedValue);
            }
            else if (result == -1)
            {
                ShowError("This quiz can't be deleted because students have already attempted it.");
            }
            else
            {
                ShowError("Quiz not found, or it doesn't belong to you.");
            }
        }

        // ---------------------------------------------------------
        // Filters
        // ---------------------------------------------------------
        protected void ddlFilterCourse_Changed(object sender, EventArgs e)
        {
            // Changing course invalidates the current quiz selection.
            PendingAttachmentService.Clear(AttachBucket);
            LoadQuizDropdown();
            ClearBulkRows();
            LoadQuestions();
            LoadAttachments(ddlQuiz.SelectedValue);
            LoadQuizDetails(ddlQuiz.SelectedValue);
        }

        protected void ddlQuiz_Changed(object sender, EventArgs e)
        {
            // Selecting an existing quiz abandons anything staged for a not-yet-created
            // quiz, so it is not silently attached to the quiz just picked.
            if (!string.IsNullOrEmpty(ddlQuiz.SelectedValue))
                PendingAttachmentService.Clear(AttachBucket);

            LoadQuestions();
            LoadAttachments(ddlQuiz.SelectedValue);
            LoadQuizDetails(ddlQuiz.SelectedValue);
            ShowMarksSummary();
        }
        protected void tbSearch_TextChanged(object sender, EventArgs e) => LoadQuestions();
        protected void ddlFilterType_Changed(object sender, EventArgs e) => LoadQuestions();

        /// <summary>
        /// Question bank actions. Questions are authored in the bulk form, so delete is
        /// the only action here; to change a question, delete it and add it again.
        /// </summary>
        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Edit") { LoadQuestionIntoRow(e.CommandArgument.ToString()); return; }
            if (e.CommandName != "Delete") return;

            string questionId = e.CommandArgument.ToString();
            int result = QuizService.DeleteQuestion(questionId, CurrentInstructorId);

            if (result == 1)
                ShowSuccess("Question deleted.");
            else if (result == -1)
                ShowError("This question can't be deleted because students have already answered it.");
            else
                ShowError("Question not found, or it doesn't belong to you.");

            LoadQuestions();
        }

        // ---------------------------------------------------------
        // Attachments (articles, pictures, media links, documents)
        // ---------------------------------------------------------
        /// <summary>Session bucket holding attachments staged before a quiz exists.</summary>
        private const string AttachBucket = "Quiz";
        // =========================================================
        // Bulk question entry: build several questions, save in one go
        // =========================================================

        /// <summary>One unsaved question row in the bulk-add grid.</summary>
        [Serializable]
        public class BulkRow
        {
            /// <summary>
            /// Stable per-row id. Attachments staged for this row live in their own
            /// bucket keyed on it, so removing or reordering rows cannot mix them up.
            /// </summary>
            public string RowKey { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

            /// <summary>How many attachments are staged for this row (bound by the markup).</summary>
            public int AttachmentCount => PendingAttachmentService.Count(RowBucket(RowKey));

            public string Type { get; set; } = "MCQ";
            public string Text { get; set; } = "";
            public int Marks { get; set; } = 5;
            public string OptA { get; set; } = "";
            public string OptB { get; set; } = "";
            public string OptC { get; set; } = "";
            public string OptD { get; set; } = "";
            public bool CorrectA { get; set; }
            public bool CorrectB { get; set; }
            public bool CorrectC { get; set; }
            public bool CorrectD { get; set; }
            public string AnswerText { get; set; } = "";
            public string TfAnswer { get; set; } = "True";

            /// <summary>Optional rationale shown to students after they submit.</summary>
            public string Explanation { get; set; } = "";

            /// <summary>
            /// Set when this row is editing an existing question rather than creating a
            /// new one; Save All then updates that question in place.
            /// </summary>
            public string QuestionID { get; set; } = "";

            public bool IsEdit => !string.IsNullOrEmpty(QuestionID);

            /// <summary>Marks this question already contributes to the quiz (edit rows only).</summary>
            public int ExistingMarks { get; set; }
        }

        private const string BulkRowsKey = "BulkQuestionRows";

        /// <summary>Staging bucket name for one bulk row's attachments.</summary>
        private static string RowBucket(string rowKey) => "QuestionRow_" + rowKey;

        /// <summary>
        /// Shows how the selected quiz's marks are accounted for: what it is out of,
        /// what its questions already use, and what the unsaved rows would add.
        /// </summary>
        private void ShowMarksSummary()
        {
            string quizId = ddlQuiz.SelectedValue;
            if (string.IsNullOrEmpty(quizId))
            {
                pnlMarks.CssClass = "marks-summary";
                litMarks.Text = "Select a quiz to see how its marks add up.";
                return;
            }

            QuizService.MarkSummary summary = QuizService.GetMarkSummary(quizId);

            // A row editing an existing question replaces that question's marks rather
            // than adding to them, so only count the difference.
            int pending = 0;
            foreach (BulkRow row in BulkRows) pending += row.Marks - row.ExistingMarks;

            if (!summary.TotalMarks.HasValue)
            {
                pnlMarks.CssClass = "marks-summary";
                litMarks.Text =
                    $"This quiz has no total marks set. Its {summary.QuestionCount} question(s) " +
                    $"currently add up to <strong>{summary.AllocatedMarks}</strong>.";
                return;
            }

            int total = summary.TotalMarks.Value;
            int used = summary.AllocatedMarks;
            int projected = used + pending;

            string basis =
                $"Quiz is out of <strong>{total}</strong> marks &middot; " +
                $"{summary.QuestionCount} saved question(s) use <strong>{used}</strong>";

            if (pending > 0)
                basis += $" &middot; unsaved rows add <strong>{pending}</strong> (total {projected})";

            if (projected > total)
            {
                pnlMarks.CssClass = "marks-summary warn";
                litMarks.Text = $"{basis}. That is <strong>{projected - total}</strong> over the quiz total.";
            }
            else if (projected == total)
            {
                pnlMarks.CssClass = "marks-summary ok";
                litMarks.Text = $"{basis}. The marks add up exactly.";
            }
            else
            {
                pnlMarks.CssClass = "marks-summary";
                litMarks.Text = $"{basis}. <strong>{total - projected}</strong> mark(s) still to allocate.";
            }
        }

        /// <summary>Drops every bulk row and the attachments staged against them.</summary>
        private void ClearBulkRows()
        {
            foreach (BulkRow row in BulkRows)
                PendingAttachmentService.Clear(RowBucket(row.RowKey));

            BulkRows.Clear();
            BindBulkRows();
        }

        private List<BulkRow> BulkRows
        {
            get
            {
                var rows = Session[BulkRowsKey] as List<BulkRow>;
                if (rows == null) { rows = new List<BulkRow>(); Session[BulkRowsKey] = rows; }
                return rows;
            }
        }

        private void BindBulkRows()
        {
            rptBulk.DataSource = BulkRows;
            rptBulk.DataBind();
            pnlBulkEmpty.Visible = BulkRows.Count == 0;

            // Show each row's staged attachments in its own nested repeater.
            for (int i = 0; i < rptBulk.Items.Count && i < BulkRows.Count; i++)
            {
                var nested = (Repeater)rptBulk.Items[i].FindControl("rptRowAtt");
                if (nested == null) continue;

                nested.DataSource = PendingAttachmentService.BuildDisplayTable(null, RowBucket(BulkRows[i].RowKey));
                nested.DataBind();
            }

            ShowMarksSummary();
        }

        /// <summary>
        /// Copies what the user has typed back into the session rows. Must run before
        /// any rebind, otherwise adding or removing a row would discard their input.
        /// </summary>
        private void CaptureBulkRows()
        {
            var rows = BulkRows;
            for (int i = 0; i < rptBulk.Items.Count && i < rows.Count; i++)
            {
                RepeaterItem item = rptBulk.Items[i];
                BulkRow row = rows[i];

                row.Type = ((DropDownList)item.FindControl("ddlBulkType")).SelectedValue;
                row.Text = ((TextBox)item.FindControl("tbBulkText")).Text.Trim();

                string marksText = ((TextBox)item.FindControl("tbBulkMarks")).Text;
                row.Marks = int.TryParse(marksText, out int m) && m > 0 ? m : 5;

                row.OptA = ((TextBox)item.FindControl("tbBulkOptA")).Text.Trim();
                row.OptB = ((TextBox)item.FindControl("tbBulkOptB")).Text.Trim();
                row.OptC = ((TextBox)item.FindControl("tbBulkOptC")).Text.Trim();
                row.OptD = ((TextBox)item.FindControl("tbBulkOptD")).Text.Trim();

                row.CorrectA = ((CheckBox)item.FindControl("cbBulkA")).Checked;
                row.CorrectB = ((CheckBox)item.FindControl("cbBulkB")).Checked;
                row.CorrectC = ((CheckBox)item.FindControl("cbBulkC")).Checked;
                row.CorrectD = ((CheckBox)item.FindControl("cbBulkD")).Checked;

                row.AnswerText = ((TextBox)item.FindControl("tbBulkAnswer")).Text.Trim();
                row.TfAnswer = ((DropDownList)item.FindControl("ddlBulkTF")).SelectedValue;
                row.Explanation = ((TextBox)item.FindControl("tbBulkExplanation")).Text.Trim();
            }
        }

        /// <summary>
        /// Stages any file still sitting in a row's picker. Lecturers reasonably expect
        /// a chosen file to be included when they hit Save All, without having pressed
        /// that row's Upload button first — this picks those up.
        /// Returns the names of files that were rejected.
        /// </summary>
        private List<string> StagePickedRowFiles()
        {
            var rejected = new List<string>();

            for (int i = 0; i < rptBulk.Items.Count && i < BulkRows.Count; i++)
            {
                var picker = (FileUpload)rptBulk.Items[i].FindControl("fuRowFiles");
                if (picker == null || !picker.HasFiles) continue;

                PendingAttachmentService.StageFiles(
                    picker.PostedFiles, RowBucket(BulkRows[i].RowKey), out List<string> rowRejected);

                foreach (string r in rowRejected) rejected.Add($"Row #{i + 1}: {r}");
            }

            return rejected;
        }

        protected void btnBulkAddRow_Click(object sender, EventArgs e)
        {
            CaptureBulkRows();
            BulkRows.Add(new BulkRow());
            BindBulkRows();
        }

        /// <summary>
        /// Loads an existing question into the builder as an "editing" row. Saving the
        /// batch updates that question in place rather than creating a duplicate.
        /// </summary>
        private void LoadQuestionIntoRow(string questionId)
        {
            CaptureBulkRows();

            // Already open for editing? Don't add it twice.
            foreach (BulkRow existing in BulkRows)
            {
                if (existing.QuestionID == questionId)
                { ShowError("That question is already open in the editor below."); BindBulkRows(); return; }
            }

            DataRow q = QuizService.GetQuestionById(questionId, CurrentInstructorId);
            if (q == null) { ShowError("Question not found, or it doesn't belong to you."); BindBulkRows(); return; }

            string type = Convert.ToString(q["QuestionType"]);
            string correct = Convert.ToString(q["CorrectAnswer"]);

            var row = new BulkRow
            {
                QuestionID = questionId,
                Type = type,
                Text = Convert.ToString(q["QuestionText"]),
                Marks = Convert.ToInt32(q["Points"]),
                ExistingMarks = Convert.ToInt32(q["Points"]),
                Explanation = q["Explanation"] == DBNull.Value ? "" : Convert.ToString(q["Explanation"])
            };

            if (type == "MCQ")
            {
                row.OptA = q["OptionA"] == DBNull.Value ? "" : Convert.ToString(q["OptionA"]);
                row.OptB = q["OptionB"] == DBNull.Value ? "" : Convert.ToString(q["OptionB"]);
                row.OptC = q["OptionC"] == DBNull.Value ? "" : Convert.ToString(q["OptionC"]);
                row.OptD = q["OptionD"] == DBNull.Value ? "" : Convert.ToString(q["OptionD"]);

                var keys = new List<string>(correct.Split(','));
                for (int i = 0; i < keys.Count; i++) keys[i] = keys[i].Trim().ToUpperInvariant();
                row.CorrectA = keys.Contains("A");
                row.CorrectB = keys.Contains("B");
                row.CorrectC = keys.Contains("C");
                row.CorrectD = keys.Contains("D");
            }
            else if (type == "Structure")
            {
                row.AnswerText = correct;
            }
            else
            {
                row.TfAnswer = correct == "False" ? "False" : "True";
            }

            BulkRows.Add(row);
            BindBulkRows();
            ShowSuccess("Question loaded below. Change it and press Save All Questions.");
        }

        protected void rptBulk_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            CaptureBulkRows();

            int index = Convert.ToInt32(e.CommandArgument);
            if (index < 0 || index >= BulkRows.Count) { BindBulkRows(); return; }

            BulkRow row = BulkRows[index];
            string bucket = RowBucket(row.RowKey);
            int rowNumber = index + 1;

            switch (e.CommandName)
            {
                case "RemoveRow":
                    // Drop the row's staged files too, so they cannot leak into a later batch.
                    PendingAttachmentService.Clear(bucket);
                    BulkRows.RemoveAt(index);
                    break;

                case "RowUpload":
                {
                    var picker = (FileUpload)e.Item.FindControl("fuRowFiles");
                    if (picker == null || !picker.HasFiles)
                    { ShowError($"Row #{rowNumber}: choose at least one file to upload."); break; }

                    int staged = PendingAttachmentService.StageFiles(
                        picker.PostedFiles, bucket, out List<string> rejected);

                    if (staged > 0 && rejected.Count == 0)
                        ShowSuccess($"Row #{rowNumber}: {staged} file(s) ready — saved when you save the questions.");
                    else if (staged > 0)
                        ShowError($"Row #{rowNumber}: {staged} file(s) ready. Skipped: {string.Join(", ", rejected)}");
                    else
                        ShowError($"Row #{rowNumber}: no files added. {string.Join(", ", rejected)}".Trim());
                    break;
                }

                case "RowAddLink":
                {
                    var titleBox = (TextBox)e.Item.FindControl("tbRowLinkTitle");
                    var urlBox = (TextBox)e.Item.FindControl("tbRowLinkUrl");
                    string title = titleBox?.Text.Trim() ?? "";
                    string url = urlBox?.Text.Trim() ?? "";

                    if (title.Length == 0 || url.Length == 0)
                    { ShowError($"Row #{rowNumber}: both a link title and URL are required."); break; }
                    if (!Regex.IsMatch(url, @"^https?://[^\s]{4,}$"))
                    { ShowError($"Row #{rowNumber}: enter a valid URL starting with http:// or https://"); break; }

                    PendingAttachmentService.StageLink(bucket, title, url);
                    ShowSuccess($"Row #{rowNumber}: link ready — saved when you save the questions.");
                    break;
                }

                case "RowClearAtt":
                    PendingAttachmentService.Clear(bucket);
                    ShowSuccess($"Row #{rowNumber}: attachments cleared.");
                    break;
            }

            BindBulkRows();
        }

        protected void btnBulkClear_Click(object sender, EventArgs e)
        {
            ClearBulkRows();
            ShowSuccess("Unsaved rows discarded.");
        }

        /// <summary>
        /// Validates every row, then writes them all to the selected quiz. Nothing is
        /// saved unless all rows are valid, so a batch never lands half-finished.
        /// </summary>
        protected void btnBulkSaveAll_Click(object sender, EventArgs e)
        {
            CaptureBulkRows();

            // Pick up files chosen but never pushed with a row's Upload button.
            List<string> pickerRejects = StagePickedRowFiles();

            string quizId = ddlQuiz.SelectedValue;
            if (string.IsNullOrEmpty(quizId))
            { ShowError("Select a course and quiz before saving questions."); BindBulkRows(); return; }

            if (BulkRows.Count == 0)
            { ShowError("Add at least one row first."); BindBulkRows(); return; }

            // ---- validate everything up front ----
            var prepared = new List<(BulkRow Row, string CorrectAnswer)>();
            for (int i = 0; i < BulkRows.Count; i++)
            {
                BulkRow row = BulkRows[i];
                int n = i + 1;

                if (string.IsNullOrWhiteSpace(row.Text))
                { ShowError($"Row #{n}: question text is required."); BindBulkRows(); return; }

                string correct;
                if (row.Type == "MCQ")
                {
                    if (row.OptA.Length == 0 || row.OptB.Length == 0)
                    { ShowError($"Row #{n}: options A and B are both required for an MCQ."); BindBulkRows(); return; }

                    var keys = new List<string>();
                    if (row.CorrectA) keys.Add("A");
                    if (row.CorrectB) keys.Add("B");
                    if (row.CorrectC) keys.Add("C");
                    if (row.CorrectD) keys.Add("D");

                    if (keys.Count == 0)
                    { ShowError($"Row #{n}: tick at least one correct option."); BindBulkRows(); return; }
                    if (keys.Contains("C") && row.OptC.Length == 0)
                    { ShowError($"Row #{n}: option C is ticked but has no text."); BindBulkRows(); return; }
                    if (keys.Contains("D") && row.OptD.Length == 0)
                    { ShowError($"Row #{n}: option D is ticked but has no text."); BindBulkRows(); return; }

                    correct = string.Join(",", keys);
                }
                else if (row.Type == "Structure")
                {
                    if (string.IsNullOrWhiteSpace(row.AnswerText))
                    { ShowError($"Row #{n}: a structure question needs an answer."); BindBulkRows(); return; }
                    correct = row.AnswerText;
                }
                else // TrueFalse
                {
                    correct = row.TfAnswer == "False" ? "False" : "True";
                }

                prepared.Add((row, correct));
            }

            // ---- the questions' marks must not exceed what the quiz is out of ----
            QuizService.MarkSummary marks = QuizService.GetMarkSummary(quizId);
            if (marks.TotalMarks.HasValue)
            {
                // Edit rows replace their question's existing marks; new rows add.
                int batch = 0;
                foreach (var (row, _) in prepared) batch += row.Marks - row.ExistingMarks;
                int projected = marks.AllocatedMarks + batch;

                if (projected > marks.TotalMarks.Value)
                {
                    ShowError(
                        $"These {prepared.Count} question(s) would take the quiz to " +
                        $"{projected} of {marks.TotalMarks.Value} marks. Reduce them by " +
                        $"{projected - marks.TotalMarks.Value}, or raise the quiz total.");
                    BindBulkRows();
                    return;
                }
            }

            // ---- all valid: save ----
            try
            {
                int attachmentsSaved = 0;

                foreach (var (row, correct) in prepared)
                {
                    bool isMcq = row.Type == "MCQ";
                    // A row carrying a QuestionID updates that question instead of inserting.
                    string questionId = QuizService.SaveQuestion(
                        row.QuestionID, CurrentInstructorId, quizId,
                        row.Type, row.Text,
                        isMcq ? row.OptA : "", isMcq ? row.OptB : "",
                        isMcq ? row.OptC : "", isMcq ? row.OptD : "",
                        correct,
                        row.Type == "Structure" ? "ExactIgnoreCase" : null,
                        row.Explanation, row.Marks);

                    // Flush the files/links staged against this row onto its new question.
                    attachmentsSaved += PendingAttachmentService.Commit(
                        RowBucket(row.RowKey), "Question", questionId, CurrentInstructorId);
                }

                int updated = 0, added = 0;
                foreach (var (row, _) in prepared) { if (row.IsEdit) updated++; else added++; }

                ClearBulkRows();
                LoadQuestions();

                var parts = new List<string>();
                if (added > 0) parts.Add($"{added} question(s) added");
                if (updated > 0) parts.Add($"{updated} question(s) updated");
                if (attachmentsSaved > 0) parts.Add($"{attachmentsSaved} attachment(s) saved");
                string message = string.Join(", ", parts) + ".";

                if (pickerRejects.Count > 0)
                    message += $" Skipped: {string.Join("; ", pickerRejects)}.";

                // Tell the lecturer where the quiz now stands against its total.
                QuizService.MarkSummary after = QuizService.GetMarkSummary(quizId);
                if (after.TotalMarks.HasValue)
                {
                    message += after.IsBalanced
                        ? $" Marks now add up to {after.TotalMarks.Value} exactly."
                        : $" {after.RemainingMarks} mark(s) of {after.TotalMarks.Value} still to allocate.";
                }

                ShowSuccess(message);
            }
            catch (Exception ex)
            {
                BindBulkRows();
                ShowError("Could not save the questions. " + ex.Message);
            }
        }

        // Bound by the attachment repeater markup.
        public string GetAttachmentHref(object type, object filePath, object linkUrl, object isPending)
            => PendingAttachmentService.DisplayHref(type, filePath, linkUrl, isPending);

        public string GetAttachmentMeta(object type, object by, object at, object isPending)
            => PendingAttachmentService.DisplayMeta(type, by, at, isPending);

        private void LoadAttachments(string quizId)
        {
            // The upload controls are always available: anything chosen before a quiz
            // is selected/created is staged and committed when the quiz is created.
            pnlAttachments.Visible = true;

            DataTable committed = string.IsNullOrEmpty(quizId)
                ? null
                : AttachmentService.GetByQuiz(quizId);

            DataTable dt = PendingAttachmentService.BuildDisplayTable(committed, AttachBucket);
            rptAttachments.DataSource = dt;
            rptAttachments.DataBind();
            litAttCount.Text = dt.Rows.Count.ToString();
            pnlNoAttachments.Visible = dt.Rows.Count == 0;
        }

        protected void btnUploadFiles_Click(object sender, EventArgs e)
        {
            string quizId = ddlQuiz.SelectedValue;

            if (!fuAttachFiles.HasFiles)
            { ShowError("Choose at least one file to upload."); LoadAttachments(quizId); return; }

            List<string> rejected;
            int saved;
            bool staged = string.IsNullOrEmpty(quizId);

            if (staged)
                saved = PendingAttachmentService.StageFiles(fuAttachFiles.PostedFiles, AttachBucket, out rejected);
            else
                saved = AttachmentService.SaveFiles(fuAttachFiles.PostedFiles, "Quiz", quizId, CurrentInstructorId, out rejected);

            string verb = staged ? "file(s) ready — saved when you create the quiz." : "file(s) uploaded.";
            if (saved > 0 && rejected.Count == 0)
                ShowSuccess($"{saved} {verb}");
            else if (saved > 0)
                ShowError($"{saved} {verb} Skipped: {string.Join(", ", rejected)}");
            else
                ShowError(rejected.Count > 0 ? $"No files added. Skipped: {string.Join(", ", rejected)}" : "No files added.");

            LoadAttachments(quizId);
        }

        protected void btnAddLink_Click(object sender, EventArgs e)
        {
            string quizId = ddlQuiz.SelectedValue;

            string title = tbLinkTitle.Text.Trim();
            string url = tbLinkUrl.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
            { ShowError("Both a link title and URL are required."); LoadAttachments(quizId); return; }

            if (!Regex.IsMatch(url, @"^https?://[^\s]{4,}$"))
            { ShowError("Enter a valid URL starting with http:// or https://"); LoadAttachments(quizId); return; }

            if (string.IsNullOrEmpty(quizId))
            {
                PendingAttachmentService.StageLink(AttachBucket, title, url);
                ShowSuccess("Link ready — saved when you create the quiz.");
            }
            else
            {
                AttachmentService.SaveLink("Quiz", quizId, title, url, CurrentInstructorId);
                ShowSuccess("Link added.");
            }

            tbLinkTitle.Text = "";
            tbLinkUrl.Text = "";
            LoadAttachments(quizId);
        }

        protected void rptAttachments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                string attachmentId = e.CommandArgument.ToString();

                bool removed = attachmentId.StartsWith("PND")
                    ? PendingAttachmentService.Remove(AttachBucket, attachmentId)
                    : AttachmentService.Delete(attachmentId, CurrentInstructorId);

                ShowSuccess(removed ? "Attachment removed." : "Attachment not found.");
                LoadAttachments(ddlQuiz.SelectedValue);
            }
        }

        public string GetAttachmentIcon(string type) =>
            type == "Image" ? "ti-photo" : type == "Link" ? "ti-link" : "ti-file-text";

        // ---------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------
        private void ShowSuccess(string msg)
        {
            pnlError.Visible = false;
            pnlSuccess.Visible = true;
            litSuccess.Text = msg;
        }

        private void ShowError(string msg)
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;
            litError.Text = msg;
        }

        public string GetTypeBadge(string t) =>
            t == "MCQ" ? "badge-blue" : t == "Structure" ? "badge-amber" : "badge-green";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}