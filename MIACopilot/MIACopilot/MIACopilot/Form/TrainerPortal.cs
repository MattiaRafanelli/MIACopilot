/// <summary>
/// Personal portal for a logged-in vocational trainer.
/// Provides profile view and apprentice overview.
/// </summary>
public class TrainerPortal : Form
{
    // ── Logged-in trainer and required services ─────────────────────────────
    private readonly VocationalTrainer        _me;
    private readonly ApprenticeService        _apprenticeService;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;
    private readonly GradeService             _gradeService;

    // ── Layout & navigation ────────────────────────────────────────────────
    private Panel _sidebar = new();

    private Button btnProfile     = new();
    private Button btnApprentices = new();

    private Panel pnlProfile     = new();
    private Panel pnlApprentices = new();

    // ── Data grids ─────────────────────────────────────────────────────────
    private DataGridView dgvApprentices = new();
    private DataGridView dgvJournals    = new();
    private DataGridView dgvGrades      = new();

    // ── Detail state ───────────────────────────────────────────────────────
    private Panel       pnlDetail = new();
    private Apprentice? _selected;

    // ── Theme colors ───────────────────────────────────────────────────────
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
    static readonly Color C_Red     = Color.FromArgb(239, 68, 68);
    static readonly Color C_Orange  = Color.FromArgb(245, 158, 11);
    static readonly Color C_Purple  = Color.FromArgb(139, 92, 246);

    /// <summary>
    /// Initializes the trainer portal with the logged-in trainer and services.
    /// </summary>
    public TrainerPortal(
        VocationalTrainer me,
        ApprenticeService apprenticeService,
        CompanyService companyService,
        VocationalTrainerService trainerService,
        GradeService gradeService)
    {
        _me                = me;
        _apprenticeService = apprenticeService;
        _companyService    = companyService;
        _trainerService    = trainerService;
        _gradeService      = gradeService;

        // Build full UI and show profile view by default
        BuildUI();
        ShowPanel(pnlProfile, btnProfile);
    }

    /// <summary>
    /// Builds the main window layout (sidebar + content).
    /// </summary>
    void BuildUI()
    {
        Text          = $"MIA Copilot — {_me.FullName} (Trainer)";
        Size          = new Size(1120, 700);
        MinimumSize   = new Size(900, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = C_Bg;
        Font          = new Font("Segoe UI", 10f);

        // ── Sidebar ───────────────────────────────────────────────────────
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = C_Sidebar
        };

        // Avatar + identity header
        var avatarPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = C_Sidebar };
        avatarPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Trainer avatar circle
            using var avBrush = new SolidBrush(C_Purple);
            g.FillEllipse(avBrush, 20, 20, 58, 58);

