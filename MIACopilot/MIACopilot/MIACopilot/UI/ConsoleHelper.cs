namespace MIACopilot.UI; output.
/// Used by the console-based UI to standardize formatting,
/// colors and user input handling.
/// </summary>
public static class ConsoleHelper
{
    /// <summary>
    /// Prints a styled section header with a title.
    /// Uses cyan color and separator lines for readability.
    /// </summary>
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 50));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('═', 50));
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a success message in green with a checkmark.
    /// </summary>
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message in red with a cross.
    /// </summary>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a warning message in yellow with a warning icon.
    /// </summary>
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prompts the user for a required (non-empty) string.
    /// Repeats until a valid value is entered.
    /// </summary>
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            var value = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            PrintError("This field cannot be empty. Please try again.");
        }
    }

    /// <summary>
    /// Prompts the user for an optional string.
    /// Returns an empty string if nothing is entered.
    /// </summary>
    public static string ReadOptionalString(string prompt)
    {
        Console.Write($"  {prompt}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Prompts the user for a valid integer.
    /// Repeats until a valid number is entered.
    /// </summary>
    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (int.TryParse(Console.ReadLine(), out int result))
                return result;

            PrintError("Please enter a valid number.");
        }
    }

    /// <summary>
    /// Prompts the user for a valid date in format dd.MM.yyyy.
    /// </summary>
    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt} (dd.MM.yyyy): ");
            if (DateTime.TryParseExact(
                    Console.ReadLine(),
                    "dd.MM.yyyy",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime result))
                return result;

            PrintError("Invalid date format. Use dd.MM.yyyy (e.g. 01.08.2024).");
        }
    }

    /// <summary>
    /// Waits for the user to press any key before continuing.
    /// Useful between menu screens.
    /// </summary>
    public static void PressAnyKey()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    /// <summary>
    /// Asks the user for confirmation (yes/no).
    /// Returns true only if the user enters 'y'.
    /// </summary>
    public static bool Confirm(string prompt)
    {
        Console.Write($"  {prompt} (y/n): ");
        return Console.ReadLine()?.Trim().ToLower() == "y";
    }
}