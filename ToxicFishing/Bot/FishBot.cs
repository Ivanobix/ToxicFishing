using log4net;
using System.Diagnostics;
using ToxicFishing.Events;
using ToxicFishing.Platform;

namespace ToxicFishing.Bot
{
    public class FishingBot
    {
        private static readonly ILog logger = LogManager.GetLogger("Fishbot");
        private static readonly Random random = new();

        private readonly SearchBobberFinder bobberFinder;
        private readonly PositionBiteWatcher biteWatcher;
        private readonly ConsoleKey castKey;
        private readonly List<ConsoleKey> tenMinKey;
        private readonly Stopwatch stopwatch = new();

        private DateTime StartTime;

        public event EventHandler<FishingEvent> FishingEventHandler = (s, e) => { };

        public FishingBot(SearchBobberFinder bobberFinder, PositionBiteWatcher biteWatcher, ConsoleKey castKey, List<ConsoleKey> tenMinKey)
        {
            this.bobberFinder = bobberFinder ?? throw new ArgumentNullException(nameof(bobberFinder));
            this.biteWatcher = biteWatcher ?? throw new ArgumentNullException(nameof(biteWatcher));
            this.castKey = castKey;
            this.tenMinKey = tenMinKey ?? new List<ConsoleKey>();

            StartTime = DateTime.Now;
        }

        public async Task StartAsync()
        {
            biteWatcher.FishingEventHandler = (e) => FishingEventHandler?.Invoke(this, e);
            DoTenMinuteKey();

            while (true)
            {
                try
                {
                    PressTenMinKeyIfDue();
                    InvokeFishingEvent(FishingAction.Cast);

                    WowProcess.PressKey(castKey);

                    await WatchAsync(2000);
                    await WaitForBiteAsync();
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    await SleepAsync(2000);
                }
            }
        }

        private async Task WatchAsync(int milliseconds)
        {
            bobberFinder.Reset();
            stopwatch.Restart();

            while (stopwatch.ElapsedMilliseconds < milliseconds)
                bobberFinder.Find();

            await Task.Delay(milliseconds);
            stopwatch.Stop();
        }

        private async Task WaitForBiteAsync()
        {
            bobberFinder.Reset();
            Point bobberPosition = bobberFinder.Find();

            if (bobberPosition == Point.Empty)
                return;

            biteWatcher.Reset(bobberPosition);
            logger.Info($"Bobber start position: {bobberPosition}");
            TimedAction timedTask = new((a) => { logger.Info("Fishing timed out!"); }, 25_000, 25);

            while (true)
            {
                Point currentBobberPosition = bobberFinder.Find();

                if (currentBobberPosition == Point.Empty || currentBobberPosition.X == 0)
                    return;

                if (biteWatcher.IsBite(currentBobberPosition))
                {
                    Loot(bobberPosition);
                    PressTenMinKeyIfDue();
                    return;
                }

                if (!timedTask.ExecuteIfDue())
                    return;

                await Task.Delay(100); // Adding a small delay to not saturate the loop
            }
        }

        private void PressTenMinKeyIfDue()
        {
            if ((DateTime.Now - StartTime).TotalMinutes > 10 && tenMinKey.Any())
                DoTenMinuteKey();
        }

        private void DoTenMinuteKey()
        {
            StartTime = DateTime.Now;

            if (!tenMinKey.Any())
            {
                logger.Info("Ten Minute Key: No keys defined in tenMinKey, so nothing to do (Define in call to FishingBot constructor).");
                return;
            }

            InvokeFishingEvent(FishingAction.Cast);

            foreach (ConsoleKey key in tenMinKey)
            {
                logger.Info($"Ten Minute Key: Pressing key {key} to run a macro, delete junk fish or apply a lure etc.");
                WowProcess.PressKey(key);
            }
        }

        private static void Loot(Point bobberPosition)
        {
            logger.Info("Right clicking mouse to Loot.");
            WowProcess.RightClickMouse(logger, bobberPosition);

            logger.Info("Trying to accept soulbound loot.");
            WowProcess.PressKey(ConsoleKey.D6);
        }

        private static async Task SleepAsync(int ms)
        {
            await Task.Delay(ms + random.Next(0, 225));
        }

        private void InvokeFishingEvent(FishingAction action)
        {
            FishingEventHandler?.Invoke(this, new FishingEvent { Action = action });
        }
    }
}
