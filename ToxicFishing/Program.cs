using ToxicFishing.Bot;
using ToxicFishing.Platform;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ToxicFishing
{
    public class ToxicFishingApp
    {
        public static void Main()
        {
            PrintHeader();
            Separate();

            char environmentChoice = GetUserChoice("Where would you like to fish?", "1. Water", "2. Lava");
            char durationChoice = GetUserChoice("How long do you want the bot to run?", "1. Minutes", "2. Hours", "3. Indefinitely");
            TimeSpan executionDuration = GetExecutionDuration(durationChoice);

            Separate();

            if (GetUserChoice("Choose an option:", "1. Start", "2. Exit") == '2')
                return;

            SetupAndStartBot(environmentChoice, durationChoice, executionDuration);
        }

        private static void PrintHeader()
        {
            PrintWithColor(ConsoleColor.Red, "###################################");
            PrintWithColor(ConsoleColor.Yellow, "###      TOXIC FISHING BOT      ###");
            PrintWithColor(ConsoleColor.Red, "###################################");
        }

        private static void Separate() => Console.WriteLine();

        private static void PrintWithColor(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static char GetUserChoice(string title, params string[] options)
        {
            PrintWithColor(ConsoleColor.Yellow, title);

            foreach (var option in options)
            {
                Console.WriteLine(option);
            }

            char choice;
            var validChoices = string.Join("", options).ToCharArray();
            do
            {
                choice = Console.ReadKey().KeyChar;
                if (!Array.Exists(validChoices, c => c == choice))
                    PrintWithColor(ConsoleColor.Red, "Please select a valid option.");

            } while (!Array.Exists(validChoices, c => c == choice));

            return choice;
        }

        private static TimeSpan GetExecutionDuration(char choice)
        {
            return choice switch
            {
                '1' => AskTimeSpan("How many minutes?", TimeSpan.FromMinutes),
                '2' => AskTimeSpan("How many hours?", TimeSpan.FromHours),
                _ => TimeSpan.Zero,
            };
        }

        private static TimeSpan AskTimeSpan(string prompt, Func<double, TimeSpan> timeConverter)
        {
            PrintWithColor(ConsoleColor.Yellow, prompt);

            return int.TryParse(Console.ReadLine(), out int time) ? timeConverter(time) : TimeSpan.Zero;
        }

        private static void SetupAndStartBot(char envChoice, char durationChoice, TimeSpan duration)
        {
            var mode = envChoice == '1' ? PixelClassifier.ClassifierMode.Red : PixelClassifier.ClassifierMode.Blue;

            PixelClassifier pixelClassifier = new() { Mode = mode };
            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

            FishingBot bot = new(
                new SearchBobberFinder(pixelClassifier),
                new PositionBiteWatcher(),
                ConsoleKey.D4,
                new List<ConsoleKey> { ConsoleKey.D5 }
            );

            bot.FishingEventHandler += (b, e) => Console.WriteLine(e);

            Separate();

            CountdownToStart(5);

            WowProcess.PressKey(ConsoleKey.Spacebar);
            Thread.Sleep(1500);

            CancellationTokenSource cts = new();
            if (durationChoice != '3')
                cts.CancelAfter(duration);

            bot.StartAsync(cts.Token).Wait();
        }

        private static void CountdownToStart(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                Console.WriteLine($"Starting in {i}...");
                Thread.Sleep(1000);
            }
        }
    }
}
