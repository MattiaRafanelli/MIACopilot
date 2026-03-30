using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Provides CRUD operations and search/filter functionality for apprentices.
/// Also manages work journal entries belonging to apprentices.
/// </summary>
public class ApprenticeService
{
    // Underlying persistence service (load/save)
    private readonly DataService _dataService;

    // In-memory list of all apprentices
    private List<Apprentice> _apprentices;

    /// <summary>
    /// Initializes the service and loads apprentices from storage.
    /// </summary>
    public ApprenticeService(DataService dataService)
    {
        _dataService = dataService;
        _apprentices = _dataService.LoadApprentices();
    }

    /// <summary>
    /// Returns all apprentices.
    /// </summary>
    public List<Apprentice> GetAll() => _apprentices;

    /// <summary>
    /// Finds an apprentice by its unique ID.
    /// Returns null if not found.
    /// </summary>
    public Apprentice? GetById(int id) =>
        _apprentices.FirstOrDefault(a => a.Id == id);

    /// <summary>
    /// Creates a new apprentice, assigns a new incremental ID,
    /// and persists the updated list.
    /// </summary>
    public void Create(Apprentice apprentice)
    {
        apprentice.Id = _apprentices.Any()
            ? _apprentices.Max(a => a.Id) + 1
            : 1;

        _apprentices.Add(apprentice);
        _dataService.SaveApprentices(_apprentices);
    }

    /// <summary>
    /// Updates an existing apprentice and persists the changes.
    /// Returns false if the apprentice does not exist.
    /// </summary>
    public bool Update(Apprentice updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) return false;

        existing.FirstName           = updated.FirstName;
        existing.LastName            = updated.LastName;
        existing.Email               = updated.Email;
        existing.StartDate           = updated.StartDate;
        existing.CompanyId           = updated.CompanyId;
        existing.VocationalTrainerId = updated.VocationalTrainerId;
        existing.Username            = updated.Username;
        existing.Pin                 = updated.Pin;

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>
    /// Deletes an apprentice by ID and persists the change.
    /// Returns false if the apprentice does not exist.
    /// </summary>
    public bool Delete(int id)
    {
        var apprentice = GetById(id);
        if (apprentice == null) return false;

        _apprentices.Remove(apprentice);
        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>
    /// Searches apprentices by full name (case-insensitive).
    /// </summary>
    public List<Apprentice> SearchByName(string query) =>
        _apprentices
            .Where(a => a.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>
    /// Returns all apprentices belonging to a specific company.
    /// </summary>
    public List<Apprentice> FilterByCompany(int companyId) =>
        _apprentices.Where(a => a.CompanyId == companyId).ToList();


    // ── WorkJournal sub-operations ──────────────────────────────────────────

    /// <summary>
    /// Adds a new work journal entry to the given apprentice.
    /// Automatically assigns a journal ID.
    /// </summary>
    public bool AddJournal(int apprenticeId, WorkJournal journal)
    {
        var apprentice = GetById(apprenticeId);
        if (apprentice == null) return false;

        journal.Id = apprentice.WorkJournals.Any()
            ? apprentice.WorkJournals.Max(j => j.Id) + 1
            : 1;

        journal.ApprenticeId = apprenticeId;
        apprentice.WorkJournals.Add(journal);

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>
    /// Updates an existing work journal entry of an apprentice.
    /// </summary>
    public bool UpdateJournal(int apprenticeId, WorkJournal updated)
    {
        var apprentice = GetById(apprenticeId);
        if (apprentice == null) return false;

        var journal = apprentice.WorkJournals.FirstOrDefault(j => j.Id == updated.Id);
        if (journal == null) return false;

        journal.Date       = updated.Date;
        journal.Title      = updated.Title;
        journal.Content    = updated.Content;
        journal.WeekNumber = updated.WeekNumber;

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>
    /// Deletes a work journal entry from an apprentice.
    /// </summary>
    public bool DeleteJournal(int apprenticeId, int journalId)
    {
        var apprentice = GetById(apprenticeId);
        if (apprentice == null) return false;

        var journal = apprentice.WorkJournals.FirstOrDefault(j => j.Id == journalId);
        if (journal == null) return false;

        apprentice.WorkJournals.Remove(journal);
        _dataService.SaveApprentices(_apprentices);
        return true;
    }
}