using System.Diagnostics;
using System.Windows;
using WPass.Utility.OtherHandler;

namespace WPass.Utility.WindowHandler
{
    public class TriggerHelper
    {
        public static void RestartCurrentApplication()
        {
            // Get the current process
            var currentProcess = Process.GetCurrentProcess();
            // Start a new instance of the application
            if (currentProcess.MainModule != null)
            {
                MainWindow.ForceToClose = true;

                // Command to execute after a 1-second delay
                string command = $"/C timeout /t 1 && {currentProcess.MainModule.FileName}";

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
                Logger.Write("Failed to restart.");
            }
        }
    }
}
