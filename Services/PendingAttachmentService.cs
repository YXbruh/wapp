using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace CSA.Services
{
    /// <summary>
    /// Holds attachments chosen for a chapter/lab/quiz that has not been saved yet.
    ///
    /// The Attachments table requires exactly one parent FK (CK_Att_OneParent), so a
    /// row cannot be inserted before the parent exists. Instructors still expect to
    /// attach files while composing new content, so uploads are staged here and
    /// flushed by <see cref="Commit"/> when the parent is finally saved.
    ///
    /// Files are staged on disk (not in Session) so a few 20 MB uploads do not sit in
    /// server memory for the life of the session.
    /// </summary>
    public static class PendingAttachmentService
    {
        private const string StagingFolder = "~/Content/Uploads/_Staging";

        [Serializable]
        public class PendingItem
        {
            public string PendingID { get; set; }
            public string AttachmentType { get; set; }  // 'File' | 'Image' | 'Link'
            public string Title { get; set; }
            public string StagedFileName { get; set; }  // file name inside the staging folder
            public string LinkUrl { get; set; }
            public int? FileSizeBytes { get; set; }
        }

        // ------------------------------------------------------------------
        // Session-backed list, one bucket per entity type ("Chapter"/"Lab"/"Quiz")
        // ------------------------------------------------------------------
        private static List<PendingItem> GetList(string bucket)
        {
            string key = "PendingAttachments_" + bucket;
            var list = HttpContext.Current.Session[key] as List<PendingItem>;
            if (list == null)
            {
                list = new List<PendingItem>();
                HttpContext.Current.Session[key] = list;
            }
            return list;
        }

        public static int Count(string bucket) => GetList(bucket).Count;

        private static string StagingDir()
        {
            string dir = HttpContext.Current.Server.MapPath(StagingFolder);
            Directory.CreateDirectory(dir);
            return dir;
        }

        // ------------------------------------------------------------------
        // Adding
        // ------------------------------------------------------------------

        /// <summary>
        /// Validates and stages posted files. Rejected files are reported the same way
        /// as a direct save. Returns the number staged.
        /// </summary>
        public static int StageFiles(IList<HttpPostedFile> files, string bucket, out List<string> rejected)
        {
            rejected = new List<string>();
            int staged = 0;
            string dir = StagingDir();
            var list = GetList(bucket);

            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFile file = files[i];
                if (file == null || file.ContentLength == 0) continue;

                if (!AttachmentService.ValidateFile(file, out string attachmentType, out string rejectReason))
                { rejected.Add(rejectReason); continue; }

                string storedName = AttachmentService.BuildStoredName(file.FileName);
                file.SaveAs(Path.Combine(dir, storedName));

                list.Add(new PendingItem
                {
                    PendingID = "PND" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    AttachmentType = attachmentType,
                    Title = Path.GetFileName(file.FileName),
                    StagedFileName = storedName,
                    FileSizeBytes = file.ContentLength
                });
                staged++;
            }

            return staged;
        }

        public static void StageLink(string bucket, string title, string url)
        {
            GetList(bucket).Add(new PendingItem
            {
                PendingID = "PND" + Guid.NewGuid().ToString("N").Substring(0, 8),
                AttachmentType = "Link",
                Title = title,
                LinkUrl = url
            });
        }

        // ------------------------------------------------------------------
        // Removing / clearing
        // ------------------------------------------------------------------

        /// <summary>Removes one staged item (and its staged file). Returns true if found.</summary>
        public static bool Remove(string bucket, string pendingId)
        {
            var list = GetList(bucket);
            PendingItem item = list.FirstOrDefault(p => p.PendingID == pendingId);
            if (item == null) return false;

            DeleteStagedFile(item);
            list.Remove(item);
            return true;
        }

        /// <summary>Drops every staged item for a bucket, deleting staged files.</summary>
        public static void Clear(string bucket)
        {
            var list = GetList(bucket);
            foreach (PendingItem item in list) DeleteStagedFile(item);
            list.Clear();
        }

        private static void DeleteStagedFile(PendingItem item)
        {
            if (string.IsNullOrEmpty(item.StagedFileName)) return;
            try
            {
                string path = Path.Combine(StagingDir(), item.StagedFileName);
                if (File.Exists(path)) File.Delete(path);
            }
            catch { /* best-effort cleanup */ }
        }

        // ------------------------------------------------------------------
        // Committing
        // ------------------------------------------------------------------

        /// <summary>
        /// Moves staged files into the entity's upload folder and writes an
        /// Attachments row for every staged item. Clears the bucket. Returns the
        /// number of attachments committed.
        /// </summary>
        public static int Commit(string bucket, string entityType, string entityId, string instructorId)
        {
            var list = GetList(bucket);
            if (list.Count == 0) return 0;

            string targetDir = HttpContext.Current.Server.MapPath($"~/Content/Uploads/{entityType}");
            Directory.CreateDirectory(targetDir);

            int committed = 0;
            foreach (PendingItem item in list)
            {
                if (item.AttachmentType == "Link")
                {
                    AttachmentService.InsertRow(entityType, entityId, "Link", item.Title,
                        null, item.LinkUrl, null, instructorId);
                    committed++;
                    continue;
                }

                string stagedPath = Path.Combine(StagingDir(), item.StagedFileName);
                if (!File.Exists(stagedPath)) continue;   // staged file vanished; skip rather than fail the save

                File.Move(stagedPath, Path.Combine(targetDir, item.StagedFileName));

                AttachmentService.InsertRow(entityType, entityId, item.AttachmentType, item.Title,
                    $"~/Content/Uploads/{entityType}/{item.StagedFileName}", null,
                    item.FileSizeBytes, instructorId);
                committed++;
            }

            list.Clear();
            return committed;
        }

        // ------------------------------------------------------------------
        // Display
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns saved attachments for the parent (if any) with staged items appended,
        /// in the shape the attachment repeaters bind to. Staged rows carry IsPending = true.
        /// </summary>
        public static DataTable BuildDisplayTable(DataTable committedRows, string bucket)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AttachmentID", typeof(string));
            dt.Columns.Add("AttachmentType", typeof(string));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("FilePath", typeof(string));
            dt.Columns.Add("LinkUrl", typeof(string));
            dt.Columns.Add("FileSizeBytes", typeof(object));
            dt.Columns.Add("UploadedAt", typeof(object));
            dt.Columns.Add("UploadedByName", typeof(string));
            dt.Columns.Add("IsPending", typeof(bool));

            if (committedRows != null)
            {
                foreach (DataRow src in committedRows.Rows)
                {
                    DataRow r = dt.NewRow();
                    r["AttachmentID"] = src["AttachmentID"];
                    r["AttachmentType"] = src["AttachmentType"];
                    r["Title"] = src["Title"];
                    r["FilePath"] = src["FilePath"];
                    r["LinkUrl"] = src["LinkUrl"];
                    r["FileSizeBytes"] = src["FileSizeBytes"];
                    r["UploadedAt"] = src["UploadedAt"];
                    r["UploadedByName"] = src["UploadedByName"];
                    r["IsPending"] = false;
                    dt.Rows.Add(r);
                }
            }

            foreach (PendingItem item in GetList(bucket))
            {
                DataRow r = dt.NewRow();
                r["AttachmentID"] = item.PendingID;
                r["AttachmentType"] = item.AttachmentType;
                r["Title"] = item.Title;
                r["FilePath"] = (object)null ?? DBNull.Value;
                r["LinkUrl"] = (object)item.LinkUrl ?? DBNull.Value;
                r["FileSizeBytes"] = item.FileSizeBytes.HasValue ? (object)item.FileSizeBytes.Value : DBNull.Value;
                r["UploadedAt"] = DBNull.Value;
                r["UploadedByName"] = "Not saved yet";
                r["IsPending"] = true;
                dt.Rows.Add(r);
            }

            return dt;
        }

        /// <summary>Href for an attachment row; staged files have no browsable URL yet.</summary>
        public static string DisplayHref(object attachmentType, object filePath, object linkUrl, object isPending)
        {
            if (Convert.ToString(attachmentType) == "Link")
                return Convert.ToString(linkUrl);

            if (isPending != null && isPending != DBNull.Value && Convert.ToBoolean(isPending))
                return "javascript:void(0)";

            if (filePath == null || filePath == DBNull.Value) return "javascript:void(0)";
            return HttpContext.Current.Response.ApplyAppPathModifier(
                VirtualPathUtility.ToAbsolute(Convert.ToString(filePath)));
        }

        /// <summary>Secondary line under an attachment title.</summary>
        public static string DisplayMeta(object attachmentType, object uploadedByName, object uploadedAt, object isPending)
        {
            bool pending = isPending != null && isPending != DBNull.Value && Convert.ToBoolean(isPending);
            if (pending)
                return $"{attachmentType} &middot; pending &mdash; saves when you save this content";

            string when = (uploadedAt == null || uploadedAt == DBNull.Value)
                ? "" : $" &middot; {Convert.ToDateTime(uploadedAt):dd MMM yyyy}";
            return $"{attachmentType} &middot; {uploadedByName}{when}";
        }
    }
}
