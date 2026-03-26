using MIACopilot.Models;

namespace MIACopilot.Services;

/// <summary>
/// Provides CRUD operations and search for companies.
/// </summary>
public class CompanyService
{
    private readonly DataService _dataService;
    private List<Company> _companies;

    public CompanyService(DataService dataService)
    {
        _dataService = dataService;
        _companies = _dataService.LoadCompanies();
    }

    /// <summary>Returns all companies.</summary>
    public List<Company> GetAll() => _companies;

    /// <summary>Finds a company by ID.</summary>
    public Company? GetById(int id) =>
        _companies.FirstOrDefault(c => c.Id == id);

    /// <summary>Creates a new company and persists data.</summary>
    public void Create(Company company)
    {
        company.Id = _companies.Any() ? _companies.Max(c => c.Id) + 1 : 1;
        _companies.Add(company);
        _dataService.SaveCompanies(_companies);
    }

    /// <summary>Updates an existing company and persists data.</summary>
    public bool Update(Company updated)
    {
        var existing = GetById(updated.Id);
        if (existing == null) return false;

        existing.Name = updated.Name;
        existing.Address = updated.Address;
        existing.Phone = updated.Phone;
        existing.Email = updated.Email;
        existing.Industry = updated.Industry;

        _dataService.SaveCompanies(_companies);
        return true;
    }

    /// <summary>Deletes a company by ID and persists data.</summary>
    public bool Delete(int id)
    {
        var company = GetById(id);
        if (company == null) return false;

        _companies.Remove(company);
        _dataService.SaveCompanies(_companies);
        return true;
    }

    /// <summary>Searches companies by name (case-insensitive).</summary>
    public List<Company> SearchByName(string query) =>
        _companies.Where(c =>
            c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>Filters companies by industry.</summary>
    public List<Company> FilterByIndustry(string industry) =>
        _companies.Where(c =>
            c.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase)).ToList();
}