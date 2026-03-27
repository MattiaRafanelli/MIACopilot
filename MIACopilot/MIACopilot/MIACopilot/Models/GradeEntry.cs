namespace MIACopilot.Models;

/// <summary>
/// Represents a single weighted grade entry for an apprentice.
/// Typically used for calculating averages (e.g. semester grades).
/// </summary>
public class GradeEntry
{
    // Unique identifier of the grade entry
    public int Id { get; set; }

    // Reference to the apprentice this grade belongs to
    public int ApprenticeId { get; set; }

    // Subject or module name
    public string Subject { get; set; } = string.Empty;

    // Numeric grade value (Swiss scale: 1.0 – 6.0)
    public double Grade { get; set; }

    // Weight of this grade in percent (0 – 100)
    public double WeightPercent { get; set; }

    // Semester number the grade belongs to
    public int Semester { get; set; }

    // Date when the grade was recorded
    public DateTime Date { get; set; } = DateTime.Today;
}