            // Initials inside avatar
            using var initFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            var init =
                ((string.IsNullOrEmpty(_me.FirstName) ? "?" : _me.FirstName[0].ToString()) +
                 (string.IsNullOrEmpty(_me.LastName)  ? "?" : _me.LastName[0].ToString()))
                .ToUpper();
            var sz = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White,
                20 + (58 - sz.Width) / 2,
                20 + (58 - sz.Height) / 2);

            // Name, role and username
            using var nameFont  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            using var subFont   = new Font("Segoe UI", 8.5f);
            using var muteBrush = new SolidBrush(C_Muted);

            g.DrawString(_me.FullName, nameFont, Brushes.White, 88, 28);
            g.DrawString("Vocational Trainer", subFont, muteBrush, 88, 52);
            g.DrawString($"@{_me.Username}", subFont, muteBrush, 88, 70);
        };

        // Separator
        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

        // Navigation buttons
        btnProfile     = NavBtn("👤   My Profile");
        btnApprentices = NavBtn("🎓   My Apprentices");

        btnProfile.Click     += (_, _) => ShowPanel(pnlProfile,     btnProfile);
        btnApprentices.Click += (_, _) => ShowPanel(pnlApprentices, btnApprentices);

        // Logout button
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

        // Assemble sidebar
        _sidebar.Controls.Add(btnLogout);
        _sidebar.Controls.Add(sepBottom);
        _sidebar.Controls.Add(btnApprentices);
        _sidebar.Controls.Add(btnProfile);
        _sidebar.Controls.Add(sep);
        _sidebar.Controls.Add(avatarPanel);

        // ── Content area ───────────────────────────────────────────────────
        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = C_Bg,
            Padding = new Padding(28, 24, 28, 24)
        };

        // Build panels
        BuildProfilePanel();
        BuildApprenticesPanel();

        // Register panels
        foreach (var p in new[] { pnlProfile, pnlApprentices })
        {
            p.Dock = DockStyle.Fill;
            content.Controls.Add(p);
        }

        Controls.Add(content);
        Controls.Add(_sidebar);
    }

    // ═══════════════════════ PROFILE ══════════════════════════════════════════

    /// <summary>
    /// Builds the trainer profile panel (read-only information + PIN change).
    /// </summary>
    void BuildProfilePanel()
    {
        pnlProfile.BackColor = C_Bg;

        // Page header
        var title = PageTitle("My Profile", "Your trainer information");

        // Profile banner with avatar and identity
        var banner = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = C_Sidebar };
        banner.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Decorative background shape
            using var b1 = new SolidBrush(Color.FromArgb(20, 139, 92, 246));
            g.FillEllipse(b1, banner.Width - 120, -40, 160, 160);

            // Avatar circle
            using var avBrush = new SolidBrush(C_Purple);
            g.FillEllipse(avBrush, 28, 30, 72, 72);

            // Initials
            using var initFont = new Font("Segoe UI", 24f, FontStyle.Bold);
            var init =
                ((string.IsNullOrEmpty(_me.FirstName) ? "?" : _me.FirstName[0].ToString()) +
                 (string.IsNullOrEmpty(_me.LastName)  ? "?" : _me.LastName[0].ToString()))
                .ToUpper();
            var sz = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White,
                28 + (72 - sz.Width) / 2,
                30 + (72 - sz.Height) / 2);

            // Name, role and username
            using var nameFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var roleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var subFont  = new Font("Segoe UI", 8.5f);
            using var purple  = new SolidBrush(C_Purple);
            using var mute    = new SolidBrush(Color.FromArgb(148, 163, 184));

            g.DrawString(_me.FullName, nameFont, Brushes.White, 116, 38);
            g.DrawString("● VOCATIONAL TRAINER", roleFont, purple, 118, 66);
            g.DrawString($"@{_me.Username}", subFont, mute, 118, 88);
        };

        // Resolve company and apprentice count
        var company       = _companyService.GetById(_me.CompanyId);
        var myApprentices = _apprenticeService.GetAll()
            .Where(a => a.VocationalTrainerId == _me.Id)
            .ToList();

        // Profile fields (label + value)
        var fields = new[]
        {
            ("First Name",     _me.FirstName),
            ("Last Name",      _me.LastName),
            ("Email",          _me.Email),
            ("Phone",          _me.Phone),
            ("Company",        company?.Name ?? "—"),
            ("My Apprentices", myApprentices.Count.ToString())
        };

        // Card container
        var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
        card.Paint += BorderPaint(card);

        int y = 22;
        foreach (var (lbl, val) in fields)
        {
            card.Controls.Add(new Label
            {
                Text = lbl,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_Muted,
                Location = new Point(24, y),
                Size = new Size(170, 22)
            });
            card.Controls.Add(new Label
            {
                Text = val,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = C_Text,
                Location = new Point(200, y),
                Size = new Size(400, 22)
            });
            y += 36;
        }

        // Change PIN button
        var btnPin = new Button
        {
            Text      = "🔑  Change PIN",
            Location  = new Point(24, y + 8),
            Size      = new Size(160, 38),
            BackColor = C_Purple,
            ForeColor = C_White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnPin.FlatAppearance.BorderSize = 0;
        btnPin.Click += (_, _) =>
        {
            using var f = new ChangePinForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                _me.Pin = f.NewPin;
                _trainerService.Update(_me);
                MessageBox.Show(
                    "PIN changed successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        };

        card.Controls.Add(btnPin);

        // Assemble profile panel
        pnlProfile.Controls.Add(card);
        pnlProfile.Controls.Add(banner);
        pnlProfile.Controls.Add(title);
    }

    // ═══════════════════════ APPRENTICES ══════════════════════════════════════

    // Builds the "My Apprentices" main panel (split view: list on the left, details on the right)
void BuildApprenticesPanel()
{
    // Base background
    pnlApprentices.BackColor = C_Bg;

    // Page title + subtitle
    var title = PageTitle(
        "My Apprentices",
        "View journals — add and manage grades for your apprentices"
    );

    // Split layout: left = apprentice list, right = detail view
    var split = new SplitContainer
    {
        Dock          = DockStyle.Fill,
        BackColor     = C_Bg,
        Panel1MinSize = 260,   // minimum width for list
        Panel2MinSize = 380,   // minimum width for detail
        SplitterWidth = 8,
        BorderStyle   = BorderStyle.None
    };

    // Ensure a safe initial splitter position after layout is known
    bool splitterReady = false;
    split.SizeChanged += (_, _) =>
    {
        if (splitterReady || split.Width == 0) return;

        int min = split.Panel1MinSize;
        int max = split.Width - split.Panel2MinSize;
        if (max <= min) return;

        split.SplitterDistance = Math.Clamp(split.Width / 3, min, max);
        splitterReady = true;
    };

    // ── LEFT: apprentice list ────────────────────────────────────────────
    var leftCard = CardPanel();

    var leftTitle = new Label
    {
        Text      = "Apprentices",
        Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
        ForeColor = C_Text,
        Dock      = DockStyle.Top,
        Height    = 46,
        Padding   = new Padding(16, 14, 0, 0),
        BackColor = C_White
    };

    // Bottom border under the title
    leftTitle.Paint += (_, e) =>
    {
        using var pen = new Pen(C_Border);
        e.Graphics.DrawLine(pen, 0, leftTitle.Height - 1, leftTitle.Width, leftTitle.Height - 1);
    };

    // Grid with apprentices
    dgvApprentices = MakeGrid();
    dgvApprentices.SelectionChanged += OnApprenticeSelected;

    leftCard.Controls.Add(dgvApprentices);
    leftCard.Controls.Add(leftTitle);
    split.Panel1.Controls.Add(leftCard);

    // ── RIGHT: detail placeholder ─────────────────────────────────────────
    pnlDetail = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg };

    var placeholder = new Label
    {
        Text      = "← Select an apprentice to view details",
        Font      = new Font("Segoe UI", 11f),
        ForeColor = C_Muted,
        Dock      = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        BackColor = C_Bg
    };

    pnlDetail.Controls.Add(placeholder);
    split.Panel2.Controls.Add(pnlDetail);

    // Assemble page
    pnlApprentices.Controls.Add(split);
    pnlApprentices.Controls.Add(title);

    // Initial load
    LoadMyApprentices();
}


// Loads only apprentices assigned to the logged-in trainer
void LoadMyApprentices()
{
    var list = _apprenticeService.GetAll()
        .Where(a => a.VocationalTrainerId == _me.Id)
        .ToList();

    // Bind projected data to grid
    dgvApprentices.DataSource = list.Select(a => new
    {
        a.Id,
        a.FirstName,
        a.LastName,
        Start    = a.StartDate.ToString("dd.MM.yyyy"),
        Journals = a.WorkJournals.Count,
        Grades   = _gradeService.GetByApprentice(a.Id).Count,
        Avg      = _gradeService.GetAverage(a.Id) is double d && d > 0
                    ? d.ToString("0.0")
                    : "—"
    }).ToList();

    // Let LastName column fill remaining space
    AutoSizeCols(dgvApprentices, "LastName");
}


// Handles selection change in apprentice list
void OnApprenticeSelected(object? sender, EventArgs e)
{
    if (dgvApprentices.SelectedRows.Count == 0) return;

    // Resolve selected apprentice
    var id = (int)dgvApprentices.SelectedRows[0].Cells["Id"].Value;
    _selected = _apprenticeService.GetById(id);
    if (_selected == null) return;

    ShowDetailPanel();
}


// Builds the right-side detail panel for the selected apprentice
void ShowDetailPanel()
{
    if (_selected == null) return;
    pnlDetail.Controls.Clear();

    // Header with apprentice name
    var hdr = new Label
    {
        Text      = $"📋  {_selected.FullName}",
        Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
        ForeColor = C_Text,
        Dock      = DockStyle.Top,
        Height    = 42,
        Padding   = new Padding(4, 10, 0, 0),
        BackColor = C_Bg
    };

    // Tab control: Journals + Grades
    var tabs      = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
    var tJournals = new TabPage { Text = "📓  Journals", BackColor = C_White };
    var tGrades   = new TabPage { Text = "📊  Grades",   BackColor = C_White };

    // ── Journals tab (read-only) ─────────────────────────────────────────
    dgvJournals = MakeGrid();
    dgvJournals.DataSource = _selected.WorkJournals
        .OrderByDescending(j => j.Date)
        .Select(j => new
        {
            j.Id,
            Week    = j.WeekNumber,
            Date    = j.Date.ToString("dd.MM.yyyy"),
            j.Title,
            Preview = j.Content.Length > 60 ? j.Content[..60] + "…" : j.Content
        }).ToList();

    AutoSizeCols(dgvJournals, "Preview");
    tJournals.Controls.Add(dgvJournals);

    // ── Grades tab (full CRUD for trainer) ───────────────────────────────
    var gradeToolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_White };
    gradeToolbar.Paint += (_, e) =>
    {
        using var pen = new Pen(C_Border);
        e.Graphics.DrawLine(pen, 0, gradeToolbar.Height - 1, gradeToolbar.Width, gradeToolbar.Height - 1);
    };

    var btnAdd    = ActionBtn("➕  Add Grade",  C_Green);
    var btnEdit   = ActionBtn("✏️  Edit Grade", C_Blue);
    var btnDelete = ActionBtn("🗑  Delete",     C_Red);

    btnAdd.Location    = new Point(10, 11);
    btnEdit.Location   = new Point(152, 11);
    btnDelete.Location = new Point(294, 11);

    btnAdd.Click    += (_, _) => AddGrade();
    btnEdit.Click   += (_, _) => EditGrade();
    btnDelete.Click += (_, _) => DeleteGrade();

    gradeToolbar.Controls.Add(btnAdd);
    gradeToolbar.Controls.Add(btnEdit);
    gradeToolbar.Controls.Add(btnDelete);

    dgvGrades = MakeGrid();
    dgvGrades.CellFormatting += ColourGrades;
    LoadGradesForSelected();

    tGrades.Controls.Add(dgvGrades);
    tGrades.Controls.Add(gradeToolbar);

    tabs.TabPages.Add(tJournals);
    tabs.TabPages.Add(tGrades);

    // Assemble detail panel
    pnlDetail.Controls.Add(tabs);
    pnlDetail.Controls.Add(hdr);
}


