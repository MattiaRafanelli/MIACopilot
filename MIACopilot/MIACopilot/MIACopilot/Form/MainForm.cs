using MIACopilot.Models;
using MIACopilot.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MIACopilot.Forms;

/// <summary>Admin portal — read-only overview of all data.</summary>
public class MainForm : Form
{
    private readonly ApprenticeService        _apprenticeService;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;
    private readonly GradeService             _gradeService;

    private Panel pnlSidebar     = new();
    private Panel pnlContent     = new();
    private Panel pnlDashboard   = new();
    private Panel pnlApprentices = new();
    private Panel pnlCompanies   = new();
    private Panel pnlTrainers    = new();
    private Panel pnlGrades      = new();

    private Button btnNavDashboard   = new();
    private Button btnNavApprentices = new();
    private Button btnNavCompanies   = new();
    private Button btnNavTrainers    = new();
    private Button btnNavGrades      = new();

    private DataGridView dgvApprentices = new();
    private DataGridView dgvCompanies   = new();
    private DataGridView dgvTrainers    = new();
    private DataGridView dgvGrades      = new();
    private DataGridView dgvRecent      = new();

    private TextBox txtSearchApp   = new();
    private TextBox txtSearchComp  = new();
    private TextBox txtSearchTrain = new();
    private TextBox txtSearchGrade = new();

    private Label lblStatApprentices = new();
    private Label lblStatCompanies   = new();
    private Label lblStatTrainers    = new();
    private Label lblStatGrades      = new();

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

    /// <summary>Initialises services and builds all panels before showing the dashboard.</summary>
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

