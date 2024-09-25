using System.Windows;
using System.Windows.Media.Animation;
using WPass.Core;
using WPass.Utility.OtherHandler;
using WPass.ViewModels;

namespace WPass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVM? vm;
        public static bool ForceToClose { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            vm = Resources["mainVm"] as MainVM;

            if (App.FirstUsed)
            {
                new TutorialWindow().ShowDialog();
            }
        }

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainVM.Initialize(this);
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            MainVM.Destroy(this);
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "Copyright© 2024. All rights reserved.", "About WPass");
        }

        private void ButtonClearSearch_Click(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                vm.FilteredSearchValue = string.Empty;
            }
        }

        #endregion

        //// Handle NotifyIcon
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!ForceToClose)
            {
                WPContext context = new();
                var hideOnClose = context.Settings.Find(Constant.Setting.HIDE_ON_CLOSE)?.Value == "true";
                if (hideOnClose)
                {
                    ToastHelper.Show("Hidden", "App is running in taskbar.");
                    Hide();
                    e.Cancel = true;
                    if (vm != null)
                    {
                        vm.NotifyIconVisible = true;
                    }
                    else
                    {
                        MyNotifyIcon.Visibility = Visibility.Visible;
                    }
                    return;
                }
                if (MessageBox.Show(this, "Are you sure ?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            MyNotifyIcon?.Dispose(); //clean up notifyicon (would otherwise stay open until application finishes)
            base.OnClosing(e);
        }

        #region Filter click animation

        private bool _isExpanded = false;
        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the state of the StackPanel
            if (_isExpanded)
            {
                // Collapse the StackPanel
                AnimateFilterPanel(0, 0); // Height to 0 and Opacity to 0
            }
            else
            {
                // Expand the StackPanel
                AnimateFilterPanel(35, 1); // Height to any desired value and Opacity to 1
            }
            _isExpanded = !_isExpanded; // Toggle the expanded state
        }
        private void AnimateFilterPanel(double newHeight, double newOpacity)
        {
            // Create a storyboard to hold the animations
            Storyboard storyboard = new();

            // Animate Height
            DoubleAnimation heightAnimation = new()
            {
                To = newHeight,
                Duration = TimeSpan.FromSeconds(0.25) // 0.5 seconds duration
            };
            Storyboard.SetTarget(heightAnimation, AnimatedFilterPanel);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(HeightProperty));

            // Animate Opacity
            DoubleAnimation opacityAnimation = new()
            {
                To = newOpacity,
                Duration = TimeSpan.FromSeconds(0.25) // 0.5 seconds duration
            };
            Storyboard.SetTarget(opacityAnimation, AnimatedFilterPanel);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            // Add animations to the storyboard
            storyboard.Children.Add(heightAnimation);
            storyboard.Children.Add(opacityAnimation);

            // Start the storyboard
            storyboard.Begin();
        }

        #endregion
    }
}