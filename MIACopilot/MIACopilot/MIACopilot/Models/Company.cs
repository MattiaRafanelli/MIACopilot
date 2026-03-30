namespace MIACopilot.Models;

/// <summary>
/// Represents a company that hosts apprentices.
/// Stores basic company contact and classification information.
/// </summary>
public class Company
{
    // Unique identifier of the company
    public int Id { get; set; }

    // Company name
    public string Name { get; set; } = string.Empty;

    // Physical address of the company
    public string Address { get; set; } = string.Empty;

    // Contact phone number
    public string Phone { get; set; } = string.Empty;

    // Contact email address
    public string Email { get; set; } = string.Empty;

    // Industry or business sector
    public string Industry { get; set; } = string.Empty;

    // Company Admin login credentials
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPin      { get; set; } = string.Empty;

    /// <summary>
    /// Returns a readable string representation of the company.
    /// Useful for debugging and logging.
    /// </summary>
    public override string ToString() =>
        $"[{Id}] {Name} | {Industry} | {Address}";
}
