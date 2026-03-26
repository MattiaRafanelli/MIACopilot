using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Main window with tabs for Apprentices, Companies and Trainers.
/// </summary>
public class MainForm : Form
{
    // ── Services ─────────────────────────────────────────────────────────────
    private readonly ApprenticeService    _apprenticeService;
    private readonly CompanyService       _companyService;
    private readonly VocationalTrainerService _trainerService;

    // ── Controls ─────────────────────────────────────────────────────────────
    private TabControl    tabControl      = new();
    private TabPage       tabApprentices  = new();
    private TabPage       tabCompanies    = new();
    private TabPage       tabTrainers     = new();

    // Apprentice tab
    private DataGridView  dgvApprentices  = new();
    private Button        btnAddApp       = new();
    private Button        btnEditApp      = new();
    private Button        btnDeleteApp    = new();
    private Button        btnJournals     = new();
    private TextBox       txtSearchApp    = new();
    private Button        btnSearchApp    = new();

    // Company tab
    private DataGridView  dgvCompanies    = new();
    private Button        btnAddComp      = new();
    private Button        btnEditComp     = new();
    private Button        btnDeleteComp   = new();
    private TextBox       txtSearchComp   = new();
    private Button        btnSearchComp   = new();

    // Trainer tab
    private DataGridView  dgvTrainers     = new();
    private Button        btnAddTrain     = new();
    private Button        btnEditTrain    = new();
    private Button        btnDeleteTrain  = new();
    private TextBox       txtSearchTrain  = new();
    private Button        btnSearchTrain  = new();

