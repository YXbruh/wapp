using System;
using System.Security.Cryptography;
using System.Text;

namespace CSA.Services
{
    /// <summary>
    /// Password hashing helper.
    ///
    /// New passwords are hashed with PBKDF2 (HMAC-SHA256, 100k iterations) and stored as
    ///     pbkdf2$&lt;iterations&gt;$&lt;saltHex&gt;$&lt;hashHex&gt;
    ///
    /// Verify() also accepts the older salted-SHA-256 format ( saltHex$hashHex ) so that
    /// accounts seeded before the upgrade still log in. Use NeedsUpgrade() after a
    /// successful login to detect a legacy hash and transparently re-hash it.
    /// </summary>
    public static class PasswordHelper
    {
        private const int Iterations = 100_000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;
        private const string Pbkdf2Prefix = "pbkdf2";

        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static byte[] HexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        /// <summary>Creates a new PBKDF2 hash for a registration or password change.</summary>
        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltBytes];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Pbkdf2(password, salt, Iterations);
            return $"{Pbkdf2Prefix}${Iterations}${BytesToHex(salt)}${BytesToHex(hash)}";
        }

        /// <summary>
        /// Verifies a password against either the new PBKDF2 format or the legacy
        /// salted-SHA-256 format. Returns false for any malformed stored value.
        /// </summary>
        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            string[] parts = storedHash.Split('$');

            // New format: pbkdf2$iterations$salt$hash
            if (parts.Length == 4 && parts[0] == Pbkdf2Prefix)
            {
                if (!int.TryParse(parts[1], out int iterations)) return false;
                byte[] salt = HexToBytes(parts[2]);
                byte[] expected = HexToBytes(parts[3]);
                byte[] actual = Pbkdf2(password, salt, iterations);
                return FixedTimeEquals(actual, expected);
            }

            // Legacy format: saltHex$sha256Hex
            if (parts.Length == 2)
            {
                string saltHex = parts[0];
                byte[] input = Encoding.UTF8.GetBytes(saltHex + password);
                using (SHA256 sha = SHA256.Create())
                {
                    string computed = BytesToHex(sha.ComputeHash(input));
                    return FixedTimeEquals(
                        Encoding.ASCII.GetBytes(computed),
                        Encoding.ASCII.GetBytes(parts[1]));
                }
            }

            return false;
        }

        /// <summary>
        /// True when a stored hash is in the legacy SHA-256 format and should be
        /// re-hashed with PBKDF2 (call after a successful Verify).
        /// </summary>
        public static bool NeedsUpgrade(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            string[] parts = storedHash.Split('$');
            return !(parts.Length == 4 && parts[0] == Pbkdf2Prefix);
        }

        public static string GenerateTempPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
            byte[] data = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            StringBuilder sb = new StringBuilder(length);
            foreach (byte b in data)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return kdf.GetBytes(HashBytes);
        }

        /// <summary>Length-constant comparison to avoid leaking timing information.</summary>
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
