using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace WPass.Core
{
    public static class DpapiHelper
    {
        [SupportedOSPlatform("windows")]
        public static void SavePassword(string password, string filePath)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(password),
                null, // Optional entropy (useful for added security)
                DataProtectionScope.CurrentUser); // Use CurrentUser or LocalMachine

            File.WriteAllBytes(filePath, encryptedData);
        }

        [SupportedOSPlatform("windows")]
        public static string LoadPassword(string filePath)
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedData);
        }

        private static readonly Random random = new Random();
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public static string GenerateStrongPassword(int length = 16)
        {
            if (length < 16) // Minimum length requirement
            {
                throw new ArgumentException("Password length must be at least 8 characters.");
            }

            // Ensure at least one character from each category
            char[] password = new char[length];
            int position = 0;

            // Add one character from each required set
            password[position++] = Uppercase[random.Next(Uppercase.Length)];
            password[position++] = Lowercase[random.Next(Lowercase.Length)];
            password[position++] = Numbers[random.Next(Numbers.Length)];
            password[position++] = SpecialChars[random.Next(SpecialChars.Length)];

            // Fill the rest with random characters from all sets
            string allChars = Uppercase + Lowercase + Numbers + SpecialChars;
            for (int i = position; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Shuffle the password
            for (int i = length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = password[i];
                password[i] = password[j];
                password[j] = temp;
            }

            return new string(password);
        }
    }
}
