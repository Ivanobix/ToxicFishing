using System.Diagnostics;
using ToxicFishing.Events;
using ToxicFishing.Platform;

namespace ToxicFishing.Bot
{
    public class SearchBobberFinder
    {
        private readonly PixelClassifier pixelClassifier;

        private Point previousLocation;
        private Bitmap? bitmap;

        public event EventHandler<BobberBitmapEvent> BitmapEvent = delegate { };

        private static DateTime _lastConsoleWriteTime = DateTime.MinValue;

        public SearchBobberFinder(PixelClassifier pixelClassifier)
        {
            this.pixelClassifier = pixelClassifier;
        }

        public void Reset()
        {
            previousLocation = Point.Empty;
        }

        public Point Find()
        {
            using (bitmap = WowScreen.GetBitmap())
            {
                Score? bestScore = ScorePoints(FindRedPoints());

                if (previousLocation != Point.Empty && bestScore == null)
                {
                    previousLocation = Point.Empty;
                    bestScore = ScorePoints(FindRedPoints());
                }

                Point returnPoint = previousLocation == Point.Empty
                                  ? Point.Empty
                                  : WowScreen.GetScreenPositionFromBitmapPostion(previousLocation);

                BitmapEvent.Invoke(this, new BobberBitmapEvent { Point = previousLocation, Bitmap = bitmap });

                return returnPoint;
            }
        }

        private List<Score> FindRedPoints()
        {
            List<Score> points = new();

            bool hasPreviousLocation = previousLocation != Point.Empty;
            int minX = Math.Max(hasPreviousLocation ? previousLocation.X - 40 : 0, 0);
            int maxX = Math.Min(hasPreviousLocation ? previousLocation.X + 40 : bitmap.Width, bitmap.Width);
            int minY = Math.Max(hasPreviousLocation ? previousLocation.Y - 40 : 0, 0);
            int maxY = Math.Min(hasPreviousLocation ? previousLocation.Y + 40 : bitmap.Height, bitmap.Height);

            Stopwatch sw = Stopwatch.StartNew();

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                    ProcessPixel(points, x, y);
            }

            sw.Stop();

            if (points.Count > 1000 && DateTime.Now - _lastConsoleWriteTime > TimeSpan.FromSeconds(1))
            {
                Console.WriteLine("Error: There's an excessive amount of red (or colors resembling red like orange) at the center of your screen. To address this, please select a fishing area devoid of red hues.");
                _lastConsoleWriteTime = DateTime.Now;
                points.Clear();
            }

            return points;
        }


        private void ProcessPixel(List<Score> points, int x, int y)
        {
            Color p = bitmap.GetPixel(x, y);

            if (pixelClassifier.IsMatch(p.R, p.G, p.B))
            {
                points.Add(new Score { point = new Point(x, y) });
                bitmap.SetPixel(x, y, pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue ? Color.Blue : Color.Red);
            }
        }

        private Score? ScorePoints(List<Score> points)
        {
            foreach (Score p in points)
                p.count = points.Count(s => Math.Abs(s.point.X - p.point.X) < 10 && Math.Abs(s.point.Y - p.point.Y) < 10);

            Score? best = points.OrderByDescending(s => s.count).FirstOrDefault();

            previousLocation = best?.point ?? Point.Empty;

            return best;
        }

        private class Score
        {
            public Point point;
            public int count = 0;
        }
    }
}
