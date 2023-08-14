﻿using FishingFunBot.Bot.Interfaces;
using FishingFunBot.Platform;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FishingFunBot.Bot
{
    public class PositionBiteWatcher : IBiteWatcher
    {
        private static readonly ILog logger = LogManager.GetLogger("Fishbot");

        private List<int> yPositions = new List<int>();
        private readonly int strikeValue;
        private int yDiff;
        private TimedAction? timer;

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
            timer = new TimedAction((a) =>
            {
                RaiseEvent(new FishingEvent { Amplitude = yDiff, Action = FishingAction.BobberMove });
            }, 500, 25);
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

            _ = (timer?.ExecuteIfDue());

            if (thresholdReached)
            {
                RaiseEvent(new FishingEvent { Action = FishingAction.Loot });
                timer?.ExecuteNow();
                return true;
            }

            return false;
        }
    }
}