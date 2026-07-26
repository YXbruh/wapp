using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;   // for DBHelper only - no need to edit anything in that folder

namespace CSA.Services
{
    /// <summary>
    /// Read-only analytics for the lecturer StudentDetail page.
    /// Lives in the Services folder next to QuizService / UserService.
    ///
    /// IMPORTANT: a lecturer may only inspect a student who is enrolled in one
    /// of that lecturer's own published courses. Every method therefore takes
    /// the lecturerId and filters on it, so one lecturer can't view arbitrary
    /// students. The Owns() check gates the whole page.
    /// </summary>
    public static class StudentDetailService
    {
        /// <summary>
        /// True if the student is enrolled in at least one course owned by this
        /// lecturer. Used to authorise access to the detail page.
        /// </summary>
        public static bool Owns(string lecturerId, string studentId)
        {
            object n = DBHelper.ExecuteScalar(@"
                SELECT COUNT(*)
                FROM   Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE  e.StudentID = @StudentID
                  AND  c.LecturerID = @LecturerID;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));

            return Convert.ToInt32(n) > 0;
        }

        /// <summary>Basic profile header (FullName, Email, EnrolledCount, LastLoginDate).</summary>
        public static DataRow GetProfile(string studentId, string lecturerId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT  u.FullName,
                        u.Email,
                        u.LastLoginDate,
                        (SELECT COUNT(*)
                         FROM Enrollments e2
                         INNER JOIN Courses c2 ON e2.CourseID = c2.CourseID
                         WHERE e2.StudentID = u.UserID
                           AND c2.LecturerID = @LecturerID) AS EnrolledCount
                FROM    Users u
                WHERE   u.UserID = @StudentID;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Headline metrics for the metric cards.
        /// Columns: QuizAvg, LabsDone, LabsTotal.
        /// All scoped to this lecturer's courses.
        /// </summary>
        public static DataRow GetMetrics(string studentId, string lecturerId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT
                    -- average of the student's best score per quiz (this lecturer's quizzes)
                    ISNULL((
                        SELECT AVG(best.BestScore)
                        FROM (
                            SELECT MAX(qa.Score) AS BestScore
                            FROM   QuizAttempts qa
                            INNER JOIN Quizzes q ON qa.QuizID = q.QuizID
                            WHERE  qa.StudentID = @StudentID
                              AND  q.CreatedByID = @LecturerID
                            GROUP BY qa.QuizID
                        ) best
                    ), 0) AS QuizAvg,

                    -- labs passed (distinct) in this lecturer's courses
                    (SELECT COUNT(DISTINCT ls.LabID)
                     FROM   LabSubmissions ls
                     INNER JOIN VirtualLabs vl ON ls.LabID = vl.LabID
                     WHERE  ls.StudentID = @StudentID
                       AND  ls.Result = 'Passed'
                       AND  vl.CreatedByID = @LecturerID) AS LabsDone,

                    -- total published labs in this lecturer's courses the student is enrolled in
                    (SELECT COUNT(DISTINCT vl.LabID)
                     FROM   VirtualLabs vl
                     INNER JOIN Enrollments e
                         ON e.CourseID = vl.CourseID AND e.StudentID = @StudentID
                     WHERE  vl.IsPublished = 1
                       AND  vl.CreatedByID = @LecturerID) AS LabsTotal;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Per-quiz attempt summary for the Quiz Attempts table.
        /// Columns: QuizName, Score (best), AttemptCount, LastAttemptDisplay.
        /// </summary>
        public static DataTable GetQuizAttempts(string studentId, string lecturerId)
        {
            return DBHelper.ExecuteQuery(@"
                SELECT  q.Title AS QuizName,
                        MAX(qa.Score) AS Score,
                        COUNT(*)      AS AttemptCount,
                        CONVERT(VARCHAR(11), MAX(qa.AttemptedAt), 106) AS LastAttemptDisplay
                FROM    QuizAttempts qa
                INNER JOIN Quizzes q ON qa.QuizID = q.QuizID
                WHERE   qa.StudentID = @StudentID
                  AND   q.CreatedByID = @LecturerID
                GROUP BY q.Title
                ORDER BY MAX(qa.AttemptedAt) DESC;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));
        }

        /// <summary>
        /// Best-effort course shared between this lecturer and the student, used to
        /// tag a lecturer-initiated Feedback message for course-filtered views.
        /// Returns null if they don't share a course.
        /// </summary>
        public static string GetSharedCourseId(string lecturerId, string studentId)
        {
            object result = DBHelper.ExecuteScalar(@"
                SELECT TOP 1 c.CourseID
                FROM   Enrollments e
                INNER JOIN Courses c ON e.CourseID = c.CourseID
                WHERE  e.StudentID = @StudentID AND c.LecturerID = @LecturerID
                ORDER BY e.EnrolledAt DESC;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));

            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        /// <summary>
        /// Sends a new lecturer-initiated feedback message directly to a student
        /// (not a reply to an existing student review).
        /// </summary>
        public static void SendFeedback(string lecturerId, string studentId, string message)
        {
            string courseId = GetSharedCourseId(lecturerId, studentId);

            DBHelper.ExecuteNonQuery(@"
                INSERT INTO Feedback (StudentID, LecturerID, CourseID, StarRating, Comment, SubmittedAt, RepText, RepAt)
                VALUES (@StudentID, @LecturerID, @CourseID, NULL, NULL, GETDATE(), @RepText, GETDATE());",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId),
                new SqlParameter("@CourseID", (object)courseId ?? DBNull.Value),
                new SqlParameter("@RepText", message));
        }

        /// <summary>
        /// Per-lab completion status for the Lab Completion list.
        /// Columns: LabName, Status ('Done' | 'In Progress' | 'Not Started'),
        ///          CompletedDisplay.
        /// </summary>
        public static DataTable GetLabStatus(string studentId, string lecturerId)
        {
            return DBHelper.ExecuteQuery(@"
                SELECT  vl.LabTitle AS LabName,
                        CASE
                            WHEN passed.Cnt > 0 THEN 'Done'
                            WHEN anySub.Cnt > 0 THEN 'In Progress'
                            ELSE 'Not Started'
                        END AS Status,
                        CASE
                            WHEN passed.LastAt IS NOT NULL
                                THEN 'Completed ' + CONVERT(VARCHAR(11), passed.LastAt, 106)
                            WHEN anySub.LastAt IS NOT NULL
                                THEN 'Last tried ' + CONVERT(VARCHAR(11), anySub.LastAt, 106)
                            ELSE 'Not attempted'
                        END AS CompletedDisplay
                FROM    VirtualLabs vl
                INNER JOIN Enrollments e
                    ON e.CourseID = vl.CourseID AND e.StudentID = @StudentID
                OUTER APPLY (
                    SELECT COUNT(*) AS Cnt, MAX(ls.SubmittedAt) AS LastAt
                    FROM LabSubmissions ls
                    WHERE ls.LabID = vl.LabID AND ls.StudentID = @StudentID
                      AND ls.Result = 'Passed'
                ) passed
                OUTER APPLY (
                    SELECT COUNT(*) AS Cnt, MAX(ls.SubmittedAt) AS LastAt
                    FROM LabSubmissions ls
                    WHERE ls.LabID = vl.LabID AND ls.StudentID = @StudentID
                ) anySub
                WHERE   vl.IsPublished = 1
                  AND   vl.CreatedByID = @LecturerID
                ORDER BY vl.LabTitle;",
                new SqlParameter("@StudentID", studentId),
                new SqlParameter("@LecturerID", lecturerId));
        }
    }
}