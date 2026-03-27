namespace MIACopilot.Models;

/// <summary>
/// Represents a grade entry for an apprentice in a specific subject.
/// </summary>
public class Grade
{
    public int      Id           { get; set; }
    public int      ApprenticeId { get; set; }
    public string   Subject      { get; set; } = string.Empty;
    public double   Value        { get; set; }   // 1.0 – 6.0
    public DateTime Date         { get; set; }
    public string   Type         { get; set; } = string.Empty; // e.g. Test, Exam, Project
    public string   Notes        { get; set; } = string.Empty;

    /// <summary>Returns the grade formatted to one decimal place.</summary>
    public string FormattedValue => Value.ToString("0.0");

    /// <summary>Returns a colour category based on Swiss grading.</summary>
    public string Category => Value >= 5.0 ? "Excellent"
                            : Value >= 4.0 ? "Passed"
                            : Value >= 3.0 ? "Sufficient"
                            :                "Failed";

    public override string ToString() =>
        $"[{Id}] {Subject} | {FormattedValue} | {Type} | {Date:dd.MM.yyyy}";
}
