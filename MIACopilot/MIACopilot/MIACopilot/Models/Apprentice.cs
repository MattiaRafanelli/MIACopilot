namespace MIACopilot.Models;

/// <summary>
/// Represents an apprentice in the management system.
/// Holds personal data, company/trainer assignment,
/// work journals and login credentials.
/// </summary>
public class Apprentice
{
    // Unique identifier of the apprentice
    public int Id { get; set; }

    // Personal information
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;

    // Apprenticeship start date
    public DateTime StartDate { get; set; }

    // Foreign key references
    public int CompanyId           { get; set; }
    public int VocationalTrainerId { get; set; }

    // Collection of weekly work journal entries
    public List<WorkJournal> WorkJournals { get; set; } = new();

    // ── Login credentials ─────────────────────────────────────────────────

    // Username used for login
    public string Username { get; set; } = string.Empty;

    // 4-digit PIN for authentication (default for new apprentices)
    public string Pin { get; set; } = "0000";

    /// <summary>
    /// Returns the full name of the apprentice.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Returns a readable string representation of the apprentice.
    /// Useful for debugging and logging.
    /// </summary>
    public override string ToString() =>
        $"[{Id}] {FullName} | Email: {Email} | Start: {StartDate:dd.MM.yyyy}";
}
