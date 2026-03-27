using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Provides CRUD operations and search for vocational trainers.
/// </summary>
public class VocationalTrainerService
{
    private readonly DataService _dataService;
    private List<VocationalTrainer> _trainers;

    public VocationalTrainerService(DataService dataService)
    {
        _dataService = dataService;
        _trainers = _dataService.LoadTrainers();
    }

    /// <summary>Returns all trainers.</summary>
    public List<VocationalTrainer> GetAll() => _trainers;

    /// <summary>Finds a trainer by ID.</summary>
    public VocationalTrainer? GetById(int id) =>
        _trainers.FirstOrDefault(t => t.Id == id);

    /// <summary>Creates a new trainer and persists data.</summary>
    public void Create(VocationalTrainer trainer)
    {
        trainer.Id = _trainers.Any() ? _trainers.Max(t => t.Id) + 1 : 1;
        _trainers.Add(trainer);
        _dataService.SaveTrainers(_trainers);
    }

    /// <summary>Updates an existing trainer and persists data.</summary>
    public bool Update(VocationalTrainer updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) return false;

        existing.FirstName = updated.FirstName;
        existing.LastName  = updated.LastName;
        existing.Email     = updated.Email;
        existing.Phone     = updated.Phone;
        existing.CompanyId = updated.CompanyId;
        existing.Username  = updated.Username;

        _dataService.SaveTrainers(_trainers);
        return true;
    }

    /// <summary>Deletes a trainer by ID and persists data.</summary>
    public bool Delete(int id)
    {
        var trainer = GetById(id);
        if (trainer == null) return false;

        _trainers.Remove(trainer);
        _dataService.SaveTrainers(_trainers);
        return true;
    }

    /// <summary>Searches trainers by name (case-insensitive).</summary>
    public List<VocationalTrainer> SearchByName(string query) =>
        _trainers.Where(t =>
            t.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
}
