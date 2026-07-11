using System;

namespace CSA.Services
{
    public static class IdGenerator
    {
        private static readonly Random _rng = new Random();
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string NewId(string prefix)
        {
            char[] letters = new char[3];
            for (int i = 0; i < 3; i++)
                letters[i] = Letters[_rng.Next(Letters.Length)];
            int digits = _rng.Next(100, 999);
            return $"{prefix}{new string(letters)}{digits:D3}";
        }
    }
}
