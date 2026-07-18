using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using CSA.DataAccess;

namespace CSA.Services
{
    /// <summary>
    /// Data-access layer for Attachments (files, images, and external links attached
    /// to a Chapter, VirtualLab, or Quiz). Each row belongs to exactly one parent.
    /// </summary>
    public static class AttachmentService
    {
        private static readonly string[] DocumentExtensions =
            { ".pdf", ".doc", ".docx", ".txt", ".ppt", ".pptx", ".xls", ".xlsx", ".zip" };
        private static readonly string[] ImageExtensions =
            { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
        private const long MaxFileBytes = 20 * 1024 * 1024; // 20 MB per file

        /// <summary>
        /// Applies the shared extension/size rules to a posted file.
        /// <paramref name="attachmentType"/> is "Image" or "File" when accepted.
        /// Used both for direct saves and for staging uploads on unsaved content.
        /// </summary>
        public static bool ValidateFile(HttpPostedFile file, out string attachmentType, out string rejectReason)
        {
            attachmentType = null;
            rejectReason = null;

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool isImage = Array.IndexOf(ImageExtensions, ext) >= 0;
            bool isDocument = Array.IndexOf(DocumentExtensions, ext) >= 0;

            if (!isImage && !isDocument)
            { rejectReason = $"{file.FileName} (unsupported file type)"; return false; }

            if (file.ContentLength > MaxFileBytes)
            { rejectReason = $"{file.FileName} (exceeds 20 MB limit)"; return false; }

            attachmentType = isImage ? "Image" : "File";
            return true;
        }

        /// <summary>Builds a collision-proof stored file name for an original upload name.</summary>
        public static string BuildStoredName(string originalFileName)
        {
            string ext = Path.GetExtension(originalFileName).ToLowerInvariant();
            string safeStem = Regex.Replace(Path.GetFileNameWithoutExtension(originalFileName), @"[^\w\-]", "_");
            return $"{Guid.NewGuid():N}_{safeStem}{ext}";
        }

        /// <summary>Inserts an attachment row that already has its file in place.</summary>
        public static void InsertRow(string entityType, string entityId, string attachmentType,
            string title, string filePath, string linkUrl, int? fileSizeBytes, string instructorId)
        {
            Insert(entityType, entityId, attachmentType, title, filePath, linkUrl, fileSizeBytes, instructorId);
        }

        public static DataTable GetByChapter(string chapterId) => GetByParent("ChapterID", chapterId);
        public static DataTable GetByLab(string labId) => GetByParent("LabID", labId);
        public static DataTable GetByQuiz(string quizId) => GetByParent("QuizID", quizId);
        public static DataTable GetByQuestion(string questionId) => GetByParent("QuestionID", questionId);

        /// <summary>Maps an entity type ("Chapter"/"Lab"/"Quiz"/"Question") to its column.</summary>
        private static string ParentColumn(string entityType)
        {
            switch (entityType)
            {
                case "Chapter":  return "ChapterID";
                case "Lab":      return "LabID";
                case "Question": return "QuestionID";
                default:         return "QuizID";
            }
        }

        private static DataTable GetByParent(string column, string id)
        {
            string sql = $@"
                SELECT a.AttachmentID, a.AttachmentType, a.Title, a.FilePath, a.LinkUrl,
                       a.FileSizeBytes, a.UploadedAt, u.FullName AS UploadedByName
                FROM   Attachments a
                INNER JOIN Users u ON a.UploadedByID = u.UserID
                WHERE  a.{column} = @ID
                ORDER BY a.UploadedAt DESC;";

            return DBHelper.ExecuteQuery(sql, new SqlParameter("@ID", id));
        }

        /// <summary>
        /// Validates and saves every posted file under the given parent entity
        /// ("Chapter", "Lab", or "Quiz"). Files with a disallowed extension or that
        /// exceed the size limit are skipped (their names are returned in
        /// <paramref name="rejected"/>) rather than failing the whole batch.
        /// Returns the number of files actually saved.
        /// </summary>
        public static int SaveFiles(IList<HttpPostedFile> files, string entityType, string entityId,
            string instructorId, out List<string> rejected)
        {
            rejected = new List<string>();
            int saved = 0;

            string uploadDir = HttpContext.Current.Server.MapPath($"~/Content/Uploads/{entityType}");
            Directory.CreateDirectory(uploadDir);

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFile file = files[i];
                if (file == null || file.ContentLength == 0) continue;

                if (!ValidateFile(file, out string attachmentType, out string rejectReason))
                { rejected.Add(rejectReason); continue; }

                string storedName = BuildStoredName(file.FileName);
                file.SaveAs(Path.Combine(uploadDir, storedName));

                string relativePath = $"~/Content/Uploads/{entityType}/{storedName}";
                string title = Path.GetFileName(file.FileName);

                Insert(entityType, entityId, attachmentType, title,
                    relativePath, null, file.ContentLength, instructorId);
                saved++;
            }

            return saved;
        }

        /// <summary>Adds an external link attachment.</summary>
        public static void SaveLink(string entityType, string entityId, string title, string url, string instructorId)
        {
            Insert(entityType, entityId, "Link", title, null, url, null, instructorId);
        }

