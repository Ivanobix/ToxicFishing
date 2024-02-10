using ToxicFishing.Events;

namespace ToxicFishing.Bot
{
    public class PositionBiteWatcher
    {
        private SortedSet<int> yPositions = [];
        private readonly int strikeValue = 7;
        private int yDiff;

        public Action<FishingEvent> FishingEventHandler { set; get; } = (e) => { };

        public void RaiseEvent(FishingEvent ev)
        {
            FishingEventHandler?.Invoke(ev);
        }

        public void Reset(Point InitialBobberPosition)
        {
            RaiseEvent(new FishingEvent { Action = FishingAction.Reset });

            yPositions.Clear();
            yPositions.Add(InitialBobberPosition.Y);
        }

        public bool IsBite(Point currentBobberPosition)
        {
            yPositions.Add(currentBobberPosition.Y);

            int medianPosition;
            if (yPositions.Count % 2 == 0)
                medianPosition = (yPositions.Count / 2) - 1;  // For even lengths
            else
                medianPosition = yPositions.Count / 2;      // For odd lengths

            int medianValue = yPositions.ElementAt(medianPosition);

            yDiff = medianValue - currentBobberPosition.Y;

            bool thresholdReached = yDiff <= -strikeValue;

            if (thresholdReached)
            {
                RaiseEvent(new FishingEvent { Action = FishingAction.Loot });
                return true;
            }

            return false;
        }
    }
}
