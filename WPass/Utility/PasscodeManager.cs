using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using WPass.Constant;
using WPass.Core;
using WPass.Utility.OtherHandler;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
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
                    var msg = "This action will reset master passcode, " +
                        "but in order to protect your data, " +
                        "we will delete all your saved entries." +
                        Environment.NewLine +
                        "\nApplication will be restart after this action. Please wait a second." +
                        Environment.NewLine +
                        "\nContinue ?";

                    // Execute the operation
                    if (MessageBox.Show(msg, "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
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

                            // Command to execute after a 2-second delay
                            string command = $"/C timeout /t 2 && {currentProcess.MainModule.FileName}";

                            // Create a process to run CMD with the command
                            ProcessStartInfo processStartInfo = new("cmd.exe", command)
                            {
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            // Start the process
                            Process.Start(processStartInfo);

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
