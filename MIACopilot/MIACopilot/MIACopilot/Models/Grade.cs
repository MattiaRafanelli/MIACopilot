namespace MIACopilot.Models;

/// <summary>
/// Represents a grade entry for an apprentice in a specific subject.
/// Includes value, metadata and helper properties for display.
/// </summary>
public class Grade
{
    // Unique identifier of the grade entry
    public int Id { get; set; }

    // Reference to the apprentice this grade belongs to
    public int ApprenticeId { get; set; }

    // Subject or module name
    public string Subject { get; set; } = string.Empty;

    // Numeric grade value (Swiss grading scale: 1.0 – 6.0)
    public double Value { get; set; }

    // Date when the grade was given
    public DateTime Date { get; set; }

    // Type of assessment (e.g. Test, Exam, Project)
    public string Type { get; set; } = string.Empty;

    // Optional notes or remarks
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Returns the grade formatted with one decimal place.
    /// Used for consistent UI display.
    /// </summary>
    public string FormattedValue => Value.ToString("0.0");

    /// <summary>
    /// Returns true when the grade is a passing grade (≥ 4.0).
    /// </summary>
    public bool IsPassing => Value >= 4.0;

    /// <summary>
    /// Returns the textual classification based on the Swiss grading scale:
    /// 5.6–6.0 = Excellent, 5.0–5.5 = Great, 4.0–4.9 = Sufficient, 1.0–3.9 = Insufficient.
    /// </summary>
    public string Category =>
        Value >= 5.6 ? "Excellent"
      : Value >= 5.0 ? "Great"
      : Value >= 4.0 ? "Sufficient"
      :               "Insufficient";

    /// <summary>
    /// Returns a readable string representation of the grade.
    /// Useful for debugging and logging.
    /// </summary>
    public override string ToString() =>
        $"[{Id}] {Subject} | {FormattedValue} | {Type} | {Date:dd.MM.yyyy}";
}
