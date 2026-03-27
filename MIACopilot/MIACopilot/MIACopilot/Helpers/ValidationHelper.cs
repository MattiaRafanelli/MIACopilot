namespace MIACopilot.Helpers;

/// <summary>
/// Helper methods for common validation and string cleanup logic.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Checks whether a string value is required and not null, empty or whitespace.
    /// </summary>
    public static bool Required(string? value) =>
        !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Validates that a grade is within the allowed range (1.0 – 6.0).
    /// </summary>
    public static bool GradeInRange(double grade) =>
        grade >= 1.0 && grade <= 6.0;

    /// <summary>
    /// Validates that a weight (in percent) is between 0 and 100.
    /// </summary>
    public static bool WeightInRange(double weightPercent) =>
        weightPercent >= 0 && weightPercent <= 100;

    /// <summary>
    /// Trims a string or returns an empty string if the value is null.
    /// Useful for safe assignments.
    /// </summary>
    public static string TrimOrEmpty(string? s) =>
        (s ?? string.Empty).Trim();
}