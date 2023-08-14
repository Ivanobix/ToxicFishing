using FishingFunBot.Bot.Interfaces;
using FishingFunBot.Platform;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace FishingFunBot.Bot
{
    public class FishingBot
    {
        public static ILog logger = LogManager.GetLogger("Fishbot");

        private ConsoleKey castKey;
        private readonly List<ConsoleKey> tenMinKey;
        private readonly IBobberFinder bobberFinder;
        private readonly IBiteWatcher biteWatcher;
        private bool isEnabled;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly Random random = new Random();

        public event EventHandler<FishingEvent> FishingEventHandler;

        public FishingBot(IBobberFinder bobberFinder, IBiteWatcher biteWatcher, ConsoleKey castKey, List<ConsoleKey> tenMinKey)
        {
            this.bobberFinder = bobberFinder;
            this.biteWatcher = biteWatcher;
            this.castKey = castKey;
            this.tenMinKey = tenMinKey;

            logger.Info("FishBot Created.");

            FishingEventHandler += (s, e) => { };
        }

        public void Start()
        {
            biteWatcher.FishingEventHandler = (e) => FishingEventHandler?.Invoke(this, e);

            isEnabled = true;

            DoTenMinuteKey();

            while (isEnabled)
            {
                try
                {
                    logger.Info($"Pressing key {castKey} to Cast.");

                    PressTenMinKeyIfDue();

                    FishingEventHandler?.Invoke(this, new FishingEvent { Action = FishingAction.Cast });
                    WowProcess.PressKey(castKey);

                    Watch(2000);

                    WaitForBite();
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    Sleep(2000);
                }
            }

            logger.Error("Bot has Stopped.");
        }

        public void SetCastKey(ConsoleKey castKey)
        {
            this.castKey = castKey;
        }

        private void Watch(int milliseconds)
        {
            bobberFinder.Reset();
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < milliseconds)
            {
                _ = bobberFinder.Find();
            }
            stopwatch.Stop();
        }

        public void Stop()
        {
            isEnabled = false;
            logger.Error("Bot is Stopping...");
        }

        private void WaitForBite()
        {
            bobberFinder.Reset();

            Point bobberPosition = FindBobber();
            if (bobberPosition == Point.Empty)
                return;

            biteWatcher.Reset(bobberPosition);

            logger.Info("Bobber start position: " + bobberPosition);

            TimedAction timedTask = new TimedAction((a) => { logger.Info("Fishing timed out!"); }, 25 * 1000, 25);

            // Wait for the bobber to move
            while (isEnabled)
            {
                Point currentBobberPosition = FindBobber();
                if (currentBobberPosition == Point.Empty || currentBobberPosition.X == 0) return;
                if (biteWatcher.IsBite(currentBobberPosition))
                {
                    Loot(bobberPosition);
                    PressTenMinKeyIfDue();
                    return;
                }

                if (!timedTask.ExecuteIfDue()) return;
            }
        }

        private DateTime StartTime = DateTime.Now;

        private void PressTenMinKeyIfDue()
        {
            if ((DateTime.Now - StartTime).TotalMinutes > 10 && tenMinKey.Count > 0)
                DoTenMinuteKey();
        }

        /// <summary>
        /// Ten minute key can do anything you want e.g.
        /// Macro to apply a lure: 
        /// /use Bright Baubles
        /// /use 16
        /// 
        /// Or a macro to delete junk:
        /// /run for b=0,4 do for s=1,GetContainerNumSlots(b) do local n=GetContainerItemLink(b,s) if n and (strfind(n,"Raw R") or strfind(n,"Raw Spot") or strfind(n,"Raw Glo") or strfind(n,"roup")) then PickupContainerItem(b,s) DeleteCursorItem() end end end
        /// </summary>
        private void DoTenMinuteKey()
        {
            StartTime = DateTime.Now;

            if (tenMinKey.Count == 0)
                logger.Info($"Ten Minute Key:  No keys defined in tenMinKey, so nothing to do (Define in call to FishingBot constructor).");

            FishingEventHandler?.Invoke(this, new FishingEvent { Action = FishingAction.Cast });

            foreach (ConsoleKey key in tenMinKey)
            {
                logger.Info($"Ten Minute Key: Pressing key {key} to run a macro, delete junk fish or apply a lure etc.");
                WowProcess.PressKey(key);
            }
        }

        private static void Loot(Point bobberPosition)
        {
            logger.Info($"Right clicking mouse to Loot.");
            WowProcess.RightClickMouse(logger, bobberPosition);
            logger.Info($"Trying to accept souldbound loot.");
            WowProcess.PressKey(ConsoleKey.D6); // Loot souldbound objects (requires manual action)
        }

        public static void Sleep(int ms)
        {
            ms += random.Next(0, 225);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.TotalMilliseconds < ms)
            {
                FlushBuffers();
                Thread.Sleep(100);
            }
        }

        public static void FlushBuffers()
        {
            ILog log = LogManager.GetLogger("Fishbot");
            if (log.Logger is Logger logger)
            {
                foreach (IAppender appender in logger.Appenders)
                {
                    BufferingAppenderSkeleton? buffered = appender as BufferingAppenderSkeleton;
                    buffered?.Flush();
                }
            }
        }

        private Point FindBobber()
        {
            TimedAction timer = new TimedAction((a) => { logger.Info("Waited seconds for target: " + a.ElapsedSecs); }, 1000, 5);

            while (true)
            {
                Point target = bobberFinder.Find();
                if (target != Point.Empty || !timer.ExecuteIfDue()) return target;
            }
        }
    }
}