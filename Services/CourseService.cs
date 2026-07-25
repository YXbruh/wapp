using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;

namespace CSA.Services
{
    public static class CourseService
    {
        public static DataTable AdminSearch(string keyword, string status, string level)
        {
            return AdminSearch(keyword, status, level, 0, 0, out _);
        }

        public static DataTable AdminSearch(string keyword, string status, string level,
            int page, int pageSize, out int total)
        {
            string where = @"WHERE (c.CourseName LIKE @Keyword OR @Keyword = '')
                  AND (@Status = '' OR
                       CASE WHEN c.IsPublished = 1 THEN 'Published' ELSE 'Draft' END = @Status)
                  AND (@Level = '' OR c.Level = @Level)";

            string countSql = "SELECT COUNT(*) FROM Courses c " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Status", status ?? ""),
                new SqlParameter("@Level", level ?? "")));

            int offset = Math.Max(0, (page - 1) * pageSize);

            string order = "ORDER BY c.CreatedAt DESC";

            string sql = $@"
                SELECT c.CourseID, c.CourseName, c.Level, c.IsPublished, c.CreatedAt,
                       u.FullName AS InstructorName, cat.CategoryName AS Category,
                       (SELECT COUNT(*) FROM Enrollments WHERE CourseID = c.CourseID) AS EnrolledCount,
                       (SELECT COUNT(*) FROM VirtualLabs WHERE CourseID = c.CourseID) AS LabCount,
                       CASE WHEN c.IsPublished = 1 THEN 'Published' ELSE 'Draft' END AS Status
                FROM Courses c
                JOIN Users u ON c.InstructorID = u.UserID
                LEFT JOIN CourseCategories cat ON c.CategoryID = cat.CategoryID
                {where}
                {order}";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Status", status ?? ""),
                new SqlParameter("@Level", level ?? "")
            };
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static DataTable GetById(string courseId)
        {
            return DBHelper.ExecuteQuery(
                @"SELECT c.*, u.FullName AS InstructorName, cat.CategoryName
                  FROM Courses c
                  JOIN Users u ON c.InstructorID = u.UserID
                  LEFT JOIN CourseCategories cat ON c.CategoryID = cat.CategoryID
                  WHERE c.CourseID = @ID",
                new SqlParameter("@ID", courseId));
        }

        public static void TogglePublish(string courseId)
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE Courses SET IsPublished = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END,
                        UpdatedAt = GETDATE()
                  WHERE CourseID = @ID",
                new SqlParameter("@ID", courseId));
        }

        public static string Create(string name, string description, string categoryId, string instructorId,
            string level, int durationHours)
        {
            string courseId = IdGenerator.NewId("CRS");
            DBHelper.ExecuteNonQuery(@"
                INSERT INTO Courses (CourseID, CourseName, Description, CategoryID, InstructorID, Level, DurationHours, CreatedAt, UpdatedAt)
                VALUES (@CourseID, @Name, @Desc, @CatID, @InstructorID, @Level, @Hours, GETDATE(), GETDATE())",
                new SqlParameter("@CourseID", courseId),
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", (object)description ?? DBNull.Value),
                new SqlParameter("@CatID", string.IsNullOrEmpty(categoryId) ? DBNull.Value : (object)categoryId),
                new SqlParameter("@InstructorID", instructorId),
                new SqlParameter("@Level", level),
                new SqlParameter("@Hours", durationHours));
            return courseId;
        }

        public static void Update(string courseId, string name, string description, string categoryId,
            string instructorId, string level, int durationHours)
        {
            DBHelper.ExecuteNonQuery(@"
                UPDATE Courses SET CourseName = @Name, Description = @Desc, CategoryID = @CatID,
                    InstructorID = @InstructorID, Level = @Level, DurationHours = @Hours, UpdatedAt = GETDATE()
                WHERE CourseID = @ID",
                new SqlParameter("@ID", courseId),
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", (object)description ?? DBNull.Value),
                new SqlParameter("@CatID", string.IsNullOrEmpty(categoryId) ? DBNull.Value : (object)categoryId),
                new SqlParameter("@InstructorID", instructorId),
                new SqlParameter("@Level", level),
                new SqlParameter("@Hours", durationHours));
        }

        public static DataTable GetPublishedCourses(string keyword, string category, string userId)
        {
            string where = "WHERE c.IsPublished = 1";
            if (!string.IsNullOrEmpty(keyword))
                where += " AND c.CourseName LIKE @Keyword";
            if (!string.IsNullOrEmpty(category) && category != "All")
                where += " AND cat.CategoryName = @Category";

            string sql = $@"
                SELECT c.CourseID, c.CourseName, c.Description, c.Level, c.DurationHours,
                       u.FullName AS InstructorName, cat.CategoryName,
                       (SELECT COUNT(*) FROM Enrollments WHERE CourseID = c.CourseID) AS EnrolledCount,
                       (SELECT COUNT(*) FROM VirtualLabs WHERE CourseID = c.CourseID) AS LabCount,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM Enrollments WHERE StudentID = @UserID AND CourseID = c.CourseID) THEN 1 ELSE 0 END AS BIT) AS IsEnrolled
                FROM Courses c
                JOIN Users u ON c.InstructorID = u.UserID
                LEFT JOIN CourseCategories cat ON c.CategoryID = cat.CategoryID
                {where}
                ORDER BY c.CreatedAt DESC";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Category", category ?? ""),
                new SqlParameter("@UserID", (object)userId ?? "")
            };

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static DataTable GetCategories()
        {
            return GetCategories(0, 0, out _);
        }

        public static DataTable GetCategories(int page, int pageSize, out int total)
        {
            string countSql = "SELECT COUNT(*) FROM CourseCategories";
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql));

            int offset = Math.Max(0, (page - 1) * pageSize);
            string sql = @"
                SELECT cc.CategoryID, cc.CategoryName, cc.Description,
                       FORMAT(cc.CreatedAt, 'dd MMM yyyy') AS CreatedDisplay,
                       (SELECT COUNT(*) FROM Courses WHERE CategoryID = cc.CategoryID) AS CourseCount
                FROM CourseCategories cc ORDER BY cc.CategoryName";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>();
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        /// <summary>True when another category already uses this name (CategoryName is UNIQUE).</summary>
        public static bool CategoryNameExists(string name, string excludeCategoryId = null)
        {
            object count = DBHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM CourseCategories
                  WHERE CategoryName = @Name AND (@Exclude = '' OR CategoryID <> @Exclude)",
                new SqlParameter("@Name", (name ?? "").Trim()),
                new SqlParameter("@Exclude", excludeCategoryId ?? ""));
            return count != null && Convert.ToInt32(count) > 0;
        }

        // Courses table limits, checked before the INSERT/UPDATE so an over-long value
        // returns a message rather than an unhandled truncation SqlException.
        public const int MaxCourseNameLength = 200;
        public const int MaxCourseDescriptionLength = 2000;
        public const int MaxDurationHours = 1000;

        /// <summary>Shared validation for the create and edit course forms.</summary>
        public static bool ValidateCourse(string name, string description, int durationHours,
            out string errorMsg)
        {
            errorMsg = "";

            if (string.IsNullOrWhiteSpace(name))
            { errorMsg = "Course name is required."; return false; }
            if (name.Trim().Length > MaxCourseNameLength)
            { errorMsg = $"Course name cannot exceed {MaxCourseNameLength} characters."; return false; }
            if (description != null && description.Trim().Length > MaxCourseDescriptionLength)
            { errorMsg = $"Description cannot exceed {MaxCourseDescriptionLength} characters."; return false; }

            // A course cannot run for a negative number of hours; this was previously
            // unchecked and -5 was accepted straight into the database.
            if (durationHours < 0)
            { errorMsg = "Duration cannot be negative."; return false; }
            if (durationHours > MaxDurationHours)
            { errorMsg = $"Duration cannot exceed {MaxDurationHours} hours."; return false; }

            return true;
        }

        public static string CreateCategory(string name, string description)
        {
            string id = IdGenerator.NewId("CAT");
            DBHelper.ExecuteNonQuery(
                "INSERT INTO CourseCategories (CategoryID, CategoryName, Description, CreatedAt) VALUES (@ID, @Name, @Desc, GETDATE())",
                new SqlParameter("@ID", id),
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", (object)description ?? DBNull.Value));
            return id;
        }

        public static void UpdateCategory(string id, string name, string description)
        {
            DBHelper.ExecuteNonQuery(
                "UPDATE CourseCategories SET CategoryName = @Name, Description = @Desc WHERE CategoryID = @ID",
                new SqlParameter("@Name", name),
                new SqlParameter("@Desc", (object)description ?? DBNull.Value),
                new SqlParameter("@ID", id));
        }

        public static void DeleteCategory(string id)
        {
            DBHelper.ExecuteNonQuery(
                "DELETE FROM CourseCategories WHERE CategoryID = @ID",
                new SqlParameter("@ID", id));
        }

        public static DataTable GetCategoryById(string id)
        {
            return DBHelper.ExecuteQuery(
                "SELECT * FROM CourseCategories WHERE CategoryID = @ID",
                new SqlParameter("@ID", id));
        }

        public static DataTable GetInstructors()
        {
            return DBHelper.ExecuteQuery(
                @"SELECT UserID, FullName FROM Users WHERE RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Lecturer')
                  ORDER BY FullName");
        }

        public static void Delete(string courseId)
        {
            DBHelper.ExecuteNonQuery(
                "DELETE FROM Courses WHERE CourseID = @ID",
                new SqlParameter("@ID", courseId));
        }

        public static string ExportCsv(string keyword, string status, string level)
        {
            DataTable dt = AdminSearch(keyword, status, level);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CourseID,CourseName,Level,Instructor,Category,EnrolledCount,Status,CreatedAt");
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    UserService.CsvField(row["CourseID"]),   UserService.CsvField(row["CourseName"]),
                    UserService.CsvField(row["Level"]),      UserService.CsvField(row["InstructorName"]),
                    UserService.CsvField(row["Category"]),   UserService.CsvField(row["EnrolledCount"]),
                    UserService.CsvField(row["Status"]),     UserService.CsvField(row["CreatedAt"])
                }));
            }
            return sb.ToString();
        }

        public static void GetCounts(out int total, out int published, out int draft, out int pending)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN IsPublished = 1 THEN 1 ELSE 0 END) AS Published,
                    SUM(CASE WHEN IsPublished = 0 THEN 1 ELSE 0 END) AS Draft,
                    0 AS Pending
                FROM Courses");
            total = 0; published = 0; draft = 0; pending = 0;
            if (dt.Rows.Count > 0)
            {
                total = Convert.ToInt32(dt.Rows[0]["Total"]);
                published = Convert.ToInt32(dt.Rows[0]["Published"]);
                draft = Convert.ToInt32(dt.Rows[0]["Draft"]);
                pending = Convert.ToInt32(dt.Rows[0]["Pending"]);
            }
        }

        public static void GetCounts(out int total, out int published, out int draft)
        {
            GetCounts(out total, out published, out draft, out _);
        }

        // ====================================================================
        // COURSE PROGRESS
        //
        // A student's progress in a course is measured across every published
        // learning item — chapters, virtual labs AND quizzes — not chapters
        // alone. An item counts as done when:
        //   Chapter : ChapterProgress.IsCompleted = 1
        //   Lab     : the student has at least one correct LabSubmission
        //   Quiz    : the student has at least one passing QuizAttempt
        // Progress = done items / total published items * 100.
        // ====================================================================

        /// <summary>
        /// Recomputes a student's progress in one course from chapters + labs + quizzes
        /// and stores it (with Status/CompletedAt) on the Enrollments row. Returns true
        /// only when this call is what took the course to 100%, so a completion is logged
        /// once rather than on every recalculation.
        /// </summary>
        public static bool RecalculateProgress(string studentId, string courseId)
        {
            object result = DBHelper.ExecuteScalar(@"
                DECLARE @AlreadyFinished INT =
                    ISNULL((SELECT MAX(CASE WHEN CompletedAt IS NOT NULL THEN 1 ELSE 0 END)
                            FROM Enrollments WHERE StudentID = @StudentID AND CourseID = @CourseID), 0);

                DECLARE @Total INT =
                    (SELECT COUNT(*) FROM Chapters    WHERE CourseID = @CourseID AND IsPublished = 1)
                  + (SELECT COUNT(*) FROM VirtualLabs WHERE CourseID = @CourseID AND IsPublished = 1)
                  + (SELECT COUNT(*) FROM Quizzes     WHERE CourseID = @CourseID AND IsPublished = 1);

                DECLARE @Completed INT =
                    (SELECT COUNT(*)
                       FROM ChapterProgress cp
                       INNER JOIN Chapters ch ON ch.ChapterID = cp.ChapterID
                      WHERE cp.StudentID = @StudentID AND ch.CourseID = @CourseID
                        AND ch.IsPublished = 1 AND cp.IsCompleted = 1)
                  + (SELECT COUNT(*)
                       FROM VirtualLabs vl
                      WHERE vl.CourseID = @CourseID AND vl.IsPublished = 1
                        AND EXISTS (SELECT 1 FROM LabSubmissions ls
                                     WHERE ls.LabID = vl.LabID AND ls.StudentID = @StudentID AND ls.IsCorrect = 1))
                  + (SELECT COUNT(*)
                       FROM Quizzes q
                      WHERE q.CourseID = @CourseID AND q.IsPublished = 1
                        AND EXISTS (SELECT 1 FROM QuizAttempts qa
                                     WHERE qa.QuizID = q.QuizID AND qa.StudentID = @StudentID AND qa.IsPassed = 1));

                DECLARE @Progress DECIMAL(5,2) =
                    CASE WHEN @Total = 0 THEN 0
                         ELSE CAST(@Completed * 100.0 / @Total AS DECIMAL(5,2)) END;

                UPDATE Enrollments
                   SET Progress = @Progress,
                       Status = CASE WHEN @Progress >= 100 THEN 'Completed'
                                     WHEN @Progress > 0    THEN 'In Progress'
                                     ELSE 'Not Started' END,
                       CompletedAt = CASE WHEN @Progress >= 100 AND CompletedAt IS NULL THEN GETDATE()
                                          WHEN @Progress >= 100 THEN CompletedAt
                                          ELSE NULL END
                 WHERE StudentID = @StudentID AND CourseID = @CourseID;

                SELECT CASE WHEN @Progress >= 100 AND @AlreadyFinished = 0 THEN 1 ELSE 0 END;",
                new SqlParameter("@StudentID", studentId ?? ""),
                new SqlParameter("@CourseID", courseId ?? ""));

            return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
        }

        /// <summary>Recalculates progress for every course the student is enrolled in.</summary>
        public static void RecalculateAllForStudent(string studentId)
        {
            DataTable courses = DBHelper.ExecuteQuery(
                "SELECT CourseID FROM Enrollments WHERE StudentID = @ID",
                new SqlParameter("@ID", studentId ?? ""));
            foreach (DataRow row in courses.Rows)
                RecalculateProgress(studentId, row["CourseID"].ToString());
        }

        /// <summary>Resolves a lab's course, then recalculates that course's progress.</summary>
        public static void RecalculateProgressForLab(string studentId, string labId)
        {
            object courseId = DBHelper.ExecuteScalar(
                "SELECT CourseID FROM VirtualLabs WHERE LabID = @ID", new SqlParameter("@ID", labId ?? ""));
            if (courseId != null && courseId != DBNull.Value)
                RecalculateProgress(studentId, courseId.ToString());
        }

        /// <summary>Resolves a quiz's course, then recalculates that course's progress.</summary>
        public static void RecalculateProgressForQuiz(string studentId, string quizId)
        {
            object courseId = DBHelper.ExecuteScalar(
                "SELECT CourseID FROM Quizzes WHERE QuizID = @ID", new SqlParameter("@ID", quizId ?? ""));
            if (courseId != null && courseId != DBNull.Value)
                RecalculateProgress(studentId, courseId.ToString());
        }
    }
}
