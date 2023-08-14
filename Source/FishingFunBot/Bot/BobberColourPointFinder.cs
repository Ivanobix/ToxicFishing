using System;
using System.Drawing;

namespace FishingFun
{
    public class BobberColourPointFinder : IBobberFinder, IImageProvider
    {
        private readonly Color targetColor;
        private Bitmap bmp = new Bitmap(1, 1);

        public BobberColourPointFinder(Color targetColor)
        {
            this.targetColor = targetColor;
            BitmapEvent += (s, e) => { };
        }

        public event EventHandler<BobberBitmapEvent> BitmapEvent;

        public Point Find()
        {
            bmp = WowScreen.GetBitmap();

            const int targetOffset = 15;

            int widthLower = 0;
            int widthHigher = bmp.Width;
            int heightLower = 0;
            int heightHigher = bmp.Height;

            int targetRedLb = targetColor.R - targetOffset;
            int targetRedHb = targetColor.R + targetOffset;
            int targetBlueLb = targetColor.B - targetOffset;
            int targetBlueHb = targetColor.B + targetOffset;
            int targetGreenLb = targetColor.G - targetOffset;
            int targetGreenHb = targetColor.G + targetOffset;

            Point pos = new Point(0, 0);

            for (int i = widthLower; i < widthHigher; i++)
            {
                for (int j = heightLower; j < heightHigher; j++)
                {
                    pos.X = i;
                    pos.Y = j;
                    Color colorAt = WowScreen.GetColorAt(pos, bmp);
                    if (colorAt.R > targetRedLb &&
                        colorAt.R < targetRedHb &&
                        colorAt.B > targetBlueLb &&
                        colorAt.B < targetBlueHb &&
                        colorAt.G > targetGreenLb &&
                        colorAt.G < targetGreenHb)
                    {
                        BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = new Point(i, j), Bitmap = bmp });
                        return WowScreen.GetScreenPositionFromBitmapPostion(pos);
                    }
                }
            }

            BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = Point.Empty, Bitmap = bmp });
            bmp.Dispose();
            return Point.Empty;
        }

        public Bitmap GetBitmap()
        {
            return bmp;
        }

        public void Reset()
        {
        }
    }
}