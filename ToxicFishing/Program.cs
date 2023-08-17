using log4net;
using log4net.Config;
using ToxicFishing;
using ToxicFishing.Bot;
using ToxicFishing.Platform;

XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));

int strikeValue = 7;

PixelClassifier pixelClassifier = new();
if (args.Contains("blue"))
{
    Console.WriteLine("Blue mode");
    pixelClassifier.Mode = PixelClassifier.ClassifierMode.Blue;
}

pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

SearchBobberFinder bobberFinder = new(pixelClassifier);
PositionBiteWatcher biteWatcher = new(strikeValue);

FishingBot bot = new(bobberFinder, biteWatcher, ConsoleKey.D4, new List<ConsoleKey> { ConsoleKey.D5 });
bot.FishingEventHandler += (b, e) => LogManager.GetLogger("Fishbot").Info(e);

WowProcess.PressKey(ConsoleKey.Spacebar);
Thread.Sleep(1500);

await bot.StartAsync();