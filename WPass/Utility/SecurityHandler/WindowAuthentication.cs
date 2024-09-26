using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace WPass.Utility.SecurityHandler
{
    public partial class WindowAuthentication
    {
        #region WinAPI
        [DllImport("credui.dll", CharSet = CharSet.Auto)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern int CredUIPromptForWindowsCredentials(
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            ref CREDUI_INFO pUiInfo,
            int dwAuthError,
            ref uint pulAuthPackage,
            IntPtr pvInAuthBuffer,
            uint ulInAuthBufferSize,
            out IntPtr ppvOutAuthBuffer,
            out uint pulOutAuthBufferSize,
            ref bool pfSave,
            int dwFlags);

        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredUnPackAuthenticationBuffer(
            int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainName,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern bool LogonUser(
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
         string lpszUsername,
         string lpszDomain,
         string lpszPassword,
         int dwLogonType,
         int dwLogonProvider,
         out IntPtr phToken);
        #endregion

        #region Fields
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        private const int CREDUIWIN_GENERIC = 0x1;
        //private const int CREDUIWIN_ENUMERATE_ADMINS = 0x100;
        //private const int CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200;

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;

        #endregion

        public static bool PromptForCredentials(nint parent, out string? username, out string? password)
        {
            CREDUI_INFO info = new();
            info.cbSize = Marshal.SizeOf(info);
            info.pszCaptionText = "Enter your window credentials";
            info.pszMessageText = "This action is required to proceed";
            info.hwndParent = parent;

            uint authPackage = 0;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            IntPtr outCredBuffer = IntPtr.Zero;
#pragma warning disable IDE0018 // Inline variable declaration
            uint outCredSize = 0;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            bool save = false;

            int result = CredUIPromptForWindowsCredentials(
                ref info,
                0,
                ref authPackage,
                IntPtr.Zero,
                0,
                out outCredBuffer,
                out outCredSize,
                ref save,
                CREDUIWIN_GENERIC);

            if (result == 0)
            {
                int maxUserName = 100;
                int maxPassword = 100;
                int maxDomain = 100;

                StringBuilder userNameBuilder = new(maxUserName);
                StringBuilder passwordBuilder = new(maxPassword);
                StringBuilder domainBuilder = new(maxDomain);

                if (CredUnPackAuthenticationBuffer(0x1, outCredBuffer, outCredSize, userNameBuilder, ref maxUserName, domainBuilder, ref maxDomain, passwordBuilder, ref maxPassword))
                {
                    Marshal.FreeCoTaskMem(outCredBuffer);

                    username = userNameBuilder.ToString();
                    password = passwordBuilder.ToString();
                    return true;
                }
            }

            username = null;
            password = null;
            return false;
        }

        public static bool ValidateUser(string? username, string? password)
        {
            if (username == null || password == null)
            {
                return false;
            }

            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                bool isValid = LogonUser(username, Environment.MachineName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle);
                return isValid; // return true if login is successful
            }
            finally
            {
                // Close the token handle if successful
                if (tokenHandle != IntPtr.Zero)
                {
                    WindowsIdentity identity = new(tokenHandle);
                    identity.Dispose(); // Important to clean up
                }
            }
        }
    }
}
