using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;

namespace CSA.Services
{
    public static class UserService
    {
        // Column limits from the Users table, enforced before the INSERT/UPDATE so an
        // over-long value returns a message instead of an unhandled truncation SqlException.
        public const int MaxFullNameLength = 150;
        public const int MaxEmailLength = 255;
        public const int MaxPhoneLength = 50;
        public const int MaxDepartmentLength = 100;
        public const int MaxStudentIdLength = 20;

        /// <summary>
        /// Returns DBNull for blank text, otherwise the trimmed value.
        ///
        /// Optional columns must be stored as NULL, never as "". StudentID carries a
        /// filtered unique index (WHERE StudentID IS NOT NULL), and an empty string is a
        /// real value to that index - so a second user saved with a blank Student ID
        /// collided with the first and could not be created or edited at all.
        /// </summary>
        private static object NullIfBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        /// <summary>
        /// Validates the shared user field rules. Returns false with a message when a
        /// required field is missing or a value is longer than its column allows.
        /// </summary>
        public static bool ValidateFields(string fullName, string email, string phone,
            string department, string studentId, out string errorMsg)
        {
            errorMsg = "";

            if (string.IsNullOrWhiteSpace(fullName))
            { errorMsg = "Full name is required."; return false; }
            if (fullName.Trim().Length > MaxFullNameLength)
            { errorMsg = $"Full name cannot exceed {MaxFullNameLength} characters."; return false; }

            if (string.IsNullOrWhiteSpace(email))
            { errorMsg = "Email is required."; return false; }
            if (email.Trim().Length > MaxEmailLength)
            { errorMsg = $"Email cannot exceed {MaxEmailLength} characters."; return false; }

            if (phone != null && phone.Trim().Length > MaxPhoneLength)
            { errorMsg = $"Phone number cannot exceed {MaxPhoneLength} characters."; return false; }

            if (department != null && department.Trim().Length > MaxDepartmentLength)
            { errorMsg = $"Department cannot exceed {MaxDepartmentLength} characters."; return false; }

            if (studentId != null && studentId.Trim().Length > MaxStudentIdLength)
            { errorMsg = $"Student ID cannot exceed {MaxStudentIdLength} characters."; return false; }

            return true;
        }

        /// <summary>True when another account already uses this email.</summary>
        public static bool EmailExists(string email, string excludeUserId = null)
        {
            object count = DBHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM Users
                  WHERE Email = @Email AND (@Exclude = '' OR UserID <> @Exclude)",
                new SqlParameter("@Email", (email ?? "").Trim()),
                new SqlParameter("@Exclude", excludeUserId ?? ""));
            return count != null && Convert.ToInt32(count) > 0;
        }

