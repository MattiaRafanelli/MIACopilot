using MIACopilot.Models;
using MIACopilot.Services;

var dataService = new DataService();
dataService.Load();

bool running = true;

while (running)
{
    Console.Clear();
    Console.WriteLine("=== Apprentice Management ===");
    Console.WriteLine("1. Apprentice erstellen");
    Console.WriteLine("2. WorkJournal erstellen");
    Console.WriteLine("3. Alle Apprentices anzeigen");
    Console.WriteLine("4. Beenden");
    Console.Write("Auswahl: ");

    switch (Console.ReadLine())
    {
        case "1":
            CreateApprentice();
            break;
        case "2":
            CreateWorkJournal();
            break;
        case "3":
            ShowApprentices();
            break;
        case "4":
            running = false;
            break;
        default:
            Console.WriteLine("Ungültige Eingabe!");
            Console.ReadKey();
            break;
    }
}

dataService.Save();


// Erstellt einen neuen Lernenden
void CreateApprentice()
{
    Console.Write("Name: ");
    var name = Console.ReadLine();

    Console.Write("Alter: ");
    if (!int.TryParse(Console.ReadLine(), out int age))
    {
        Console.WriteLine("Alter muss eine Zahl sein!");
        Console.ReadKey();
        return;
    }

    dataService.Apprentices.Add(new Apprentice
    {
        Id = dataService.Apprentices.Count + 1,
        Name = name ?? "",
        Age = age
    });

    dataService.Save();
}

// Erstellt ein neues Arbeitsjournal
void CreateWorkJournal()
{
    Console.Write("Apprentice ID: ");
    if (!int.TryParse(Console.ReadLine(), out int apprenticeId))
    {
        Console.WriteLine("Ungültige ID!");
        Console.ReadKey();
        return;
    }

    Console.Write("Beschreibung: ");
    var text = Console.ReadLine();

    dataService.WorkJournals.Add(new WorkJournal
    {
        Id = dataService.WorkJournals.Count + 1,
        ApprenticeId = apprenticeId,
        Date = DateTime.Now,
        Description = text ?? ""
    });

    dataService.Save();
}

// Zeigt alle Lernenden an
void ShowApprentices()
{
    foreach (var a in dataService.Apprentices)
    {
        Console.WriteLine($"{a.Id}: {a.Name} ({a.Age} Jahre)");
    }
    Console.ReadKey();
}
``
