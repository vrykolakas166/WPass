using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using WPass.Constant;
using WPass.Core;
using WPass.Utility.OtherHandler;
using static WPass.Utility.SecurityHandler.WindowAuthentication;

namespace WPass.Utility
{
    public static class PasscodeManager
    {
        public static async Task ResetAsync()
        {
            // Prompt for the user's credentials
            if (PromptForCredentials(new WindowInteropHelper(Application.Current.MainWindow).Handle, out string? username, out string? password))
            {
                // Verify the user's password using LogonUser or another method
                if (ValidateUser(username, password))
                {
                    // Execute the operation
                    if (MessageBox.Show("This action will reset master passcode, but in order to protect your data, we will delete all your saved entries. Continue ?\n Application will be restart after this.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        using var context = new WPContext();
                        // Reset passcode
                        var stgPass = await context.Settings.FirstAsync(f => f.Key.Equals(Setting.PASSCODE));
                        context.Settings.Remove(stgPass);

                        // Remove all entries
                        context.Entries.RemoveRange(context.Entries);

                        // save changes
                        await context.SaveChangesAsync();

                        // Restart
                        // Get the current process
                        var currentProcess = Process.GetCurrentProcess();
                        // Start a new instance of the application
                        if (currentProcess.MainModule != null)
                        {
                            MainWindow.ForceToClose = true;
                            Process.Start(currentProcess.MainModule.FileName);
                            // Close the current application
                            Application.Current.Shutdown();
                        }
                        else
                        {
                            Logger.Write("Failed to restart after reset login passcode.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
