# MIACopilot

Apprentice Management System — .NET 10 Console Application

## GitHub Repository

[MIACopilot Repository](https://github.com/YOUR_USERNAME/MIACopilot)

## Features

- Manage **Apprentices** (CRUD + Work Journals)
- Manage **Companies** (CRUD + industry filter)
- Manage **Vocational Trainers** (CRUD)
- **Search & Filter** across all entities
- Local **JSON persistence** (auto-loaded on startup)
- Basic **error handling** throughout

## Project Structure

```
MIACopilot/
├── Models/
│   ├── Apprentice.cs
│   ├── WorkJournal.cs
│   ├── Company.cs
│   └── VocationalTrainer.cs
├── Services/
│   ├── DataService.cs
│   ├── ApprenticeService.cs
│   ├── CompanyService.cs
│   └── VocationalTrainerService.cs
├── UI/
│   ├── ConsoleHelper.cs
│   └── MenuController.cs
├── Program.cs
└── MIACopilot.csproj
```

## Run

```bash
dotnet run
```
