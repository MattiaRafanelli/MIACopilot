using MIACopilot.Models;
using MIACopilot.Services;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MIACopilot.Forms;

/// <summary>Personal portal for a logged-in apprentice.</summary>
public class ApprenticePortal : Form
{
    private readonly Apprentice               _me;
    private readonly ApprenticeService        _apprenticeService;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;
    private readonly GradeService             _gradeService;

    private Panel _sidebar = new();

    private Button btnProfile  = new();
    private Button btnJournals = new();
    private Button btnGrades   = new();

    private Panel pnlProfile  = new();
    private Panel pnlJournals = new();
    private Panel pnlGrades   = new();

    private DataGridView dgvJournals = new();
    private DataGridView dgvGrades   = new();
    private Label        lblAvg      = new();
    private Label        lblStatus   = new();

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

    public ApprenticePortal(
        Apprentice me,
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

        BuildUI();
        ShowPanel(pnlProfile, btnProfile);
    }

    void BuildUI()
    {
        Text          = $"MIA Copilot — {_me.FullName}";
        Size          = new Size(1020, 680);
        MinimumSize   = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = C_Bg;
        Font          = new Font("Segoe UI", 10f);

        // ── Sidebar ───────────────────────────────────────────────────────────
        _sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = C_Sidebar };

