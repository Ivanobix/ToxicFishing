using FishingFun;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Powershell
{
    public class Program
    {
        private static void Main(string[] args)
        {
            _ = log4net.Config.XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));

            int strikeValue = 7;

            PixelClassifier pixelClassifier = new PixelClassifier();
            if (args.Contains("blue"))
            {
                Console.WriteLine("Blue mode");
                pixelClassifier.Mode = PixelClassifier.ClassifierMode.Blue;
            }

            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

            SearchBobberFinder bobberFinder = new SearchBobberFinder(pixelClassifier);
            PositionBiteWatcher biteWatcher = new PositionBiteWatcher(strikeValue);

            FishingBot bot = new FishingBot(bobberFinder, biteWatcher, ConsoleKey.D4, new List<ConsoleKey> { ConsoleKey.D5 });
            bot.FishingEventHandler += (b, e) => LogManager.GetLogger("Fishbot").Info(e);

            WowProcess.PressKey(ConsoleKey.Spacebar);
            System.Threading.Thread.Sleep(1500);

            bot.Start();
        }
    }
}