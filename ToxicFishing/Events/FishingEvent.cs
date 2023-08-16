namespace ToxicFishing.Events
{
    public class FishingEvent : EventArgs
    {
        public FishingAction Action;
        public int Amplitude;

        public override string ToString()
        {
            return Action.ToString();
        }
    }

    public enum FishingAction
    {
        Reset,
        Loot,
        Cast
    }
}