        /// <summary>True when another account already uses this Student ID (blank is always free).</summary>
        public static bool StudentIdExists(string studentId, string excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return false;
            object count = DBHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM Users
                  WHERE StudentID = @SID AND (@Exclude = '' OR UserID <> @Exclude)",
                new SqlParameter("@SID", studentId.Trim()),
                new SqlParameter("@Exclude", excludeUserId ?? ""));
            return count != null && Convert.ToInt32(count) > 0;
        }

        public static DataTable GetAllRoles()
        {
            return DBHelper.ExecuteQuery("SELECT RoleID, RoleName FROM Roles");
        }

        public static string GetRoleIdByName(string roleName)
        {
            object result = DBHelper.ExecuteScalar(
                "SELECT RoleID FROM Roles WHERE RoleName = @Name",
                new SqlParameter("@Name", roleName));
            return result != null ? result.ToString() : "ROLIDQ053";
        }

        // ----- LOGIN -----
        public static bool Authenticate(string email, string password,
            out string role, out string userId, out string fullName, out string errorMsg)
        {
            role = ""; userId = ""; fullName = ""; errorMsg = "";
            DataTable dt = DBHelper.ExecuteQuery(
                @"SELECT u.UserID, u.FullName, u.Email, r.RoleName, u.IsActive, u.PasswordHash
                  FROM Users u JOIN Roles r ON u.RoleID = r.RoleID
                  WHERE u.Email = @Email",
                new SqlParameter("@Email", email));

            if (dt.Rows.Count == 0)
            { errorMsg = "No account found with that email."; return false; }

            DataRow row = dt.Rows[0];
            if (!Convert.ToBoolean(row["IsActive"]))
            { errorMsg = "This account has been deactivated. Contact your administrator."; return false; }

            string storedHash = row["PasswordHash"].ToString();
            if (!PasswordHelper.Verify(password, storedHash))
            { errorMsg = "Incorrect password."; return false; }

            userId = row["UserID"].ToString();
            fullName = row["FullName"].ToString();
            role = row["RoleName"].ToString();

            // Transparently upgrade legacy SHA-256 hashes to PBKDF2 on a correct login.
            if (PasswordHelper.NeedsUpgrade(storedHash))
                UpdatePassword(userId, password);

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET LastLoginDate = GETDATE() WHERE UserID = @ID",
                new SqlParameter("@ID", userId));

            return true;
        }

        // ----- REGISTER -----
        public static bool Register(string fullName, string email, string password, out string errorMsg)
        {
            if (!ValidateFields(fullName, email, null, null, null, out errorMsg))
                return false;

            if (EmailExists(email))
            { errorMsg = "An account with that email already exists."; return false; }

            string studentRoleId = GetRoleIdByName("Student");
            if (string.IsNullOrEmpty(studentRoleId))
            { errorMsg = "Student role is not configured. Contact an administrator."; return false; }

            string hash = PasswordHelper.Hash(password);

            // UserID is a NOT NULL primary key with no default, so it must be supplied.
            string userId = IdGenerator.NewId("USR");

            try
            {
                DBHelper.ExecuteNonQuery(
                    @"INSERT INTO Users (UserID, FullName, Email, PasswordHash, RoleID, IsActive, CreatedAt)
                      VALUES (@UserID, @Name, @Email, @Hash, @RoleID, 1, GETDATE())",
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@Name", fullName.Trim()),
                    new SqlParameter("@Email", email.Trim()),
                    new SqlParameter("@Hash", hash),
                    new SqlParameter("@RoleID", studentRoleId));
            }
            catch (SqlException ex)
            {
                // Never surface raw SQL text: it leaks the .mdf path and index names.
                errorMsg = DescribeSqlError(ex, "create your account");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Turns a SqlException into a message that is safe to show a user.
        /// 2601/2627 = unique index / unique constraint, 8152 = value too long for its column.
        /// </summary>
        internal static string DescribeSqlError(SqlException ex, string action)
        {
            switch (ex.Number)
            {
                case 2601:
                case 2627:
                    return ex.Message.IndexOf("StudentID", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "That Student ID is already assigned to another user."
                        : "An account with that email already exists.";
                case 8152:
                case 2628:
                    return "One of the values entered is too long for its field.";
                case 547:
                    return "That change conflicts with existing linked records.";
                default:
                    return $"Could not {action}. Please try again or contact an administrator.";
            }
        }

        // ----- USER CRUD -----
        public static DataTable Search(string keyword, string role, string status)
        {
            return Search(keyword, role, status, 0, 0, out _);
        }

        public static DataTable Search(string keyword, string role, string status,
            int page, int pageSize, out int total)
        {
            string where = @"WHERE (u.FullName LIKE @Keyword OR u.Email LIKE @Keyword)
                  AND (@Role = '' OR r.RoleName = @Role)
                  AND (@Status = '' OR CAST(u.IsActive AS INT) = CAST(@Status AS INT))";

            string countSql = "SELECT COUNT(*) FROM Users u JOIN Roles r ON u.RoleID = r.RoleID " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Role", role ?? ""),
                new SqlParameter("@Status", status ?? "")));

            string order = "ORDER BY u.CreatedAt DESC";
            int offset = Math.Max(0, (page - 1) * pageSize);

            string sql = $@"
                SELECT u.UserID, u.FullName, u.Email, u.PhoneNumber, u.Department, u.StudentID,
                       u.IsActive, u.LastLoginDate, u.CreatedAt,
                       r.RoleName AS Role,
                       (SELECT COUNT(*) FROM Enrollments WHERE StudentID = u.UserID) AS EnrolledCount
                FROM Users u
                JOIN Roles r ON u.RoleID = r.RoleID
                {where}
                {order}";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Role", role ?? ""),
                new SqlParameter("@Status", status ?? "")
            };
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static DataTable GetById(string userId)
        {
            return DBHelper.ExecuteQuery(
                @"SELECT u.*, r.RoleName AS Role
                  FROM Users u JOIN Roles r ON u.RoleID = r.RoleID
                  WHERE u.UserID = @ID",
                new SqlParameter("@ID", userId));
        }

        public static string Create(string fullName, string email, string password, string roleId,
            string studentId, string phone, string department)
        {
            string hash = PasswordHelper.Hash(password);
            string userId = IdGenerator.NewId("USR");
            DBHelper.ExecuteNonQuery(
                @"INSERT INTO Users (UserID, FullName, Email, PasswordHash, RoleID, StudentID, PhoneNumber, Department, IsActive, CreatedAt)
                  VALUES (@UserID, @Name, @Email, @Hash, @RoleID, @StudentID, @Phone, @Dept, 1, GETDATE())",
                new SqlParameter("@UserID", userId),
                new SqlParameter("@Name", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@Hash", hash),
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@StudentID", NullIfBlank(studentId)),
                new SqlParameter("@Phone", NullIfBlank(phone)),
                new SqlParameter("@Dept", NullIfBlank(department)));
            return userId;
        }

        public static void Update(string userId, string fullName, string email, string roleId, bool isActive,
            string phone = "", string department = "", string studentId = "")
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE Users SET FullName = @Name, Email = @Email, RoleID = @RoleID,
                        IsActive = @Active, PhoneNumber = @Phone, Department = @Dept, StudentID = @StudentID
                  WHERE UserID = @ID",
                new SqlParameter("@Name", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@Active", isActive),
                new SqlParameter("@Phone", NullIfBlank(phone)),
                new SqlParameter("@Dept", NullIfBlank(department)),
                new SqlParameter("@StudentID", NullIfBlank(studentId)),
                new SqlParameter("@ID", userId));
        }

        public static void ToggleActive(string userId)
        {
            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE UserID = @ID",
                new SqlParameter("@ID", userId));
        }

        public static void UpdatePassword(string userId, string newPassword)
        {
            string hash = PasswordHelper.Hash(newPassword);
            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET PasswordHash = @Hash WHERE UserID = @ID",
                new SqlParameter("@Hash", hash),
                new SqlParameter("@ID", userId));
        }

        public static void Delete(string userId)
        {
            DBHelper.ExecuteNonQuery(
                "DELETE FROM Users WHERE UserID = @ID",
                new SqlParameter("@ID", userId));
        }

        public static int GetActiveTodayCount()
        {
            object result = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE CAST(LastLoginDate AS DATE) = CAST(GETDATE() AS DATE)");
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static void GetCountsByRole(out int totalStudents, out int totalLecturers)
        {
            totalStudents = Convert.ToInt32(DBHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM Users u JOIN Roles r ON u.RoleID = r.RoleID WHERE r.RoleName = 'Student'"));
            totalLecturers = Convert.ToInt32(DBHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM Users u JOIN Roles r ON u.RoleID = r.RoleID WHERE r.RoleName = 'Lecturer'"));
        }

        public static string ExportCsv(string keyword, string role, string status)
        {
            DataTable dt = Search(keyword, role, status);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("UserID,FullName,Email,Role,IsActive,LastLogin,CreatedAt");
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    CsvField(row["UserID"]),   CsvField(row["FullName"]), CsvField(row["Email"]),
                    CsvField(row["Role"]),     CsvField(row["IsActive"]),
                    CsvField(row["LastLoginDate"]), CsvField(row["CreatedAt"])
                }));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Quotes a value for CSV: doubles any embedded quote (a name containing " would
        /// otherwise break the column layout) and prefixes a leading =, +, - or @ with a
        /// single quote so spreadsheets treat it as text rather than a formula.
        /// </summary>
        internal static string CsvField(object value)
        {
            string text = value == null || value == DBNull.Value ? "" : value.ToString();
            if (text.Length > 0 && "=+-@\t\r".IndexOf(text[0]) >= 0)
                text = "'" + text;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
    }
}