        ShowPanel(pnlDashboard, btnNavDashboard);
        RefreshAll();
    }

    // ═══════════════════════ SHELL ════════════════════════════════════════════

    /// <summary>Configures the form shell: size, background, sidebar and content containers.</summary>
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

        foreach (var p in new[] { pnlDashboard, pnlApprentices, pnlCompanies, pnlTrainers, pnlGrades })
        {
            p.Dock      = DockStyle.Fill;
            p.BackColor = C_Bg;
            pnlContent.Controls.Add(p);
        }

        Controls.Add(pnlContent);
        Controls.Add(pnlSidebar);
    }

    // ═══════════════════════ SIDEBAR ══════════════════════════════════════════

    /// <summary>Builds the left navigation sidebar with logo, nav buttons, version label and logout.</summary>
    void BuildSidebar()
    {
        var logo = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = C_Sidebar };
        logo.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using var f1 = new Font("Segoe UI", 13f, FontStyle.Bold);
            using var f2 = new Font("Segoe UI", 8.5f);
            e.Graphics.DrawString("MIA Copilot", f1, Brushes.White, 20f, 18f);
            using var br = new SolidBrush(C_Muted);
            e.Graphics.DrawString("Apprentice Management", f2, br, 20f, 46f);
        };

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

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

    /// <summary>Creates a flat-style navigation button for the sidebar.</summary>
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

    /// <summary>Shows the target panel, highlights the active nav button, and refreshes dashboard if needed.</summary>
    void ShowPanel(Panel target, Button nav)
    {
        foreach (var p in new[] { pnlDashboard, pnlApprentices, pnlCompanies, pnlTrainers, pnlGrades })
            p.Visible = p == target;

        foreach (Control c in pnlSidebar.Controls)
            if (c is Button b) { b.BackColor = C_Sidebar; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 10f); }

        nav.BackColor = C_Active;
        nav.ForeColor = Color.White;
        nav.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);

        if (target == pnlDashboard) UpdateDashboardStats();
    }

    // ═══════════════════════ DASHBOARD ════════════════════════════════════════

    /// <summary>Builds the dashboard panel: four stat cards and a recent-apprentices table.</summary>
    void BuildDashboard()
    {
        var title = PageTitle("Dashboard", "Overview of your management data");

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

    /// <summary>Creates a large bold number label used inside a stat card.</summary>
    Label StatNum(Color color) => new()
    {
        Text      = "0",
        Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
        ForeColor = color,
        TextAlign = ContentAlignment.MiddleCenter,
        Dock      = DockStyle.Fill,
        BackColor = C_White
    };

    /// <summary>Builds a stat card panel with a 3-px colored top bar, title text and a number label.</summary>
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
        numLabel.BackColor = C_White;
        card.Controls.Add(numLabel);
        card.Controls.Add(lbl);
        return card;
    }

    /// <summary>Reloads stat counts and the recent-apprentices grid with the latest data.</summary>
    void UpdateDashboardStats()
    {
        lblStatApprentices.Text = _apprenticeService.GetAll().Count.ToString();
        lblStatCompanies.Text   = _companyService.GetAll().Count.ToString();
        lblStatTrainers.Text    = _trainerService.GetAll().Count.ToString();
        lblStatGrades.Text      = _gradeService.GetAll().Count.ToString();

        dgvRecent.DataSource = _apprenticeService.GetAll()
            .OrderByDescending(a => a.StartDate).Take(10)
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

    // ═══════════════════════ APPRENTICES (read-only) ══════════════════════════

    /// <summary>Builds the read-only apprentice list panel with a search bar.</summary>
    void BuildApprenticePanel()
    {
        var title  = PageTitle("Apprentices", "View all apprentices");
        var search = SearchBar(txtSearchApp, "Search by name…",
            () => LoadApprentices(string.IsNullOrWhiteSpace(txtSearchApp.Text)
                    ? null : _apprenticeService.SearchByName(txtSearchApp.Text)));

        dgvApprentices = MakeGrid();
        var card = CardPanel();
        card.Controls.Add(dgvApprentices);
        card.Controls.Add(search);

        pnlApprentices.Controls.Add(card);
        pnlApprentices.Controls.Add(title);
    }

    /// <summary>Loads apprentices into the grid; uses the provided list or all apprentices if null.</summary>
    void LoadApprentices(List<Apprentice>? list = null)
    {
        list ??= _apprenticeService.GetAll();
        dgvApprentices.DataSource = list.Select(a => new
        {
            a.Id,
            a.FirstName,
            a.LastName,
            a.Email,
            Start    = a.StartDate.ToString("dd.MM.yyyy"),
            Company  = _companyService.GetById(a.CompanyId)?.Name ?? "—",
            Trainer  = _trainerService.GetById(a.VocationalTrainerId)?.FullName ?? "—",
            Journals = a.WorkJournals.Count,
            Grades   = _gradeService.GetByApprentice(a.Id).Count,
            Average  = _gradeService.GetAverage(a.Id) is double d && d > 0 ? d.ToString("0.0") : "—"
        }).ToList();
        AutoSizeCols(dgvApprentices, "Email");
    }

    // ═══════════════════════ COMPANIES (read-only) ════════════════════════════

    /// <summary>Builds the read-only company list panel with a search bar.</summary>
    void BuildCompanyPanel()
    {
        var title  = PageTitle("Companies", "View all registered companies");
        var search = SearchBar(txtSearchComp, "Search by name…",
            () => LoadCompanies(string.IsNullOrWhiteSpace(txtSearchComp.Text)
                    ? null : _companyService.SearchByName(txtSearchComp.Text)));

        dgvCompanies = MakeGrid();
        var card = CardPanel();
        card.Controls.Add(dgvCompanies);
        card.Controls.Add(search);

        pnlCompanies.Controls.Add(card);
        pnlCompanies.Controls.Add(title);
    }

    /// <summary>Loads companies into the grid; uses the provided list or all companies if null.</summary>
    void LoadCompanies(List<Company>? list = null)
    {
        list ??= _companyService.GetAll();
        dgvCompanies.DataSource = list.Select(c => new
        {
            c.Id, c.Name, c.Industry, c.Address, c.Phone, c.Email,
            Apprentices = _apprenticeService.FilterByCompany(c.Id).Count
        }).ToList();
        AutoSizeCols(dgvCompanies, "Address");
    }

    // ═══════════════════════ TRAINERS (read-only) ═════════════════════════════

    /// <summary>Builds the read-only trainer list panel with a search bar.</summary>
    void BuildTrainerPanel()
    {
        var title  = PageTitle("Vocational Trainers", "View all trainers");
        var search = SearchBar(txtSearchTrain, "Search by name…",
            () => LoadTrainers(string.IsNullOrWhiteSpace(txtSearchTrain.Text)
                    ? null : _trainerService.SearchByName(txtSearchTrain.Text)));

        dgvTrainers = MakeGrid();
        var card = CardPanel();
        card.Controls.Add(dgvTrainers);
        card.Controls.Add(search);

        pnlTrainers.Controls.Add(card);
        pnlTrainers.Controls.Add(title);
    }

    /// <summary>Loads trainers into the grid; uses the provided list or all trainers if null.</summary>
    void LoadTrainers(List<VocationalTrainer>? list = null)
    {
        list ??= _trainerService.GetAll();
        dgvTrainers.DataSource = list.Select(t => new
        {
            t.Id,
            t.FirstName,
            t.LastName,
            t.Email,
            t.Phone,
            Company     = _companyService.GetById(t.CompanyId)?.Name ?? "—",
            Apprentices = _apprenticeService.GetAll().Count(a => a.VocationalTrainerId == t.Id)
        }).ToList();
        AutoSizeCols(dgvTrainers, "Email");
    }

    // ═══════════════════════ GRADES (read-only) ═══════════════════════════════

    /// <summary>Builds the read-only grades overview panel with a search bar.</summary>
    void BuildGradesPanel()
    {
        var title  = PageTitle("Grades", "Overview of all apprentice grades");
        var search = SearchBar(txtSearchGrade, "Search by apprentice name…",
            () => LoadGrades(txtSearchGrade.Text));

        dgvGrades = MakeGrid();
        dgvGrades.CellFormatting += ColourGradeCell;
        var card = CardPanel();
        card.Controls.Add(dgvGrades);
        card.Controls.Add(search);

        pnlGrades.Controls.Add(card);
        pnlGrades.Controls.Add(title);
    }

    /// <summary>Loads all grades into the grid, optionally filtered by apprentice name.</summary>
    void LoadGrades(string filter = "")
    {
        var all = _gradeService.GetAll();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var ids = _apprenticeService.SearchByName(filter).Select(a => a.Id).ToHashSet();
            all = all.Where(g => ids.Contains(g.ApprenticeId)).ToList();
        }
        dgvGrades.DataSource = all.OrderByDescending(g => g.Date).Select(g => new
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
        AutoSizeCols(dgvGrades, "Notes");
    }

    /// <summary>Colors the Grade column green/blue/orange/red based on the numeric score.</summary>
    void ColourGradeCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvGrades.Columns[e.ColumnIndex].Name != "Grade") return;
        if (e.Value is not string s) return;
        if (!double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v)) return;
        e.CellStyle.ForeColor = v >= 5.0 ? C_Green : v >= 4.0 ? C_Blue : v >= 3.0 ? C_Orange : C_Red;
        e.CellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
    }

    // ═══════════════════════ HELPERS ══════════════════════════════════════════

    /// <summary>Reloads all data grids and updates the dashboard statistics.</summary>
    void RefreshAll() { LoadApprentices(); LoadCompanies(); LoadTrainers(); UpdateDashboardStats(); }

    /// <summary>Sets each column to size by content; the named column fills remaining width.</summary>
    static void AutoSizeCols(DataGridView d, string fillCol)
    {
        d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        foreach (DataGridViewColumn col in d.Columns)
        {
            col.AutoSizeMode = col.Name == fillCol
                ? DataGridViewAutoSizeColumnMode.Fill
                : DataGridViewAutoSizeColumnMode.AllCells;
        }
    }

    /// <summary>Creates a page-header panel; title and subtitle are painted directly to avoid label artifacts.</summary>
    static Panel PageTitle(string title, string sub)
    {
        var p = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = C_Bg };
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

    /// <summary>Creates a white card panel with a subtle border drawn via Paint.</summary>
    static Panel CardPanel()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border, 0.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };
        return p;
    }

    /// <summary>Creates a search toolbar panel containing a single text input that triggers onSearch.</summary>
    static Panel SearchBar(TextBox sb, string placeholder, Action onSearch)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
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
        sb.TextChanged    += (_, _) => { if (string.IsNullOrWhiteSpace(sb.Text)) onSearch(); };
        sb.KeyDown        += (_, e) => { if (e.KeyCode == Keys.Enter) onSearch(); };
        panel.Controls.Add(sb);
        return panel;
    }

    /// <summary>Creates a consistently styled read-only DataGridView with zebra rows and blue selection.</summary>
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
        d.ColumnHeadersDefaultCellStyle.BackColor          = C_Bg;
        d.ColumnHeadersDefaultCellStyle.ForeColor          = C_Muted;
        d.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Bold);
        d.ColumnHeadersDefaultCellStyle.SelectionBackColor = C_Bg;
        d.ColumnHeadersDefaultCellStyle.Padding            = new Padding(10, 0, 0, 0);
        d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        d.ColumnHeadersHeight      = 44;
        d.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        d.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(239, 246, 255);
        d.DefaultCellStyle.SelectionForeColor       = C_Text;
        d.DefaultCellStyle.Padding                  = new Padding(10, 0, 10, 0);
        return d;
    }
}
