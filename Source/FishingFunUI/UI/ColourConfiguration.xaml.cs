using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace FishingFun
{
    public partial class ColourConfiguration : System.Windows.Window
    {
        private readonly IPixelClassifier pixelClassifier;

        private Bitmap ScreenCapture = new Bitmap(1, 1);

        public int FindColourValue { get; set; }

        public string PrimaryColor = "Red";
        public string SecondaryColor = "blue";

        public int ColourMultiplier
        {
            get => (int)(pixelClassifier.ColourMultiplier * 100);
            set => pixelClassifier.ColourMultiplier = ((double)value) / 100;
        }

        public int ColourClosenessMultiplier
        {
            get => (int)(pixelClassifier.ColourClosenessMultiplier * 100);
            set => pixelClassifier.ColourClosenessMultiplier = ((double)value) / 100;
        }

        public ColourConfiguration(IPixelClassifier pixelClassifier)
        {
            this.pixelClassifier = pixelClassifier;
            FindColourValue = 100;

            InitializeComponent();

            DataContext = this;

            cmbColors.ItemsSource = typeof(System.Windows.Media.Colors).GetProperties().Where(p => new List<string> { "Red", "Blue" }.Contains(p.Name));
            cmbColors.SelectedIndex = this.pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue ? 0 : 1;
            LootDelay.Value = WowProcess.LootDelay;
        }

        private void RenderColour(bool renderMatchedArea)
        {
            Bitmap bitmap = new System.Drawing.Bitmap(256, 256);

            List<Point> points = new List<Point>();

            for (int i = 0; i < 256; i++)
            {
                for (int g = 0; g < 256; g++)
                {
                    byte r = (byte)FindColourValue;
                    byte b = (byte)i;

                    if (pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue)
                    {
                        r = (byte)i;
                        b = (byte)FindColourValue;

                    }

                    if (pixelClassifier.IsMatch(r, (byte)g, b))
                    {
                        points.Add(new Point(i, g));
                    }
                    bitmap.SetPixel(i, g, Color.FromArgb(r, g, b));

                }
            }

            if (ScreenCapture == null)
            {
                ScreenCapture = WowScreen.GetBitmap();
                renderMatchedArea = true;
            }

            ColourDisplay.Source = bitmap.ToBitmapImage();
            WowScreenshot.Source = ScreenCapture.ToBitmapImage();

            if (renderMatchedArea)
            {
                Dispatch(() =>
                {
                    MarkEdgeOfRedArea(bitmap, points);
                    ColourDisplay.Source = bitmap.ToBitmapImage();
                });

                Dispatch(() =>
                {
                    Bitmap bmp = new Bitmap(ScreenCapture);
                    MarkHighlightOnBitmap(bmp);
                    WowScreenshot.Source = bmp.ToBitmapImage();
                });
            }
        }

        private void MarkHighlightOnBitmap(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    if (pixelClassifier.IsMatch(pixel.R, pixel.G, pixel.B))
                    {
                        bmp.SetPixel(x, y, pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue ? Color.Blue : Color.Red);
                    }
                }
            }
        }

        private static void MarkEdgeOfRedArea(Bitmap bitmap, List<Point> points)
        {
            foreach (Point point in points)
            {
                int pointsClose = points.Count(p => (p.X == point.X && (p.Y == point.Y - 1 || p.Y == point.Y + 1)) || (p.Y == point.Y && (p.X == point.X - 1 || p.X == point.X + 1)));
                if (pointsClose < 4)
                {
                    bitmap.SetPixel(point.X, point.Y, Color.White);
                }
            }
        }
        private void LootDelay_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            WowProcess.LootDelay = (int)LootDelay.Value;
        }

        private void FindColour_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            LabelRed.Content = FindColourValue;
            RenderColour(false);
        }

        private void ColourMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColourText();
        }

        private void ColourClosenessMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColourText();
        }

        public void UpdateColourText()
        {
            LabelColourMultiplier.Text = $"{PrimaryColor} multiplied by {pixelClassifier.ColourMultiplier} must be greater than green and {SecondaryColor}.";
            LabelColourClosenessMultiplier.Text = $"How close green and {SecondaryColor} need to be to each other: {pixelClassifier.ColourClosenessMultiplier}";
            ColourLabel.Content = PrimaryColor + ":";
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            RenderColour(true);
        }

        private void Capture_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ScreenCapture = WowScreen.GetBitmap();
            RenderColour(true);
        }

        private void Dispatch(Action action)
        {
            _ = (System.Windows.Application.Current?.Dispatcher.BeginInvoke((Action)(() => action())));
            _ = (System.Windows.Application.Current?.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { })));
        }

        private void cmbColors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbColors.SelectedItem != null)
            {
                PropertyInfo propertyInfo = cmbColors.SelectedItem as PropertyInfo;
                object item = propertyInfo?.GetValue(null, null);
                if (item != null)
                {
                    if (propertyInfo?.Name == "Red")
                    {
                        PrimaryColor = "Red";
                        SecondaryColor = "blue";
                        pixelClassifier.Mode = PixelClassifier.ClassifierMode.Red;
                    }
                    else
                    {
                        PrimaryColor = "Blue";
                        SecondaryColor = "red";
                        pixelClassifier.Mode = PixelClassifier.ClassifierMode.Blue;
                    }

                    UpdateColourText();
                    RenderColour(true);
                }
            }
        }
    }
}