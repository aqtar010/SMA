using System.Security.Cryptography;
using System.Text;

namespace SMA.API.Utilities
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Hashes a password using PBKDF2 with SHA256.
        /// </summary>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[20];
            RandomNumberGenerator.Fill(salt);

            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
            // Use overload that returns a byte[] (output length specified as int)
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(pwdBytes, salt, 10000, HashAlgorithmName.SHA256, 20);

            byte[] hashBytes = new byte[40];
            System.Buffer.BlockCopy(salt, 0, hashBytes, 0, 20);
            System.Buffer.BlockCopy(key, 0, hashBytes, 20, 20);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a password against its hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            byte[] salt = new byte[20];
            System.Buffer.BlockCopy(hashBytes, 0, salt, 0, 20);

            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
            // Match the same overload used in HashPassword
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(pwdBytes, salt, 10000, HashAlgorithmName.SHA256, 20);

            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 20] != key[i])
                    return false;
            return true;
        }
    }
}
