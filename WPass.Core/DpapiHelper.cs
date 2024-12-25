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
    }
}
