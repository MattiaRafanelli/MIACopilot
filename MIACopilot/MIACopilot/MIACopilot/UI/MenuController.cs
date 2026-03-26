using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.UI;

/// <summary>
/// Main application menu controller — handles all navigation and user interaction.
/// </summary>
public class MenuController
{
    private readonly ApprenticeService _apprenticeService;
    private readonly CompanyService _companyService;
    private readonly VocationalTrainerService _trainerService;

    public MenuController(
        ApprenticeService apprenticeService,
        CompanyService companyService,
        VocationalTrainerService trainerService)
    {
        _apprenticeService = apprenticeService;
        _companyService = companyService;
        _trainerService = trainerService;
    }

    // ── Main Menu ────────────────────────────────────────────────────────────

    /// <summary>Runs the main application loop.</summary>
    public void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("MIA Copilot — Apprentice Management System");
            Console.WriteLine("  [1] Manage Apprentices");
            Console.WriteLine("  [2] Manage Companies");
            Console.WriteLine("  [3] Manage Vocational Trainers");
            Console.WriteLine("  [4] Search & Filter");
            Console.WriteLine("  [0] Exit");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1": ApprenticeMenu(); break;
                case "2": CompanyMenu(); break;
                case "3": TrainerMenu(); break;
                case "4": SearchMenu(); break;
                case "0":
                    ConsoleHelper.PrintSuccess("Goodbye!");
                    return;
                default:
                    ConsoleHelper.PrintError("Invalid option. Please choose 0–4.");
                    ConsoleHelper.PressAnyKey();
                    break;
            }
        }
    }

    // ── Apprentice Menu ──────────────────────────────────────────────────────

    private void ApprenticeMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Apprentices");
            Console.WriteLine("  [1] List all");
            Console.WriteLine("  [2] Add new");
            Console.WriteLine("  [3] Edit");
            Console.WriteLine("  [4] Delete");
            Console.WriteLine("  [5] Manage Work Journals");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1": ListApprentices(); break;
                case "2": AddApprentice(); break;
                case "3": EditApprentice(); break;
                case "4": DeleteApprentice(); break;
                case "5": WorkJournalMenu(); break;
                case "0": return;
                default:
                    ConsoleHelper.PrintError("Invalid option.");
                    break;
            }
            ConsoleHelper.PressAnyKey();
        }
    }

    private void ListApprentices()
    {
        ConsoleHelper.PrintHeader("All Apprentices");
        var list = _apprenticeService.GetAll();
        if (!list.Any()) { ConsoleHelper.PrintWarning("No apprentices found."); return; }
        foreach (var a in list) Console.WriteLine($"  {a}");
    }

    private void AddApprentice()
    {
        ConsoleHelper.PrintHeader("Add Apprentice");
        var a = new Apprentice
        {
            FirstName = ConsoleHelper.ReadRequiredString("First Name"),
            LastName = ConsoleHelper.ReadRequiredString("Last Name"),
            Email = ConsoleHelper.ReadRequiredString("Email"),
            StartDate = ConsoleHelper.ReadDate("Start Date"),
            CompanyId = ConsoleHelper.ReadInt("Company ID"),
            VocationalTrainerId = ConsoleHelper.ReadInt("Trainer ID")
        };
        _apprenticeService.Create(a);
        ConsoleHelper.PrintSuccess($"Apprentice '{a.FullName}' created with ID {a.Id}.");
    }

    private void EditApprentice()
    {
        ConsoleHelper.PrintHeader("Edit Apprentice");
        var id = ConsoleHelper.ReadInt("Apprentice ID to edit");
        var existing = _apprenticeService.GetById(id);
        if (existing == null) { ConsoleHelper.PrintError("Apprentice not found."); return; }

        Console.WriteLine($"  Editing: {existing}");
        existing.FirstName = ConsoleHelper.ReadRequiredString("New First Name");
        existing.LastName = ConsoleHelper.ReadRequiredString("New Last Name");
        existing.Email = ConsoleHelper.ReadRequiredString("New Email");
        existing.StartDate = ConsoleHelper.ReadDate("New Start Date");
        existing.CompanyId = ConsoleHelper.ReadInt("New Company ID");
        existing.VocationalTrainerId = ConsoleHelper.ReadInt("New Trainer ID");

        _apprenticeService.Update(existing);
        ConsoleHelper.PrintSuccess("Apprentice updated.");
    }

    private void DeleteApprentice()
    {
        ConsoleHelper.PrintHeader("Delete Apprentice");
        var id = ConsoleHelper.ReadInt("Apprentice ID to delete");
        var existing = _apprenticeService.GetById(id);
        if (existing == null) { ConsoleHelper.PrintError("Apprentice not found."); return; }

        if (!ConsoleHelper.Confirm($"Delete '{existing.FullName}'?")) return;
        _apprenticeService.Delete(id);
        ConsoleHelper.PrintSuccess("Apprentice deleted.");
    }

    // ── Work Journal Menu ────────────────────────────────────────────────────

    private void WorkJournalMenu()
    {
        ConsoleHelper.PrintHeader("Work Journal Management");
        var appId = ConsoleHelper.ReadInt("Apprentice ID");
        var apprentice = _apprenticeService.GetById(appId);
        if (apprentice == null) { ConsoleHelper.PrintError("Apprentice not found."); return; }

        while (true)
        {
            ConsoleHelper.PrintHeader($"Journals for {apprentice.FullName}");
            Console.WriteLine("  [1] List journals");
            Console.WriteLine("  [2] Add journal");
            Console.WriteLine("  [3] Edit journal");
            Console.WriteLine("  [4] Delete journal");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1":
                    if (!apprentice.WorkJournals.Any())
                        ConsoleHelper.PrintWarning("No journals found.");
                    else
                        apprentice.WorkJournals.ForEach(j => Console.WriteLine($"  {j}"));
                    break;
                case "2":
                    var newJ = new WorkJournal
                    {
                        Date = ConsoleHelper.ReadDate("Date"),
                        WeekNumber = ConsoleHelper.ReadInt("Week Number"),
                        Title = ConsoleHelper.ReadRequiredString("Title"),
                        Content = ConsoleHelper.ReadRequiredString("Content")
                    };
                    _apprenticeService.AddJournal(appId, newJ);
                    ConsoleHelper.PrintSuccess("Journal entry added.");
                    break;
                case "3":
                    var jid = ConsoleHelper.ReadInt("Journal ID to edit");
                    var jEdit = apprentice.WorkJournals.FirstOrDefault(j => j.Id == jid);
                    if (jEdit == null) { ConsoleHelper.PrintError("Journal not found."); break; }
                    jEdit.Date = ConsoleHelper.ReadDate("New Date");
                    jEdit.WeekNumber = ConsoleHelper.ReadInt("New Week Number");
                    jEdit.Title = ConsoleHelper.ReadRequiredString("New Title");
                    jEdit.Content = ConsoleHelper.ReadRequiredString("New Content");
                    _apprenticeService.UpdateJournal(appId, jEdit);
                    ConsoleHelper.PrintSuccess("Journal updated.");
                    break;
                case "4":
                    var delId = ConsoleHelper.ReadInt("Journal ID to delete");
                    _apprenticeService.DeleteJournal(appId, delId);
                    ConsoleHelper.PrintSuccess("Journal deleted.");
                    break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
            ConsoleHelper.PressAnyKey();
        }
    }

    // ── Company Menu ─────────────────────────────────────────────────────────

    private void CompanyMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Companies");
            Console.WriteLine("  [1] List all");
            Console.WriteLine("  [2] Add new");
            Console.WriteLine("  [3] Edit");
            Console.WriteLine("  [4] Delete");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1":
                    var list = _companyService.GetAll();
                    if (!list.Any()) ConsoleHelper.PrintWarning("No companies found.");
                    else list.ForEach(c => Console.WriteLine($"  {c}"));
                    break;
                case "2":
                    var c = new Company
                    {
                        Name = ConsoleHelper.ReadRequiredString("Company Name"),
                        Address = ConsoleHelper.ReadRequiredString("Address"),
                        Phone = ConsoleHelper.ReadOptionalString("Phone"),
                        Email = ConsoleHelper.ReadOptionalString("Email"),
                        Industry = ConsoleHelper.ReadRequiredString("Industry")
                    };
                    _companyService.Create(c);
                    ConsoleHelper.PrintSuccess($"Company '{c.Name}' created with ID {c.Id}.");
                    break;
                case "3":
                    var editId = ConsoleHelper.ReadInt("Company ID to edit");
                    var existing = _companyService.GetById(editId);
                    if (existing == null) { ConsoleHelper.PrintError("Company not found."); break; }
                    existing.Name = ConsoleHelper.ReadRequiredString("New Name");
                    existing.Address = ConsoleHelper.ReadRequiredString("New Address");
                    existing.Phone = ConsoleHelper.ReadOptionalString("New Phone");
                    existing.Email = ConsoleHelper.ReadOptionalString("New Email");
                    existing.Industry = ConsoleHelper.ReadRequiredString("New Industry");
                    _companyService.Update(existing);
                    ConsoleHelper.PrintSuccess("Company updated.");
                    break;
                case "4":
                    var delId = ConsoleHelper.ReadInt("Company ID to delete");
                    var comp = _companyService.GetById(delId);
                    if (comp == null) { ConsoleHelper.PrintError("Company not found."); break; }
                    if (ConsoleHelper.Confirm($"Delete '{comp.Name}'?"))
                    {
                        _companyService.Delete(delId);
                        ConsoleHelper.PrintSuccess("Company deleted.");
                    }
                    break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
            ConsoleHelper.PressAnyKey();
        }
    }

    // ── Trainer Menu ─────────────────────────────────────────────────────────

    private void TrainerMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Vocational Trainers");
            Console.WriteLine("  [1] List all");
            Console.WriteLine("  [2] Add new");
            Console.WriteLine("  [3] Edit");
            Console.WriteLine("  [4] Delete");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1":
                    var list = _trainerService.GetAll();
                    if (!list.Any()) ConsoleHelper.PrintWarning("No trainers found.");
                    else list.ForEach(t => Console.WriteLine($"  {t}"));
                    break;
                case "2":
                    var t = new VocationalTrainer
                    {
                        FirstName = ConsoleHelper.ReadRequiredString("First Name"),
                        LastName = ConsoleHelper.ReadRequiredString("Last Name"),
                        Email = ConsoleHelper.ReadRequiredString("Email"),
                        Phone = ConsoleHelper.ReadOptionalString("Phone"),
                        CompanyId = ConsoleHelper.ReadInt("Company ID")
                    };
                    _trainerService.Create(t);
                    ConsoleHelper.PrintSuccess($"Trainer '{t.FullName}' created with ID {t.Id}.");
                    break;
                case "3":
                    var editId = ConsoleHelper.ReadInt("Trainer ID to edit");
                    var existing = _trainerService.GetById(editId);
                    if (existing == null) { ConsoleHelper.PrintError("Trainer not found."); break; }
                    existing.FirstName = ConsoleHelper.ReadRequiredString("New First Name");
                    existing.LastName = ConsoleHelper.ReadRequiredString("New Last Name");
                    existing.Email = ConsoleHelper.ReadRequiredString("New Email");
                    existing.Phone = ConsoleHelper.ReadOptionalString("New Phone");
                    existing.CompanyId = ConsoleHelper.ReadInt("New Company ID");
                    _trainerService.Update(existing);
                    ConsoleHelper.PrintSuccess("Trainer updated.");
                    break;
                case "4":
                    var delId = ConsoleHelper.ReadInt("Trainer ID to delete");
                    var trainer = _trainerService.GetById(delId);
                    if (trainer == null) { ConsoleHelper.PrintError("Trainer not found."); break; }
                    if (ConsoleHelper.Confirm($"Delete '{trainer.FullName}'?"))
                    {
                        _trainerService.Delete(delId);
                        ConsoleHelper.PrintSuccess("Trainer deleted.");
                    }
                    break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
            ConsoleHelper.PressAnyKey();
        }
    }

    // ── Search & Filter Menu ─────────────────────────────────────────────────

    private void SearchMenu()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Search & Filter");
            Console.WriteLine("  [1] Search apprentices by name");
            Console.WriteLine("  [2] Filter apprentices by company");
            Console.WriteLine("  [3] Search companies by name");
            Console.WriteLine("  [4] Filter companies by industry");
            Console.WriteLine("  [5] Search trainers by name");
            Console.WriteLine("  [0] Back");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadRequiredString("Your choice");
            switch (choice)
            {
                case "1":
                    var nameQ = ConsoleHelper.ReadRequiredString("Search name");
                    var results = _apprenticeService.SearchByName(nameQ);
                    PrintSearchResults(results, "apprentice");
                    break;
                case "2":
                    var compId = ConsoleHelper.ReadInt("Company ID");
                    var byComp = _apprenticeService.FilterByCompany(compId);
                    PrintSearchResults(byComp, "apprentice");
                    break;
                case "3":
                    var compQ = ConsoleHelper.ReadRequiredString("Search company name");
                    var companies = _companyService.SearchByName(compQ);
                    PrintSearchResults(companies, "company");
                    break;
                case "4":
                    var industry = ConsoleHelper.ReadRequiredString("Industry");
                    var byIndustry = _companyService.FilterByIndustry(industry);
                    PrintSearchResults(byIndustry, "company");
                    break;
                case "5":
                    var trainerQ = ConsoleHelper.ReadRequiredString("Search trainer name");
                    var trainers = _trainerService.SearchByName(trainerQ);
                    PrintSearchResults(trainers, "trainer");
                    break;
                case "0": return;
                default: ConsoleHelper.PrintError("Invalid option."); break;
            }
            ConsoleHelper.PressAnyKey();
        }
    }

    /// <summary>Prints a list of search results or a 'no results' message.</summary>
    private void PrintSearchResults<T>(List<T> results, string entityName)
    {
        if (!results.Any())
        {
            ConsoleHelper.PrintWarning($"No {entityName}s found.");
            return;
        }
        ConsoleHelper.PrintSuccess($"{results.Count} result(s) found:");
        foreach (var item in results) Console.WriteLine($"  {item}");
    }
}
