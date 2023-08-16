namespace ToxicFishing.Events
{
    public class BobberBitmapEvent : EventArgs
    {
        public Bitmap Bitmap { get; set; } = new Bitmap(1, 1);
        public Point Point { get; set; }
    }
}
