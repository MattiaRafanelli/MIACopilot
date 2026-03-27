using MIACopilot.Forms;
using MIACopilot.Models;
using MIACopilot.Services;

ApplicationConfiguration.Initialize();

// ── Bootstrap services ────────────────────────────────────────────────────────
var dataService       = new DataService();
var apprenticeService = new ApprenticeService(dataService);
var companyService    = new CompanyService(dataService);
var trainerService    = new VocationalTrainerService(dataService);
var gradeService      = new GradeService(dataService);

// ── Login loop (supports logout → back to login) ──────────────────────────────
while (true)
{
    Session.Clear();

    using var login = new LoginForm(apprenticeService, trainerService);
    if (login.ShowDialog() != DialogResult.OK) break; // window closed → exit

    Form portal;
    try
    {
        portal = Session.Role switch
        {
            UserRole.SuperAdmin => new MainForm(apprenticeService, companyService, trainerService, gradeService),
            UserRole.Trainer    => trainerService.GetById(Session.UserId) is { } trainer
                                        ? new TrainerPortal(trainer, apprenticeService, companyService, trainerService, gradeService)
                                        : throw new InvalidOperationException($"Trainer with ID {Session.UserId} was not found."),
            UserRole.Apprentice => apprenticeService.GetById(Session.UserId) is { } apprentice
                                        ? new ApprenticePortal(apprentice, apprenticeService, companyService, trainerService, gradeService)
                                        : throw new InvalidOperationException($"Apprentice with ID {Session.UserId} was not found."),
            _                   => new MainForm(apprenticeService, companyService, trainerService, gradeService)
        };
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Failed to open your session:\n{ex.Message}", "Session Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        continue; // back to login loop
    }

    Application.Run(portal);

    // If the portal returned DialogResult.Retry it means "logout → show login again"
    if (portal.DialogResult != DialogResult.Retry) break;
}
