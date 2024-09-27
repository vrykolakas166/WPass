using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Interop;
using WPass.Constant;
using WPass.Core;
using WPass.Utility.WindowHandler;
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
                        TriggerHelper.RestartCurrentApplication();
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
