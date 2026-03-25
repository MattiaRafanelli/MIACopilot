namespace MIACopilot.UI;

/// <summary>
/// Helper methods for consistent, readable console output.
/// </summary>
public static class ConsoleHelper
{
    /// <summary>Prints a styled section header.</summary>
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 50));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('═', 50));
        Console.ResetColor();
    }

    /// <summary>Prints a success message in green.</summary>
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    /// <summary>Prints an error message in red.</summary>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    /// <summary>Prints a warning message in yellow.</summary>
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>Prompts user for a non-empty string input.</summary>
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value)) return value;
            PrintError("This field cannot be empty. Please try again.");
        }
    }

    /// <summary>Prompts user for an optional string (can be empty).</summary>
    public static string ReadOptionalString(string prompt)
    {
        Console.Write($"  {prompt}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    /// <summary>Prompts user for a valid integer input.</summary>
    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            if (int.TryParse(Console.ReadLine(), out int result)) return result;
            PrintError("Please enter a valid number.");
        }
    }

    /// <summary>Prompts user for a valid date input.</summary>
    public static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt} (dd.MM.yyyy): ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy",
                null, System.Globalization.DateTimeStyles.None, out DateTime result))
                return result;
            PrintError("Invalid date format. Use dd.MM.yyyy (e.g. 01.08.2024).");
        }
    }

    /// <summary>Waits for the user to press a key before continuing.</summary>
    public static void PressAnyKey()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    /// <summary>Asks for confirmation (y/n).</summary>
    public static bool Confirm(string prompt)
    {
        Console.Write($"  {prompt} (y/n): ");
        return Console.ReadLine()?.Trim().ToLower() == "y";
    }
}
