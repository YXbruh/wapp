using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;

namespace CSA.Services
{
    /// <summary>
    /// Data-access layer for Quizzes and QuizQuestions (Quiz Editor page).
    /// All methods use parameterised ADO.NET commands to prevent SQL injection.
    /// </summary>
    public static class QuizService
    {
        /// <summary>
        /// Returns quizzes owned by an instructor, shaped for the ddlQuiz / ddlNewQuizCourse
        /// dropdowns. Columns: QuizID, DisplayName ("Quiz Title — Course Name").
        /// </summary>
        public static DataTable GetQuizzesByInstructor(string instructorId)
            => GetQuizzesByInstructor(instructorId, null);

        /// <summary>
        /// Quizzes owned by the instructor, optionally narrowed to one course so the
        /// quiz picker only lists quizzes belonging to the course chosen above it.
        /// </summary>
        public static DataTable GetQuizzesByInstructor(string instructorId, string courseId)
        {
            string sql = @"
                SELECT  q.QuizID,
                        q.Title AS QuizTitle,
                        q.Title + ' — ' + c.CourseName AS DisplayName
                FROM    Quizzes q
                INNER JOIN Courses c ON q.CourseID = c.CourseID
                WHERE   q.CreatedByID = @InstructorID
                  AND   (@CourseID = '' OR q.CourseID = @CourseID)
                ORDER BY q.CreatedAt DESC;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@InstructorID", instructorId),
                new SqlParameter("@CourseID", (object)courseId ?? ""));
        }

        /// <summary>
        /// How a quiz's marks are accounted for: what it is out of, and how much its
        /// questions currently add up to.
        /// </summary>
        public class MarkSummary
        {
            /// <summary>Marks the quiz is out of; null when the lecturer has not set one.</summary>
            public int? TotalMarks { get; set; }

            /// <summary>Sum of Points across the quiz's questions.</summary>
            public int AllocatedMarks { get; set; }

            public int QuestionCount { get; set; }

            /// <summary>Marks still to be handed out, or null when no total is set.</summary>
            public int? RemainingMarks => TotalMarks.HasValue ? TotalMarks.Value - AllocatedMarks : (int?)null;

            /// <summary>True when the questions add up to exactly the quiz total.</summary>
            public bool IsBalanced => TotalMarks.HasValue && RemainingMarks == 0;
        }

        /// <summary>Marks accounting for one quiz.</summary>
        public static MarkSummary GetMarkSummary(string quizId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT  q.TotalMarks,
                        ISNULL(SUM(qq.Points), 0) AS Allocated,
                        COUNT(qq.QuestionID)      AS QuestionCount
                FROM    Quizzes q
                LEFT JOIN QuizQuestions qq ON qq.QuizID = q.QuizID
                WHERE   q.QuizID = @QuizID
                GROUP BY q.TotalMarks;",
                new SqlParameter("@QuizID", quizId));

            if (dt.Rows.Count == 0) return new MarkSummary();

            DataRow row = dt.Rows[0];
            return new MarkSummary
            {
                TotalMarks = row["TotalMarks"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TotalMarks"]),
                AllocatedMarks = Convert.ToInt32(row["Allocated"]),
                QuestionCount = Convert.ToInt32(row["QuestionCount"])
            };
        }

        /// <summary>
        /// Chapters of one course, ordered the way students see them, for the quiz's
        /// Chapter picker. Columns: ChapterID, DisplayName ("3. Firewall Basics").
        /// </summary>
        public static DataTable GetChaptersForCourse(string courseId)
        {
            return DBHelper.ExecuteQuery(@"
                SELECT  ChapterID,
                        CAST(SortOrder AS NVARCHAR(10)) + '. ' + ChapterTitle AS DisplayName
                FROM    Chapters
                WHERE   CourseID = @CourseID
                ORDER BY SortOrder, ChapterTitle;",
                new SqlParameter("@CourseID", courseId ?? ""));
        }

