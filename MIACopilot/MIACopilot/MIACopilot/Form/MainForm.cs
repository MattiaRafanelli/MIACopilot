using MIACopilot.Models;
using MIACopilot.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MIACopilot.Forms;

/// <summary>
/// Admin portal — read-only overview of all data.
/// Provides dashboard statistics and navigation to overview panels
/// for apprentices, companies, trainers, and grades.
/// </summary>
public class MainForm : Form
{
    // ── Services ──────────────────────────────────────────────────────────
    private readonly ApprenticeService        _apprenticeService;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;
    private readonly GradeService             _gradeService;

    // ── Layout panels ─────────────────────────────────────────────────────
    private Panel pnlSidebar     = new();
    private Panel pnlContent     = new();
    private Panel pnlDashboard   = new();
    private Panel pnlApprentices = new();
    private Panel pnlCompanies   = new();
    private Panel pnlTrainers    = new();
    private Panel pnlGrades      = new();

    // ── Sidebar navigation buttons ────────────────────────────────────────
    private Button btnNavDashboard   = new();
    private Button btnNavApprentices = new();
    private Button btnNavCompanies   = new();
    private Button btnNavTrainers    = new();
    private Button btnNavGrades      = new();

    // ── Data grids ─────────────────────────────────────────────────────────
    private DataGridView dgvApprentices = new();
    private DataGridView dgvCompanies   = new();
    private DataGridView dgvTrainers    = new();
    private DataGridView dgvGrades      = new();
    private DataGridView dgvRecent      = new();

    // ── Search boxes (per panel) ──────────────────────────────────────────
    private TextBox txtSearchApp   = new();
    private TextBox txtSearchComp  = new();
    private TextBox txtSearchTrain = new();
    private TextBox txtSearchGrade = new();

    // ── Dashboard stat labels ─────────────────────────────────────────────
    private Label lblStatApprentices = new();
    private Label lblStatCompanies   = new();
    private Label lblStatTrainers    = new();
    private Label lblStatGrades      = new();

    // ── Theme colors ──────────────────────────────────────────────────────
    static readonly Color C_Sidebar = Color.FromArgb(15, 23, 41);
    static readonly Color C_SideHov = Color.FromArgb(30, 41, 59);
    static readonly Color C_Active  = Color.FromArgb(59, 130, 246);
    static readonly Color C_Bg      = Color.FromArgb(248, 250, 252);
    static readonly Color C_White   = Color.White;
    static readonly Color C_Border  = Color.FromArgb(226, 232, 240);
    static readonly Color C_Text    = Color.FromArgb(15, 23, 42);
    static readonly Color C_Muted   = Color.FromArgb(100, 116, 139);
    static readonly Color C_Green   = Color.FromArgb(16, 185, 129);
    static readonly Color C_Blue    = Color.FromArgb(59, 130, 246);
    static readonly Color C_Orange  = Color.FromArgb(245, 158, 11);
    static readonly Color C_Red     = Color.FromArgb(239, 68, 68);
    static readonly Color C_Purple  = Color.FromArgb(139, 92, 246);

    /// <summary>
    /// Initializes services and builds the entire admin UI.
    /// </summary>
    public MainForm(
        ApprenticeService apprenticeService,
        CompanyService companyService,
        VocationalTrainerService trainerService,
        GradeService gradeService)
    {
        _apprenticeService = apprenticeService;
        _companyService    = companyService;
        _trainerService    = trainerService;
        _gradeService      = gradeService;

        BuildWindow();
        BuildSidebar();
        BuildDashboard();
        BuildApprenticePanel();
        BuildCompanyPanel();
        BuildTrainerPanel();
        BuildGradesPanel();

        // Default view
        ShowPanel(pnlDashboard, btnNavDashboard);
        RefreshAll();
    }

    // ═══════════════════════ WINDOW SHELL ═══════════════════════════════════

