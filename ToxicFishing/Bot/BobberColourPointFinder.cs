using ToxicFishing.Events;
using ToxicFishing.Platform;

namespace ToxicFishing.Bot
{
    public class BobberColourPointFinder
    {
        private readonly Color targetColor;
        private readonly int targetOffset = 15;

        public event EventHandler<BobberBitmapEvent> BitmapEvent;

        public BobberColourPointFinder(Color targetColor)
        {
            this.targetColor = targetColor;
            BitmapEvent += (s, e) => { };
        }

        public Point Find()
        {
            using (Bitmap bmp = WowScreen.GetBitmap())
            {
                int targetRedLb = targetColor.R - targetOffset;
                int targetRedHb = targetColor.R + targetOffset;
                int targetBlueLb = targetColor.B - targetOffset;
                int targetBlueHb = targetColor.B + targetOffset;
                int targetGreenLb = targetColor.G - targetOffset;
                int targetGreenHb = targetColor.G + targetOffset;

                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color colorAt = bmp.GetPixel(i, j);
                        if (colorAt.R > targetRedLb && colorAt.R < targetRedHb &&
                            colorAt.B > targetBlueLb && colorAt.B < targetBlueHb &&
                            colorAt.G > targetGreenLb && colorAt.G < targetGreenHb)
                        {
                            Point pos = new(i, j);
                            BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = pos, Bitmap = bmp });
                            return WowScreen.GetScreenPositionFromBitmapPostion(pos);
                        }
                    }
                }

                BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = Point.Empty, Bitmap = bmp });
            }
            
            return Point.Empty;
        }
    }
}
