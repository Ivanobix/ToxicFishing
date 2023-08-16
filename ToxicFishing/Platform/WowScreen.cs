namespace ToxicFishing.Platform
{
    public static class WowScreen
    {
        private static readonly int ScreenWidthDivided = Screen.PrimaryScreen.Bounds.Width / 4;
        private static readonly int ScreenHeightDivided = Screen.PrimaryScreen.Bounds.Height / 4;

        public static Bitmap GetBitmap()
        {
            int width = Screen.PrimaryScreen.Bounds.Width / 2;
            int height = (Screen.PrimaryScreen.Bounds.Height / 2) - 100;

            Bitmap bmpScreen = new(width, height);
            
            using (Graphics graphics = Graphics.FromImage(bmpScreen))
            {
                graphics.CopyFromScreen(ScreenWidthDivided, ScreenHeightDivided, 0, 0, bmpScreen.Size);
            }
            
            return bmpScreen;
        }

        public static Point GetScreenPositionFromBitmapPostion(Point pos)
        {
            return new Point(pos.X + ScreenWidthDivided, pos.Y + ScreenHeightDivided);
        }
    }
}
