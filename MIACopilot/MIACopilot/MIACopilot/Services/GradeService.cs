using System.Text.Json;
using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Provides CRUD operations, filtering and average calculations for grades.
/// Uses a dedicated JSON file for persistence.
/// </summary>
public class GradeService
{
    // Path to the grades JSON file
    private readonly string _file;

    // JSON serialization options
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    // In-memory list of all grades
    private List<Grade> _grades;

    /// <summary>
    /// Initializes the service and loads grades from disk.
    /// </summary>
    public GradeService(DataService dataService)
    {
        _file   = Path.Combine("data", "grades.json");
        _grades = Load();
    }

    // ── Persistence ──────────────────────────────────────────────────────

    /// <summary>
    /// Loads grades from the JSON file.
    /// Returns an empty list if the file does not exist.
    /// </summary>
    private List<Grade> Load()
    {
        if (!File.Exists(_file))
            return new();

        var json = File.ReadAllText(_file);
        return JsonSerializer.Deserialize<List<Grade>>(json, _options) ?? new();
    }

    /// <summary>
    /// Saves all grades to the JSON file.
    /// </summary>
    private void Save() =>
        File.WriteAllText(_file, JsonSerializer.Serialize(_grades, _options));

    // ── CRUD ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all grades.
    /// </summary>
    public List<Grade> GetAll() => _grades;

    /// <summary>
    /// Returns all grades belonging to a specific apprentice.
    /// </summary>
    public List<Grade> GetByApprentice(int apprenticeId) =>
        _grades.Where(g => g.ApprenticeId == apprenticeId).ToList();

    /// <summary>
    /// Finds a grade by its ID.
    /// </summary>
    public Grade? GetById(int id) =>
        _grades.FirstOrDefault(g => g.Id == id);

    /// <summary>
    /// Creates a new grade, assigns a new ID and persists data.
    /// </summary>
    public void Create(Grade grade)
    {
        grade.Id = _grades.Any()
            ? _grades.Max(g => g.Id) + 1
            : 1;

        _grades.Add(grade);
        Save();
    }

    /// <summary>
    /// Updates an existing grade and persists data.
    /// Returns false if the grade does not exist.
    /// </summary>
    public bool Update(Grade updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) return false;

        existing.Subject = updated.Subject;
        existing.Value   = updated.Value;
        existing.Date    = updated.Date;
        existing.Type    = updated.Type;
        existing.Notes   = updated.Notes;

        Save();
        return true;
    }

    /// <summary>
    /// Deletes a grade by ID and persists data.
    /// Returns false if the grade does not exist.
    /// </summary>
    public bool Delete(int id)
    {
        var grade = GetById(id);
        if (grade == null) return false;

        _grades.Remove(grade);
        Save();
        return true;
    }

    // ── Analytics ─────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates the overall average grade for one apprentice.
    /// Returns 0 if no grades exist.
    /// </summary>
    public double GetAverage(int apprenticeId)
    {
        var list = GetByApprentice(apprenticeId);
        return list.Any()
            ? Math.Round(list.Average(g => g.Value), 2)
            : 0;
    }

    /// <summary>
    /// Calculates the average grade per subject for one apprentice.
    /// </summary>
    public Dictionary<string, double> GetAveragePerSubject(int apprenticeId) =>
        GetByApprentice(apprenticeId)
            .GroupBy(g => g.Subject)
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(x => x.Value), 2)
            );

    /// <summary>
    /// Returns all grades for an apprentice filtered by subject.
    /// </summary>
    public List<Grade> FilterBySubject(int apprenticeId, string subject) =>
        GetByApprentice(apprenticeId)
            .Where(g => g.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>
    /// Returns all distinct subjects for one apprentice, sorted alphabetically.
    /// </summary>
    public List<string> GetSubjects(int apprenticeId) =>
        GetByApprentice(apprenticeId)
            .Select(g => g.Subject)
            .Distinct()
            .OrderBy(s => s)
            .ToList();
}