    /// <summary>
    /// Configures the main window and root containers (sidebar + content).
    /// </summary>
    void BuildWindow()
    {
        Text          = "MIA Copilot — Apprentice Management";
        Size          = new Size(1200, 740);
        MinimumSize   = new Size(960, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = C_Bg;
        Font          = new Font("Segoe UI", 10f);

        pnlSidebar.Dock      = DockStyle.Left;
        pnlSidebar.Width     = 220;
        pnlSidebar.BackColor = C_Sidebar;

        pnlContent.Dock      = DockStyle.Fill;
        pnlContent.BackColor = C_Bg;
        pnlContent.Padding   = new Padding(28, 24, 28, 24);

        // Register all panels in content area
        foreach (var p in new[] { pnlDashboard, pnlApprentices, pnlCompanies, pnlTrainers, pnlGrades })
        {
            p.Dock      = DockStyle.Fill;
            p.BackColor = C_Bg;
            pnlContent.Controls.Add(p);
        }

        Controls.Add(pnlContent);
        Controls.Add(pnlSidebar);
    }

    // ═══════════════════════ SIDEBAR ═════════════════════════════════════════

    /// <summary>
    /// Builds the left sidebar including logo, navigation buttons, and logout.
    /// </summary>
    void BuildSidebar()
    {
        // ── Logo header ───────────────────────────────────────────────────
        var logo = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = C_Sidebar };
        logo.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using var f1 = new Font("Segoe UI", 13f, FontStyle.Bold);
            using var f2 = new Font("Segoe UI", 8.5f);
            using var muted = new SolidBrush(C_Muted);

            e.Graphics.DrawString("MIA Copilot", f1, Brushes.White, 20f, 18f);
            e.Graphics.DrawString("Apprentice Management", f2, muted, 20f, 46f);
        };

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

        // ── Navigation buttons ────────────────────────────────────────────
        btnNavDashboard   = NavBtn("🏠   Dashboard");
        btnNavApprentices = NavBtn("🎓   Apprentices");
        btnNavCompanies   = NavBtn("🏢   Companies");
        btnNavTrainers    = NavBtn("👤   Trainers");
        btnNavGrades      = NavBtn("📊   Grades");

        btnNavDashboard.Click   += (_, _) => ShowPanel(pnlDashboard,   btnNavDashboard);
        btnNavApprentices.Click += (_, _) => ShowPanel(pnlApprentices, btnNavApprentices);
        btnNavCompanies.Click   += (_, _) => ShowPanel(pnlCompanies,   btnNavCompanies);
        btnNavTrainers.Click    += (_, _) => ShowPanel(pnlTrainers,    btnNavTrainers);
        btnNavGrades.Click      += (_, _) => { ShowPanel(pnlGrades, btnNavGrades); LoadGrades(); };

