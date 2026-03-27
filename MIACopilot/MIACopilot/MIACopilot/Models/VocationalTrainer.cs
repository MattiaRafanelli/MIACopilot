namespace MIACopilot.Models;

/// <summary>
/// Represents a vocational trainer responsible for apprentices.
/// </summary>
public class VocationalTrainer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int CompanyId { get; set; }

    // ── Login credentials ─────────────────────────────────────────────────
    public string Username { get; set; } = string.Empty;
    public string Pin      { get; set; } = "0000"; // 4-digit PIN

    /// <summary>Returns full name of the trainer.</summary>
    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() =>
        $"[{Id}] {FullName} | Email: {Email} | Phone: {Phone}";
}
