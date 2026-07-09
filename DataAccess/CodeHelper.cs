using System;
using System.Text;

namespace CSA.DataAccess
{
    /// <summary>
    /// Generates readable record IDs in the format PREFIX + 3 letters + 3 digits
    /// (e.g. CodeHelper.Generate("LAB") -> "LABMKQ847").
    /// Used when inserting new rows, since the database no longer auto-generates IDs.
    /// </summary>
    public static class CodeHelper
    {
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random _rng = new Random();
        private static readonly object _lock = new object();

        /// <summary>Returns a new code, e.g. Generate("USR") -> "USRQXK472".</summary>
        public static string Generate(string prefix)
        {
            var sb = new StringBuilder(prefix);
            lock (_lock)   // Random is not thread-safe; guard it
            {
                for (int i = 0; i < 3; i++)
                    sb.Append(Letters[_rng.Next(Letters.Length)]);
                sb.Append(_rng.Next(0, 1000).ToString("D3"));  // 000-999
            }
            return sb.ToString();
        }
    }
}