using MIACopilot.Services;
using MIACopilot.UI;

// Entry point — wire up services and launch the menu
try
{
    var dataService = new DataService();
    var apprenticeService = new ApprenticeService(dataService);
    var companyService = new CompanyService(dataService);
    var trainerService = new VocationalTrainerService(dataService);

    var menu = new MenuController(apprenticeService, companyService, trainerService);
    menu.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n  Fatal error: {ex.Message}");
    Console.ResetColor();
}
