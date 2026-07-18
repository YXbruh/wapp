using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using CSA.DataAccess;

namespace CSA.Services
{
    /// <summary>
    /// Shared profile management for every role: personal details, password change,
    /// and profile picture upload. Centralised so the three profile pages cannot
    /// drift apart (they previously hashed passwords differently).
    /// </summary>
    public static class ProfileService
    {
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
        private const long MaxPictureBytes = 5 * 1024 * 1024; // 5 MB
        private const string PictureFolder = "~/Content/Uploads/Profile";

        /// <summary>Profile row for a user, or null when the user is missing.</summary>
        public static DataRow Get(string userId)
        {
            DataTable dt = DBHelper.ExecuteQuery(@"
                SELECT UserID, FullName, Email, PhoneNumber, Department,
                       ProfilePicture, CreatedAt
                FROM   Users
                WHERE  UserID = @UserID;",
                new SqlParameter("@UserID", userId));

            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>
        /// Updates name, email and optional contact fields. Fails if the email is
        /// already used by a different account.
        /// </summary>
        public static bool UpdateDetails(string userId, string fullName, string email,
            string phoneNumber, string department, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(fullName))
            { error = "Full name is required."; return false; }

            if (!Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { error = "Enter a valid email address."; return false; }

            object clash = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID <> @UserID;",
                new SqlParameter("@Email", email),
                new SqlParameter("@UserID", userId));

            if (Convert.ToInt32(clash) > 0)
            { error = "That email address is already in use by another account."; return false; }

            DBHelper.ExecuteNonQuery(@"
                UPDATE Users
                SET    FullName    = @FullName,
                       Email       = @Email,
                       PhoneNumber = @Phone,
                       Department  = @Department
                WHERE  UserID = @UserID;",
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@Phone", (object)phoneNumber ?? DBNull.Value),
                new SqlParameter("@Department", (object)department ?? DBNull.Value),
                new SqlParameter("@UserID", userId));

            return true;
        }

        /// <summary>
        /// Verifies the current password and stores a new one using the same salted
        /// format as login (PasswordHelper), so the account still authenticates after.
        /// </summary>
        public static bool ChangePassword(string userId, string currentPassword,
            string newPassword, out string error)
        {
            error = "";

            if ((newPassword ?? "").Length < 8)
            { error = "New password must be at least 8 characters."; return false; }

            object stored = DBHelper.ExecuteScalar(
                "SELECT PasswordHash FROM Users WHERE UserID = @UserID;",
                new SqlParameter("@UserID", userId));

            if (stored == null || stored == DBNull.Value)
            { error = "Could not find your account. Please log in again."; return false; }

            if (!PasswordHelper.Verify(currentPassword ?? "", stored.ToString()))
            { error = "Current password is incorrect."; return false; }

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET PasswordHash = @Hash WHERE UserID = @UserID;",
                new SqlParameter("@Hash", PasswordHelper.Hash(newPassword)),
                new SqlParameter("@UserID", userId));

            return true;
        }

        /// <summary>
        /// Validates and stores a new profile picture, replacing any previous one.
        /// Returns the stored site-relative path, or null when rejected.
        /// </summary>
        public static string SavePicture(HttpPostedFile file, string userId, out string error)
        {
            error = "";

            if (file == null || file.ContentLength == 0)
            { error = "Choose an image to upload."; return null; }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (Array.IndexOf(ImageExtensions, ext) < 0)
            { error = "Profile picture must be a PNG, JPG, GIF or WEBP image."; return null; }

            if (file.ContentLength > MaxPictureBytes)
            { error = "Profile picture must be 5 MB or smaller."; return null; }

            string dir = HttpContext.Current.Server.MapPath(PictureFolder);
            Directory.CreateDirectory(dir);

            string storedName = $"{userId}_{Guid.NewGuid():N}{ext}";
            file.SaveAs(Path.Combine(dir, storedName));
            string relativePath = $"{PictureFolder}/{storedName}";

            string previous = Convert.ToString(DBHelper.ExecuteScalar(
                "SELECT ProfilePicture FROM Users WHERE UserID = @UserID;",
                new SqlParameter("@UserID", userId)));

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET ProfilePicture = @Path WHERE UserID = @UserID;",
                new SqlParameter("@Path", relativePath),
                new SqlParameter("@UserID", userId));

            // Keep the nav avatar's cached path in step with the database.
            HttpContext.Current.Session["ProfilePicture"] = relativePath;

            // Remove the superseded image so old uploads do not pile up.
            if (!string.IsNullOrEmpty(previous) && previous != relativePath)
            {
                try
                {
                    string old = HttpContext.Current.Server.MapPath(previous);
                    if (File.Exists(old)) File.Delete(old);
                }
                catch { /* best-effort cleanup */ }
            }

            return relativePath;
        }

        /// <summary>Removes the current picture, falling back to initials.</summary>
        public static void RemovePicture(string userId)
        {
            string previous = Convert.ToString(DBHelper.ExecuteScalar(
                "SELECT ProfilePicture FROM Users WHERE UserID = @UserID;",
                new SqlParameter("@UserID", userId)));

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET ProfilePicture = NULL WHERE UserID = @UserID;",
                new SqlParameter("@UserID", userId));

            // Keep the nav avatar's cached path in step with the database.
            HttpContext.Current.Session["ProfilePicture"] = "";

            if (string.IsNullOrEmpty(previous)) return;
            try
            {
                string old = HttpContext.Current.Server.MapPath(previous);
                if (File.Exists(old)) File.Delete(old);
            }
            catch { /* best-effort cleanup */ }
        }

        /// <summary>Two-letter initials used when no picture is set.</summary>
        public static string MakeInitials(string name)
        {
            string[] parts = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return "CS";
        }
    }
}
