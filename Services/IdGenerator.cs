using System;
using System.Security.Cryptography;

namespace CSA.Services
{
    /// <summary>
    /// Builds the PREFIX + 3 letters + 3 digits keys used by every main entity
    /// (e.g. "USRABC123"). Backed by a crypto RNG rather than a shared Random:
    /// System.Random is not thread-safe, and two concurrent requests sharing one
    /// instance can return the same sequence and collide on the primary key.
    /// </summary>
    public static class IdGenerator
    {
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string NewId(string prefix)
        {
            byte[] buffer = new byte[8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                rng.GetBytes(buffer);

            char[] letters = new char[3];
            for (int i = 0; i < 3; i++)
                letters[i] = Letters[buffer[i] % Letters.Length];

            // Full 000-999 range. The old Random.Next(100, 999) silently excluded 999
            // and every value below 100 that the "D3" format was written to allow.
            int digits = ((buffer[3] << 8) | buffer[4]) % 1000;

            return $"{prefix}{new string(letters)}{digits:D3}";
        }
    }
}
