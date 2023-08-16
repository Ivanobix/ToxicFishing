using System.Diagnostics;

namespace ToxicFishing.Platform
{
    public class TimedAction
    {
        public int actionTimeoutMs;
        public int maxTimeSecs;
        public Action<TimedAction> action;
        public Stopwatch stopwatch = new();
        public Stopwatch maxTime = new();

        public int ElapsedSecs => (int)maxTime.Elapsed.TotalSeconds;

        public TimedAction(Action<TimedAction> action, int actionTimeoutMs, int maxTimeSecs)
        {
            this.action = action;
            this.actionTimeoutMs = actionTimeoutMs;
            this.maxTimeSecs = maxTimeSecs;
            stopwatch.Start();
            maxTime.Start();
        }

        public void ExecuteNow()
        {
            action(this);
        }

        public bool ExecuteIfDue()
        {
            if (stopwatch.Elapsed.TotalMilliseconds > actionTimeoutMs)
            {
                action(this);
                stopwatch.Reset();
                stopwatch.Start();
            }

            return maxTime.Elapsed.TotalSeconds < maxTimeSecs;
        }
    }
}