// Loads grades for the currently selected apprentice
void LoadGradesForSelected()
{
    if (_selected == null) return;

    dgvGrades.DataSource = _gradeService.GetByApprentice(_selected.Id)
        .OrderByDescending(g => g.Date)
        .Select(g => new
        {
            g.Id,
            g.Subject,
            Grade  = g.FormattedValue,
            g.Type,
            Date   = g.Date.ToString("dd.MM.yyyy"),
            Status = g.Category,
            g.Notes
        }).ToList();

    AutoSizeCols(dgvGrades, "Notes");
}


// Opens dialog to add a new grade
void AddGrade()
{
    if (_selected == null) return;

    using var f = new GradeDetailForm(null, _selected.Id);
    if (f.ShowDialog() == DialogResult.OK)
    {
        _gradeService.Create(f.Result!);
        LoadGradesForSelected();
        LoadMyApprentices();
    }
}


// Opens dialog to edit the selected grade
void EditGrade()
{
    if (dgvGrades.SelectedRows.Count == 0)
    {
        Info("Select a grade first.");
        return;
    }

    var id = (int)dgvGrades.SelectedRows[0].Cells["Id"].Value;
    var gr = _gradeService.GetById(id);
    if (gr == null) return;

    using var f = new GradeDetailForm(gr, gr.ApprenticeId);
    if (f.ShowDialog() == DialogResult.OK)
        LoadGradesForSelected();
}


// Deletes the selected grade after confirmation
void DeleteGrade()
{
    if (dgvGrades.SelectedRows.Count == 0)
    {
        Info("Select a grade first.");
        return;
    }

    var id = (int)dgvGrades.SelectedRows[0].Cells["Id"].Value;
    var gr = _gradeService.GetById(id);
    if (gr == null) return;

    if (MessageBox.Show(
            $"Delete grade '{gr.Subject} {gr.FormattedValue}'?",
            "Confirm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
    {
        _gradeService.Delete(id);
        LoadGradesForSelected();
        LoadMyApprentices();
    }
}
