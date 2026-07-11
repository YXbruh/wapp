using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;

namespace CSA.Services
{
    public static class UserService
    {
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

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET LastLoginDate = GETDATE() WHERE UserID = @ID",
                new SqlParameter("@ID", userId));

            return true;
        }

        // ----- REGISTER -----
        public static bool Register(string fullName, string email, string password, out string errorMsg)
        {
            errorMsg = "";
            object exists = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE Email = @Email",
                new SqlParameter("@Email", email));
            if (exists != null && Convert.ToInt32(exists) > 0)
            { errorMsg = "An account with that email already exists."; return false; }

            string hash = PasswordHelper.Hash(password);
            string studentRoleId = GetRoleIdByName("Student");

            DBHelper.ExecuteNonQuery(
                @"INSERT INTO Users (FullName, Email, PasswordHash, RoleID, IsActive, CreatedAt)
                  VALUES (@Name, @Email, @Hash, @RoleID, 1, GETDATE())",
                new SqlParameter("@Name", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@Hash", hash),
                new SqlParameter("@RoleID", studentRoleId));

            return true;
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
                new SqlParameter("@StudentID", (object)studentId ?? DBNull.Value),
                new SqlParameter("@Phone", (object)phone ?? DBNull.Value),
                new SqlParameter("@Dept", (object)department ?? DBNull.Value));
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
                new SqlParameter("@Phone", (object)phone ?? DBNull.Value),
                new SqlParameter("@Dept", (object)department ?? DBNull.Value),
                new SqlParameter("@StudentID", (object)studentId ?? DBNull.Value),
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

        public static string ExportCsv(string keyword, string role, string status)
        {
            DataTable dt = Search(keyword, role, status);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("UserID,FullName,Email,Role,IsActive,LastLogin,CreatedAt");
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine($"{row["UserID"]},\"{row["FullName"]}\",\"{row["Email"]}\",{row["Role"]},{row["IsActive"]},{row["LastLoginDate"]},{row["CreatedAt"]}");
            }
            return sb.ToString();
        }
    }
}