        // ── Footer: version + logout ──────────────────────────────────────
        var ver = new Label
        {
            Text      = "v1.0  ·  .NET 8  ·  WinForms",
            ForeColor = Color.FromArgb(51, 65, 85),
            BackColor = C_Sidebar,
            Font      = new Font("Segoe UI", 7.5f),
            Dock      = DockStyle.Bottom,
            Height    = 28,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnLogout = new Button
        {
            Text      = "⎋   Logout",
            Dock      = DockStyle.Bottom,
            Height    = 46,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(148, 163, 184),
            BackColor = C_Sidebar,
            Font      = new Font("Segoe UI", 10f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(16, 0, 0, 0),
            Cursor    = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize         = 0;
        btnLogout.FlatAppearance.MouseOverBackColor = C_SideHov;
        btnLogout.Click += (_, _) => { DialogResult = DialogResult.Retry; Close(); };

        var sepBottom = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

        pnlSidebar.Controls.Add(ver);
        pnlSidebar.Controls.Add(btnLogout);
        pnlSidebar.Controls.Add(sepBottom);
        pnlSidebar.Controls.Add(btnNavGrades);
        pnlSidebar.Controls.Add(btnNavTrainers);
        pnlSidebar.Controls.Add(btnNavCompanies);
        pnlSidebar.Controls.Add(btnNavApprentices);
        pnlSidebar.Controls.Add(btnNavDashboard);
        pnlSidebar.Controls.Add(sep);
        pnlSidebar.Controls.Add(logo);
    }

    /// <summary>
    /// Creates a flat-style navigation button for the sidebar.
    /// </summary>
    Button NavBtn(string text)
    {
        var b = new Button
        {
            Text      = "  " + text,
            Dock      = DockStyle.Top,
            Height    = 48,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(148, 163, 184),
            BackColor = C_Sidebar,
            Font      = new Font("Segoe UI", 10f),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderSize         = 0;
        b.FlatAppearance.MouseOverBackColor = C_SideHov;
        return b;
    }

    /// <summary>
    /// Shows the selected panel and highlights the active navigation button.
    /// </summary>
    void ShowPanel(Panel target, Button nav)
    {
        foreach (var p in new[] { pnlDashboard, pnlApprentices, pnlCompanies, pnlTrainers, pnlGrades })
            p.Visible = p == target;

        foreach (Control c in pnlSidebar.Controls)
            if (c is Button b)
            {
                b.BackColor = C_Sidebar;
                b.ForeColor = Color.FromArgb(148, 163, 184);
                b.Font      = new Font("Segoe UI", 10f);
            }

        nav.BackColor = C_Active;
        nav.ForeColor = Color.White;
        nav.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);

        if (target == pnlDashboard)
            UpdateDashboardStats();
    }

    // ═══════════════════════ DASHBOARD ═══════════════════════════════════════

    /// <summary>
    /// Builds the dashboard with stat cards and a recent-apprentices table.
    /// </summary>
    void BuildDashboard()
    {
        var title = PageTitle("Dashboard", "Overview of your management data");

        // ── Stat cards row ────────────────────────────────────────────────
        var cards = new TableLayoutPanel
        {
            Dock        = DockStyle.Top,
            Height      = 120,
            ColumnCount = 4,
            RowCount    = 1,
            BackColor   = C_Bg,
            Padding     = new Padding(0, 0, 0, 14)
        };
        for (int i = 0; i < 4; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        lblStatApprentices = StatNum(C_Blue);
        lblStatCompanies   = StatNum(C_Green);
        lblStatTrainers    = StatNum(C_Purple);
        lblStatGrades      = StatNum(C_Orange);

        cards.Controls.Add(StatCard("🎓  Apprentices",  lblStatApprentices, C_Blue),   0, 0);
        cards.Controls.Add(StatCard("🏢  Companies",    lblStatCompanies,   C_Green),  1, 0);
        cards.Controls.Add(StatCard("👤  Trainers",     lblStatTrainers,    C_Purple), 2, 0);
        cards.Controls.Add(StatCard("📊  Total Grades", lblStatGrades,      C_Orange), 3, 0);

        // ── Recent apprentices table ─────────────────────────────────────
        var recentHdr = new Label
        {
            Text      = "Recent Apprentices",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = C_Text,
            BackColor = C_White,
            Dock      = DockStyle.Top,
            Height    = 48,
            Padding   = new Padding(18, 14, 0, 0)
        };
        recentHdr.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, recentHdr.Height - 1, recentHdr.Width, recentHdr.Height - 1);
        };

        dgvRecent = MakeGrid();
        var card = CardPanel();
        card.Controls.Add(dgvRecent);
        card.Controls.Add(recentHdr);

        pnlDashboard.Controls.Add(card);
        pnlDashboard.Controls.Add(cards);
        pnlDashboard.Controls.Add(title);
    }

    /// <summary>
    /// Creates a large numeric label for stat cards.
    /// </summary>
    Label StatNum(Color color) => new()
    {
        Text      = "0",
        Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
        ForeColor = color,
        TextAlign = ContentAlignment.MiddleCenter,
        Dock      = DockStyle.Fill,
        BackColor = C_White
    };

    /// <summary>
    /// Builds a stat card with accent bar, title, and number.
    /// </summary>
    Panel StatCard(string title, Label numLabel, Color accent)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White, Margin = new Padding(0, 0, 12, 0) };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border, 0.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var bar = new SolidBrush(accent);
            e.Graphics.FillRectangle(bar, 0, 0, card.Width, 3);
        };

        var lbl = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = C_Muted,
            BackColor = C_White,
            Dock      = DockStyle.Top,
            Height    = 34,
            Padding   = new Padding(16, 12, 0, 0)
        };

        card.Controls.Add(numLabel);
        card.Controls.Add(lbl);
        return card;
    }

    /// <summary>
    /// Refreshes dashboard statistics and recent apprentices grid.
    /// </summary>
    void UpdateDashboardStats()
    {
        lblStatApprentices.Text = _apprenticeService.GetAll().Count.ToString();
        lblStatCompanies.Text   = _companyService.GetAll().Count.ToString();
        lblStatTrainers.Text    = _trainerService.GetAll().Count.ToString();
        lblStatGrades.Text      = _gradeService.GetAll().Count.ToString();

        dgvRecent.DataSource = _apprenticeService.GetAll()
            .OrderByDescending(a => a.StartDate)
            .Take(10)
            .Select(a => new
            {
                a.Id,
                a.FirstName,
                a.LastName,
                Start   = a.StartDate.ToString("dd.MM.yyyy"),
                Company = _companyService.GetById(a.CompanyId)?.Name ?? "—",
                Trainer = _trainerService.GetById(a.VocationalTrainerId)?.FullName ?? "—",
                Avg     = _gradeService.GetAverage(a.Id) is double d && d > 0 ? d.ToString("0.0") : "—"
            }).ToList();

        AutoSizeCols(dgvRecent, "Trainer");
    }
}

    // ═══════════════════════ APPRENTICES (read-only) ══════════════════════════

    /// <summary>
