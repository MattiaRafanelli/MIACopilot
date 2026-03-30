using MIACopilot.Forms;
using MIACopilot.Models;
using MIACopilot.Services;

// Initializes WinForms application configuration
ApplicationConfiguration.Initialize();

// ── Bootstrap services ─────────────────────────────────────────────────────
// Create the shared data service (JSON persistence)
var dataService       = new DataService();

// Create domain services using the shared data service
var apprenticeService = new ApprenticeService(dataService);
var companyService    = new CompanyService(dataService);
var trainerService    = new VocationalTrainerService(dataService);
var gradeService      = new GradeService(dataService);

// ── Login loop (supports logout → back to login) ────────────────────────────
// The loop allows logging out and returning to the login screen
while (true)
{
    // Clear any previous session data
    Session.Clear();

    // Show login dialog
    using var login = new LoginForm(apprenticeService, trainerService, companyService);

    // If login dialog was closed or cancelled → exit application
    if (login.ShowDialog() != DialogResult.OK)
        break;

    Form portal;

    try
    {
        // Select the correct portal based on the logged-in user role
        portal = Session.Role switch
        {
            // Super admin → full admin portal
            UserRole.SuperAdmin =>
                new MainForm(
                    apprenticeService,
                    companyService,
                    trainerService,
                    gradeService
                ),

            // Trainer → trainer portal (resolve trainer from service)
            UserRole.Trainer =>
                trainerService.GetById(Session.UserId) is { } trainer
                    ? new TrainerPortal(
                        trainer,
                        apprenticeService,
                        companyService,
                        trainerService,
                        gradeService
                    )
                    : throw new InvalidOperationException(
                        $"Trainer with ID {Session.UserId} was not found."
                    ),

            // Apprentice → apprentice portal (resolve apprentice from service)
            UserRole.Apprentice =>
                apprenticeService.GetById(Session.UserId) is { } apprentice
                    ? new ApprenticePortal(
                        apprentice,
                        apprenticeService,
                        companyService,
                        trainerService,
                        gradeService
                    )
                    : throw new InvalidOperationException(
                        $"Apprentice with ID {Session.UserId} was not found."
                    ),

            // Company admin → company-scoped portal
            UserRole.CompanyAdmin =>
                companyService.GetById(Session.CompanyId) is { } company
                    ? new CompanyAdminPortal(
                        company.Id,
                        company.Name,
                        apprenticeService,
                        trainerService,
                        companyService,
                        gradeService
                    )
                    : throw new InvalidOperationException(
                        $"Company with ID {Session.CompanyId} was not found."
                    ),

            // Fallback (should not normally occur)
            _ =>
                new MainForm(
                    apprenticeService,
                    companyService,
                    trainerService,
                    gradeService
                )
        };
    }
    catch (Exception ex)
    {
        // If something goes wrong during portal creation, show error and return to login
        MessageBox.Show(
            $"Failed to open your session:\n{ex.Message}",
            "Session Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
        continue;
    }

    // Run the selected portal form
    Application.Run(portal);

    // If the portal returns DialogResult.Retry,
    // it signals a logout → show login again
    if (portal.DialogResult != DialogResult.Retry)
        break;
}