        /// <summary>Full details of one quiz, for the edit form. Null if not owned.</summary>
        public static DataRow GetQuizById(string quizId, string instructorId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT QuizID, CourseID, ChapterID, Title, Description, StartDate, EndDate,
                       DurationMinutes, TotalMarks, MaxAttempts, PassMark, IsPublished
                FROM   Quizzes
                WHERE  QuizID = @QuizID AND CreatedByID = @InstructorID;",
                new SqlParameter("@QuizID", quizId),
                new SqlParameter("@InstructorID", instructorId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Updates an existing quiz's details. Only touches the quiz row — its questions
        /// and attachments are left alone. Returns rows affected (0 = not owned).
        /// </summary>
        public static int UpdateQuiz(
            string quizId, string instructorId, string title,
            decimal passMark, int maxAttempts,
            string description, DateTime? startDate, DateTime? endDate,
            int? durationMinutes, int? totalMarks)
            => UpdateQuiz(quizId, instructorId, title, passMark, maxAttempts,
                          description, startDate, endDate, durationMinutes, totalMarks, null);

        /// <summary>
        /// Updates an existing quiz, including which chapter it belongs to. A blank
        /// <paramref name="chapterId"/> stores NULL, i.e. a course-wide quiz.
        /// </summary>
        public static int UpdateQuiz(
            string quizId, string instructorId, string title,
            decimal passMark, int maxAttempts,
            string description, DateTime? startDate, DateTime? endDate,
            int? durationMinutes, int? totalMarks, string chapterId)
        {
            string sql = @"
                UPDATE Quizzes
                SET    ChapterID       = @ChapterID,
                       Title           = @Title,
                       Description     = @Description,
                       StartDate       = @StartDate,
                       EndDate         = @EndDate,
                       DurationMinutes = @DurationMinutes,
                       TotalMarks      = @TotalMarks,
                       MaxAttempts     = @MaxAttempts,
                       PassMark        = @PassMark,
                       UpdatedAt       = GETDATE()
                WHERE  QuizID = @QuizID AND CreatedByID = @InstructorID;";

            return DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@QuizID", quizId),
                new SqlParameter("@InstructorID", instructorId),
                new SqlParameter("@ChapterID", string.IsNullOrWhiteSpace(chapterId) ? (object)DBNull.Value : chapterId),
                new SqlParameter("@Title", title),
                new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description),
                new SqlParameter("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value),
                new SqlParameter("@EndDate", endDate.HasValue ? (object)endDate.Value : DBNull.Value),
                new SqlParameter("@DurationMinutes", durationMinutes.HasValue ? (object)durationMinutes.Value : DBNull.Value),
                new SqlParameter("@TotalMarks", totalMarks.HasValue ? (object)totalMarks.Value : DBNull.Value),
                new SqlParameter("@MaxAttempts", maxAttempts),
                new SqlParameter("@PassMark", passMark));
        }

        /// <summary>
        /// Creates a new quiz and returns the generated QuizID.
        /// </summary>
        public static string CreateQuiz(
            string instructorId, string courseId, string title,
            decimal passMark, int maxAttempts)
            => CreateQuiz(instructorId, courseId, title, passMark, maxAttempts, null, null, null, null);

        /// <summary>
        /// Creates a quiz with its full details. Description, schedule and duration are
        /// all optional: a quiz may exist with no questions and no window, carrying only
        /// an uploaded worksheet as its content.
        /// </summary>
        public static string CreateQuiz(
            string instructorId, string courseId, string title,
            decimal passMark, int maxAttempts,
            string description, DateTime? startDate, DateTime? endDate, int? durationMinutes)
            => CreateQuiz(instructorId, courseId, title, passMark, maxAttempts,
                          description, startDate, endDate, durationMinutes, null);

        /// <summary>
        /// Creates a quiz, including the marks it is out of. The Points of the quiz's
        /// questions are expected to add up to <paramref name="totalMarks"/>.
        /// </summary>
        public static string CreateQuiz(
            string instructorId, string courseId, string title,
            decimal passMark, int maxAttempts,
            string description, DateTime? startDate, DateTime? endDate, int? durationMinutes,
            int? totalMarks)
            => CreateQuiz(instructorId, courseId, null, title, passMark, maxAttempts,
                          description, startDate, endDate, durationMinutes, totalMarks);

        /// <summary>
        /// Creates a quiz attached to a specific chapter of the course. A blank
        /// <paramref name="chapterId"/> stores NULL, i.e. a course-wide quiz.
        /// </summary>
        public static string CreateQuiz(
            string instructorId, string courseId, string chapterId, string title,
            decimal passMark, int maxAttempts,
            string description, DateTime? startDate, DateTime? endDate, int? durationMinutes,
            int? totalMarks)
        {
            string quizId = IdGenerator.NewId("QUZ");

            string sql = @"
                INSERT INTO Quizzes
                    (QuizID, CourseID, ChapterID, Title, Description, StartDate, EndDate, DurationMinutes,
                     TotalMarks, MaxAttempts, PassMark, CreatedByID, IsPublished)
                VALUES
                    (@QuizID, @CourseID, @ChapterID, @Title, @Description, @StartDate, @EndDate, @DurationMinutes,
                     @TotalMarks, @MaxAttempts, @PassMark, @CreatedByID, 0);";

            DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@QuizID", quizId),
                new SqlParameter("@CourseID", courseId),
                new SqlParameter("@ChapterID", string.IsNullOrWhiteSpace(chapterId) ? (object)DBNull.Value : chapterId),
                new SqlParameter("@Title", title),
                new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description),
                new SqlParameter("@StartDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value),
                new SqlParameter("@EndDate", endDate.HasValue ? (object)endDate.Value : DBNull.Value),
                new SqlParameter("@DurationMinutes", durationMinutes.HasValue ? (object)durationMinutes.Value : DBNull.Value),
                new SqlParameter("@TotalMarks", totalMarks.HasValue ? (object)totalMarks.Value : DBNull.Value),
                new SqlParameter("@MaxAttempts", maxAttempts),
                new SqlParameter("@PassMark", passMark),
                new SqlParameter("@CreatedByID", instructorId));

            return quizId;
        }

