using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Windows;
using WPass.Constant;
using WPass.Core;
using WPass.Core.Model;
using WPass.Utility.OtherHandler;

namespace WPass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private bool _firstUsed;
        private bool _passcodeIsNotCreated;

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
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

                // Start the application
                LoginWindow login = new(LoginWindow.Mode.Normal);
                if (_firstUsed)
                {
                    new TutorialWindow().ShowDialog();

                    login = new(LoginWindow.Mode.Create);
                }

                if (_passcodeIsNotCreated)
                {
                    login = new(LoginWindow.Mode.Create);
                }
                login.Show();

                // test 
                //new MainWindow().Show();
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                MessageBox.Show("Something's wrong. Please contact developer.", "CRITICAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private async Task SeedData()
        {
            try
            {
                using var context = new WPContext();
                context.Database.Migrate();
                GlobalSession.BrowserElements.Clear(); // new session

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
                    _firstUsed = true;
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
                else
                {
                    var passSetting = await context.Settings.FirstOrDefaultAsync(s => s.Key.Equals(Constant.Setting.PASSCODE));
                    if (passSetting == null)
                    {

                        _passcodeIsNotCreated = true;
                    }
                }

                await context.SaveChangesAsync();

                GlobalSession.BrowserElements = browserElements; // saved session data
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
            }
        }
    }
}
