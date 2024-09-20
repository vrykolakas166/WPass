using System.Windows;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;

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
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                var error = $"Unhandled exception: {ex.Message}\n{ex.StackTrace}";
                var log = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "error.txt");
                if (!System.IO.File.Exists(log)) System.IO.File.Create(log).Close();
                System.IO.File.WriteAllText(log, error);
            };

            CheckBeforeStart();

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
            var browserElement1 = new BrowserElement() { Name = BElement.DEFAULT_0 };
            var browserElement2 = new BrowserElement() { Name = BElement.DEFAULT_1 };
            var browserElement3 = new BrowserElement() { Name = BElement.DEFAULT_2 };

            var check1 = context.BrowserElements.Find(browserElement1.Name);
            var check2 = context.BrowserElements.Find(browserElement2.Name);
            var check3 = context.BrowserElements.Find(browserElement3.Name);

            if (check1 == null)
            {
                await context.BrowserElements.AddAsync(browserElement1);
            }
            if (check2 == null)
            {
                await context.BrowserElements.AddAsync(browserElement2);
            }
            if (check3 == null)
            {
                await context.BrowserElements.AddAsync(browserElement3);
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
        }
    }
}
