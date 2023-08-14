namespace FishingFun
{
    public class ReticleDrawer
    {
        public void Draw(System.Drawing.Bitmap bmp, System.Drawing.Point point)
        {
            _ = bmp.GetPixel(point.X, point.Y);

            bmp.SetPixel(point.X, point.Y, System.Drawing.Color.White);

            using System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using System.Drawing.Pen thick_pen = new System.Drawing.Pen(System.Drawing.Color.White, 2);
            int cornerSize = 15;
            int recSize = 40;
            DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X - recSize, point.Y - recSize), cornerSize, cornerSize);
            DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X - recSize, point.Y + recSize), cornerSize, -cornerSize);
            DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X + recSize, point.Y - recSize), -cornerSize, cornerSize);
            DrawCorner(thick_pen, gr, new System.Drawing.Point(point.X + recSize, point.Y + recSize), -cornerSize, -cornerSize);
        }

        private void DrawCorner(System.Drawing.Pen pen, System.Drawing.Graphics gr, System.Drawing.Point corner, int xDiff, int yDiff)
        {
            System.Drawing.Point[] lines = new System.Drawing.Point[]
            {
                new System.Drawing.Point(corner.X + xDiff, corner.Y),
                corner,
                new System.Drawing.Point(corner.X, corner.Y + yDiff)
            };

            gr.DrawLines(pen, lines);
        }
    }
}