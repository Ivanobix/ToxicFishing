using FishingFun;
using FishingFunBot.Bot.Interfaces;
using FishingFunBot.Platform;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace FishingFunBot.Bot
{
    public class SearchBobberFinder : IBobberFinder, IImageProvider
    {
        private readonly IPixelClassifier pixelClassifier;

        private static readonly ILog logger = LogManager.GetLogger("Fishbot");

        private Point previousLocation;

        private Bitmap bitmap = new Bitmap(1, 1);

        public event EventHandler<BobberBitmapEvent> BitmapEvent;

        public SearchBobberFinder(IPixelClassifier pixelClassifier)
        {
            this.pixelClassifier = pixelClassifier;
            BitmapEvent += (s, e) => { };
        }

        public void Reset()
        {
            previousLocation = Point.Empty;
        }

        public Point Find()
        {
            bitmap = WowScreen.GetBitmap();

            Score? best = Score.ScorePoints(FindRedPoints());

            if (previousLocation != Point.Empty && best == null)
            {
                previousLocation = Point.Empty;
                best = Score.ScorePoints(FindRedPoints());
            }

            previousLocation = Point.Empty;
            if (best != null)
                previousLocation = best.point;

            BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = new Point(previousLocation.X, previousLocation.Y), Bitmap = bitmap });

            bitmap.Dispose();

            return previousLocation == Point.Empty ? Point.Empty : WowScreen.GetScreenPositionFromBitmapPostion(previousLocation);
        }

        private List<Score> FindRedPoints()
        {
            List<Score> points = new List<Score>();

            bool hasPreviousLocation = previousLocation != Point.Empty;

            // search around last found location
            int minX = Math.Max(hasPreviousLocation ? previousLocation.X - 40 : 0, 0);
            int maxX = Math.Min(hasPreviousLocation ? previousLocation.X + 40 : bitmap.Width, bitmap.Width);
            int minY = Math.Max(hasPreviousLocation ? previousLocation.Y - 40 : 0, 0);
            int maxY = Math.Min(hasPreviousLocation ? previousLocation.Y + 40 : bitmap.Height, bitmap.Height);

            //System.Diagnostics.Debug.WriteLine($"Search from X {minX}-{maxX}, Y {minY}-{maxY}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    ProcessPixel(points, x, y);
                }
            }
            sw.Stop();

            if (sw.ElapsedMilliseconds > 200)
            {
                string prevText = hasPreviousLocation ? " using previous location" : "";
                Debug.WriteLine($"Feather points found: {points.Count} in {sw.ElapsedMilliseconds}{prevText}.");
            }

            if (points.Count > 1000)
            {
                logger.Error("Error: Too much of the feather colour in this image, please adjust the colour configuration !");
                points.Clear();
            }

            return points;
        }

        private void ProcessPixel(List<Score> points, int x, int y)
        {
            Color p = bitmap.GetPixel(x, y);

            bool isMatch = pixelClassifier.IsMatch(p.R, p.G, p.B);

            if (isMatch)
            {
                points.Add(new Score { point = new Point(x, y) });
                bitmap.SetPixel(x, y, pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue ? Color.Blue : Color.Red);
            }
        }

        private class Score
        {
            public Point point;
            public int count = 0;

            public static Score? ScorePoints(List<Score> points)
            {
                foreach (Score p in points)
                {
                    p.count = points.Where(s => Math.Abs(s.point.X - p.point.X) < 10) // + or - 10 pixels horizontally
                        .Where(s => Math.Abs(s.point.Y - p.point.Y) < 10) // + or - 10 pixels vertically
                        .Count();
                }

                Score? best = points.OrderByDescending(s => s.count).FirstOrDefault();

                if (best != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"best score: {best.count} at {best.point.X},{best.point.Y}");
                }
                else
                {
                    Debug.WriteLine("No red found");
                }

                return best;
            }
        }
    }
}