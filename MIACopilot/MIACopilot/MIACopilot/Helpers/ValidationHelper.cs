namespace MIACopilot.Helpers;

public static class ValidationHelper
{
    public static bool Required(string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static bool GradeInRange(double grade) =>
        grade >= 1.0 && grade <= 6.0;

    public static bool WeightInRange(double weightPercent) =>
        weightPercent >= 0 && weightPercent <= 100;

    public static string TrimOrEmpty(string? s) =>
        (s ?? string.Empty).Trim();
}
