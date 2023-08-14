using System;
using System.Drawing;

namespace FishingFunBot.Bot.Interfaces
{
    public interface IImageProvider
    {
        event EventHandler<BobberBitmapEvent> BitmapEvent;
    }

    public class BobberBitmapEvent : EventArgs
    {
        public Bitmap Bitmap { get; set; } = new Bitmap(1, 1);
        public Point Point { get; set; }
    }

    public enum FishingAction
    {
        Reset,
        Loot,
        Cast
    }

    public class FishingEvent : EventArgs
    {
        public FishingAction Action;
        public int Amplitude;

        public override string ToString()
        {
            return Action.ToString();
        }
    }
}