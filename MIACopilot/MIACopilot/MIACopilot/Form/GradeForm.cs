using MIACopilot.Models;
using MIACopilot.Services;
using System.Drawing.Drawing2D;

namespace MIACopilot.Forms;

/// <summary>
/// Grade tool — modern split layout: grade list left, stats + bar chart right.
/// </summary>
public class GradeForm : Form
{
    private readonly ApprenticeService _apprenticeService;
    private readonly GradeService      _gradeService;

    private ComboBox     cmbApprentice  = new();
    private ComboBox     cmbFilter      = new();
    private DataGridView dgvGrades      = new();
    private Label        lblAvg         = new();
    private Label        lblStatus      = new();
    private Label        lblCount       = new();
    private Panel        pnlChart       = new();

    private Apprentice? _current;

    static readonly Color C_Bg     = Color.FromArgb(248, 250, 252);
    static readonly Color C_White  = Color.White;
    static readonly Color C_Border = Color.FromArgb(226, 232, 240);
    static readonly Color C_Text   = Color.FromArgb(15, 23, 42);
    static readonly Color C_Muted  = Color.FromArgb(100, 116, 139);
    static readonly Color C_Green  = Color.FromArgb(16, 185, 129);
    static readonly Color C_Blue   = Color.FromArgb(59, 130, 246);
    static readonly Color C_Orange = Color.FromArgb(245, 158, 11);
    static readonly Color C_Red    = Color.FromArgb(239, 68, 68);
    static readonly Color C_Purple = Color.FromArgb(139, 92, 246);

    public GradeForm(ApprenticeService apprenticeService, GradeService gradeService, int preSelectId)
    {
        _apprenticeService = apprenticeService;
        _gradeService      = gradeService;
        BuildUI();
        LoadApprenticeDropdown(preSelectId);
    }

    // ═══════════════════════ BUILD ════════════════════════════════════════════

    void BuildUI()
    {
        Text            = "📊  Grade Tool";
        Size            = new Size(1060, 680);
        MinimumSize     = new Size(860, 540);
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = C_Bg;

        // ── Header bar ──────────────────────────────────────────────────────
        var header = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(15, 23, 42) };

        var lblTitle = new Label
        {
            Text      = "📊  Grade Tool",
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White,
            Location  = new Point(20, 16),
            AutoSize  = true
        };

        var lblPick = new Label
        {
            Text      = "Apprentice:",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font      = new Font("Segoe UI", 9.5f),
            Location  = new Point(220, 21),
            AutoSize  = true
        };

