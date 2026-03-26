namespace MIACopilot.Models;

/// <summary>
/// Represents a work journal entry written by an apprentice.
/// </summary>
public class WorkJournal
{
    public int Id { get; set; }
    public int ApprenticeId { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int WeekNumber { get; set; }

    public override string ToString() =>
        $"[{Id}] Week {WeekNumber} | {Date:dd.MM.yyyy} | {Title}";
}
