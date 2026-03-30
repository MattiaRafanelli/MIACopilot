using System;
using MIACopilot.Services;

namespace MIACopilot.UI
{
    /// <summary>
    /// Main application menu controller — handles navigation and user interaction.
    /// </summary>
    public class MenuController
    {
        private readonly ApprenticeService _apprenticeService;
        private readonly CompanyService _companyService;

        public MenuController(
            ApprenticeService apprenticeService,
            CompanyService companyService)
        {
            _apprenticeService = apprenticeService;
            _companyService = companyService;
        }

        /// <summary>
        /// Runs the main application loop.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("MIA Copilot — Apprentice Management System");

                Console.WriteLine(" [1] Manage Apprentices");
                Console.WriteLine(" [2] Manage Companies");
                Console.WriteLine(" [0] Exit");
                Console.WriteLine();

                var choice = ConsoleHelper.ReadRequiredString("Your choice");

                switch (choice)
                {
                    case "1":
                        ApprenticeMenu();
                        break;

                    case "2":
                        CompanyMenu();
                        break;

                    case "0":
                        ConsoleHelper.PrintSuccess("Goodbye!");
                        return;

                    default:
                        ConsoleHelper.PrintError("Invalid option.");
                        ConsoleHelper.PressAnyKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Apprentice management menu. Lists all apprentices to the console.
        /// </summary>
        private void ApprenticeMenu()
        {
            ConsoleHelper.PrintHeader("Apprentices");
            foreach (var a in _apprenticeService.GetAll())
                Console.WriteLine($"  [{a.Id}] {a.FullName}");
            ConsoleHelper.PressAnyKey();
        }

        /// <summary>
        /// Company management menu. Lists all companies to the console.
        /// </summary>
        private void CompanyMenu()
        {
            ConsoleHelper.PrintHeader("Companies");
            foreach (var c in _companyService.GetAll())
                Console.WriteLine($"  [{c.Id}] {c.Name}");
            ConsoleHelper.PressAnyKey();
        }
    }
}