using Newtonsoft.Json;
using System.Windows;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;
using WPass.Utility;

namespace WPass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex;
        public static bool FirstUsed { get; set; } = false;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Check instance
            CheckBeforeStart();

            // Error handle logger
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                var error = $"Unhandled exception: {ex.Message}\n{ex.StackTrace}";
                Logger.Write(error);
            };

            // Create default data
            await SeedData();

            base.OnStartup(e);
        }

        private static void CheckBeforeStart()
        {
            string pid = "Global\\e4ef46b0-b1f5-4f6d-9edb-68affacd2157";

            _mutex = new Mutex(true, pid, out var createdNew);

            if (!createdNew)
            {
                MessageBox.Show("An instance of the application is already running.");
                _mutex.ReleaseMutex();
                Environment.Exit(0);
                return;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }

        private static async Task SeedData()
        {
            var context = new WPContext();
            GlobalSession.BrowserElements = []; // new session

            var browserElements = JsonConvert.DeserializeObject<List<BrowserElement>>(BElement.DEFAULT_JSON) ?? [];

            foreach (var item in browserElements)
            {
                var check = context.BrowserElements.Find(item.Name);
                if (check == null)
                {
                    await context.BrowserElements.AddAsync(item);
                }
            }

            if (!context.Settings.Any())
            {
                FirstUsed = true;
                await context.Settings.AddAsync(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HOTKEY_FILL_DATA,
                    Value = "Ctrl + `"
                });
                await context.Settings.AddAsync(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HOTKEY_CLEAR_DATA,
                    Value = "Ctrl + Q"
                });
                await context.Settings.AddAsync(new Core.Model.Setting()
                {
                    Key = Constant.Setting.HIDE_ON_CLOSE,
                    Value = "false"
                });
                await context.Settings.AddAsync(new Core.Model.Setting()
                {
                    Key = Constant.Setting.WINDOW_STARTUP,
                    Value = "false"
                });
            }

            await context.SaveChangesAsync();

            GlobalSession.BrowserElements.AddRange(browserElements); // saved session data 
        }
    }
}