        private static void Insert(string entityType, string entityId, string attachmentType,
            string title, string filePath, string linkUrl, int? fileSizeBytes, string instructorId)
        {
            string column = ParentColumn(entityType);
            string newId = IdGenerator.NewId("ATT");

            string sql = $@"
                INSERT INTO Attachments
                    (AttachmentID, {column}, AttachmentType, Title, FilePath, LinkUrl, FileSizeBytes, UploadedByID)
                VALUES
                    (@AttachmentID, @EntityID, @AttachmentType, @Title, @FilePath, @LinkUrl, @FileSizeBytes, @UploadedByID);";

            DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@AttachmentID", newId),
                new SqlParameter("@EntityID", entityId),
                new SqlParameter("@AttachmentType", attachmentType),
                new SqlParameter("@Title", title),
                new SqlParameter("@FilePath", (object)filePath ?? DBNull.Value),
                new SqlParameter("@LinkUrl", (object)linkUrl ?? DBNull.Value),
                new SqlParameter("@FileSizeBytes", (object)fileSizeBytes ?? DBNull.Value),
                new SqlParameter("@UploadedByID", instructorId));
        }

        /// <summary>
        /// Removes every attachment belonging to a chapter/lab/quiz, together with the
        /// physical files. Must be called before deleting the parent row, otherwise the
        /// Attachments foreign keys (FK_Att_Chapter / FK_Att_Lab / FK_Att_Quiz) block it.
        /// No-op unless the instructor owns the parent. Returns rows deleted.
        /// </summary>
        public static int DeleteByParent(string entityType, string entityId, string instructorId)
        {
            string column = ParentColumn(entityType);
            string ownerSql;
            switch (entityType)
            {
                case "Chapter":
                    ownerSql = "SELECT COUNT(*) FROM Chapters WHERE ChapterID = @ID AND CreatedByID = @Owner;";
                    break;
                case "Lab":
                    ownerSql = "SELECT COUNT(*) FROM VirtualLabs WHERE LabID = @ID AND CreatedByID = @Owner;";
                    break;
                case "Question":
                    // A question is owned by whoever owns its quiz.
                    ownerSql = @"SELECT COUNT(*) FROM QuizQuestions qq
                                 INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                                 WHERE qq.QuestionID = @ID AND q.CreatedByID = @Owner;";
                    break;
                default:
                    ownerSql = "SELECT COUNT(*) FROM Quizzes WHERE QuizID = @ID AND CreatedByID = @Owner;";
                    break;
            }

            object owned = DBHelper.ExecuteScalar(ownerSql,
                new SqlParameter("@ID", entityId),
                new SqlParameter("@Owner", instructorId));
            if (Convert.ToInt32(owned) == 0) return 0;

            DataTable files = DBHelper.ExecuteQuery(
                $"SELECT FilePath FROM Attachments WHERE {column} = @ID;",
                new SqlParameter("@ID", entityId));

            int rows = DBHelper.ExecuteNonQuery(
                $"DELETE FROM Attachments WHERE {column} = @ID;",
                new SqlParameter("@ID", entityId));

            foreach (DataRow row in files.Rows)
            {
                if (row["FilePath"] == DBNull.Value) continue;
                try
                {
                    string physicalPath = HttpContext.Current.Server.MapPath(row["FilePath"].ToString());
                    if (File.Exists(physicalPath)) File.Delete(physicalPath);
                }
                catch { /* best-effort cleanup */ }
            }

            return rows;
        }

        /// <summary>
        /// Deletes an attachment (and its physical file, if any), but only if the
        /// given instructor owns the parent chapter/lab/quiz. Returns true if deleted.
        /// </summary>
        public static bool Delete(string attachmentId, string instructorId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT a.FilePath,
                       CASE
                           WHEN a.ChapterID IS NOT NULL THEN
                               (SELECT c.CreatedByID FROM Chapters c WHERE c.ChapterID = a.ChapterID)
                           WHEN a.LabID IS NOT NULL THEN
                               (SELECT vl.CreatedByID FROM VirtualLabs vl WHERE vl.LabID = a.LabID)
                           WHEN a.QuizID IS NOT NULL THEN
                               (SELECT q.CreatedByID FROM Quizzes q WHERE q.QuizID = a.QuizID)
                           WHEN a.QuestionID IS NOT NULL THEN
                               (SELECT q.CreatedByID FROM QuizQuestions qq
                                INNER JOIN Quizzes q ON qq.QuizID = q.QuizID
                                WHERE qq.QuestionID = a.QuestionID)
                       END AS OwnerID
                FROM Attachments a
                WHERE a.AttachmentID = @AttachmentID;",
                new SqlParameter("@AttachmentID", attachmentId));

            if (dt.Rows.Count == 0) return false;

            string ownerId = dt.Rows[0]["OwnerID"] == DBNull.Value ? null : dt.Rows[0]["OwnerID"].ToString();
            if (ownerId != instructorId) return false;

            int rows = DBHelper.ExecuteNonQuery(
                "DELETE FROM Attachments WHERE AttachmentID = @AttachmentID;",
                new SqlParameter("@AttachmentID", attachmentId));

            if (rows > 0 && dt.Rows[0]["FilePath"] != DBNull.Value)
            {
                string physicalPath = HttpContext.Current.Server.MapPath(dt.Rows[0]["FilePath"].ToString());
                try { if (File.Exists(physicalPath)) File.Delete(physicalPath); } catch { /* best-effort cleanup */ }
            }

            return rows > 0;
        }
    }
}
