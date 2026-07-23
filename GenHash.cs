using System;
using System.Security.Cryptography;
using System.Text;

class Program {
    static void Main() {
        string password = "Student@123";
        
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
            string hashHex = BytesToHex(hash);
            string result = saltHex + "$" + hashHex;
            Console.WriteLine(result);
        }
    }
    
    static string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}