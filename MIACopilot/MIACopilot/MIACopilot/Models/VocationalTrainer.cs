namespace MIACopilot.Models;

/// <summary>
/// Represents a vocational trainer responsible for apprentices.
/// Contains personal information and login credentials.
/// </summary>
public class VocationalTrainer
{
    // Unique identifier of the trainer
    public int Id { get; set; }

    // Personal information
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Phone     { get; set; } = string.Empty;

    // Reference to the company the trainer belongs to
    public int CompanyId { get; set; }

    // ── Login credentials ─────────────────────────────────────────────────

    // Username used for authentication
    public string Username { get; set; } = string.Empty;

    // 4-digit PIN for login (default value for new trainers)
    public string Pin { get; set; } = "0000";

    /// <summary>
    /// Returns the full name of the trainer.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Returns a readable string representation of the trainer.
    /// Useful for debugging and logging.
    /// </summary>
    public override string ToString() =>
        $"[{Id}] {FullName} | Email: {Email} | Phone: {Phone}";
}
