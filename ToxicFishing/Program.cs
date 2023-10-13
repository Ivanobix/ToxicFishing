using ToxicFishing.Bot;
using ToxicFishing.Platform;

namespace ToxicFishing
{
    public class ToxicFishingApp
    {
        public static void Main()
        {
            PrintHeader();

            if (GetUserChoice("\n1. Start.\n2. Exit.") == '2')
                return;

            char environmentChoice = GetUserChoice("\n\nWhere would you like to fish?\n1. Water\n2. Lava");
            char durationChoice = GetUserChoice("\n\nHow long do you want the bot to run?\n1. Minutes\n2. Hours\n3. Indefinitely");

            TimeSpan executionDuration = GetExecutionDuration(durationChoice);

            SetupAndStartBot(environmentChoice, durationChoice, executionDuration);
        }

        private static void PrintHeader()
        {
            PrintWithColor(ConsoleColor.Red, "###################################");
            PrintWithColor(ConsoleColor.Yellow, "###      TOXIC FISHING BOT      ###");
            PrintWithColor(ConsoleColor.Red, "###################################");
        }

        private static void PrintWithColor(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static char GetUserChoice(string prompt)
        {
            PrintWithColor(ConsoleColor.Yellow, prompt);
            char choice;
            do
            {
                choice = Console.ReadKey().KeyChar;

                if (!prompt.Contains(choice.ToString() + ". "))
                    PrintWithColor(ConsoleColor.Red, "\nPlease select a valid option.");

            } while (!prompt.Contains(choice.ToString() + ". "));
            return choice;
        }

        private static TimeSpan GetExecutionDuration(char choice)
        {
            TimeSpan time = choice switch
            {
                '1' => AskTimeSpan("\nHow many minutes?", TimeSpan.FromMinutes),
                '2' => AskTimeSpan("\nHow many hours?", TimeSpan.FromHours),
                _ => TimeSpan.Zero,
            };

            if ((choice == '1' || choice == '2') && time == TimeSpan.Zero)
                return GetExecutionDuration(choice);

            return time;
        }

        private static TimeSpan AskTimeSpan(string prompt, Func<double, TimeSpan> timeConverter)
        {
            PrintWithColor(ConsoleColor.Yellow, prompt);

            if (int.TryParse(Console.ReadLine(), out int time))
                return timeConverter(time);

            return TimeSpan.Zero;
        }

        private static void SetupAndStartBot(char envChoice, char durationChoice, TimeSpan duration)
        {
            Console.WriteLine(Environment.NewLine);

            PixelClassifier pixelClassifier = new()
            {
                Mode = envChoice == '1' ? PixelClassifier.ClassifierMode.Red : PixelClassifier.ClassifierMode.Blue
            };
            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

            FishingBot bot = new(
                new SearchBobberFinder(pixelClassifier),
                new PositionBiteWatcher(),
                ConsoleKey.D4,
                new List<ConsoleKey> { ConsoleKey.D5 }
            );

            bot.FishingEventHandler += (b, e) => Console.WriteLine(e);

            Console.WriteLine(Environment.NewLine);

            for (int i = 5; i > 0; i--)
            {
                Console.WriteLine($"Starting in {i}...");
                Thread.Sleep(1000);
            }

            WowProcess.PressKey(ConsoleKey.Spacebar);
            Thread.Sleep(1500);

            CancellationTokenSource cts = new();

            if (durationChoice != '3')
                cts.CancelAfter(duration);

            bot.StartAsync(cts.Token).Wait();
        }
    }
}