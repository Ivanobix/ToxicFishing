﻿using ToxicFishing.Bot;
using ToxicFishing.Platform;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ToxicFishing
{
    public class ToxicFishingApp
    {
        private const int CountdownSeconds = 5;

        private enum EnvironmentChoice { Water, Lava }
        private enum DurationChoice { Minutes, Hours, Indefinitely }

        public static async Task Main()
        {
            PrintHeader();

            EnvironmentChoice environmentChoice = (EnvironmentChoice)(GetUserChoice(ConsoleColor.Cyan, "Where would you like to fish?", "Water", "Lava") - '1');
            DurationChoice durationChoice = (DurationChoice)(GetUserChoice(ConsoleColor.Cyan, "How long do you want the bot to run?", "Minutes", "Hours", "Indefinitely") - '1');
            TimeSpan executionDuration = GetExecutionDuration(durationChoice);

            Separate();

            if (GetUserChoice(ConsoleColor.Cyan, "Choose an option:", "Start", "Exit") == '2')
                return;

            await SetupAndStartBot(environmentChoice, durationChoice, executionDuration);
        }

        private static void PrintHeader()
        {
            const string Title = "TOXIC FISHING BOT";
            int width = Title.Length + 10;

            PrintWithColor(ConsoleColor.Red, new string('#', width));
            PrintWithColor(ConsoleColor.Yellow, $"### {Title} ###");
            PrintWithColor(ConsoleColor.Red, new string('#', width));

            Separate();
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
            List<char> validChoices = new List<char>();

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


        private static TimeSpan GetExecutionDuration(DurationChoice choice)
        {
            return choice switch
            {
                DurationChoice.Minutes => AskTimeSpan(ConsoleColor.Cyan, "How many minutes?", TimeSpan.FromMinutes),
                DurationChoice.Hours => AskTimeSpan(ConsoleColor.Cyan, "How many hours?", TimeSpan.FromHours),
                _ => TimeSpan.Zero
            };
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


        private static async Task SetupAndStartBot(EnvironmentChoice envChoice, DurationChoice durationChoice, TimeSpan duration)
        {
            PixelClassifier.ClassifierMode mode = envChoice == EnvironmentChoice.Water ? PixelClassifier.ClassifierMode.Red : PixelClassifier.ClassifierMode.Blue;
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
            CountdownToStart(CountdownSeconds);

            WowProcess.PressKey(ConsoleKey.Spacebar);
            Thread.Sleep(1500);

            CancellationTokenSource cts = new();
            if (durationChoice != DurationChoice.Indefinitely)
                cts.CancelAfter(duration);

            await bot.StartAsync(cts.Token);
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