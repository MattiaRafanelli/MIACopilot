namespace MIACopilot.Models;

/// <summary>
/// Represents a work journal entry written by an apprentice.
/// Used to document weekly learning progress and activities.
/// </summary>
public class WorkJournal
{
    // Unique identifier of the journal entry
    public int Id { get; set; }

    // Reference to the apprentice who wrote this journal
    public int ApprenticeId { get; set; }

    // Date of the journal entry
    public DateTime Date { get; set; }

    // Short title summarizing the week or topic
    public string Title { get; set; } = string.Empty;

    // Full journal content (free text)
    public string Content { get; set; } = string.Empty;

    // Calendar week number of the entry
    public int WeekNumber { get; set; }

    /// <summary>
    /// Returns a readable string representation of the journal entry.
    /// Useful for debugging and logging.
    /// </summary>
    public override string ToString() =>
        $"[{Id}] Week {WeekNumber} | {Date:dd.MM.yyyy} | {Title}";
}