/// Builds the read-only apprentice list panel with a search bar.
/// </summary>
void BuildApprenticePanel()
{
    // Page header (title + subtitle)
    var title = PageTitle("Apprentices", "View all apprentices");

    // Search bar that filters apprentices by name
    var search = SearchBar(
        txtSearchApp,
        "Search by name…",
        () => LoadApprentices(
            string.IsNullOrWhiteSpace(txtSearchApp.Text)
                ? null
                : _apprenticeService.SearchByName(txtSearchApp.Text))
    );

    // Read-only grid for apprentices
    dgvApprentices = MakeGrid();

    // Card container (white background + border)
    var card = CardPanel();
    card.Controls.Add(dgvApprentices);
    card.Controls.Add(search);

    // Assemble panel
    pnlApprentices.Controls.Add(card);
    pnlApprentices.Controls.Add(title);
}

/// <summary>
/// Loads apprentices into the grid.
/// Uses the provided list or all apprentices if null.
/// </summary>
void LoadApprentices(List<Apprentice>? list = null)
{
    // Fallback to all apprentices
    list ??= _apprenticeService.GetAll();

    // Project domain objects into a grid-friendly view model
    dgvApprentices.DataSource = list.Select(a => new
    {
        a.Id,
        a.FirstName,
        a.LastName,
        Start    = a.StartDate.ToString("dd.MM.yyyy"),
        Company  = _companyService.GetById(a.CompanyId)?.Name ?? "—",
        Trainer  = _trainerService.GetById(a.VocationalTrainerId)?.FullName ?? "—",
        Journals = a.WorkJournals.Count,
        Grades   = _gradeService.GetByApprentice(a.Id).Count,
        Average  = _gradeService.GetAverage(a.Id) is double d && d > 0
                    ? d.ToString("0.0")
                    : "—"
    }).ToList();

    // Let the Trainer column fill remaining width
    AutoSizeCols(dgvApprentices, "Trainer");
}


// ═══════════════════════ COMPANIES (CRUD) ════════════════════════════════════

/// <summary>
/// Builds the company list panel with search bar and CRUD toolbar.
/// </summary>
void BuildCompanyPanel()
{
    // Page header
    var title = PageTitle("Companies", "View all registered companies");

    // Search bar (filter by company name)
    var search = SearchBar(
        txtSearchComp,
        "Search by name…",
        () => LoadCompanies(
            string.IsNullOrWhiteSpace(txtSearchComp.Text)
                ? null
                : _companyService.SearchByName(txtSearchComp.Text))
    );

    // CRUD toolbar container
    var crudBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_White };

    // Bottom separator line
    crudBar.Paint += (_, e) =>
    {
        using var pen = new Pen(C_Border);
        e.Graphics.DrawLine(pen, 0, crudBar.Height - 1, crudBar.Width, crudBar.Height - 1);
    };

    // CRUD buttons
    var btnAdd    = CrudBtn("➕  Add Company", C_Green);
    var btnEdit   = CrudBtn("✏️  Edit",        C_Blue);
    var btnDelete = CrudBtn("🗑  Delete",      C_Red);

    // Manual positioning inside toolbar
    btnAdd.Location    = new Point(12, 10);
    btnEdit.Location   = new Point(170, 10);
    btnDelete.Location = new Point(286, 10);

    // Wire actions
    btnAdd.Click    += (_, _) => AddCompany();
    btnEdit.Click   += (_, _) => EditCompany();
    btnDelete.Click += (_, _) => DeleteCompany();

    crudBar.Controls.Add(btnAdd);
    crudBar.Controls.Add(btnEdit);
    crudBar.Controls.Add(btnDelete);

    // Company grid
    dgvCompanies = MakeGrid();

    // Card container
    var card = CardPanel();
    card.Controls.Add(dgvCompanies);
    card.Controls.Add(search);
    card.Controls.Add(crudBar); // added last → appears at the top

    pnlCompanies.Controls.Add(card);
    pnlCompanies.Controls.Add(title);
}

