namespace MIACopilot.Models;

/// <summary>
/// Represents a company that hosts apprentices.
/// </summary>
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;

    public override string ToString() =>
        $"[{Id}] {Name} | {Industry} | {Address}";
}
