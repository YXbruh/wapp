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
            return DBHelper.ExecuteQuery(@"
                SELECT cc.CategoryID, cc.CategoryName, cc.Description,
                       FORMAT(cc.CreatedAt, 'dd MMM yyyy') AS CreatedDisplay,
                       (SELECT COUNT(*) FROM Courses WHERE CategoryID = cc.CategoryID) AS CourseCount
                FROM CourseCategories cc ORDER BY cc.CategoryName");
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
                sb.AppendLine($"{row["CourseID"]},\"{row["CourseName"]}\",\"{row["Level"]}\",\"{row["InstructorName"]}\",\"{row["Category"]}\",{row["EnrolledCount"]},{row["Status"]},{row["CreatedAt"]}");
            }
            return sb.ToString();
        }

        public static void GetCounts(out int total, out int published, out int draft)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN IsPublished = 1 THEN 1 ELSE 0 END) AS Published,
                    SUM(CASE WHEN IsPublished = 0 THEN 1 ELSE 0 END) AS Draft
                FROM Courses");
            total = 0; published = 0; draft = 0;
            if (dt.Rows.Count > 0)
            {
                total = Convert.ToInt32(dt.Rows[0]["Total"]);
                published = Convert.ToInt32(dt.Rows[0]["Published"]);
                draft = Convert.ToInt32(dt.Rows[0]["Draft"]);
            }
        }
    }
}