/// <summary>
/// Opens the dialog to add a new company.
/// </summary>
void AddCompany()
{
    using var f = new CompanyDetailForm(null);
    if (f.ShowDialog() == DialogResult.OK)
    {
        _companyService.Create(f.Result!);
        LoadCompanies();
        UpdateDashboardStats();
    }
}

/// <summary>
/// Opens the dialog to edit the selected company.
/// </summary>
void EditCompany()
{
    // Ensure a row is selected
    if (dgvCompanies.SelectedRows.Count == 0)
    {
        Info("Select a company first.");
        return;
    }

    // Get selected company
    var id = (int)dgvCompanies.SelectedRows[0].Cells["Id"].Value;
    var co = _companyService.GetById(id);
    if (co == null) return;

    // Edit dialog
    using var f = new CompanyDetailForm(co);
    if (f.ShowDialog() == DialogResult.OK)
    {
        _companyService.Update(f.Result!);
        LoadCompanies();
    }
}

/// <summary>
/// Deletes the selected company if no apprentices are assigned.
/// </summary>
void DeleteCompany()
{
    if (dgvCompanies.SelectedRows.Count == 0)
    {
        Info("Select a company first.");
        return;
    }

    var id = (int)dgvCompanies.SelectedRows[0].Cells["Id"].Value;
    var co = _companyService.GetById(id);
    if (co == null) return;

    // Prevent deletion if apprentices are assigned
    var assigned = _apprenticeService.FilterByCompany(id).Count;
    if (assigned > 0)
    {
        MessageBox.Show(
            $"Cannot delete '{co.Name}' — {assigned} apprentice(s) are still assigned to it.",
            "Deletion Blocked",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return;
    }

    // Confirm deletion
    if (MessageBox.Show($"Delete company '{co.Name}'?", "Confirm Delete",
        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
    {
        _companyService.Delete(id);
        LoadCompanies();
        UpdateDashboardStats();
    }
}

/// <summary>
/// Creates a styled CRUD button.
/// </summary>
static Button CrudBtn(string text, Color color) => new()
{
    Text      = text,
    BackColor = color,
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Height    = 36,
    Width     = 150,
    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
    Cursor    = Cursors.Hand,
    FlatAppearance = { BorderSize = 0 }
};

/// <summary>
/// Shows a simple information message box.
/// </summary>
static void Info(string msg) =>
    MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

/// <summary>
/// Loads companies into the grid.
/// Uses the provided list or all companies if null.
/// </summary>
void LoadCompanies(List<Company>? list = null)
{
    list ??= _companyService.GetAll();

    dgvCompanies.DataSource = list.Select(c => new
    {
        c.Id,
        c.Name,
        c.Industry,
        c.Address,
        c.Phone,
        c.Email,
        Apprentices = _apprenticeService.FilterByCompany(c.Id).Count
    }).ToList();

    // Let the Email column fill remaining width
    AutoSizeCols(dgvCompanies, "Email");
}


    // ═══════════════════════ TRAINERS (read-only) ═════════════════════════════

    /// <summary>
/// Builds the read-only trainer list panel with a search bar.
/// </summary>
void BuildTrainerPanel()
{
    // Page header (title + subtitle)
    var title = PageTitle("Vocational Trainers", "View all trainers");

    // Search bar that filters trainers by name
    var search = SearchBar(
        txtSearchTrain,
        "Search by name…",
        () => LoadTrainers(
            string.IsNullOrWhiteSpace(txtSearchTrain.Text)
                ? null
                : _trainerService.SearchByName(txtSearchTrain.Text))
    );

    // Read-only grid for trainers
    dgvTrainers = MakeGrid();

    // Card container (white background + border)
    var card = CardPanel();
    card.Controls.Add(dgvTrainers);
    card.Controls.Add(search);

    // Assemble panel
    pnlTrainers.Controls.Add(card);
    pnlTrainers.Controls.Add(title);
}

/// <summary>
/// Loads trainers into the grid.
/// Uses the provided list or all trainers if null.
/// </summary>
void LoadTrainers(List<VocationalTrainer>? list = null)
{
    // Fallback to all trainers
    list ??= _trainerService.GetAll();

    // Project trainer data into a grid-friendly shape
    dgvTrainers.DataSource = list.Select(t => new
    {
        t.Id,
        t.FirstName,
        t.LastName,
        t.Phone,
        Company     = _companyService.GetById(t.CompanyId)?.Name ?? "—",
        Apprentices = _apprenticeService.GetAll()
            .Count(a => a.VocationalTrainerId == t.Id)
    }).ToList();

    // Let the Company column fill remaining width
    AutoSizeCols(dgvTrainers, "Company");
}


// ═══════════════════════ GRADES (read-only) ═══════════════════════════════

/// <summary>
/// Builds the read-only grades overview panel with a search bar.
/// </summary>
void BuildGradesPanel()
{
    // Page header
    var title = PageTitle("Grades", "Overview of all apprentice grades");

    // Search bar that filters grades by apprentice name
    var search = SearchBar(
        txtSearchGrade,
        "Search by apprentice name…",
        () => LoadGrades(txtSearchGrade.Text)
    );

    // Read-only grid for grades
    dgvGrades = MakeGrid();

    // Apply custom coloring to grade cells
    dgvGrades.CellFormatting += ColourGradeCell;

    // Card container
    var card = CardPanel();
    card.Controls.Add(dgvGrades);
    card.Controls.Add(search);

    // Assemble panel
    pnlGrades.Controls.Add(card);
    pnlGrades.Controls.Add(title);
}

/// <summary>
/// Loads all grades into the grid.
/// Optionally filters by apprentice name.
/// </summary>
void LoadGrades(string filter = "")
{
    // Load all grades
    var all = _gradeService.GetAll();

    // Filter grades by apprentice name if a filter is provided
    if (!string.IsNullOrWhiteSpace(filter))
    {
        var ids = _apprenticeService.SearchByName(filter)
            .Select(a => a.Id)
            .ToHashSet();

        all = all.Where(g => ids.Contains(g.ApprenticeId)).ToList();
    }

    // Bind projected grade data to the grid
    dgvGrades.DataSource = all
        .OrderByDescending(g => g.Date)
        .Select(g => new
        {
            g.Id,
            Apprentice = _apprenticeService.GetById(g.ApprenticeId)?.FullName ?? "—",
            g.Subject,
            Grade  = g.FormattedValue,
            g.Type,
            Date   = g.Date.ToString("dd.MM.yyyy"),
            Status = g.Category,
            g.Notes
        }).ToList();

    // Let the Notes column fill remaining width
    AutoSizeCols(dgvGrades, "Notes");
}

/// <summary>
/// Colors the Grade column based on the numeric value.
/// </summary>
void ColourGradeCell(object? sender, DataGridViewCellFormattingEventArgs e)
{
    // Only apply formatting to the Grade column
    if (dgvGrades.Columns[e.ColumnIndex].Name != "Grade") return;

    // Ensure the value is a string
    if (e.Value is not string s) return;

    // Parse numeric grade value
    if (!double.TryParse(
            s,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out double v)) return;

    // Apply color based on grade thresholds
    e.CellStyle.ForeColor =
        v >= 5.0 ? C_Green :
        v >= 4.0 ? C_Blue  :
        v >= 3.0 ? C_Orange :
                   C_Red;

    // Emphasize grade visually
    e.CellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
}

// ═══════════════════════ HELPERS ══════════════════════════════════════════

/// <summary>
/// Reloads all data grids and updates dashboard statistics.
/// </summary>
void RefreshAll()
{
    LoadApprentices();
    LoadCompanies();
    LoadTrainers();
    UpdateDashboardStats();
}

/// <summary>
/// Sets each column to auto-size by content,
/// while the specified column fills remaining width.
/// </summary>
static void AutoSizeCols(DataGridView d, string fillCol)
{
    d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

    foreach (DataGridViewColumn col in d.Columns)
    {
        col.AutoSizeMode =
            col.Name == fillCol
                ? DataGridViewAutoSizeColumnMode.Fill
                : DataGridViewAutoSizeColumnMode.AllCells;
    }
}

/// <summary>
/// Creates a page header panel.
/// Title and subtitle are drawn manually to avoid label rendering artifacts.
/// </summary>
static Panel PageTitle(string title, string sub)
{
    var p = new Panel
    {
        Dock = DockStyle.Top,
        Height = 72,
        BackColor = C_Bg
    };

    p.Paint += (_, e) =>
    {
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using var fT = new Font("Segoe UI", 18f, FontStyle.Bold);
        using var fS = new Font("Segoe UI", 9.5f);
        using var bT = new SolidBrush(C_Text);
        using var bS = new SolidBrush(C_Muted);

        e.Graphics.DrawString(title, fT, bT, 0f, 4f);
        e.Graphics.DrawString(sub,   fS, bS, 2f, 46f);
    };

    return p;
}

/// <summary>
/// Creates a white card panel with a subtle border.
/// </summary>
static Panel CardPanel()
{
    var p = new Panel
    {
        Dock = DockStyle.Fill,
        BackColor = C_White
    };

    p.Paint += (_, e) =>
    {
        using var pen = new Pen(C_Border, 0.5f);
        e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
    };

    return p;
}

/// <summary>
/// Creates a search toolbar panel with a single text input.
/// Triggers the provided search action on Enter or when cleared.
/// </summary>
static Panel SearchBar(TextBox sb, string placeholder, Action onSearch)
{
    var panel = new Panel
    {
        Dock = DockStyle.Top,
        Height = 58,
        BackColor = C_White
    };

    panel.Paint += (_, e) =>
    {
        using var pen = new Pen(C_Border);
        e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
    };

    sb.Width           = 280;
    sb.Height          = 36;
    sb.Font            = new Font("Segoe UI", 10f);
    sb.BorderStyle     = BorderStyle.FixedSingle;
    sb.Location        = new Point(16, 11);
    sb.PlaceholderText = placeholder;

    // Auto-reset search when input is cleared
    sb.TextChanged += (_, _) =>
    {
        if (string.IsNullOrWhiteSpace(sb.Text))
            onSearch();
    };

    // Trigger search on Enter
    sb.KeyDown += (_, e) =>
    {
        if (e.KeyCode == Keys.Enter)
            onSearch();
    };

    panel.Controls.Add(sb);
    return panel;
}

/// <summary>
/// Creates a consistently styled, read-only DataGridView
/// with zebra rows and blue selection.
/// </summary>
static DataGridView MakeGrid()
{
    var d = new DataGridView
    {
        Dock                  = DockStyle.Fill,
        ReadOnly              = true,
        AllowUserToAddRows    = false,
        AllowUserToDeleteRows = false,
        SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect           = false,
        AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.AllCells,
        BackgroundColor       = C_White,
        BorderStyle           = BorderStyle.None,
        RowHeadersVisible     = false,
        CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
        GridColor             = C_Border,
        Font                  = new Font("Segoe UI", 10f),
        RowTemplate           = { Height = 42 }
    };

    // Header styling
    d.ColumnHeadersDefaultCellStyle.BackColor          = C_Bg;
    d.ColumnHeadersDefaultCellStyle.ForeColor          = C_Muted;
    d.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Bold);
    d.ColumnHeadersDefaultCellStyle.SelectionBackColor = C_Bg;
    d.ColumnHeadersDefaultCellStyle.Padding            = new Padding(10, 0, 0, 0);
    d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
    d.ColumnHeadersHeight      = 44;

    // Row styling
    d.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
    d.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(239, 246, 255);
    d.DefaultCellStyle.SelectionForeColor       = C_Text;
    d.DefaultCellStyle.Padding                  = new Padding(10, 0, 10, 0);

    return d;
}