using System.Diagnostics;
using ToxicFishing.Events;
using ToxicFishing.Platform;

namespace ToxicFishing.Bot
{
    public class FishingBot
    {
        public event EventHandler<FishingEvent> FishingEventHandler = (s, e) => { };

        private static readonly Random random = new();

        private readonly SearchBobberFinder bobberFinder;
        private readonly PositionBiteWatcher biteWatcher;
        private readonly ConsoleKey castKey;
        private readonly ConsoleKey applyLureKey;
        private readonly Stopwatch stopwatch = new();

        private DateTime StartTime = DateTime.Now;

        private int numberOfCasts = 0;
        private int numberOfSuccessfulLoots = 0;
        private int numberOfLuresApplied = 0;
        private int numberOfTimeouts = 0;

        public FishingBot(SearchBobberFinder bobberFinder, PositionBiteWatcher biteWatcher)
        {
            this.bobberFinder = bobberFinder ?? throw new ArgumentNullException(nameof(bobberFinder));
            this.biteWatcher = biteWatcher ?? throw new ArgumentNullException(nameof(biteWatcher));

            castKey = ConsoleKey.D4;
            applyLureKey = ConsoleKey.D5;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            biteWatcher.FishingEventHandler = (e) => FishingEventHandler?.Invoke(this, e);
            ApplyLure();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    ApplyLureIfDue();
                    InvokeFishingEvent(FishingAction.Cast);

                    WowProcess.PressKey(castKey);

                    await WatchAsync(2000);
                    await WaitForBiteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    await SleepAsync(2000);
                }
            }

            DisplayStatistics();
            Console.ReadLine();
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

            TimedAction timedTask = new((a) => { Console.WriteLine("Fishing timed out!"); }, 25_000, 25);

            while (true)
            {
                Point currentBobberPosition = bobberFinder.Find();

                if (currentBobberPosition == Point.Empty || currentBobberPosition.X == 0)
                    return;

                if (biteWatcher.IsBite(currentBobberPosition))
                {
                    Loot(bobberPosition);
                    numberOfSuccessfulLoots++;
                    ApplyLureIfDue();
                    return;
                }

                if (!timedTask.ExecuteIfDue())
                {
                    numberOfTimeouts++;
                    return;
                }

                await Task.Delay(100);
            }
        }

        private void ApplyLureIfDue()
        {
            if ((DateTime.Now - StartTime).TotalMinutes > 10)
                ApplyLure();
        }

        private void ApplyLure()
        {
            Console.Write($"\nUsing lure...\n");

            WowProcess.PressKey(applyLureKey);
            StartTime = DateTime.Now;
            numberOfLuresApplied++;

            Thread.Sleep(5000);
        }

        private static void Loot(Point bobberPosition)
        {
            Console.Write("Looting...\n");

            WowProcess.RightClickMouse(bobberPosition);
            WowProcess.PressKey(ConsoleKey.D6);
        }

        private static async Task SleepAsync(int ms)
        {
            await Task.Delay(ms + random.Next(0, 225));
        }

        private void InvokeFishingEvent(FishingAction action)
        {
            if (action == FishingAction.Cast)
            {
                Console.Write("Casting... ");
                numberOfCasts++;
            }

            FishingEventHandler?.Invoke(this, new FishingEvent { Action = action });
        }

        private void DisplayStatistics()
        {
            Console.WriteLine();
            Console.WriteLine($"====== Fishing Statistics ======");
            Console.WriteLine($"Number of casts: {numberOfCasts}");
            Console.WriteLine($"Number of successful loots: {numberOfSuccessfulLoots}");
            Console.WriteLine($"Number of lures applied: {numberOfLuresApplied}");
            Console.WriteLine($"Number of timeouts: {numberOfTimeouts}");
            Console.WriteLine($"===============================\n");
        }
    }
}
