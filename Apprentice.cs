namespace MIACopilot.Models;

/// <summary>
/// Represents an apprentice in the management system.
/// </summary>
public class Apprentice
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int CompanyId { get; set; }
    public int VocationalTrainerId { get; set; }
    public List<WorkJournal> WorkJournals { get; set; } = new();

    /// <summary>Returns full name of the apprentice.</summary>
    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() =>
        $"[{Id}] {FullName} | Email: {Email} | Start: {StartDate:dd.MM.yyyy}";
}
