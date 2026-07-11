using System;
using System.Security.Cryptography;
using System.Text;

namespace CSA.Services
{
    public static class PasswordHelper
    {
        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static string Hash(string password)
        {
            byte[] salt = new byte[8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string saltHex = BytesToHex(salt);
            byte[] input = Encoding.UTF8.GetBytes(saltHex + password);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(input);
                return saltHex + "$" + BytesToHex(hash);
            }
        }

        public static bool Verify(string password, string storedHash)
        {
            int sep = storedHash.IndexOf('$');
            if (sep < 0) return false;
            string saltHex = storedHash.Substring(0, sep);
            byte[] input = Encoding.UTF8.GetBytes(saltHex + password);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(input);
                return BytesToHex(hash) == storedHash.Substring(sep + 1);
            }
        }

        public static string GenerateTempPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
            byte[] data = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            StringBuilder sb = new StringBuilder(length);
            foreach (byte b in data)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }
    }
}
