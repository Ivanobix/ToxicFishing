using ToxicFishing;
using ToxicFishing.Bot;
using ToxicFishing.Platform;

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
bot.FishingEventHandler += (b, e) => Console.WriteLine(e);

WowProcess.PressKey(ConsoleKey.Spacebar);
Thread.Sleep(1500);

await bot.StartAsync();