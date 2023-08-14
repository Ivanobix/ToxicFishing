using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

#nullable enable
namespace FishingFun
{
    public partial class MainWindow : Window, IAppender
    {
        private System.Drawing.Point lastPoint = System.Drawing.Point.Empty;
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private readonly IBobberFinder bobberFinder;
        private readonly IPixelClassifier pixelClassifier;
        private readonly IBiteWatcher biteWatcher;
        private readonly ReticleDrawer reticleDrawer = new ReticleDrawer();

        private FishingBot? bot;
        private readonly int strikeValue = 7; // this is the depth the bobber must go for the bite to be detected
        private bool setImageBackgroundColour = true;
        private readonly Timer WindowSizeChangedTimer;
        private System.Threading.Thread? botThread;

        public MainWindow()
        {
            InitializeComponent();

            ((Logger)FishingBot.logger.Logger).AddAppender(this);

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            pixelClassifier = new PixelClassifier();
            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

            bobberFinder = new SearchBobberFinder(pixelClassifier);

            if (bobberFinder is IImageProvider imageProvider)
            {
                imageProvider.BitmapEvent += ImageProvider_BitmapEvent;
            }

            biteWatcher = new PositionBiteWatcher(strikeValue);

            WindowSizeChangedTimer = new Timer { AutoReset = false, Interval = 100 };
            WindowSizeChangedTimer.Elapsed += SizeChangedTimer_Elapsed;
            CardGrid.SizeChanged += MainWindow_SizeChanged;
            Closing += (s, e) => botThread?.Abort();

            KeyChooser.CastKeyChanged += (s, e) =>
            {
                _ = Settings.Focus();
                bot?.SetCastKey(KeyChooser.CastKey);
            };
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reset the timer so it only fires 100ms after the user stop dragging the window.
            WindowSizeChangedTimer.Stop();
            WindowSizeChangedTimer.Start();
        }

        private void SizeChangedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatch(() =>
            {
                flyingFishAnimation.AnimationWidth = (int)ActualWidth;
                flyingFishAnimation.AnimationHeight = (int)ActualHeight;
                LogGrid.Height = LogFlipper.ActualHeight;
                GraphGrid.Height = GraphFlipper.ActualHeight;
                GraphGrid.Visibility = Visibility.Visible;
                GraphFlipper.IsFlipped = true;
                LogFlipper.IsFlipped = true;
                GraphFlipper.IsFlipped = false;
                LogFlipper.IsFlipped = false;
            });
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            bot?.Stop();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            new ColourConfiguration(pixelClassifier).Show();
        }

        private void CastKey_Click(object sender, RoutedEventArgs e)
        {
            _ = KeyChooser.Focus();
        }

        private void FishingEventHandler(object sender, FishingEvent e)
        {
            Dispatch(() =>
            {
                switch (e.Action)
                {
                    case FishingAction.BobberMove:
                        if (!GraphFlipper.IsFlipped)
                        {
                            Chart.Add(e.Amplitude);
                        }
                        break;

                    case FishingAction.Loot:
                        flyingFishAnimation.Start();
                        LootingGrid.Visibility = Visibility.Visible;
                        break;

                    case FishingAction.Cast:
                        Chart.ClearChart();
                        LootingGrid.Visibility = Visibility.Collapsed;
                        flyingFishAnimation.Stop();
                        setImageBackgroundColour = true;
                        break;
                };
            });
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            Dispatch(() =>
                LogEntries.Insert(0, new LogEntry()
                {
                    DateTime = DateTime.Now,
                    Message = loggingEvent.RenderedMessage
                })
            );
        }

        private void SetImageVisibility(Image imageForVisible, Image imageForCollapsed, bool state)
        {
            imageForVisible.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            imageForCollapsed.Visibility = !state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetButtonStates(bool isBotRunning)
        {
            Dispatch(() =>
            {
                Play.IsEnabled = isBotRunning;
                Stop.IsEnabled = !Play.IsEnabled;
                SetImageVisibility(PlayImage, PlayImage_Disabled, Play.IsEnabled);
                SetImageVisibility(StopImage, StopImage_Disabled, Stop.IsEnabled);
            });
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (bot == null)
            {
                WowProcess.PressKey(ConsoleKey.Spacebar);
                System.Threading.Thread.Sleep(1500);

                SetButtonStates(false);
                botThread = new System.Threading.Thread(new System.Threading.ThreadStart(BotThread));
                botThread.Start();

                // Hide cards after 10 minutes
                Timer timer = new Timer { Interval = 1000 * 60 * 10, AutoReset = false };
                timer.Elapsed += (s, ev) => Dispatch(() => LogFlipper.IsFlipped = GraphFlipper.IsFlipped = true);
                timer.Start();
            }
        }

        public void BotThread()
        {
            bot = new FishingBot(bobberFinder, biteWatcher, KeyChooser.CastKey, new List<ConsoleKey> { ConsoleKey.D5, ConsoleKey.D6 });
            bot.FishingEventHandler += FishingEventHandler;
            bot.Start();

            bot = null;
            SetButtonStates(true);
        }

        private void ImageProvider_BitmapEvent(object sender, BobberBitmapEvent e)
        {
            Dispatch(() =>
            {
                SetBackgroundImageColour(e);
                reticleDrawer.Draw(e.Bitmap, e.Point);
                System.Windows.Media.Imaging.BitmapImage bitmapImage = e.Bitmap.ToBitmapImage();
                e.Bitmap.Dispose();
                Screenshot.Source = bitmapImage;
            });
        }

        private void SetBackgroundImageColour(BobberBitmapEvent e)
        {
            if (setImageBackgroundColour)
            {
                setImageBackgroundColour = false;
                ImageBackground.Background = e.Bitmap.GetBackgroundColourBrush();
            }
        }

        private void Dispatch(Action action)
        {
            _ = (Application.Current?.Dispatcher.BeginInvoke((Action)(() => action())));
            _ = (Application.Current?.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { })));
        }
    }
}