        // Avatar / name area
        var avatarPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = C_Sidebar };
        avatarPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Avatar circle
            using var avBrush = new SolidBrush(C_Blue);
            g.FillEllipse(avBrush, 20, 20, 58, 58);
            using var initFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            var init = $"{_me.FirstName[0]}{_me.LastName[0]}".ToUpper();
            var sz   = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White, 20 + (58 - sz.Width) / 2, 20 + (58 - sz.Height) / 2);

            // Name and role
            using var nameFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            using var subFont  = new Font("Segoe UI", 8.5f);
            using var muteBrush = new SolidBrush(C_Muted);
            g.DrawString(_me.FullName, nameFont, Brushes.White, 88, 28);
            g.DrawString("Apprentice", subFont, muteBrush, 88, 52);
            g.DrawString($"@{_me.Username}", subFont, muteBrush, 88, 70);
        };

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

        btnProfile  = NavBtn("👤   My Profile");
        btnJournals = NavBtn("📓   My Journals");
        btnGrades   = NavBtn("📊   My Grades");

        btnProfile.Click  += (_, _) => ShowPanel(pnlProfile,  btnProfile);
        btnJournals.Click += (_, _) => ShowPanel(pnlJournals, btnJournals);
        btnGrades.Click   += (_, _) => ShowPanel(pnlGrades,   btnGrades);

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

        _sidebar.Controls.Add(btnLogout);
        _sidebar.Controls.Add(sepBottom);
        _sidebar.Controls.Add(btnGrades);
        _sidebar.Controls.Add(btnJournals);
        _sidebar.Controls.Add(btnProfile);
        _sidebar.Controls.Add(sep);
        _sidebar.Controls.Add(avatarPanel);

        // ── Content area ──────────────────────────────────────────────────────
        var content = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(28, 24, 28, 24) };

        BuildProfilePanel();
        BuildJournalsPanel();
        BuildGradesPanel();

        foreach (var p in new[] { pnlProfile, pnlJournals, pnlGrades })
        {
            p.Dock = DockStyle.Fill;
            content.Controls.Add(p);
        }

        Controls.Add(content);
        Controls.Add(_sidebar);
    }

    // ═══════════════════════ PROFILE ══════════════════════════════════════════

    void BuildProfilePanel()
    {
        pnlProfile.BackColor = C_Bg;

        var title = PageTitle("My Profile", "Your personal information");

        // Profile banner (painted)
        var banner = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = C_Sidebar };
        banner.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Decorative circles
            using var b1 = new SolidBrush(Color.FromArgb(20, 59, 130, 246));
            g.FillEllipse(b1, banner.Width - 120, -40, 160, 160);

            // Avatar
            using var avBrush = new SolidBrush(C_Blue);
            g.FillEllipse(avBrush, 28, 30, 72, 72);
            using var initFont = new Font("Segoe UI", 24f, FontStyle.Bold);
            var init = $"{_me.FirstName[0]}{_me.LastName[0]}".ToUpper();
            var sz   = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White, 28 + (72 - sz.Width) / 2, 30 + (72 - sz.Height) / 2);

            // Name, role, username
            using var nameFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var roleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var subFont  = new Font("Segoe UI", 8.5f);
            using var accentBr = new SolidBrush(C_Blue);
            using var muteBr   = new SolidBrush(Color.FromArgb(148, 163, 184));
            g.DrawString(_me.FullName, nameFont, Brushes.White, 116, 38);
            g.DrawString("● APPRENTICE", roleFont, accentBr, 118, 66);
            g.DrawString($"@{_me.Username}", subFont, muteBr, 118, 88);
        };

        // Fields card
        var company = _companyService.GetById(_me.CompanyId);
        var trainer = _trainerService.GetById(_me.VocationalTrainerId);

        var fields = new[]
        {
            ("First Name",         _me.FirstName),
            ("Last Name",          _me.LastName),
            ("Email",              _me.Email),
            ("Start Date",         _me.StartDate.ToString("dd.MM.yyyy")),
            ("Company",            company?.Name ?? "—"),
            ("Vocational Trainer", trainer?.FullName ?? "—"),
        };

        var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
        card.Paint += BorderPaint(card);

        int y = 22;
        foreach (var (label, value) in fields)
        {
            card.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_Muted,
                Location  = new Point(24, y),
                Size      = new Size(170, 22),
                TextAlign = ContentAlignment.MiddleLeft
            });
            card.Controls.Add(new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 10.5f),
                ForeColor = C_Text,
                Location  = new Point(200, y),
                Size      = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleLeft
            });
            y += 36;
        }

        var btnChangePin = new Button
        {
            Text      = "🔑  Change PIN",
            Location  = new Point(24, y + 8),
            Size      = new Size(160, 38),
            BackColor = C_Blue,
            ForeColor = C_White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnChangePin.FlatAppearance.BorderSize = 0;
        btnChangePin.Click += ChangePinApprentice;
        card.Controls.Add(btnChangePin);

        pnlProfile.Controls.Add(card);
        pnlProfile.Controls.Add(banner);
        pnlProfile.Controls.Add(title);
    }

    void ChangePinApprentice(object? sender, EventArgs e)
    {
        using var f = new ChangePinForm();
        if (f.ShowDialog() == DialogResult.OK)
        {
            _me.Pin = f.NewPin;
            _apprenticeService.Update(_me);
            MessageBox.Show("PIN changed successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // ═══════════════════════ JOURNALS ═════════════════════════════════════════

    void BuildJournalsPanel()
    {
        pnlJournals.BackColor = C_Bg;

        var title = PageTitle("My Work Journals", "Create and manage your weekly journal entries");

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
        toolbar.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
        };

        var btnAdd    = ActionBtn("➕  Add Entry",  C_Green);
        var btnEdit   = ActionBtn("✏️  Edit",       C_Blue);
        var btnDelete = ActionBtn("🗑  Delete",     C_Red);
        btnAdd.Location    = new Point(16, 12);
        btnEdit.Location   = new Point(158, 12);
        btnDelete.Location = new Point(300, 12);

        btnAdd.Click    += (_, _) => AddJournal();
        btnEdit.Click   += (_, _) => EditJournal();
        btnDelete.Click += (_, _) => DeleteJournal();

        toolbar.Controls.Add(btnAdd);
        toolbar.Controls.Add(btnEdit);
        toolbar.Controls.Add(btnDelete);

        dgvJournals = MakeGrid();
        LoadJournals();

        var card = CardPanel();
        card.Controls.Add(dgvJournals);
        card.Controls.Add(toolbar);

        pnlJournals.Controls.Add(card);
        pnlJournals.Controls.Add(title);
    }

    void LoadJournals()
    {
        var fresh    = _apprenticeService.GetById(_me.Id);
        var journals = fresh?.WorkJournals ?? _me.WorkJournals;

        dgvJournals.DataSource = journals
            .OrderByDescending(j => j.Date)
            .Select(j => new
            {
                j.Id,
                Week    = j.WeekNumber,
                Date    = j.Date.ToString("dd.MM.yyyy"),
                j.Title,
                Preview = j.Content.Length > 80 ? j.Content[..80] + "…" : j.Content
            }).ToList();

        AutoSizeCols(dgvJournals, "Preview");
    }

    void AddJournal()
    {
        using var f = new JournalDetailForm(null);
        if (f.ShowDialog() == DialogResult.OK) { _apprenticeService.AddJournal(_me.Id, f.Result!); LoadJournals(); }
    }

    void EditJournal()
    {
        if (dgvJournals.SelectedRows.Count == 0) { Info("Select a journal entry first."); return; }
        var id      = (int)dgvJournals.SelectedRows[0].Cells["Id"].Value;
        var fresh   = _apprenticeService.GetById(_me.Id);
        var journal = fresh?.WorkJournals.FirstOrDefault(j => j.Id == id);
        if (journal == null) return;

        using var f = new JournalDetailForm(journal);
        if (f.ShowDialog() == DialogResult.OK) { _apprenticeService.UpdateJournal(_me.Id, f.Result!); LoadJournals(); }
    }

    void DeleteJournal()
    {
        if (dgvJournals.SelectedRows.Count == 0) { Info("Select a journal entry first."); return; }
        var id = (int)dgvJournals.SelectedRows[0].Cells["Id"].Value;
        if (MessageBox.Show("Delete this journal entry?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        { _apprenticeService.DeleteJournal(_me.Id, id); LoadJournals(); }
    }

    // ═══════════════════════ GRADES (read-only) ═══════════════════════════════

    void BuildGradesPanel()
    {
        pnlGrades.BackColor = C_Bg;

        var title = PageTitle("My Grades", "View your grades — assigned by your trainer");

        // Summary bar
        var statsBar = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = C_White };
        statsBar.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, statsBar.Height - 1, statsBar.Width, statsBar.Height - 1);
        };

        lblAvg = new Label
        {
            Text      = "—",
            Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
            ForeColor = C_Blue,
            Location  = new Point(24, 12),
            Size      = new Size(110, 48),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = C_White
        };
        lblStatus = new Label
        {
            Text      = "No grades yet",
            Font      = new Font("Segoe UI", 11f),
            ForeColor = C_Muted,
            Location  = new Point(142, 24),
            Size      = new Size(300, 30),
            BackColor = C_White
        };

        statsBar.Controls.Add(lblStatus);
        statsBar.Controls.Add(lblAvg);

        dgvGrades = MakeGrid();
        dgvGrades.CellFormatting += ColourGrades;
        LoadGrades();

        var card = CardPanel();
        card.Controls.Add(dgvGrades);
        card.Controls.Add(statsBar);

        pnlGrades.Controls.Add(card);
        pnlGrades.Controls.Add(title);
    }

    void LoadGrades()
    {
        var grades = _gradeService.GetByApprentice(_me.Id);

        dgvGrades.DataSource = grades
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

        var avg = _gradeService.GetAverage(_me.Id);
        if (avg == 0) { lblAvg.Text = "—"; lblAvg.ForeColor = C_Muted; lblStatus.Text = "No grades yet"; lblStatus.ForeColor = C_Muted; return; }

        lblAvg.Text         = $"Ø {avg:0.0}";
        lblAvg.ForeColor    = GradeColor(avg);
        lblStatus.Text      = avg >= 5.0 ? "🌟  Excellent" : avg >= 4.0 ? "✅  Passed" : avg >= 3.0 ? "⚠️  Sufficient" : "❌  Failed";
        lblStatus.ForeColor = GradeColor(avg);
    }

    // ═══════════════════════ HELPERS ══════════════════════════════════════════

    void ShowPanel(Panel target, Button nav)
    {
        foreach (var p in new[] { pnlProfile, pnlJournals, pnlGrades }) p.Visible = p == target;
        foreach (Control c in _sidebar.Controls)
            if (c is Button b) { b.BackColor = C_Sidebar; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 10f); }
        nav.BackColor = C_Active;
        nav.ForeColor = Color.White;
        nav.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
    }

    static Button NavBtn(string text) => new()
    {
        Text      = "  " + text,
        Dock      = DockStyle.Top,
        Height    = 48,
        FlatStyle = FlatStyle.Flat,
        ForeColor = Color.FromArgb(148, 163, 184),
        BackColor = Color.FromArgb(15, 23, 41),
        Font      = new Font("Segoe UI", 10f),
        TextAlign = ContentAlignment.MiddleLeft,
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(30, 41, 59) }
    };

    static Button ActionBtn(string text, Color color) => new()
    {
        Text      = text, BackColor = color, ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat, Height = 36, Width = 134,
        Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };

    static Panel PageTitle(string t, string sub)
    {
        var p = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = C_Bg };
        p.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 9.5f),               ForeColor = Color.FromArgb(100, 116, 139), BackColor = C_Bg, Location = new Point(2, 40), AutoSize = true });
        p.Controls.Add(new Label { Text = t,   Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42),    BackColor = C_Bg, Location = new Point(0, 4),  AutoSize = true });
        return p;
    }

    static Panel CardPanel()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        p.Paint += BorderPaint(p);
        return p;
    }

    static PaintEventHandler BorderPaint(Panel p) => (_, e) =>
    {
        using var pen = new Pen(Color.FromArgb(226, 232, 240), 0.5f);
        e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
    };

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

    static DataGridView MakeGrid()
    {
        var d = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            RowHeadersVisible = false, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(226, 232, 240), Font = new Font("Segoe UI", 10f),
            RowTemplate = { Height = 42 }
        };
        var bg = Color.FromArgb(248, 250, 252);
        d.ColumnHeadersDefaultCellStyle.BackColor          = bg;
        d.ColumnHeadersDefaultCellStyle.ForeColor          = Color.FromArgb(100, 116, 139);
        d.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Bold);
        d.ColumnHeadersDefaultCellStyle.SelectionBackColor = bg;
        d.ColumnHeadersDefaultCellStyle.Padding            = new Padding(10, 0, 0, 0);
        d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        d.ColumnHeadersHeight      = 44;
        d.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        d.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(239, 246, 255);
        d.DefaultCellStyle.SelectionForeColor       = Color.FromArgb(15, 23, 42);
        d.DefaultCellStyle.Padding                  = new Padding(10, 0, 10, 0);
        return d;
    }

    void ColourGrades(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvGrades.Columns[e.ColumnIndex].Name != "Grade") return;
        if (e.Value is not string s) return;
        if (!double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v)) return;
        e.CellStyle.ForeColor = GradeColor(v);
        e.CellStyle.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
    }

    static Color GradeColor(double v) =>
        v >= 5.0 ? Color.FromArgb(16, 185, 129)
      : v >= 4.0 ? Color.FromArgb(59, 130, 246)
      : v >= 3.0 ? Color.FromArgb(245, 158, 11)
      :            Color.FromArgb(239, 68, 68);

    static void Info(string msg) =>
        MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
