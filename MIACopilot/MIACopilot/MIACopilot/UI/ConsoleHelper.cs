using System;

namespace MIACopilot.UI
{
    /// <summary>
    /// Helper methods for consistent, readable console output and input handling.
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Prints a styled header section.
        /// </summary>
        /// <param name="title">Title to show in the header.</param>
        public static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 50));
            Console.WriteLine(" " + title);
            Console.WriteLine(new string('=', 50));
            Console.ResetColor();
        }

        /// <summary>
        /// Prints a success message (green).
        /// </summary>
        /// <param name="message">Message to display.</param>
        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK] " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Prints an error message (red).
        /// </summary>
        /// <param name="message">Message to display.</param>
        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Prints a warning message (yellow).
        /// </summary>
        /// <param name="message">Message to display.</param>
        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARN] " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Waits for a key press to continue (does not echo the pressed key).
        /// </summary>
        public static void PressAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        /// <summary>
        /// Reads a non-empty string from the console. Repeats until input is valid.
        /// </summary>
        /// <param name="prompt">Prompt shown to the user.</param>
        /// <returns>Trimmed, non-empty string.</returns>
        public static string ReadRequiredString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                var value = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;

                PrintError("This field cannot be empty. Please try again.");
            }
        }

        /// <summary>
        /// Reads a string from the console. Empty input is allowed.
        /// </summary>
        /// <param name="prompt">Prompt shown to the user.</param>
        /// <returns>Trimmed string (possibly empty).</returns>
        public static string ReadOptionalString(string prompt)
        {
            Console.Write(prompt + ": ");
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Reads an integer from the console. Repeats until input is valid.
        /// </summary>
        /// <param name="prompt">Prompt shown to the user.</param>
        /// <returns>Parsed integer.</returns>
        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                if (int.TryParse(Console.ReadLine(), out var result))
                    return result;

                PrintError("Please enter a valid number.");
            }
        }

        /// <summary>
        /// Reads a date from the console. Repeats until input is valid.
        /// </summary>
        /// <param name="prompt">Prompt shown to the user.</param>
        /// <returns>Parsed <see cref="DateTime"/>.</returns>
        public static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + " (dd.MM.yyyy): ");
                if (DateTime.TryParse(Console.ReadLine(), out var dt))
                    return dt;

                PrintError("Please enter a valid date.");
            }
        }

        /// <summary>
        /// Asks the user for confirmation (yes/no).
        /// </summary>
        /// <param name="prompt">Question text (e.g. "Delete item?").</param>
        /// <returns>True if user answers 'y', false if user answers 'n'.</returns>
        public static bool Confirm(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                var key = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();

                if (key == 'y' || key == 'Y') return true;
                if (key == 'n' || key == 'N') return false;

                PrintError("Please type y or n.");
            }
        }

        /// <summary>
        /// Asks the user for confirmation (yes/no) with a default answer when Enter is pressed.
        /// </summary>
        /// <param name="prompt">Question text.</param>
        /// <param name="defaultYes">If true, Enter means Yes; if false, Enter means No.</param>
        /// <returns>Chosen confirmation result.</returns>
        public static bool Confirm(string prompt, bool defaultYes)
        {
            var defText = defaultYes ? "Y/n" : "y/N";

            while (true)
            {
                Console.Write($"{prompt} ({defText}): ");
                var keyInfo = Console.ReadKey(intercept: true);
                Console.WriteLine();

                if (keyInfo.Key == ConsoleKey.Enter)
                    return defaultYes;

                var key = keyInfo.KeyChar;
                if (key == 'y' || key == 'Y') return true;
                if (key == 'n' || key == 'N') return false;

                PrintError("Please type y or n (or press Enter).");
            }
        }
    }
}