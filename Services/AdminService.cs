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
                    (SELECT COUNT(*) FROM Chapters     WHERE IsPublished = 0)
                  + (SELECT COUNT(*) FROM Quizzes      WHERE IsPublished = 0)
                  + (SELECT COUNT(*) FROM VirtualLabs  WHERE IsPublished = 0) AS PendingReviewCount");
            return dt;
        }

        /// <summary>
        /// Live counts for the public home page, so the landing stats reflect the
        /// real platform instead of hard-coded placeholder numbers.
        /// AvgRating is NULL when no feedback has been left yet.
        /// </summary>
        public static DataRow GetHomepageStats()
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT
                    (SELECT COUNT(*) FROM Users u JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE r.RoleName = 'Student')                       AS StudentCount,
                    (SELECT COUNT(*) FROM Courses     WHERE IsPublished = 1) AS CourseCount,
                    (SELECT COUNT(*) FROM VirtualLabs WHERE IsPublished = 1) AS LabCount,
                    (SELECT AVG(CAST(StarRating AS FLOAT)) FROM Feedback
                        WHERE StarRating IS NOT NULL)                       AS AvgRating");
            return dt.Rows[0];
        }

        // ========================================================================
        // CONTENT REVIEW
        //
        // Lecturers author chapters, quizzes and labs as drafts (IsPublished = 0).
        // Nothing reaches students until an admin reviews it here and publishes it,
        // so "pending" simply means the draft rows across those three tables.
        // ========================================================================

        /// <summary>
        /// Draft chapters, quizzes and labs, unioned into one reviewable list.
        /// ContentID is the table's own NVARCHAR key (e.g. "QUZ001"), not an int.
        /// </summary>
        private const string PendingContentCte = @"
            WITH Pending AS (
                SELECT 'Chapter' AS ContentType, ch.ChapterID AS ContentID,
                       ch.ChapterTitle AS Title, LEFT(ISNULL(ch.Content, ''), 200) AS Preview,
                       ch.CourseID, ch.CreatedByID, ch.UpdatedAt AS SubmittedAt
                FROM   Chapters ch WHERE ch.IsPublished = 0
                UNION ALL
                SELECT 'Quiz', q.QuizID,
                       q.Title, LEFT(ISNULL(q.Description, ''), 200),
                       q.CourseID, q.CreatedByID, q.UpdatedAt
                FROM   Quizzes q WHERE q.IsPublished = 0
                UNION ALL
                SELECT 'Lab', vl.LabID,
                       vl.LabTitle, LEFT(ISNULL(vl.Scenario, ''), 200),
                       vl.CourseID, vl.CreatedByID, vl.UpdatedAt
                FROM   VirtualLabs vl WHERE vl.IsPublished = 0
            )";

        public static DataTable GetPendingContent(string contentType)
        {
            return GetPendingContent(contentType, 0, 0, out _);
        }

        public static DataTable GetPendingContent(string contentType,
            int page, int pageSize, out int total)
        {
            const string where = "WHERE (@Type = '' OR p.ContentType = @Type)";

            total = Convert.ToInt32(DBHelper.ExecuteScalar(
                PendingContentCte + " SELECT COUNT(*) FROM Pending p " + where,
                new SqlParameter("@Type", contentType ?? "")));

            int offset = Math.Max(0, (page - 1) * pageSize);

            string sql = PendingContentCte + $@"
                SELECT p.ContentType, p.ContentID, p.Title, p.Preview, p.SubmittedAt,
                       u.FullName AS SubmittedBy,
                       ISNULL(c.CourseName, '—') AS CourseName
                FROM   Pending p
                JOIN   Users u ON p.CreatedByID = u.UserID
                LEFT JOIN Courses c ON p.CourseID = c.CourseID
                {where}
                ORDER BY p.SubmittedAt DESC, p.ContentID";

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

        /// <summary>
        /// One piece of content in full, for the preview page. Returns null when the
        /// type is unknown or the row no longer exists.
        /// </summary>
        public static DataRow GetContentDetail(string contentType, string contentId)
        {
            string sql;
            switch (contentType)
            {
                case "Chapter":
                    sql = @"SELECT 'Chapter' AS ContentType, ch.ChapterID AS ContentID,
                                   ch.ChapterTitle AS Title, ISNULL(ch.Content, '') AS Body,
                                   ch.IsPublished, ch.UpdatedAt AS SubmittedAt, ch.CreatedAt,
                                   ch.SortOrder,
                                   u.FullName AS SubmittedBy, ISNULL(c.CourseName, '—') AS CourseName
                            FROM   Chapters ch
                            JOIN   Users u ON ch.CreatedByID = u.UserID
                            LEFT JOIN Courses c ON ch.CourseID = c.CourseID
                            WHERE  ch.ChapterID = @ID";
                    break;
                case "Quiz":
                    sql = @"SELECT 'Quiz' AS ContentType, q.QuizID AS ContentID,
                                   q.Title, ISNULL(q.Description, '') AS Body,
                                   q.IsPublished, q.UpdatedAt AS SubmittedAt, q.CreatedAt,
                                   q.TotalMarks, q.PassMark, q.MaxAttempts, q.DurationMinutes,
                                   q.StartDate, q.EndDate,
                                   (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizID = q.QuizID) AS QuestionCount,
                                   u.FullName AS SubmittedBy, ISNULL(c.CourseName, '—') AS CourseName
                            FROM   Quizzes q
                            JOIN   Users u ON q.CreatedByID = u.UserID
                            LEFT JOIN Courses c ON q.CourseID = c.CourseID
                            WHERE  q.QuizID = @ID";
                    break;
                case "Lab":
                    sql = @"SELECT 'Lab' AS ContentType, vl.LabID AS ContentID,
                                   vl.LabTitle AS Title, ISNULL(vl.Scenario, '') AS Body,
                                   vl.IsPublished, vl.UpdatedAt AS SubmittedAt, vl.CreatedAt,
                                   vl.Difficulty, vl.SkillTag, vl.PointsReward, vl.TimeLimitMinutes,
                                   vl.ValidationType, vl.ExpectedCommand, ISNULL(vl.HintText, '') AS HintText,
                                   u.FullName AS SubmittedBy, ISNULL(c.CourseName, '—') AS CourseName
                            FROM   VirtualLabs vl
                            JOIN   Users u ON vl.CreatedByID = u.UserID
                            LEFT JOIN Courses c ON vl.CourseID = c.CourseID
                            WHERE  vl.LabID = @ID";
                    break;
                default:
                    return null;
            }

            DataTable dt = DBHelper.ExecuteQuery(sql, new SqlParameter("@ID", contentId ?? ""));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Full question list for a quiz, so an admin can review the actual questions,
        /// options and correct answers before publishing. Ordered as the author arranged them.
        /// </summary>
        public static DataTable GetQuizQuestionsForReview(string quizId)
        {
            return DBHelper.ExecuteQuery(
                @"SELECT QuestionText, QuestionType, OptionA, OptionB, OptionC, OptionD,
                         CorrectAnswer, ISNULL(Explanation, '') AS Explanation, Points, SortOrder
                  FROM   QuizQuestions
                  WHERE  QuizID = @ID
                  ORDER BY SortOrder, QuestionID",
                new SqlParameter("@ID", quizId ?? ""));
        }

        /// <summary>Table and key column backing a content type, or null if unknown.</summary>
        private static bool TryResolveContentTable(string contentType,
            out string table, out string keyColumn)
        {
            switch (contentType)
            {
                case "Chapter": table = "Chapters";     keyColumn = "ChapterID"; return true;
                case "Quiz":    table = "Quizzes";      keyColumn = "QuizID";    return true;
                case "Lab":     table = "VirtualLabs";  keyColumn = "LabID";     return true;
                default:        table = null;           keyColumn = null;        return false;
            }
        }

        /// <summary>
        /// Approves a submission: the draft goes live for students. Returns false when
        /// the content type is unknown or the row has since been deleted.
        /// </summary>
        public static bool ApproveContent(string contentType, string contentId, string adminId)
        {
            return SetContentPublished(contentType, contentId, adminId, true);
        }

        /// <summary>
        /// Rejects a submission: it stays (or returns to) a draft that only its author
        /// can see, and the decision is recorded in the audit log.
        /// </summary>
        public static bool RejectContent(string contentType, string contentId, string adminId)
        {
            return SetContentPublished(contentType, contentId, adminId, false);
        }

        /// <summary>
        /// Sends a submission back to its author for changes: it stays a draft, the
        /// requested changes are recorded in the audit log, and the author is emailed.
        /// Returns false when the content type is unknown or the row no longer exists.
        /// <paramref name="emailSent"/> reports whether the notification email was delivered.
        /// </summary>
        public static bool RequestRevision(string contentType, string contentId,
            string adminId, string message, out bool emailSent)
        {
            emailSent = false;

            if (!TryResolveContentTable(contentType, out string table, out string key))
                return false;

            // The item must stay a draft the author can edit and resubmit.
            int rows = DBHelper.ExecuteNonQuery(
                $@"UPDATE {table} SET IsPublished = 0, UpdatedAt = GETDATE()
                   WHERE {key} = @ID",
                new SqlParameter("@ID", contentId ?? ""));
            if (rows == 0) return false;

            LogAudit(adminId, "REQUEST_REVISION", table, contentId, "", message);

            // Notify the lecturer who authored it (best-effort — a mail failure must
            // not undo the revision request itself).
            DataRow author = GetContentAuthorContact(contentType, contentId);
            if (author != null)
            {
                string title = author["Title"].ToString();
                string body =
                    "<h2>Revision requested</h2>" +
                    $"<p>Hi {System.Net.WebUtility.HtmlEncode(author["FullName"].ToString())},</p>" +
                    $"<p>An administrator has requested changes to your {System.Net.WebUtility.HtmlEncode(contentType.ToLower())} " +
                    $"\"<strong>{System.Net.WebUtility.HtmlEncode(title)}</strong>\" before it can be published.</p>" +
                    "<p><strong>Requested changes:</strong></p>" +
                    $"<blockquote style='border-left:3px solid #ccc;padding-left:12px;color:#444'>{System.Net.WebUtility.HtmlEncode(message)}</blockquote>" +
                    "<p>Please update the content and resubmit it for review.</p>" +
                    "<hr><p><small>Sent from CyberShield Academy</small></p>";
                emailSent = EmailService.Send(author["Email"].ToString(),
                    "Revision requested: " + title, body);
            }
            return true;
        }

        /// <summary>Author (lecturer) name/email plus the content title, for the revision email.</summary>
        private static DataRow GetContentAuthorContact(string contentType, string contentId)
        {
            string sql;
            switch (contentType)
            {
                case "Chapter":
                    sql = @"SELECT ch.ChapterTitle AS Title, u.FullName, u.Email
                            FROM Chapters ch JOIN Users u ON ch.CreatedByID = u.UserID
                            WHERE ch.ChapterID = @ID";
                    break;
                case "Quiz":
                    sql = @"SELECT q.Title, u.FullName, u.Email
                            FROM Quizzes q JOIN Users u ON q.CreatedByID = u.UserID
                            WHERE q.QuizID = @ID";
                    break;
                case "Lab":
                    sql = @"SELECT vl.LabTitle AS Title, u.FullName, u.Email
                            FROM VirtualLabs vl JOIN Users u ON vl.CreatedByID = u.UserID
                            WHERE vl.LabID = @ID";
                    break;
                default:
                    return null;
            }
            DataTable dt = DBHelper.ExecuteQuery(sql, new SqlParameter("@ID", contentId ?? ""));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static bool SetContentPublished(string contentType, string contentId,
            string adminId, bool publish)
        {
            // The type only ever picks a hard-coded table name — never a caller string.
            if (!TryResolveContentTable(contentType, out string table, out string key))
                return false;

            int rows = DBHelper.ExecuteNonQuery(
                $@"UPDATE {table} SET IsPublished = @Published, UpdatedAt = GETDATE()
                   WHERE {key} = @ID",
                new SqlParameter("@Published", publish ? 1 : 0),
                new SqlParameter("@ID", contentId ?? ""));

            if (rows == 0) return false;

            LogAudit(adminId, publish ? "PUBLISH_CONTENT" : "REJECT_CONTENT",
                     table, contentId, "", contentType);
            return true;
        }

        /// <summary>Drafts still waiting on an admin decision.</summary>
        public static int GetPendingCount()
        {
            object r = DBHelper.ExecuteScalar(@"
                SELECT (SELECT COUNT(*) FROM Chapters    WHERE IsPublished = 0)
                     + (SELECT COUNT(*) FROM Quizzes     WHERE IsPublished = 0)
                     + (SELECT COUNT(*) FROM VirtualLabs WHERE IsPublished = 0)");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        /// <summary>Content already approved and visible to students.</summary>
        public static int GetPublishedCount()
        {
            object r = DBHelper.ExecuteScalar(@"
                SELECT (SELECT COUNT(*) FROM Chapters    WHERE IsPublished = 1)
                     + (SELECT COUNT(*) FROM Quizzes     WHERE IsPublished = 1)
                     + (SELECT COUNT(*) FROM VirtualLabs WHERE IsPublished = 1)");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        /// <summary>Review decisions an admin has recorded today, from the audit trail.</summary>
        public static int GetReviewedTodayCount()
        {
            object r = DBHelper.ExecuteScalar(@"
                SELECT COUNT(*) FROM AuditLog
                WHERE Action IN ('PUBLISH_CONTENT', 'REJECT_CONTENT')
                  AND CAST(OccurredAt AS DATE) = CAST(GETDATE() AS DATE)");
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
                       a.IsActive, a.PublishedAt,
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

        // Announcements are delivered by email the moment they are published, so there
        // is nothing left on the site to expire — the row is purely a sent-record.
        public static void CreateAnnouncement(string title, string body, string audience,
            string priority, string adminId)
        {
            // AnnouncementID is IDENTITY, so read it back with SCOPE_IDENTITY() and audit
            // the real key instead of a placeholder "0".
            object newId = DBHelper.ExecuteScalar(
                @"INSERT INTO Announcements (Title, Body, PublishedByID, IsActive, PublishedAt, Audience, Priority)
                  VALUES (@Title, @Body, @AdminID, 1, GETDATE(), @Audience, @Priority);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new SqlParameter("@Title", title),
                new SqlParameter("@Body", body),
                new SqlParameter("@AdminID", adminId),
                new SqlParameter("@Audience", audience ?? "All"),
                new SqlParameter("@Priority", priority ?? "Normal"));

            string recordId = newId == null || newId == DBNull.Value ? "" : newId.ToString();
            LogAudit(adminId, "CREATE_ANNOUNCEMENT", "Announcements", recordId, "", title);
        }

        public static void UpdateAnnouncement(int id, string title, string body,
            string audience, string priority)
        {
            DBHelper.ExecuteNonQuery(
                @"UPDATE Announcements SET Title = @Title, Body = @Body,
                        Audience = @Audience, Priority = @Priority
                  WHERE AnnouncementID = @ID",
                new SqlParameter("@Title", title),
                new SqlParameter("@Body", body),
                new SqlParameter("@Audience", audience ?? "All"),
                new SqlParameter("@Priority", priority ?? "Normal"),
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
            string sevWhere = "";
            if (!string.IsNullOrEmpty(severity))
            {
                if (severity == "Critical")
                    sevWhere = " AND (al.Action LIKE '%DELETE%' OR al.Action LIKE '%ERROR%')";
                else if (severity == "Warning")
                    sevWhere = " AND (al.Action LIKE '%CREATE%' OR al.Action LIKE '%UPDATE%') AND al.Action NOT LIKE '%DELETE%' AND al.Action NOT LIKE '%ERROR%'";
                else
                    sevWhere = " AND al.Action NOT LIKE '%CREATE%' AND al.Action NOT LIKE '%UPDATE%' AND al.Action NOT LIKE '%DELETE%' AND al.Action NOT LIKE '%ERROR%'";
            }

            string where = "1=1";
            if (!string.IsNullOrEmpty(keyword))
                where += " AND (al.Action LIKE @Keyword OR al.AfterValue LIKE @Keyword OR al.IPAddress LIKE @Keyword OR u.FullName LIKE @Keyword)";
            where += sevWhere;
            if (!string.IsNullOrEmpty(dateFrom))
                where += " AND al.OccurredAt >= @DateFrom";
            if (!string.IsNullOrEmpty(dateTo))
                where += " AND al.OccurredAt < DATEADD(DAY, 1, @DateTo)";

            string countSql = "SELECT COUNT(*) FROM AuditLog al JOIN Users u ON al.PerformedByID = u.UserID WHERE " + where;
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@DateFrom", (object)dateFrom ?? DBNull.Value),
                new SqlParameter("@DateTo", (object)dateTo ?? DBNull.Value)));

            string severityExpr = "CASE WHEN al.Action LIKE '%DELETE%' OR al.Action LIKE '%ERROR%' THEN 'Critical' WHEN al.Action LIKE '%UPDATE%' OR al.Action LIKE '%CREATE%' THEN 'Warning' ELSE 'Info' END";

            string sql = $@"
                SELECT al.AuditID, al.Action, al.TableAffected, al.RecordID, al.IPAddress, al.OccurredAt,
                       u.FullName AS UserName, u.Email AS UserEmail,
                       {severityExpr} AS Severity,
                       ISNULL(NULLIF(al.AfterValue, ''), al.Action) AS Details
                FROM AuditLog al
                JOIN Users u ON al.PerformedByID = u.UserID
                WHERE {where}
                ORDER BY al.OccurredAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@Keyword", "%" + keyword + "%"),
                new SqlParameter("@DateFrom", (object)dateFrom ?? DBNull.Value),
                new SqlParameter("@DateTo", (object)dateTo ?? DBNull.Value),
                new SqlParameter("@Offset", (page - 1) * pageSize),
                new SqlParameter("@PageSize", pageSize));
        }

        public static int CountBySeverity(string severity)
        {
            string where;
            if (severity == "Critical")
                where = "(Action LIKE '%DELETE%' OR Action LIKE '%ERROR%')";
            else if (severity == "Warning")
                where = "(Action LIKE '%CREATE%' OR Action LIKE '%UPDATE%') AND Action NOT LIKE '%DELETE%' AND Action NOT LIKE '%ERROR%'";
            else
                where = "Action NOT LIKE '%CREATE%' AND Action NOT LIKE '%UPDATE%' AND Action NOT LIKE '%DELETE%' AND Action NOT LIKE '%ERROR%'";

            object r = DBHelper.ExecuteScalar(
                $"SELECT COUNT(*) FROM AuditLog WHERE {where} AND CAST(OccurredAt AS DATE) = CAST(GETDATE() AS DATE)");
            return r != null ? Convert.ToInt32(r) : 0;
        }

        /// <summary>
        /// Records something a user did, for the admin Activity Logs page: a sign-in, a
        /// student finishing a chapter/quiz/lab/course, a lecturer authoring content.
        /// <paramref name="details"/> is the readable line the page shows, so write it as
        /// a sentence ("Completed chapter: Cross-Site Scripting").
        ///
        /// Logging must never break the action being logged, so failures are swallowed.
        /// </summary>
        public static void LogActivity(string userId, string action, string table,
            string recordId, string details)
        {
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                LogAudit(userId, action, table, recordId ?? "", "", details ?? "",
                         CurrentIpAddress());
            }
            catch
            {
                // An unwritable audit row is not worth failing a student's submission over.
            }
        }

        /// <summary>Caller's IP, or "" outside a request.</summary>
        private static string CurrentIpAddress()
        {
            var ctx = System.Web.HttpContext.Current;
            if (ctx == null) return "";
            try { return ctx.Request.UserHostAddress ?? ""; }
            catch { return ""; }
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
            return GetBackups(0, 0, out _);
        }

        public static DataTable GetBackups(int page, int pageSize, out int total)
        {
            string countSql = "SELECT COUNT(*) FROM DatabaseBackups";
            total = Convert.ToInt32(DBHelper.ExecuteScalar(countSql));

            int offset = Math.Max(0, (page - 1) * pageSize);
            string sql = @"
                SELECT BackupID, BackupLabel AS Label, BackupType, FileSize, Status, CreatedAt,
                       CASE WHEN FileSize > 1048576 THEN CAST(ROUND(FileSize/1048576.0,1) AS NVARCHAR)+' MB'
                            WHEN FileSize > 1024 THEN CAST(ROUND(FileSize/1024.0,1) AS NVARCHAR)+' KB'
                            ELSE CAST(FileSize AS NVARCHAR)+' B'
                       END AS SizeDisplay,
                       FORMAT(CreatedAt, 'dd MMM yyyy HH:mm') AS CreatedDisplay
                FROM DatabaseBackups ORDER BY CreatedAt DESC";

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

        public static bool ExecuteBackup(string label, string type, string adminId, out string error)
        {
            error = "";
            try
            {
                string backupDir = System.IO.Path.Combine(
                    System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), "Backups");
                System.IO.Directory.CreateDirectory(backupDir);

                // Get the actual database name (without path)
                string dbName = DBHelper.ExecuteScalar("SELECT DB_NAME()")?.ToString() ?? "CyberShieldAcademy";
                // Clean up database name for filename (remove path if present)
                string cleanDbName = System.IO.Path.GetFileNameWithoutExtension(dbName);
                if (string.IsNullOrEmpty(cleanDbName) || cleanDbName.Contains("\\") || cleanDbName.Contains(":"))
                {
                    cleanDbName = "CyberShieldAcademy";
                }

                string fileName = $"{cleanDbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string filePath = System.IO.Path.Combine(backupDir, fileName);

                string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = @Path WITH FORMAT, MEDIANAME = 'CSABackup', NAME = @Label; SELECT 1 AS Result";
                object result = DBHelper.ExecuteScalar(sql,
                    new SqlParameter("@Path", filePath),
                    new SqlParameter("@Label", string.IsNullOrEmpty(label) ? "Manual backup" : label));

                var fi = new System.IO.FileInfo(filePath);
                object backupId = DBHelper.ExecuteScalar(
                    @"INSERT INTO DatabaseBackups (BackupLabel, BackupType, FilePath, FileSize, Status, CreatedByID, CreatedAt)
                      VALUES (@Label, @Type, @Path, @Size, 'Success', @UserID, GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new SqlParameter("@Label", string.IsNullOrEmpty(label) ? "Manual backup" : label),
                    new SqlParameter("@Type", type),
                    new SqlParameter("@Path", filePath),
                    new SqlParameter("@Size", fi.Length),
                    new SqlParameter("@UserID", adminId));

                string backupRecordId = backupId == null || backupId == DBNull.Value ? "" : backupId.ToString();
                LogAudit(adminId, "CREATE_BACKUP", "DatabaseBackups", backupRecordId, "", filePath);
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

        public static DataTable GetEmailsByAudience(string audience)
        {
            // "All" (or anything unrecognised) means every active user.
            string roleName = null;
            if (audience == "Students") roleName = "Student";
            else if (audience == "Lecturers") roleName = "Lecturer";
            else if (audience == "Admins") roleName = "Admin";

            return DBHelper.ExecuteQuery(@"
                SELECT DISTINCT u.Email
                FROM Users u
                JOIN Roles r ON u.RoleID = r.RoleID
                WHERE (@Role IS NULL OR r.RoleName = @Role)
                  AND u.IsActive = 1
                  AND u.Email IS NOT NULL AND u.Email <> ''",
                new SqlParameter("@Role", (object)roleName ?? DBNull.Value));
        }

        public static DataTable GetAllUsers()
        {
            return DBHelper.ExecuteQuery(@"
                SELECT u.Email, u.FullName, u.RoleID, r.RoleName,
                       u.FullName + ' (' + u.Email + ')' AS DisplayName
                FROM Users u
                JOIN Roles r ON u.RoleID = r.RoleID
                WHERE u.IsActive = 1
                ORDER BY r.RoleName, u.FullName");
        }
    }
}
