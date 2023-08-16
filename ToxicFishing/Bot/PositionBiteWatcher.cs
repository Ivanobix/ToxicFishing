using ToxicFishing.Events;

namespace ToxicFishing.Bot
{
    public class PositionBiteWatcher
    {
        private List<int> yPositions = new();
        private readonly int strikeValue;
        private int yDiff;

        public Action<FishingEvent> FishingEventHandler { set; get; } = (e) => { };

        public PositionBiteWatcher(int strikeValue)
        {
            this.strikeValue = strikeValue;
        }

        public void RaiseEvent(FishingEvent ev)
        {
            FishingEventHandler?.Invoke(ev);
        }

        public void Reset(Point InitialBobberPosition)
        {
            RaiseEvent(new FishingEvent { Action = FishingAction.Reset });

            yPositions = new List<int>
            {
                InitialBobberPosition.Y
            };
        }

        public bool IsBite(Point currentBobberPosition)
        {
            if (!yPositions.Contains(currentBobberPosition.Y))
            {
                yPositions.Add(currentBobberPosition.Y);
                yPositions.Sort();
            }

            yDiff = yPositions[(int)((yPositions.Count + 0.5) / 2)] - currentBobberPosition.Y;

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