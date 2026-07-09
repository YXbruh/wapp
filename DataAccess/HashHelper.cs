using System;
using System.Security.Cryptography;
using System.Text;

namespace CSA.DataAccess
{
    /// <summary>
    /// Password hashing helper using salted SHA-256.
    /// Stored format is  salt$hash  (16 hex-char salt, then the SHA-256 hex digest).
    /// This matches the hashes in DummyData.sql, so the seeded accounts verify correctly.
    ///
    /// Usage:
    ///   Register:  string stored = HashHelper.Hash(plainPassword);   // save 'stored' to Users.PasswordHash
    ///   Login:     bool ok = HashHelper.Verify(enteredPassword, storedHashFromDb);
    /// </summary>
    public static class HashHelper
    {
        /// <summary>Creates a new salted hash when a user registers or changes password.</summary>
        public static string Hash(string password)
        {
            // Generate a random 8-byte salt -> 16 hex characters.
            byte[] saltBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(saltBytes);

            string salt = BitConverter.ToString(saltBytes).Replace("-", "").ToLower();

            return salt + "$" + Sha256Hex(salt + password);
        }

        /// <summary>Checks a login attempt against the stored salt$hash value.</summary>
        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || !stored.Contains("$"))
                return false;

            string[] parts = stored.Split('$');
            string salt = parts[0];
            string expectedHash = parts[1];

            return Sha256Hex(salt + password) == expectedHash;
        }

        /// <summary>Returns the lowercase hex SHA-256 of the input string.</summary>
        private static string Sha256Hex(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}