        cmbApprentice = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width         = 260,
            Font          = new Font("Segoe UI", 10f),
            Location      = new Point(310, 17),
            BackColor     = Color.FromArgb(30, 41, 59),
            ForeColor     = Color.White
        };
        cmbApprentice.SelectedIndexChanged += (_, _) => OnApprenticeChanged();

        header.Controls.Add(lblTitle);
        header.Controls.Add(lblPick);
        header.Controls.Add(cmbApprentice);

        // ── Left: grade list ─────────────────────────────────────────────────
        var left = new Panel { Dock = DockStyle.Left, Width = 620, BackColor = C_White, Padding = new Padding(0) };

        // Toolbar
        var bar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_White };
        bar.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
        };

        var btnAdd    = Btn("➕ Add",    C_Green);
        var btnEdit   = Btn("✏️ Edit",   C_Blue);
        var btnDelete = Btn("🗑 Delete", C_Red);

        btnAdd.Location    = new Point(16, 11);
        btnEdit.Location   = new Point(148, 11);
        btnDelete.Location = new Point(280, 11);

        btnAdd.Click    += (_, _) => AddGrade();
        btnEdit.Click   += (_, _) => EditGrade();
        btnDelete.Click += (_, _) => DeleteGrade();

        var lblFilter = new Label { Text = "Filter:", Location = new Point(416, 18), AutoSize = true, ForeColor = C_Muted };
        cmbFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width         = 160,
            Location      = new Point(460, 14),
            Font          = new Font("Segoe UI", 9.5f)
        };
        cmbFilter.SelectedIndexChanged += (_, _) => LoadGrades();

        bar.Controls.Add(btnAdd);
        bar.Controls.Add(btnEdit);
        bar.Controls.Add(btnDelete);
        bar.Controls.Add(lblFilter);
        bar.Controls.Add(cmbFilter);

        // Grade grid with colour formatting
        dgvGrades = MakeGrid();
        dgvGrades.CellFormatting += ColourGrades;

        left.Controls.Add(dgvGrades);
        left.Controls.Add(bar);

        // ── Right: stats + chart ─────────────────────────────────────────────
        var right = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(16, 0, 0, 0) };

        // Average card
        var avgCard = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = C_White, Margin = new Padding(0, 0, 0, 12) };
        avgCard.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawRectangle(pen, 0, 0, avgCard.Width - 1, avgCard.Height - 1);
        };

        var lblAvgTitle = new Label
        {
            Text      = "Overall Average",
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = C_Muted,
            Dock      = DockStyle.Top,
            Height    = 32,
            TextAlign = ContentAlignment.BottomCenter,
            BackColor = C_White
        };

        lblAvg = new Label
        {
            Text      = "—",
            Font      = new Font("Segoe UI", 34f, FontStyle.Bold),
            ForeColor = C_Blue,
            Dock      = DockStyle.Top,
            Height    = 56,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = C_White
        };

        lblStatus = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = C_Muted,
            Dock      = DockStyle.Top,
            Height    = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = C_White
        };

        lblCount = new Label
        {
            Text      = "0 grades",
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = C_Muted,
            Dock      = DockStyle.Top,
            Height    = 22,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = C_White
        };

        avgCard.Controls.Add(lblCount);
        avgCard.Controls.Add(lblStatus);
        avgCard.Controls.Add(lblAvg);
        avgCard.Controls.Add(lblAvgTitle);

        // Chart card
        var chartTitle = new Label
        {
            Text      = "Average per Subject",
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = C_Muted,
            Dock      = DockStyle.Top,
            Height    = 32,
            TextAlign = ContentAlignment.BottomCenter,
            BackColor = C_White,
            Padding   = new Padding(0, 8, 0, 0)
        };

        pnlChart = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
        pnlChart.Paint += DrawChart;
        pnlChart.Resize += (_, _) => pnlChart.Invalidate();

        var chartCard = new Panel { Dock = DockStyle.Fill, BackColor = C_White, Margin = new Padding(0, 12, 0, 0) };
        chartCard.Paint += (_, e) =>
        {
            using var pen = new Pen(C_Border);
            e.Graphics.DrawRectangle(pen, 0, 0, chartCard.Width - 1, chartCard.Height - 1);
        };
        chartCard.Controls.Add(pnlChart);
        chartCard.Controls.Add(chartTitle);

        right.Controls.Add(chartCard);
        right.Controls.Add(avgCard);

        // ── Assemble ─────────────────────────────────────────────────────────
        Controls.Add(right);
        Controls.Add(left);
        Controls.Add(header);
    }

    // ═══════════════════════ DATA ═════════════════════════════════════════════

    void LoadApprenticeDropdown(int preSelectId)
    {
        cmbApprentice.DisplayMember = "FullName";
        cmbApprentice.ValueMember   = "Id";
        var list = _apprenticeService.GetAll();
        cmbApprentice.DataSource    = list;

        if (preSelectId > 0)
        {
            var idx = list.FindIndex(a => a.Id == preSelectId);
            if (idx >= 0) cmbApprentice.SelectedIndex = idx;
        }
        else if (list.Any())
            cmbApprentice.SelectedIndex = 0;
    }

    void OnApprenticeChanged()
    {
        if (cmbApprentice.SelectedValue == null) return;
        _current = _apprenticeService.GetById((int)cmbApprentice.SelectedValue);
        RefreshFilter();
        LoadGrades();
    }

    void RefreshFilter()
    {
        cmbFilter.SelectedIndexChanged -= (_, _) => LoadGrades();
        cmbFilter.Items.Clear();
        cmbFilter.Items.Add("All subjects");
        if (_current != null)
            foreach (var s in _gradeService.GetSubjects(_current.Id))
                cmbFilter.Items.Add(s);
        cmbFilter.SelectedIndex         = 0;
        cmbFilter.SelectedIndexChanged += (_, _) => LoadGrades();
    }

    void LoadGrades()
    {
        if (_current == null) return;

        var grades = (cmbFilter.SelectedIndex > 0 && cmbFilter.SelectedItem is string s)
            ? _gradeService.FilterBySubject(_current.Id, s)
            : _gradeService.GetByApprentice(_current.Id);

        dgvGrades.DataSource = grades
            .OrderByDescending(g => g.Date)
            .Select(g => new
            {
                g.Id,
                g.Subject,
                Grade    = g.FormattedValue,
                g.Type,
                Date     = g.Date.ToString("dd.MM.yyyy"),
                Status   = g.Category,
                g.Notes
            }).ToList();

        UpdateStats();
        pnlChart.Invalidate();
    }

    void UpdateStats()
    {
        if (_current == null) { lblAvg.Text = "—"; lblStatus.Text = ""; lblCount.Text = ""; return; }

        var all = _gradeService.GetByApprentice(_current.Id);
        lblCount.Text = $"{all.Count} grade{(all.Count == 1 ? "" : "s")} total";

        var avg = _gradeService.GetAverage(_current.Id);
        if (avg == 0) { lblAvg.Text = "—"; lblStatus.Text = "No grades yet"; lblStatus.ForeColor = C_Muted; return; }

        lblAvg.Text      = $"Ø {avg:0.00}";
        lblAvg.ForeColor = GradeColor(avg);
        lblStatus.Text      = avg >= 5.0 ? "🌟 Excellent"
                            : avg >= 4.0 ? "✅ Passed"
                            : avg >= 3.0 ? "⚠️ Sufficient"
                            :              "❌ Failed";
        lblStatus.ForeColor = GradeColor(avg);
    }

    // ═══════════════════════ CHART ════════════════════════════════════════════

    void DrawChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(C_White);

        if (_current == null) return;
        var avgs = _gradeService.GetAveragePerSubject(_current.Id);
        if (!avgs.Any())
        {
            using var f = new Font("Segoe UI", 9.5f);
            using var b = new SolidBrush(C_Muted);
            var msg = "No grades to display";
            var sz  = g.MeasureString(msg, f);
            g.DrawString(msg, f, b, (pnlChart.Width - sz.Width) / 2, (pnlChart.Height - sz.Height) / 2);
            return;
        }

        int   pL = 44, pB = 48, pT = 20, pR = 16;
        float cW = pnlChart.Width  - pL - pR;
        float cH = pnlChart.Height - pT - pB;
        int   n  = avgs.Count;
        float bW = Math.Min(52f, cW / n - 12);
        float sp = cW / n;

        // Grid lines + Y labels
        using var gPen  = new Pen(Color.FromArgb(240, 244, 248));
        using var aFont = new Font("Segoe UI", 8f);
        using var aBrush = new SolidBrush(C_Muted);
        for (int gr = 1; gr <= 6; gr++)
        {
            float y = pT + cH - cH * gr / 6f;
            g.DrawLine(gPen, pL, y, pL + cW, y);
            g.DrawString(gr.ToString(), aFont, aBrush, 4, y - 8);
        }

        // Bars
        int i = 0;
        foreach (var (subj, avg) in avgs)
        {
            float barH = (float)(cH * avg / 6.0);
            float x    = pL + i * sp + (sp - bW) / 2f;
            float y    = pT + cH - barH;
            var   clr  = GradeColor(avg);

            using var br = new LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + barH),
                clr, Color.FromArgb(180, clr));

            var path = RoundedTop(x, y, bW, barH, 6f);
            g.FillPath(br, path);

            // Value label
            using var vFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var vStr = avg.ToString("0.0");
            var vSz  = g.MeasureString(vStr, vFont);
            g.DrawString(vStr, vFont, new SolidBrush(C_Text), x + (bW - vSz.Width) / 2f, y - vSz.Height - 2);

            // Subject label
            using var sFont = new Font("Segoe UI", 7.5f);
            var sStr = subj.Length > 9 ? subj[..9] + "…" : subj;
            var sSz  = g.MeasureString(sStr, sFont);
            g.DrawString(sStr, sFont, aBrush, x + (bW - sSz.Width) / 2f, pT + cH + 8);
            i++;
        }

        // X-axis
        using var axPen = new Pen(C_Border, 1.5f);
        g.DrawLine(axPen, pL, pT + cH, pL + cW, pT + cH);
    }

    static GraphicsPath RoundedTop(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath();
        p.AddArc(x, y, r, r, 180, 90);
        p.AddArc(x + w - r, y, r, r, 270, 90);
        p.AddLine(x + w, y + h, x, y + h);
        p.CloseFigure();
        return p;
    }

    // ═══════════════════════ CRUD ═════════════════════════════════════════════

    void AddGrade()
    {
        if (_current == null) { Info("Select an apprentice first."); return; }
        using var f = new GradeDetailForm(null, _current.Id);
        if (f.ShowDialog() == DialogResult.OK) { _gradeService.Create(f.Result!); RefreshFilter(); LoadGrades(); }
    }

    void EditGrade()
    {
        var id = GetId(); if (id < 0) return;
        var gr = _gradeService.GetById(id); if (gr == null) return;
        using var f = new GradeDetailForm(gr, gr.ApprenticeId);
        if (f.ShowDialog() == DialogResult.OK) { _gradeService.Update(f.Result!); LoadGrades(); }
    }

    void DeleteGrade()
    {
        var id = GetId(); if (id < 0) return;
        var gr = _gradeService.GetById(id); if (gr == null) return;
        if (MessageBox.Show($"Delete '{gr.Subject} {gr.FormattedValue}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        { _gradeService.Delete(id); RefreshFilter(); LoadGrades(); }
    }

    // ═══════════════════════ HELPERS ═════════════════════════════════════════

    void ColourGrades(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvGrades.Columns[e.ColumnIndex].Name != "Grade") return;
        if (e.Value is not string s) return;
        if (!double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v)) return;

        e.CellStyle.ForeColor = GradeColor(v);
        e.CellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
    }

    static Color GradeColor(double v) =>
        v >= 5.0 ? Color.FromArgb(16, 185, 129)   // green
      : v >= 4.0 ? Color.FromArgb(59, 130, 246)   // blue
      : v >= 3.0 ? Color.FromArgb(245, 158, 11)   // orange
      :            Color.FromArgb(239, 68, 68);    // red

    int GetId()
    {
        if (dgvGrades.SelectedRows.Count == 0) { Info("Select a grade first."); return -1; }
        return (int)dgvGrades.SelectedRows[0].Cells["Id"].Value;
    }

    static void Info(string msg) =>
        MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    static Button Btn(string text, Color color) => new()
    {
        Text      = text,
        BackColor = color,
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Height    = 34,
        Width     = 124,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };

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
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor             = Color.FromArgb(226, 232, 240),
            Font                  = new Font("Segoe UI", 9.5f),
            RowTemplate           = { Height = 40 }
        };
        var bgBg = Color.FromArgb(248, 250, 252);
        d.ColumnHeadersDefaultCellStyle.BackColor         = bgBg;
        d.ColumnHeadersDefaultCellStyle.ForeColor         = Color.FromArgb(100, 116, 139);
        d.ColumnHeadersDefaultCellStyle.Font              = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        d.ColumnHeadersDefaultCellStyle.SelectionBackColor = bgBg;
        d.ColumnHeadersDefaultCellStyle.Padding           = new Padding(6, 0, 0, 0);
        d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        d.ColumnHeadersHeight      = 42;
        d.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
        d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
        d.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        d.DefaultCellStyle.Padding            = new Padding(8, 0, 8, 0);
        return d;
    }
}