    // ── Constructor ───────────────────────────────────────────────────────────
    public MainForm(
        ApprenticeService apprenticeService,
        CompanyService companyService,
        VocationalTrainerService trainerService)
    {
        _apprenticeService = apprenticeService;
        _companyService    = companyService;
        _trainerService    = trainerService;

        BuildUI();
        RefreshAll();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Builds all controls and lays them out.</summary>
    private void BuildUI()
    {
        // ── Window settings
        Text            = "MIA Copilot — Apprentice Management";
        Size            = new Size(1000, 620);
        StartPosition   = FormStartPosition.CenterScreen;
        MinimumSize     = new Size(800, 500);
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.FromArgb(245, 247, 250);

        // ── TabControl
        tabControl.Dock     = DockStyle.Fill;
        tabControl.Font     = new Font("Segoe UI", 10f, FontStyle.Bold);
        tabControl.Padding  = new Point(16, 6);

        tabApprentices.Text     = "🎓  Apprentices";
        tabApprentices.BackColor = Color.White;
        tabCompanies.Text       = "🏢  Companies";
        tabCompanies.BackColor  = Color.White;
        tabTrainers.Text        = "👨‍🏫  Trainers";
        tabTrainers.BackColor   = Color.White;

        tabControl.TabPages.Add(tabApprentices);
        tabControl.TabPages.Add(tabCompanies);
        tabControl.TabPages.Add(tabTrainers);

        // ── Build each tab
        BuildApprenticeTab();
        BuildCompanyTab();
        BuildTrainerTab();

        Controls.Add(tabControl);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a styled action button.</summary>
    private static Button MakeButton(string text, Color color)
    {
        return new Button
        {
            Text      = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Height    = 34,
            Width     = 120,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }

    /// <summary>Creates a styled DataGridView.</summary>
    private static DataGridView MakeGrid()
    {
        var dgv = new DataGridView
        {
            Dock                  = DockStyle.Fill,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor             = Color.FromArgb(220, 225, 235),
            Font                  = new Font("Segoe UI", 9.5f),
        };
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 36;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        return dgv;
    }

    /// <summary>Creates the toolbar panel (search box + buttons).</summary>
    private static Panel MakeToolbar(
        TextBox searchBox, Button searchBtn, Button[] actionButtons)
    {
        var panel = new Panel
        {
            Dock    = DockStyle.Top,
            Height  = 52,
            Padding = new Padding(8, 8, 8, 0),
            BackColor = Color.White
        };

        // Search
        searchBox.Width       = 200;
        searchBox.Height      = 32;
        searchBox.Font        = new Font("Segoe UI", 9.5f);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.Location    = new Point(8, 9);

        searchBtn.Location = new Point(215, 9);
        searchBtn.Width    = 80;

        panel.Controls.Add(searchBox);
        panel.Controls.Add(searchBtn);

        // Action buttons — right-aligned
        int x = panel.Width - 8;
        foreach (var btn in actionButtons.Reverse())
        {
            btn.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            x           -= btn.Width + 6;
            btn.Location = new Point(x, 9);
            panel.Controls.Add(btn);
        }

        return panel;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // APPRENTICE TAB
    // ══════════════════════════════════════════════════════════════════════════

    private void BuildApprenticeTab()
    {
        btnAddApp     = MakeButton("➕ Add",     Color.FromArgb(39, 174, 96));
        btnEditApp    = MakeButton("✏️ Edit",    Color.FromArgb(41, 128, 185));
        btnDeleteApp  = MakeButton("🗑 Delete",  Color.FromArgb(192, 57, 43));
        btnJournals   = MakeButton("📓 Journals", Color.FromArgb(142, 68, 173));
        btnSearchApp  = MakeButton("🔍 Search",  Color.FromArgb(52, 73, 94));

        btnAddApp.Click    += (_, _) => AddApprentice();
        btnEditApp.Click   += (_, _) => EditApprentice();
        btnDeleteApp.Click += (_, _) => DeleteApprentice();
        btnJournals.Click  += (_, _) => OpenJournals();
        btnSearchApp.Click += (_, _) => SearchApprentices();
        txtSearchApp.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SearchApprentices(); };

        var toolbar = MakeToolbar(txtSearchApp, btnSearchApp,
            new[] { btnAddApp, btnEditApp, btnDeleteApp, btnJournals });

        dgvApprentices = MakeGrid();

        tabApprentices.Controls.Add(dgvApprentices);
        tabApprentices.Controls.Add(toolbar);
    }

    /// <summary>Loads apprentices into the grid.</summary>
    private void LoadApprentices(List<Apprentice>? list = null)
    {
        list ??= _apprenticeService.GetAll();
        dgvApprentices.DataSource = list.Select(a => new
        {
            a.Id,
            a.FirstName,
            a.LastName,
            a.Email,
            Start       = a.StartDate.ToString("dd.MM.yyyy"),
            Company     = _companyService.GetById(a.CompanyId)?.Name ?? "—",
            Trainer     = _trainerService.GetById(a.VocationalTrainerId)?.FullName ?? "—",
            Journals    = a.WorkJournals.Count
        }).ToList();
    }

    private void SearchApprentices()
    {
        var q = txtSearchApp.Text.Trim();
        LoadApprentices(string.IsNullOrEmpty(q)
            ? null
            : _apprenticeService.SearchByName(q));
    }

    private void AddApprentice()
    {
        using var form = new ApprenticeDetailForm(null, _companyService, _trainerService);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _apprenticeService.Create(form.Result!);
            LoadApprentices();
        }
    }

    private void EditApprentice()
    {
        var selected = GetSelectedId(dgvApprentices);
        if (selected < 0) return;
        var apprentice = _apprenticeService.GetById(selected);
        if (apprentice == null) return;

        using var form = new ApprenticeDetailForm(apprentice, _companyService, _trainerService);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _apprenticeService.Update(form.Result!);
            LoadApprentices();
        }
    }

    private void DeleteApprentice()
    {
        var selected = GetSelectedId(dgvApprentices);
        if (selected < 0) return;
        var a = _apprenticeService.GetById(selected);
        if (a == null) return;

        if (Confirm($"Delete apprentice '{a.FullName}'?"))
        {
            _apprenticeService.Delete(selected);
            LoadApprentices();
        }
    }

    private void OpenJournals()
    {
        var selected = GetSelectedId(dgvApprentices);
        if (selected < 0) return;
        var apprentice = _apprenticeService.GetById(selected);
        if (apprentice == null) return;

        using var form = new JournalForm(apprentice, _apprenticeService);
        form.ShowDialog();
        LoadApprentices();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // COMPANY TAB
    // ══════════════════════════════════════════════════════════════════════════

    private void BuildCompanyTab()
    {
        btnAddComp     = MakeButton("➕ Add",    Color.FromArgb(39, 174, 96));
        btnEditComp    = MakeButton("✏️ Edit",   Color.FromArgb(41, 128, 185));
        btnDeleteComp  = MakeButton("🗑 Delete", Color.FromArgb(192, 57, 43));
        btnSearchComp  = MakeButton("🔍 Search", Color.FromArgb(52, 73, 94));

        btnAddComp.Click     += (_, _) => AddCompany();
        btnEditComp.Click    += (_, _) => EditCompany();
        btnDeleteComp.Click  += (_, _) => DeleteCompany();
        btnSearchComp.Click  += (_, _) => SearchCompanies();
        txtSearchComp.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SearchCompanies(); };

        var toolbar = MakeToolbar(txtSearchComp, btnSearchComp,
            new[] { btnAddComp, btnEditComp, btnDeleteComp });

        dgvCompanies = MakeGrid();

        tabCompanies.Controls.Add(dgvCompanies);
        tabCompanies.Controls.Add(toolbar);
    }

    private void LoadCompanies(List<Company>? list = null)
    {
        list ??= _companyService.GetAll();
        dgvCompanies.DataSource = list.Select(c => new
        {
            c.Id, c.Name, c.Industry, c.Address, c.Phone, c.Email
        }).ToList();
    }

    private void SearchCompanies()
    {
        var q = txtSearchComp.Text.Trim();
        LoadCompanies(string.IsNullOrEmpty(q)
            ? null
            : _companyService.SearchByName(q));
    }

    private void AddCompany()
    {
        using var form = new CompanyDetailForm(null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _companyService.Create(form.Result!);
            LoadCompanies();
        }
    }

    private void EditCompany()
    {
        var selected = GetSelectedId(dgvCompanies);
        if (selected < 0) return;
        var company = _companyService.GetById(selected);
        if (company == null) return;

        using var form = new CompanyDetailForm(company);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _companyService.Update(form.Result!);
            LoadCompanies();
        }
    }

