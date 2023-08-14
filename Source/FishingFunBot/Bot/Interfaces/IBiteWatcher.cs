using System;
using System.Drawing;

namespace FishingFunBot.Bot.Interfaces
{
    public interface IBiteWatcher
    {
        void Reset(Point InitialBobberPosition);

        bool IsBite(Point currentBobberPosition);

        Action<FishingEvent> FishingEventHandler { set; get; }
    }
}