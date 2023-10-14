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

            char environmentChoice = GetUserChoice(ConsoleColor.Cyan, "Where would you like to fish?", "Water", "Lava");
            char durationChoice = GetUserChoice(ConsoleColor.Cyan, "How long do you want the bot to run?", "Minutes", "Hours", "Indefinitely");
            TimeSpan executionDuration = GetExecutionDuration(durationChoice);

            Separate();

            if (GetUserChoice(ConsoleColor.Cyan, "Choose an option:", "Start", "Exit") == '2')
                return;

            SetupAndStartBot(environmentChoice, durationChoice, executionDuration);
        }

        private static void PrintHeader()
        {
            var title = "TOXIC FISHING BOT";
            int width = title.Length + 10;
            PrintWithColor(ConsoleColor.Red, new string('#', width));
            PrintWithColor(ConsoleColor.Yellow, $"### {title} ###");
            PrintWithColor(ConsoleColor.Red, new string('#', width));
        }

        private static void Separate() => Console.WriteLine();

        private static void PrintWithColor(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static char GetUserChoice(ConsoleColor titleColor, string title, params string[] options)
        {
            PrintWithColor(titleColor, title);

            int topPosition = Console.CursorTop;
            var validChoices = new List<char>();

            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"    {i + 1}. {options[i]}");
                validChoices.Add((char)('1' + i));
            }

            char choice;
            do
            {
                choice = Console.ReadKey(true).KeyChar;

                if (validChoices.Contains(choice))
                {
                    Console.SetCursorPosition(0, topPosition);
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (choice == (char)('1' + i))
                            PrintWithColor(ConsoleColor.Yellow, $"    {i + 1}. {options[i]}");
                        else
                            Console.WriteLine($"    {i + 1}. {options[i]}");
                    }
                }
                else
                {
                    Console.SetCursorPosition(0, topPosition + options.Length); // Set the cursor position to the end of the options
                    Console.WriteLine(new string(' ', Console.WindowWidth)); // Clear the previous error message
                    Console.SetCursorPosition(0, topPosition + options.Length); // Set the cursor back to the end of the options
                    PrintWithColor(ConsoleColor.Red, "  > Please select a valid option.");
                }
            } while (!validChoices.Contains(choice));

            return choice;
        }


        private static TimeSpan GetExecutionDuration(char choice)
        {
            switch (choice)
            {
                case '1':
                    return AskTimeSpan(ConsoleColor.Cyan, "How many minutes?", TimeSpan.FromMinutes);
                case '2':
                    return AskTimeSpan(ConsoleColor.Cyan, "How many hours?", TimeSpan.FromHours);
                default:
                    return TimeSpan.Zero;
            }
        }

        private static TimeSpan AskTimeSpan(ConsoleColor promptColor, string prompt, Func<double, TimeSpan> timeConverter)
        {
            TimeSpan result = TimeSpan.Zero;
            bool isValid;

            do
            {
                PrintWithColor(promptColor, prompt + " (write a number and then press Enter)");

                Console.ForegroundColor = ConsoleColor.Yellow;
                int leftPosition = Console.CursorLeft + 4;
                Console.SetCursorPosition(leftPosition, Console.CursorTop);

                isValid = int.TryParse(Console.ReadLine(), out int time);

                if (isValid)
                {
                    Console.ResetColor();
                    result = timeConverter(time);
                }
                else
                {
                    Console.ResetColor();
                    PrintWithColor(ConsoleColor.Red, "Invalid input. Please enter a valid number.");
                }

            } while (!isValid);

            return result;
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
                PrintWithColor(ConsoleColor.Magenta, $"Starting in {i}...");
                Thread.Sleep(1000);
            }
        }
    }
}