    private void DeleteCompany()
    {
        var selected = GetSelectedId(dgvCompanies);
        if (selected < 0) return;
        var c = _companyService.GetById(selected);
        if (c == null) return;

        if (Confirm($"Delete company '{c.Name}'?"))
        {
            _companyService.Delete(selected);
            LoadCompanies();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TRAINER TAB
    // ══════════════════════════════════════════════════════════════════════════

    private void BuildTrainerTab()
    {
        btnAddTrain    = MakeButton("➕ Add",    Color.FromArgb(39, 174, 96));
        btnEditTrain   = MakeButton("✏️ Edit",   Color.FromArgb(41, 128, 185));
        btnDeleteTrain = MakeButton("🗑 Delete", Color.FromArgb(192, 57, 43));
        btnSearchTrain = MakeButton("🔍 Search", Color.FromArgb(52, 73, 94));

        btnAddTrain.Click    += (_, _) => AddTrainer();
        btnEditTrain.Click   += (_, _) => EditTrainer();
        btnDeleteTrain.Click += (_, _) => DeleteTrainer();
        btnSearchTrain.Click += (_, _) => SearchTrainers();
        txtSearchTrain.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SearchTrainers(); };

        var toolbar = MakeToolbar(txtSearchTrain, btnSearchTrain,
            new[] { btnAddTrain, btnEditTrain, btnDeleteTrain });

        dgvTrainers = MakeGrid();

        tabTrainers.Controls.Add(dgvTrainers);
        tabTrainers.Controls.Add(toolbar);
    }

    private void LoadTrainers(List<VocationalTrainer>? list = null)
    {
        list ??= _trainerService.GetAll();
        dgvTrainers.DataSource = list.Select(t => new
        {
            t.Id,
            t.FirstName,
            t.LastName,
            t.Email,
            t.Phone,
            Company = _companyService.GetById(t.CompanyId)?.Name ?? "—"
        }).ToList();
    }

    private void SearchTrainers()
    {
        var q = txtSearchTrain.Text.Trim();
        LoadTrainers(string.IsNullOrEmpty(q)
            ? null
            : _trainerService.SearchByName(q));
    }

    private void AddTrainer()
    {
        using var form = new TrainerDetailForm(null, _companyService);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _trainerService.Create(form.Result!);
            LoadTrainers();
        }
    }

    private void EditTrainer()
    {
        var selected = GetSelectedId(dgvTrainers);
        if (selected < 0) return;
        var trainer = _trainerService.GetById(selected);
        if (trainer == null) return;

        using var form = new TrainerDetailForm(trainer, _companyService);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _trainerService.Update(form.Result!);
            LoadTrainers();
        }
    }

    private void DeleteTrainer()
    {
        var selected = GetSelectedId(dgvTrainers);
        if (selected < 0) return;
        var t = _trainerService.GetById(selected);
        if (t == null) return;

        if (Confirm($"Delete trainer '{t.FullName}'?"))
        {
            _trainerService.Delete(selected);
            LoadTrainers();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SHARED HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Refreshes all three grids.</summary>
    private void RefreshAll()
    {
        LoadApprentices();
        LoadCompanies();
        LoadTrainers();
    }

    /// <summary>Returns the ID of the selected row, or -1 if nothing selected.</summary>
    private static int GetSelectedId(DataGridView dgv)
    {
        if (dgv.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a row first.", "No selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return -1;
        }
        return (int)dgv.SelectedRows[0].Cells["Id"].Value;
    }

    /// <summary>Shows a Yes/No confirmation dialog.</summary>
    private static bool Confirm(string message) =>
        MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes;
}