        /// <summary>
        /// Returns questions belonging to an instructor's quizzes, shaped for the
        /// rptQuestions Repeater. Columns: QuestionID, QuestionType, TypeLabel, Points,
        /// QuizName, QuestionText. Optionally filtered by quiz, search text and type.
        /// </summary>
        public static DataTable GetQuestions(
            string instructorId, string quizId, string search, string filterType)
        {
            string sql = @"
                SELECT  qq.QuestionID,
                        qq.QuestionType,
                        CASE qq.QuestionType
                            WHEN 'MCQ'       THEN 'Multiple Choice'
                            WHEN 'Structure' THEN 'Structure'
                            WHEN 'TrueFalse' THEN 'True / False'
                            ELSE qq.QuestionType
                        END AS TypeLabel,
                        qq.Points,
                        q.Title AS QuizName,
                        qq.QuestionText
                FROM    QuizQuestions qq
                INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                WHERE   q.CreatedByID = @InstructorID
                        AND (@QuizID = '' OR qq.QuizID = @QuizID)
                        AND (@FilterType = '' OR qq.QuestionType = @FilterType)
                        AND (@Search = '' OR qq.QuestionText LIKE '%' + @Search + '%')
                ORDER BY q.Title, qq.SortOrder, qq.QuestionID;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@InstructorID", instructorId),
                new SqlParameter("@QuizID", quizId ?? ""),
                new SqlParameter("@FilterType", filterType ?? ""),
                new SqlParameter("@Search", search ?? ""));
        }

        /// <summary>
        /// Returns a single question by ID (used to populate the editor form on Edit).
        /// Returns null if not found or not owned by the instructor.
        /// </summary>
        public static DataRow GetQuestionById(string questionId, string instructorId)
        {
            string sql = @"
                SELECT  qq.QuestionID, qq.QuizID, qq.QuestionText, qq.QuestionType,
                        qq.OptionA, qq.OptionB, qq.OptionC, qq.OptionD,
                        qq.CorrectAnswer, qq.MatchStrategy, qq.Explanation, qq.Points
                FROM    QuizQuestions qq
                INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                WHERE   qq.QuestionID = @QuestionID AND q.CreatedByID = @InstructorID;";

            DataTable dt = DBHelper.ExecuteQuery(sql,
                new SqlParameter("@QuestionID", questionId),
                new SqlParameter("@InstructorID", instructorId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Inserts a new question or updates an existing one. The quiz must belong to
        /// the given instructor. Returns the QuestionID.
        /// </summary>
        public static string SaveQuestion(
            string questionId, string instructorId, string quizId,
            string questionType, string questionText,
            string optionA, string optionB, string optionC, string optionD,
            string correctAnswer, string matchStrategy, string explanation, int points)
        {
            bool isNew = string.IsNullOrEmpty(questionId);

            SqlParameter[] p =
            {
                new SqlParameter("@QuizID",        quizId),
                new SqlParameter("@QuestionText",  questionText),
                new SqlParameter("@QuestionType",  questionType),
                new SqlParameter("@OptionA",        string.IsNullOrEmpty(optionA) ? (object)DBNull.Value : optionA),
                new SqlParameter("@OptionB",        string.IsNullOrEmpty(optionB) ? (object)DBNull.Value : optionB),
                new SqlParameter("@OptionC",        string.IsNullOrEmpty(optionC) ? (object)DBNull.Value : optionC),
                new SqlParameter("@OptionD",        string.IsNullOrEmpty(optionD) ? (object)DBNull.Value : optionD),
                new SqlParameter("@CorrectAnswer", correctAnswer),
                new SqlParameter("@MatchStrategy", string.IsNullOrEmpty(matchStrategy) ? (object)DBNull.Value : matchStrategy),
                new SqlParameter("@Explanation",   string.IsNullOrEmpty(explanation) ? (object)DBNull.Value : explanation),
                new SqlParameter("@Points",        points),
                new SqlParameter("@InstructorID",  instructorId)
            };

            if (isNew)
            {
                questionId = IdGenerator.NewId("QUE");
                SqlParameter[] pInsert = new SqlParameter[p.Length + 1];
                p.CopyTo(pInsert, 0);
                pInsert[p.Length] = new SqlParameter("@QuestionID", questionId);

                string insert = @"
                    INSERT INTO QuizQuestions
                        (QuestionID, QuizID, QuestionText, QuestionType,
                         OptionA, OptionB, OptionC, OptionD,
                         CorrectAnswer, MatchStrategy, Explanation, Points)
                    SELECT @QuestionID, @QuizID, @QuestionText, @QuestionType,
                           @OptionA, @OptionB, @OptionC, @OptionD,
                           @CorrectAnswer, @MatchStrategy, @Explanation, @Points
                    WHERE EXISTS (SELECT 1 FROM Quizzes WHERE QuizID = @QuizID AND CreatedByID = @InstructorID);";

                DBHelper.ExecuteNonQuery(insert, pInsert);
                return questionId;
            }
            else
            {
                SqlParameter[] pUpdate = new SqlParameter[p.Length + 1];
                p.CopyTo(pUpdate, 0);
                pUpdate[p.Length] = new SqlParameter("@QuestionID", questionId);

                string update = @"
                    UPDATE qq SET
                        QuizID        = @QuizID,
                        QuestionText  = @QuestionText,
                        QuestionType  = @QuestionType,
                        OptionA       = @OptionA,
                        OptionB       = @OptionB,
                        OptionC       = @OptionC,
                        OptionD       = @OptionD,
                        CorrectAnswer = @CorrectAnswer,
                        MatchStrategy = @MatchStrategy,
                        Explanation   = @Explanation,
                        Points        = @Points
                    FROM QuizQuestions qq
                    INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                    WHERE qq.QuestionID = @QuestionID AND q.CreatedByID = @InstructorID;";

                DBHelper.ExecuteNonQuery(update, pUpdate);
                return questionId;
            }
        }

        /// <summary>
        /// Deletes a quiz and its questions, but only if it belongs to the instructor
        /// and no student has attempted it yet. Returns 1 = deleted, -1 = blocked by
        /// existing attempts, 0 = not found / not owned.
        /// </summary>
        public static int DeleteQuiz(string quizId, string instructorId)
        {
            string ownerCheck = @"
                SELECT COUNT(*) FROM Quizzes
                WHERE QuizID = @QuizID AND CreatedByID = @InstructorID;";

            object owned = DBHelper.ExecuteScalar(ownerCheck,
                new SqlParameter("@QuizID", quizId),
                new SqlParameter("@InstructorID", instructorId));

            if (Convert.ToInt32(owned) == 0) return 0;

            string attemptCheck = "SELECT COUNT(*) FROM QuizAttempts WHERE QuizID = @QuizID;";
            object attempted = DBHelper.ExecuteScalar(attemptCheck,
                new SqlParameter("@QuizID", quizId));

            if (Convert.ToInt32(attempted) > 0) return -1;

            // Attachments reference the quiz and each of its questions, so they go first.
            AttachmentService.DeleteByParent("Quiz", quizId, instructorId);

            DataTable questions = DBHelper.ExecuteQuery(
                "SELECT QuestionID FROM QuizQuestions WHERE QuizID = @QuizID;",
                new SqlParameter("@QuizID", quizId));
            foreach (DataRow q in questions.Rows)
                AttachmentService.DeleteByParent("Question", q["QuestionID"].ToString(), instructorId);

            string delete = @"
                UPDATE Feedback SET QuizID = NULL WHERE QuizID = @QuizID;
                DELETE FROM QuizQuestions WHERE QuizID = @QuizID;
                DELETE FROM Quizzes WHERE QuizID = @QuizID;";

            DBHelper.ExecuteNonQuery(delete, new SqlParameter("@QuizID", quizId));
            return 1;
        }

        /// <summary>
        /// Deletes a question, but only if it belongs to the instructor and no student
        /// has answered it yet. Returns 1 = deleted, -1 = blocked by existing answers,
        /// 0 = not found / not owned.
        /// </summary>
        public static int DeleteQuestion(string questionId, string instructorId)
        {
            string ownerCheck = @"
                SELECT COUNT(*)
                FROM QuizQuestions qq
                INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                WHERE qq.QuestionID = @QuestionID AND q.CreatedByID = @InstructorID;";

            object owned = DBHelper.ExecuteScalar(ownerCheck,
                new SqlParameter("@QuestionID", questionId),
                new SqlParameter("@InstructorID", instructorId));

            if (Convert.ToInt32(owned) == 0) return 0;

            string answeredCheck = "SELECT COUNT(*) FROM QuizAnswers WHERE QuestionID = @QuestionID;";
            object answered = DBHelper.ExecuteScalar(answeredCheck,
                new SqlParameter("@QuestionID", questionId));

            if (Convert.ToInt32(answered) > 0) return -1;

            // Attachments reference the question, so they must go first.
            AttachmentService.DeleteByParent("Question", questionId, instructorId);

            string delete = "DELETE FROM QuizQuestions WHERE QuestionID = @QuestionID;";
            DBHelper.ExecuteNonQuery(delete, new SqlParameter("@QuestionID", questionId));
            return 1;
        }
    }
}
