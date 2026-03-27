using MIACopilot.Models;
using MIACopilot.Services;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MIACopilot.Forms;

/// <summary>Personal portal for a logged-in vocational trainer.</summary>
public class TrainerPortal : Form
{
    private readonly VocationalTrainer        _me;
    private readonly ApprenticeService        _apprenticeService;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;
    private readonly GradeService             _gradeService;

    private Panel _sidebar = new();

    private Button btnProfile    = new();
    private Button btnApprentices = new();

    private Panel pnlProfile     = new();
    private Panel pnlApprentices = new();

    private DataGridView dgvApprentices = new();
    private DataGridView dgvJournals    = new();
    private DataGridView dgvGrades      = new();

    private Panel       pnlDetail = new();
    private Apprentice? _selected;

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

        BuildUI();
        ShowPanel(pnlProfile, btnProfile);
    }

    void BuildUI()
    {
        Text          = $"MIA Copilot — {_me.FullName} (Trainer)";
        Size          = new Size(1120, 700);
        MinimumSize   = new Size(900, 580);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = C_Bg;
        Font          = new Font("Segoe UI", 10f);

        // ── Sidebar ───────────────────────────────────────────────────────────
        _sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = C_Sidebar };

        var avatarPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = C_Sidebar };
        avatarPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Avatar circle (purple for trainer)
            using var avBrush = new SolidBrush(C_Purple);
            g.FillEllipse(avBrush, 20, 20, 58, 58);
            using var initFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            var init = ((string.IsNullOrEmpty(_me.FirstName) ? "?" : _me.FirstName[0].ToString()) +
                        (string.IsNullOrEmpty(_me.LastName)  ? "?" : _me.LastName[0].ToString())).ToUpper();
            var sz   = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White, 20 + (58 - sz.Width) / 2, 20 + (58 - sz.Height) / 2);

            using var nameFont  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            using var subFont   = new Font("Segoe UI", 8.5f);
            using var muteBrush = new SolidBrush(C_Muted);
            g.DrawString(_me.FullName, nameFont, Brushes.White, 88, 28);
            g.DrawString("Vocational Trainer", subFont, muteBrush, 88, 52);
            g.DrawString($"@{_me.Username}", subFont, muteBrush, 88, 70);
        };

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

        btnProfile     = NavBtn("👤   My Profile");
        btnApprentices = NavBtn("🎓   My Apprentices");
        btnProfile.Click     += (_, _) => ShowPanel(pnlProfile,     btnProfile);
        btnApprentices.Click += (_, _) => ShowPanel(pnlApprentices, btnApprentices);

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
        _sidebar.Controls.Add(btnApprentices);
        _sidebar.Controls.Add(btnProfile);
        _sidebar.Controls.Add(sep);
        _sidebar.Controls.Add(avatarPanel);

        // ── Content ───────────────────────────────────────────────────────────
        var content = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(28, 24, 28, 24) };

        BuildProfilePanel();
        BuildApprenticesPanel();

        foreach (var p in new[] { pnlProfile, pnlApprentices })
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

        var title = PageTitle("My Profile", "Your trainer information");

        // Profile banner
        var banner = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = C_Sidebar };
        banner.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var b1 = new SolidBrush(Color.FromArgb(20, 139, 92, 246));
            g.FillEllipse(b1, banner.Width - 120, -40, 160, 160);

            using var avBrush = new SolidBrush(C_Purple);
            g.FillEllipse(avBrush, 28, 30, 72, 72);
            using var initFont = new Font("Segoe UI", 24f, FontStyle.Bold);
            var init = ((string.IsNullOrEmpty(_me.FirstName) ? "?" : _me.FirstName[0].ToString()) +
                        (string.IsNullOrEmpty(_me.LastName)  ? "?" : _me.LastName[0].ToString())).ToUpper();
            var sz   = g.MeasureString(init, initFont);
            g.DrawString(init, initFont, Brushes.White, 28 + (72 - sz.Width) / 2, 30 + (72 - sz.Height) / 2);

            using var nameFont  = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var roleFont  = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var subFont   = new Font("Segoe UI", 8.5f);
            using var purpleBr  = new SolidBrush(C_Purple);
            using var muteBr    = new SolidBrush(Color.FromArgb(148, 163, 184));
            g.DrawString(_me.FullName, nameFont, Brushes.White, 116, 38);
            g.DrawString("● VOCATIONAL TRAINER", roleFont, purpleBr, 118, 66);
            g.DrawString($"@{_me.Username}", subFont, muteBr, 118, 88);
        };

        var company       = _companyService.GetById(_me.CompanyId);
        var myApprentices = _apprenticeService.GetAll().Where(a => a.VocationalTrainerId == _me.Id).ToList();

        var fields = new[]
        {
            ("First Name",       _me.FirstName),
            ("Last Name",        _me.LastName),
            ("Email",            _me.Email),
            ("Phone",            _me.Phone),
            ("Company",          company?.Name ?? "—"),
            ("My Apprentices",   myApprentices.Count.ToString()),
        };

        var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
        card.Paint += BorderPaint(card);

        int y = 22;
        foreach (var (lbl, val) in fields)
        {
            card.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = C_Muted, Location = new Point(24, y), Size = new Size(170, 22), TextAlign = ContentAlignment.MiddleLeft });
            card.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI", 10.5f),                ForeColor = C_Text,  Location = new Point(200, y), Size = new Size(400, 22), TextAlign = ContentAlignment.MiddleLeft });
            y += 36;
        }

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
                MessageBox.Show("PIN changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        card.Controls.Add(btnPin);

        pnlProfile.Controls.Add(card);
        pnlProfile.Controls.Add(banner);
        pnlProfile.Controls.Add(title);
    }

    // ═══════════════════════ APPRENTICES ══════════════════════════════════════

    void BuildApprenticesPanel()
    {
        pnlApprentices.BackColor = C_Bg;

        var title = PageTitle("My Apprentices", "View journals — add and manage grades for your apprentices");

        var split = new SplitContainer
        {
            Dock             = DockStyle.Fill,
            SplitterDistance = 350,
            BackColor        = C_Bg,
            Panel1MinSize    = 260,
            Panel2MinSize    = 380,
            SplitterWidth    = 8,
            BorderStyle      = BorderStyle.None
        };

        // LEFT: apprentice list
        var leftCard  = CardPanel();
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
        leftTitle.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, leftTitle.Height - 1, leftTitle.Width, leftTitle.Height - 1);
        };

        dgvApprentices = MakeGrid();
        dgvApprentices.SelectionChanged += OnApprenticeSelected;

        leftCard.Controls.Add(dgvApprentices);
        leftCard.Controls.Add(leftTitle);
        split.Panel1.Controls.Add(leftCard);

        // RIGHT: detail panel
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

        pnlApprentices.Controls.Add(split);
        pnlApprentices.Controls.Add(title);

        LoadMyApprentices();
    }

    void LoadMyApprentices()
    {
        var list = _apprenticeService.GetAll()
            .Where(a => a.VocationalTrainerId == _me.Id).ToList();

        dgvApprentices.DataSource = list.Select(a => new
        {
            a.Id,
            a.FirstName,
            a.LastName,
            Start    = a.StartDate.ToString("dd.MM.yyyy"),
            Journals = a.WorkJournals.Count,
            Grades   = _gradeService.GetByApprentice(a.Id).Count,
            Avg      = _gradeService.GetAverage(a.Id) is double d && d > 0 ? d.ToString("0.0") : "—"
        }).ToList();

        AutoSizeCols(dgvApprentices, "LastName");
    }

    void OnApprenticeSelected(object? sender, EventArgs e)
    {
        if (dgvApprentices.SelectedRows.Count == 0) return;
        var id = (int)dgvApprentices.SelectedRows[0].Cells["Id"].Value;
        _selected = _apprenticeService.GetById(id);
        if (_selected == null) return;
        ShowDetailPanel();
    }

    void ShowDetailPanel()
    {
        if (_selected == null) return;
        pnlDetail.Controls.Clear();

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

        var tabs       = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
        var tJournals  = new TabPage { Text = "📓  Journals", BackColor = C_White };
        var tGrades    = new TabPage { Text = "📊  Grades",   BackColor = C_White };

        // ── Journals tab (read-only for trainer) ─────────────────────────────
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

        // ── Grades tab (trainer: full CRUD) ───────────────────────────────────
        var gradeToolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_White };
        gradeToolbar.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, gradeToolbar.Height - 1, gradeToolbar.Width, gradeToolbar.Height - 1);
        };

        var btnAdd    = ActionBtn("➕  Add Grade",   C_Green);
        var btnEdit   = ActionBtn("✏️  Edit Grade",  C_Blue);
        var btnDelete = ActionBtn("🗑  Delete",      C_Red);
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

        pnlDetail.Controls.Add(tabs);
        pnlDetail.Controls.Add(hdr);
    }

    void LoadGradesForSelected()
    {
        if (_selected == null) return;
        dgvGrades.DataSource = _gradeService.GetByApprentice(_selected.Id)
            .OrderByDescending(g => g.Date)
            .Select(g => new
            {
                g.Id, g.Subject,
                Grade  = g.FormattedValue,
                g.Type,
                Date   = g.Date.ToString("dd.MM.yyyy"),
                Status = g.Category,
                g.Notes
            }).ToList();
        AutoSizeCols(dgvGrades, "Notes");
    }

    void AddGrade()
    {
        if (_selected == null) return;
        using var f = new GradeDetailForm(null, _selected.Id);
        if (f.ShowDialog() == DialogResult.OK)
        { _gradeService.Create(f.Result!); LoadGradesForSelected(); LoadMyApprentices(); }
    }

    void EditGrade()
    {
        if (dgvGrades.SelectedRows.Count == 0) { Info("Select a grade first."); return; }
        var id = (int)dgvGrades.SelectedRows[0].Cells["Id"].Value;
        var gr = _gradeService.GetById(id); if (gr == null) return;
        using var f = new GradeDetailForm(gr, gr.ApprenticeId);
        if (f.ShowDialog() == DialogResult.OK) { _gradeService.Update(f.Result!); LoadGradesForSelected(); }
    }

    void DeleteGrade()
    {
        if (dgvGrades.SelectedRows.Count == 0) { Info("Select a grade first."); return; }
        var id = (int)dgvGrades.SelectedRows[0].Cells["Id"].Value;
        var gr = _gradeService.GetById(id); if (gr == null) return;
        if (MessageBox.Show($"Delete grade '{gr.Subject} {gr.FormattedValue}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        { _gradeService.Delete(id); LoadGradesForSelected(); LoadMyApprentices(); }
    }

    // ═══════════════════════ HELPERS ══════════════════════════════════════════

    void ShowPanel(Panel target, Button nav)
    {
        foreach (var p in new[] { pnlProfile, pnlApprentices }) p.Visible = p == target;
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
        Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
        Height = 36, Width = 134, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        Cursor = Cursors.Hand, FlatAppearance = { BorderSize = 0 }
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
        e.CellStyle.ForeColor = v >= 5.0 ? Color.FromArgb(16, 185, 129)
                              : v >= 4.0 ? Color.FromArgb(59, 130, 246)
                              : v >= 3.0 ? Color.FromArgb(245, 158, 11)
                              :            Color.FromArgb(239, 68, 68);
        e.CellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
    }

    static void Info(string msg) =>
        MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
