using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace WPass
{
    /// <summary>
    /// Interaction logic for TutorialWindow.xaml
    /// </summary>
    public partial class TutorialWindow : Window
    {
        private const double ANIMATED_TIME = 0.75;
        private Ellipse _currentDot;
        private static DateTime _lastExecutedTimes = DateTime.MinValue; // Store the last execution time for debouncing
        private static readonly TimeSpan _debounceInterval = TimeSpan.FromSeconds(ANIMATED_TIME); // Debounce interval

        private static int _totalGIf;

        public TutorialWindow()
        {
            InitializeComponent();
            _currentDot = Dot0;
        }

        public void EllipseStep_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Ellipse ell)
            {
                GoToView(ell);
            }
        }

        private void GoToView(Ellipse? ell)
        {
            DateTime now = DateTime.Now;
            // Check if the action should be debounced
            if ((now - _lastExecutedTimes) > _debounceInterval)
            {
                // Get the horizontal offset of the grid within the ScrollViewer
                double offset = view1.RenderSize.Width + view1.Margin.Left + view1.Margin.Right;
                var currentIndex = int.Parse(_currentDot.Tag.ToString() ?? "0");
                Dot0.Fill = Brushes.Transparent;
                Dot1.Fill = Brushes.Transparent;
                Dot2.Fill = Brushes.Transparent;
                Dot3.Fill = Brushes.Transparent;
                if (int.TryParse(ell?.Tag.ToString(), out int rs))
                {
                    ell.Fill = Brushes.SkyBlue;
                    if (rs > currentIndex)
                    {
                        // Scroll to the calculated offset
                        AnimateScrollToOffset(GuideScrollViewer, (rs - currentIndex) * offset + GuideScrollViewer.HorizontalOffset);
                        _currentDot = ell;
                    }
                    else if (rs < currentIndex)
                    {
                        AnimateScrollToOffset(GuideScrollViewer, GuideScrollViewer.HorizontalOffset - (currentIndex - rs) * offset);
                        _currentDot = ell;
                    }

                    if (rs < 3)
                    {
                        ButtonNext.Content = "Next";
                    }
                    else if (rs == 3)
                    {
                        ButtonNext.Content = "Finish";
                    }
                }
                _lastExecutedTimes = now; // Update the last executed time
            }
        }

        // Method to animate horizontal scrolling to a target offset
        private static void AnimateScrollToOffset(ScrollViewer scrollViewer, double toOffset)
        {
            DoubleAnimation animation = new()
            {
                To = toOffset,
                Duration = TimeSpan.FromSeconds(ANIMATED_TIME), // Animation duration
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } // Optional easing function for smoothness
            };

            // Apply the animation to the horizontal offset property
            scrollViewer.BeginAnimation(HorizontalOffsetProperty, animation);
        }

        // Attached property to animate ScrollViewer's HorizontalOffset
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        // Callback to set the ScrollViewer's HorizontalOffset when the property changes
        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonNext.Content.Equals("Finish"))
            {
                this.Close();
            }

            var currentIndex = int.Parse(_currentDot.Tag.ToString() ?? "0");
            if (currentIndex < 3)
            {
                var ind = currentIndex + 1;
                switch (ind)
                {
                    case 0:
                        GoToView(Dot0);
                        break;
                    case 1:
                        GoToView(Dot1);
                        break;
                    case 2:
                        GoToView(Dot2);
                        break;
                    case 3:
                        GoToView(Dot3);
                        break;
                }
            }
        }

        private void TWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _totalGIf = 3;
            LoadGif(WelcomeGif, "/Resources/Tutorial/welcome.gif");
            LoadGif(AddGif, "/Resources/Tutorial/manual_add.gif");
            LoadGif(ImportGif, "/Resources/Tutorial/import.gif");
        }

        private void LoadGif(Image imageControl, string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri($"pack://application:,,,{path}");
            image.EndInit();

            ImageBehavior.SetAnimatedSource(imageControl, image);

            if (ImageBehavior.GetIsAnimationLoaded(imageControl))
            {
                _totalGIf -= 1;
                if (_totalGIf == 0)
                {
                    // loaded
                }
            }
        }
    }
}
