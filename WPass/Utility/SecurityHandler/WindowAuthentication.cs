using System.Runtime.InteropServices;
using System.Security.Principal;

namespace WPass.Utility.SecurityHandler
{
    public partial class WindowAuthentication
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern bool LogonUser(
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
         string lpszUsername,
         string lpszDomain,
         string lpszPassword,
         int dwLogonType,
         int dwLogonProvider,
         out IntPtr phToken);

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        public static bool ValidateUser(string username, string password)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            bool returnValue = LogonUser(username, Environment.MachineName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle);

            // Close the token handle if successful
            if (tokenHandle != IntPtr.Zero)
            {
                WindowsIdentity identity = new(tokenHandle);
                identity.Dispose(); // Important to clean up
            }

            return returnValue; // return true if login is successful
        }
    }

}
