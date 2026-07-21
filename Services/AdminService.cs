using System;
using System.Data;
using System.Data.SqlClient;
using CSA.DataAccess;

namespace CSA.Services
{
    public static class AdminService
    {
        // ========================================================================
        // DASHBOARD
        // ========================================================================
        public static DataTable GetPlatformStats()
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT
                    (SELECT COUNT(*) FROM Users) AS UserCount,
                    (SELECT COUNT(*) FROM Courses WHERE IsPublished = 1) AS CourseCount,
                    (SELECT COUNT(*) FROM VirtualLabs WHERE IsPublished = 1) AS LabCount,
                    (SELECT COUNT(*) FROM SecurityAlerts WHERE AlertStatus = 'Open') AS AlertCount");
            return dt;
        }

        // ========================================================================
        // CONTENT REVIEW
        // ========================================================================
        public static DataTable GetPendingContent(string contentType)
        {
            return GetPendingContent(contentType, 0, 0, out _);
        }

        public static DataTable GetPendingContent(string contentType,
            int page, int pageSize, out int total)
        {
            string where = "WHERE (@Type = '' OR cf.ContentType = @Type)";

            string countSql = "SELECT COUNT(*) FROM ContentFlags cf " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Type", contentType ?? "")));

            int offset = Math.Max(0, (page - 1) * pageSize);

            string sql = $@"
                SELECT cf.FlagID, cf.ContentType, cf.ContentID, cf.Reason, cf.Status, cf.FlaggedAt,
                       u.FullName AS ReportedBy,
                       CASE cf.ContentType
                           WHEN 'Chapter' THEN ch.ChapterTitle
                           WHEN 'Quiz' THEN q.Title
                           WHEN 'Lab' THEN vl.LabTitle
                           ELSE 'Unknown'
                       END AS Title,
                       CASE cf.ContentType
                           WHEN 'Chapter' THEN LEFT(ch.Content, 100)
                           WHEN 'Quiz' THEN q.Description
                           WHEN 'Lab' THEN LEFT(vl.Scenario, 100)
                           ELSE ''
                       END AS Preview,
                       c.CourseName
                FROM ContentFlags cf
                JOIN Users u ON cf.ReportedByID = u.UserID
                LEFT JOIN Chapters ch ON cf.ContentType = 'Chapter' AND cf.ContentID = ch.ChapterID
                LEFT JOIN Quizzes q ON cf.ContentType = 'Quiz' AND cf.ContentID = q.QuizID
                LEFT JOIN VirtualLabs vl ON cf.ContentType = 'Lab' AND cf.ContentID = vl.LabID
                LEFT JOIN Courses c ON (ch.CourseID = c.CourseID OR q.CourseID = c.CourseID OR vl.CourseID = c.CourseID)
                {where}
                ORDER BY cf.FlaggedAt DESC";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Type", contentType ?? "")
            };
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static void ApproveContent(int flagId, string adminId)
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE ContentFlags SET Status = 'Removed', ReviewedByID = @AdminID, ReviewedAt = GETDATE()
                  WHERE FlagID = @ID",
                new SqlParameter("@ID", flagId),
                new SqlParameter("@AdminID", adminId));
            LogAudit(adminId, "APPROVE_CONTENT", "ContentFlags", flagId.ToString(), "", "");
        }

        public static void RejectContent(int flagId, string adminId)
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE ContentFlags SET Status = 'Dismissed', ReviewedByID = @AdminID, ReviewedAt = GETDATE()
                  WHERE FlagID = @ID",
                new SqlParameter("@ID", flagId),
                new SqlParameter("@AdminID", adminId));
            LogAudit(adminId, "REJECT_CONTENT", "ContentFlags", flagId.ToString(), "", "");
        }

        public static int GetPendingCount()
        {
            object r = DBHelper.ExecuteScalar("SELECT COUNT(*) FROM ContentFlags WHERE Status = 'Pending'");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        // ========================================================================
        // ANNOUNCEMENTS
        // ========================================================================
        public static DataTable SearchAnnouncements(string keyword)
        {
            return SearchAnnouncements(keyword, 0, 0, out _);
        }

        public static DataTable SearchAnnouncements(string keyword,
            int page, int pageSize, out int total)
        {
            string where = "WHERE (a.Title LIKE @Keyword OR @Keyword = '')";

            string countSql = "SELECT COUNT(*) FROM Announcements a " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%")));

            int offset = Math.Max(0, (page - 1) * pageSize);

            string sql = $@"
                SELECT a.AnnouncementID, a.Title, a.Body,
                       LEFT(a.Body, 80) AS MessagePreview,
                       a.IsActive, a.PublishedAt, a.ExpiresAt,
                       a.Audience, a.Priority,
                       u.FullName AS PublishedBy
                FROM Announcements a
                JOIN Users u ON a.PublishedByID = u.UserID
                {where}
                ORDER BY a.PublishedAt DESC";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static DataTable GetAnnouncementById(int id)
        {
            return DBHelper.ExecuteQuery(
                "SELECT * FROM Announcements WHERE AnnouncementID = @ID",
                new SqlParameter("@ID", id));
        }

        public static void CreateAnnouncement(string title, string body, string audience,
            string priority, DateTime? expiry, string adminId)
        {
            DBHelper.ExecuteNonQuery(
                @"INSERT INTO Announcements (Title, Body, PublishedByID, IsActive, PublishedAt, ExpiresAt, Audience, Priority)
                  VALUES (@Title, @Body, @AdminID, 1, GETDATE(), @Expiry, @Audience, @Priority)",
                new SqlParameter("@Title", title),
                new SqlParameter("@Body", body),
                new SqlParameter("@AdminID", adminId),
                new SqlParameter("@Expiry", (object)expiry ?? DBNull.Value),
                new SqlParameter("@Audience", audience ?? "All"),
                new SqlParameter("@Priority", priority ?? "Normal"));
            LogAudit(adminId, "CREATE_ANNOUNCEMENT", "Announcements", "0", "", title);
        }

        public static void UpdateAnnouncement(int id, string title, string body,
            string audience, string priority, DateTime? expiry)
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE Announcements SET Title = @Title, Body = @Body,
                        Audience = @Audience, Priority = @Priority,
                        ExpiresAt = @Expiry WHERE AnnouncementID = @ID",
                new SqlParameter("@Title", title),
                new SqlParameter("@Body", body),
                new SqlParameter("@Audience", audience ?? "All"),
                new SqlParameter("@Priority", priority ?? "Normal"),
                new SqlParameter("@Expiry", (object)expiry ?? DBNull.Value),
                new SqlParameter("@ID", id));
        }

        public static void DeleteAnnouncement(int id)
        {
            DBHelper.ExecuteNonQuery(
                "DELETE FROM Announcements WHERE AnnouncementID = @ID",
                new SqlParameter("@ID", id));
        }

        // ========================================================================
        // ACTIVITY / AUDIT LOGS
        // ========================================================================
        public static DataTable GetLogs(string keyword, string severity,
            string dateFrom, string dateTo, int page, int pageSize, out int total)
        {
            string sevExpr = @"CASE WHEN al.Action LIKE '%DELETE%' OR al.Action LIKE '%ERROR%' THEN 'Critical'
                               WHEN al.Action LIKE '%UPDATE%' OR al.Action LIKE '%CREATE%' THEN 'Warning'
                               ELSE 'Info' END";

            string where = "1=1";
            if (!string.IsNullOrEmpty(keyword))
                where += " AND (al.Action LIKE @Keyword OR u.FullName LIKE @Keyword)";
            if (!string.IsNullOrEmpty(severity))
                where += $" AND {sevExpr} = @Severity";
            if (!string.IsNullOrEmpty(dateFrom))
                where += " AND al.OccurredAt >= @DateFrom";
            if (!string.IsNullOrEmpty(dateTo))
                where += " AND al.OccurredAt < DATEADD(DAY, 1, @DateTo)";

            string countSql = "SELECT COUNT(*) FROM AuditLog al JOIN Users u ON al.PerformedByID = u.UserID WHERE " + where;
            var countPars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + (keyword ?? "") + "%"),
                new SqlParameter("@Severity", severity ?? "")
            };
            if (!string.IsNullOrEmpty(dateFrom))
                countPars.Add(new SqlParameter("@DateFrom", (object)dateFrom));
            if (!string.IsNullOrEmpty(dateTo))
                countPars.Add(new SqlParameter("@DateTo", (object)dateTo));
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql, countPars.ToArray()));

            string sql = $@"
                SELECT al.AuditID, al.Action, al.TableAffected, al.RecordID, al.OccurredAt,
                       u.FullName AS UserName, u.Email AS UserEmail,
                       {sevExpr} AS Severity,
                       al.Action AS Details
                FROM AuditLog al
                JOIN Users u ON al.PerformedByID = u.UserID
                WHERE {where}
                ORDER BY al.OccurredAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + (keyword ?? "") + "%"),
                new SqlParameter("@Severity", severity ?? "")
            };
            if (!string.IsNullOrEmpty(dateFrom))
                pars.Add(new SqlParameter("@DateFrom", (object)dateFrom));
            if (!string.IsNullOrEmpty(dateTo))
                pars.Add(new SqlParameter("@DateTo", (object)dateTo));
            pars.Add(new SqlParameter("@Offset", (page - 1) * pageSize));
            pars.Add(new SqlParameter("@PageSize", pageSize));

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static int CountBySeverity(string severity)
        {
            string sevExpr = @"CASE WHEN Action LIKE '%DELETE%' OR Action LIKE '%ERROR%' THEN 'Critical'
                               WHEN Action LIKE '%UPDATE%' OR Action LIKE '%CREATE%' THEN 'Warning'
                               ELSE 'Info' END";
            object r = DBHelper.ExecuteScalar(
                $"SELECT COUNT(*) FROM AuditLog WHERE {sevExpr} = @Severity AND CAST(OccurredAt AS DATE) = CAST(GETDATE() AS DATE)",
                new SqlParameter("@Severity", severity));
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public static void LogAudit(string performedById, string action, string table,
            string recordId, string beforeValue, string afterValue, string ipAddress = "")
        {
            DBHelper.ExecuteNonQuery(
                @"INSERT INTO AuditLog (PerformedByID, Action, TableAffected, RecordID, BeforeValue, AfterValue, IPAddress, OccurredAt)
                  VALUES (@UserID, @Action, @Table, @RecordID, @Before, @After,
                          @IP, GETDATE())",
                new SqlParameter("@UserID", performedById),
                new SqlParameter("@Action", action),
                new SqlParameter("@Table", table),
                new SqlParameter("@RecordID", recordId),
                new SqlParameter("@Before", beforeValue ?? ""),
                new SqlParameter("@After", afterValue ?? ""),
                new SqlParameter("@IP", ipAddress));
        }

        // ========================================================================
        // DATABASE BACKUP
        // ========================================================================
        public static DataTable GetBackups()
        {
            return DBHelper.ExecuteQuery(@"
                SELECT BackupID, BackupLabel AS Label, BackupType, FileSize, Status, CreatedAt,
                       CASE WHEN FileSize > 1048576 THEN CAST(ROUND(FileSize/1048576.0,1) AS NVARCHAR)+' MB'
                            WHEN FileSize > 1024 THEN CAST(ROUND(FileSize/1024.0,1) AS NVARCHAR)+' KB'
                            ELSE CAST(FileSize AS NVARCHAR)+' B'
                       END AS SizeDisplay,
                       FORMAT(CreatedAt, 'dd MMM yyyy HH:mm') AS CreatedDisplay
                FROM DatabaseBackups ORDER BY CreatedAt DESC");
        }

        public static bool ExecuteBackup(string label, string type, string adminId, out string error)
        {
            error = "";
            try
            {
                object nameResult = DBHelper.ExecuteScalar("SELECT DB_NAME()");
                string dbName = nameResult != null ? nameResult.ToString() : "CyberShieldAcademy";

                string backupDir = @"C:\Users\ivanc\source\repos\wapp\App_Data\Backups\";
                System.IO.Directory.CreateDirectory(backupDir);
                string fileName = $"CyberShieldAcademy_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string filePath = backupDir + fileName;

                string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = @Path WITH FORMAT, MEDIANAME = 'CSABackup', NAME = @Label; SELECT 1 AS Result";
                object result = DBHelper.ExecuteScalar(sql,
                    new SqlParameter("@Path", filePath),
                    new SqlParameter("@Label", string.IsNullOrEmpty(label) ? "Manual backup" : label));

                var fi = new System.IO.FileInfo(filePath);
                DBHelper.ExecuteNonQuery(
                    @"INSERT INTO DatabaseBackups (BackupLabel, BackupType, FilePath, FileSize, Status, CreatedByID, CreatedAt)
                      VALUES (@Label, @Type, @Path, @Size, 'Success', @UserID, GETDATE())",
                    new SqlParameter("@Label", string.IsNullOrEmpty(label) ? "Manual backup" : label),
                    new SqlParameter("@Type", type),
                    new SqlParameter("@Path", filePath),
                    new SqlParameter("@Size", fi.Length),
                    new SqlParameter("@UserID", adminId));

                LogAudit(adminId, "CREATE_BACKUP", "DatabaseBackups", "0", "", filePath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                DBHelper.ExecuteScalar("SELECT 1");
                return true;
            }
            catch { return false; }
        }

        // ========================================================================
        // SECURITY ALERTS
        // ========================================================================
        public static DataTable GetAlerts(string keyword, string severity, string status)
        {
            return GetAlerts(keyword, severity, status, 0, 0, out _);
        }

        public static DataTable GetAlerts(string keyword, string severity, string status,
            int page, int pageSize, out int total)
        {
            string where = @"WHERE (sa.AlertType LIKE @Keyword OR sa.Description LIKE @Keyword OR @Keyword = '')
                  AND (@Severity = '' OR sa.Severity = @Severity)
                  AND (@Status = '' OR sa.AlertStatus = @Status)";

            string countSql = "SELECT COUNT(*) FROM SecurityAlerts sa " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Severity", severity ?? ""),
                new SqlParameter("@Status", status ?? "")));

            int offset = Math.Max(0, (page - 1) * pageSize);

            string sql = $@"
                SELECT sa.AlertID, sa.AlertType, sa.Description, sa.Severity,
                       sa.AlertStatus, sa.DetectedAt,
                       u.FullName AS AffectedUser, u.Email AS AffectedEmail,
                       FORMAT(sa.DetectedAt, 'dd MMM yyyy HH:mm') AS DetectedDisplay
                FROM SecurityAlerts sa
                JOIN Users u ON sa.AffectedUserID = u.UserID
                {where}
                ORDER BY sa.DetectedAt DESC";

            if (pageSize > 0)
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var pars = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Severity", severity ?? ""),
                new SqlParameter("@Status", status ?? "")
            };
            if (pageSize > 0)
            {
                pars.Add(new SqlParameter("@Offset", offset));
                pars.Add(new SqlParameter("@PageSize", pageSize));
            }

            return DBHelper.ExecuteQuery(sql, pars.ToArray());
        }

        public static void SetAlertStatus(int alertId, string status, string adminId)
        {
            DBHelper.ExecuteNonQuery(
                "UPDATE SecurityAlerts SET AlertStatus = @Status, ReviewedByID = @AdminID, ReviewedAt = GETDATE() WHERE AlertID = @ID",
                new SqlParameter("@ID", alertId),
                new SqlParameter("@Status", status),
                new SqlParameter("@AdminID", adminId));
            LogAudit(adminId, $"ALERT_{status.ToUpper()}", "SecurityAlerts", alertId.ToString(), "", status);
        }

        public static DataTable GetAlertById(int alertId)
        {
            return DBHelper.ExecuteQuery(
                "SELECT * FROM SecurityAlerts WHERE AlertID = @ID",
                new SqlParameter("@ID", alertId));
        }

        public static DataTable GetChartData()
        {
            return DBHelper.ExecuteQuery(@"
                SELECT 'Users by Role' AS ChartGroup, r.RoleName AS Label, COUNT(*) AS Value
                FROM Users u JOIN Roles r ON u.RoleID = r.RoleID
                GROUP BY r.RoleName
                UNION ALL
                SELECT 'Courses by Level' AS ChartGroup, c.Level AS Label, COUNT(*) AS Value
                FROM Courses c
                GROUP BY c.Level
                UNION ALL
                SELECT 'Labs by Status' AS ChartGroup,
                    CASE WHEN IsPublished = 1 THEN 'Published' ELSE 'Draft' END AS Label,
                    COUNT(*) AS Value
                FROM VirtualLabs
                GROUP BY CASE WHEN IsPublished = 1 THEN 'Published' ELSE 'Draft' END");
        }

        public static int GetResolvedTodayCount()
        {
            object r = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM SecurityAlerts WHERE AlertStatus = 'Resolved' AND CAST(ReviewedAt AS DATE) = CAST(GETDATE() AS DATE)");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public static int GetAlertCountByStatus(string status)
        {
            object r = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM SecurityAlerts WHERE AlertStatus = @Status",
                new SqlParameter("@Status", status));
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public static int GetAlertCountBySeverity(string severity)
        {
            object r = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM SecurityAlerts WHERE Severity = @Severity",
                new SqlParameter("@Severity", severity));
            return r != null ? Convert.ToInt32(r) : 0;
        }

        // ========================================================================
        // ERROR LOGS
        // ========================================================================
        public static DataTable GetErrorLogs(string keyword, string severity, string dateFrom, string dateTo, int page, int pageSize, out int total)
        {
            string where = "1=1";
            if (!string.IsNullOrEmpty(keyword))
                where += " AND (el.Message LIKE @Keyword OR el.ErrorType LIKE @Keyword OR el.PageURL LIKE @Keyword)";
            if (!string.IsNullOrEmpty(severity))
                where += " AND el.Severity = @Severity";
            if (!string.IsNullOrEmpty(dateFrom))
                where += " AND el.OccurredAt >= @DateFrom";
            if (!string.IsNullOrEmpty(dateTo))
                where += " AND el.OccurredAt < DATEADD(DAY, 1, @DateTo)";

            string countSql = "SELECT COUNT(*) FROM ErrorLogs el WHERE " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Severity", severity ?? ""),
                new SqlParameter("@DateFrom", (object)dateFrom ?? DBNull.Value),
                new SqlParameter("@DateTo", (object)dateTo ?? DBNull.Value)));

            string sql = $@"
                SELECT el.ErrorID, el.ErrorType, el.Message, el.PageURL, el.Severity, el.IsResolved, el.OccurredAt,
                       u.FullName AS UserName,
                       el.UserAgent,
                       LEFT(el.Message, 200) AS MessagePreview
                FROM ErrorLogs el
                LEFT JOIN Users u ON el.UserID = u.UserID
                WHERE {where}
                ORDER BY el.OccurredAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@Severity", severity ?? ""),
                new SqlParameter("@DateFrom", (object)dateFrom ?? DBNull.Value),
                new SqlParameter("@DateTo", (object)dateTo ?? DBNull.Value),
                new SqlParameter("@Offset", (page - 1) * pageSize),
                new SqlParameter("@PageSize", pageSize));
        }

        public static int GetErrorLogCountBySeverity(string severity)
        {
            object r = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM ErrorLogs WHERE Severity = @Sev AND CAST(OccurredAt AS DATE) = CAST(GETDATE() AS DATE)",
                new SqlParameter("@Sev", severity));
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public static int GetErrorLogUnresolvedCount()
        {
            object r = DBHelper.ExecuteScalar("SELECT COUNT(*) FROM ErrorLogs WHERE IsResolved = 0");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public static string ExportAlertsCsv(string keyword, string severity, string status)
        {
            DataTable dt = GetAlerts(keyword, severity, status);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("AlertID,AlertType,Severity,Status,AffectedUser,AffectedEmail,DetectedAt,Description");
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine($"{row["AlertID"]},\"{row["AlertType"]}\",\"{row["Severity"]}\",\"{row["AlertStatus"]}\",\"{row["AffectedUser"]}\",\"{row["AffectedEmail"]}\",{row["DetectedAt"]},\"{((string)row["Description"]).Replace("\"", "\"\"")}\"");
            }
            return sb.ToString();
        }

        public static void MarkErrorResolved(int errorId, string adminId)
        {
            DBHelper.ExecuteNonQuery(
                "UPDATE ErrorLogs SET IsResolved = 1 WHERE ErrorID = @ID",
                new SqlParameter("@ID", errorId));
            LogAudit(adminId, "RESOLVE_ERROR", "ErrorLogs", errorId.ToString(), "", "");
        }
    }
}
