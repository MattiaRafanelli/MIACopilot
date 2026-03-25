using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Provides CRUD operations and search/filter for apprentices.
/// </summary>
public class ApprenticeService
{
    private readonly DataService _dataService;
    private List<Apprentice> _apprentices;

    public ApprenticeService(DataService dataService)
    {
        _dataService = dataService;
        _apprentices = _dataService.LoadApprentices();
    }

    /// <summary>Returns all apprentices.</summary>
    public List<Apprentice> GetAll() => _apprentices;

    /// <summary>Finds an apprentice by ID.</summary>
    public Apprentice? GetById(int id) =>
        _apprentices.FirstOrDefault(a => a.Id == id);

    /// <summary>Creates a new apprentice and persists data.</summary>
    public void Create(Apprentice apprentice)
    {
        apprentice.Id = _apprentices.Any() ? _apprentices.Max(a => a.Id) + 1 : 1;
        _apprentices.Add(apprentice);
        _dataService.SaveApprentices(_apprentices);
    }

    /// <summary>Updates an existing apprentice and persists data.</summary>
    public bool Update(Apprentice updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) return false;

        existing.FirstName = updated.FirstName;
        existing.LastName = updated.LastName;
        existing.Email = updated.Email;
        existing.StartDate = updated.StartDate;
        existing.CompanyId = updated.CompanyId;
        existing.VocationalTrainerId = updated.VocationalTrainerId;

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>Deletes an apprentice by ID and persists data.</summary>
    public bool Delete(int id)
    {
        var apprentice = GetById(id);
        if (apprentice == null) return false;

        _apprentices.Remove(apprentice);
        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>Searches apprentices by name (case-insensitive).</summary>
    public List<Apprentice> SearchByName(string query) =>
        _apprentices.Where(a =>
            a.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>Filters apprentices by company ID.</summary>
    public List<Apprentice> FilterByCompany(int companyId) =>
        _apprentices.Where(a => a.CompanyId == companyId).ToList();

    // ── WorkJournal sub-operations ──────────────────────────────────────────

    /// <summary>Adds a work journal entry to an apprentice.</summary>
    public bool AddJournal(int apprenticeId, WorkJournal journal)
    {
        var apprentice = GetById(apprenticeId);
        if (apprentice == null) return false;

        journal.Id = apprentice.WorkJournals.Any()
            ? apprentice.WorkJournals.Max(j => j.Id) + 1 : 1;
        journal.ApprenticeId = apprenticeId;
        apprentice.WorkJournals.Add(journal);

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>Updates an existing work journal entry.</summary>
    public bool UpdateJournal(int apprenticeId, WorkJournal updated)
    {
        var apprentice = GetById(apprenticeId);
        if (apprentice == null) return false;

        var journal = apprentice.WorkJournals.FirstOrDefault(j => j.Id == updated.Id);
        if (journal == null) return false;

        journal.Date = updated.Date;
        journal.Title = updated.Title;
        journal.Content = updated.Content;
        journal.WeekNumber = updated.WeekNumber;

        _dataService.SaveApprentices(_apprentices);
        return true;
    }

    /// <summary>Deletes a work journal entry from an apprentice.</summary>
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
