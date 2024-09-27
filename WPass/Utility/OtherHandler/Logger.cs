namespace WPass.Utility.OtherHandler
{
    public class Logger
    {
        public static void Write(string info, string logFileName = "")
        {
            var logFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!System.IO.Directory.Exists(logFolder)) System.IO.Directory.CreateDirectory(logFolder);

            string logFile = System.IO.Path.Combine(logFolder, logFileName);

            // logs
            if (string.IsNullOrEmpty(logFileName))
            {
                logFile = System.IO.Path.Combine(logFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
                System.IO.File.AppendAllText(logFile, $"▶ {DateTime.Now:HH:mm:ss} | {info} \n");
                return;
            }

            // others
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.Create(logFile).Close();
            }
            else
            {
                System.IO.File.Delete(logFile);
                System.IO.File.Create(logFile).Close();
            }

            System.IO.File.WriteAllText(logFile, info);
        }
    }
}
