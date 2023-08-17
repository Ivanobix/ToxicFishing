using System.Diagnostics;

namespace ToxicFishing.Platform
{
    public class TimedAction
    {
        private readonly Action<TimedAction> action;
        private readonly Stopwatch stopwatch = new();
        private readonly Stopwatch maxTime = new();
        public int ActionTimeoutMs { get; }
        public int MaxTimeSecs { get; }

        public int ElapsedSecs => (int)maxTime.Elapsed.TotalSeconds;

        public TimedAction(Action<TimedAction> action, int actionTimeoutMs, int maxTimeSecs)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            ActionTimeoutMs = actionTimeoutMs;
            MaxTimeSecs = maxTimeSecs;
            stopwatch.Start();
            maxTime.Start();
        }

        public void ExecuteNow()
        {
            action(this);
        }

        public bool ExecuteIfDue()
        {
            if (stopwatch.Elapsed.TotalMilliseconds > ActionTimeoutMs)
            {
                action(this);
                stopwatch.Restart();
            }

            return ElapsedSecs < MaxTimeSecs;
        }
    }
}