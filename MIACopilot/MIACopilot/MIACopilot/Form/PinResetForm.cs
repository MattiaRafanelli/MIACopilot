using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Three-step PIN reset dialog.
/// Step 1 — Verify identity (username + email or phone).
/// Step 2 — Enter and confirm a new PIN.
/// Step 3 — Confirmation.
/// </summary>
public class PinResetForm : Form
{
    private readonly ApprenticeService        _apprenticeService;
    private readonly VocationalTrainerService _trainerService;
    private readonly CompanyService           _companyService;

    // Matched user (exactly one will be non-null after step 1 succeeds)
    private Apprentice?        _apprentice;
    private VocationalTrainer? _trainer;
    private Company?           _company;

    // Step panels
    private readonly Panel _pStep1 = new();
    private readonly Panel _pStep2 = new();
    private readonly Panel _pStep3 = new();

    // Step 1 inputs
    private readonly TextBox txtUsername = new();
    private readonly TextBox txtIdentity = new();
    private readonly Label   lblErr1     = new();

    // Step 2 inputs
    private readonly TextBox txtNewPin     = new();
    private readonly TextBox txtConfirmPin = new();
    private readonly Label   lblErr2       = new();

    // Step indicator labels (three dots)
    private Label lblDot1 = new();
    private Label lblDot2 = new();
    private Label lblDot3 = new();

    // ── Theme (matches LoginForm) ─────────────────────────────────────────
    static readonly Color C_Dark   = Color.FromArgb(10, 25, 47);
    static readonly Color C_Accent = Color.FromArgb(0, 180, 216);
    static readonly Color C_White  = Color.White;
    static readonly Color C_Input  = Color.FromArgb(248, 250, 252);
    static readonly Color C_Text   = Color.FromArgb(15, 23, 42);
    static readonly Color C_Muted  = Color.FromArgb(100, 116, 139);
    static readonly Color C_Error  = Color.FromArgb(239, 68, 68);
    static readonly Color C_Green  = Color.FromArgb(16, 185, 129);
    static readonly Color C_Dim    = Color.FromArgb(203, 213, 225);

    public PinResetForm(
        ApprenticeService        apprenticeService,
        VocationalTrainerService trainerService,
        CompanyService           companyService)
    {
        _apprenticeService = apprenticeService;
        _trainerService    = trainerService;
        _companyService    = companyService;
        BuildUI();
        ShowStep(1);
    }

    // ── Layout ────────────────────────────────────────────────────────────

    void BuildUI()
    {
        Text            = "MIA Copilot — Reset PIN";
        Size            = new Size(460, 530);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 10f);
        BackColor       = C_White;

        // ── Header bar ────────────────────────────────────────────────────
        var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = C_Dark };
        header.Paint += PaintHeader;

        // ── Step indicator strip ──────────────────────────────────────────
        var strip = BuildStepStrip();

        // ── Content steps ─────────────────────────────────────────────────
        BuildStep1Panel();
        BuildStep2Panel();
        BuildStep3Panel();

        // z-order: strip above steps, header on top
        Controls.Add(_pStep3);
        Controls.Add(_pStep2);
        Controls.Add(_pStep1);
        Controls.Add(strip);
        Controls.Add(header);
    }

    void PaintHeader(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var f1 = new Font("Segoe UI", 16f, FontStyle.Bold);
        g.DrawString("Reset PIN", f1, Brushes.White, 28, 14);
        using var f2 = new Font("Segoe UI", 9f);
        using var b  = new SolidBrush(Color.FromArgb(148, 163, 184));
        g.DrawString("MIA Copilot · Account Recovery", f2, b, 30, 44);
    }

    Panel BuildStepStrip()
    {
        var strip = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = C_White };

        // Three dots + two connecting lines
        lblDot1 = MakeDot(); lblDot1.Left = 110;
        lblDot2 = MakeDot(); lblDot2.Left = 210;
        lblDot3 = MakeDot(); lblDot3.Left = 310;

        var line1 = new Panel { Left = 134, Top = 20, Width = 72, Height = 2, BackColor = C_Dim };
        var line2 = new Panel { Left = 234, Top = 20, Width = 72, Height = 2, BackColor = C_Dim };

        var lbl1 = StepLabel("Verify Identity", 76);
        var lbl2 = StepLabel("New PIN",         186);
        var lbl3 = StepLabel("Done",            298);

        strip.Controls.AddRange([line2, line1, lbl3, lbl2, lbl1, lblDot3, lblDot2, lblDot1]);
        return strip;
    }

    Label MakeDot()
    {
        var l = new Label
        {
            Size      = new Size(22, 22),
            Top       = 9,
            Text      = "",
            BackColor = C_Dim,
            ForeColor = C_White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };
        l.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var b = new SolidBrush(l.BackColor);
            g.FillEllipse(b, 0, 0, l.Width - 1, l.Height - 1);
            using var tf = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var tb = new SolidBrush(l.ForeColor);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(l.Text, tf, tb, new RectangleF(0, 0, l.Width, l.Height), sf);
        };
        return l;
    }

    Label StepLabel(string text, int left) => new()
    {
        Text      = text,
        Left      = left,
        Top       = 32,
        Width     = 90,
        Height    = 16,
        Font      = new Font("Segoe UI", 7.5f),
        ForeColor = C_Muted,
        TextAlign = ContentAlignment.TopCenter
    };

    // ── Step 1: Verify Identity ───────────────────────────────────────────

    void BuildStep1Panel()
    {
        _pStep1.Dock      = DockStyle.Fill;
        _pStep1.BackColor = C_White;
        _pStep1.Padding   = new Padding(44, 16, 44, 16);

        var lblTitle = Title("Verify Your Identity");
        var lblSub   = Sub("Enter your username and your registered email\nor phone number to confirm your identity.");

        var lblU = FieldLabel("Username");
        txtUsername.Dock        = DockStyle.Top;
        txtUsername.Height      = 42;
        txtUsername.Font        = new Font("Segoe UI", 10.5f);
        txtUsername.BackColor   = C_Input;
        txtUsername.ForeColor   = C_Text;
        txtUsername.BorderStyle = BorderStyle.FixedSingle;
        txtUsername.PlaceholderText = "Your username";

        var lblI = FieldLabel("Email or Phone");
        txtIdentity.Dock        = DockStyle.Top;
        txtIdentity.Height      = 42;
        txtIdentity.Font        = new Font("Segoe UI", 10.5f);
        txtIdentity.BackColor   = C_Input;
        txtIdentity.ForeColor   = C_Text;
        txtIdentity.BorderStyle = BorderStyle.FixedSingle;
        txtIdentity.PlaceholderText = "Registered email or phone number";

        lblErr1.Dock      = DockStyle.Top;
        lblErr1.Height    = 22;
        lblErr1.Font      = new Font("Segoe UI", 9f);
        lblErr1.ForeColor = C_Error;

        var btnContinue = PrimaryButton("Continue →");
        btnContinue.Click += OnStep1Continue;

        var btnCancel = SecondaryButton("Cancel");
        btnCancel.Click += (_, _) => Close();

        var btnRow = ButtonRow(btnCancel, btnContinue);

        _pStep1.Controls.Add(btnRow);
        _pStep1.Controls.Add(Spacer(8));
        _pStep1.Controls.Add(lblErr1);
        _pStep1.Controls.Add(Spacer(6));
        _pStep1.Controls.Add(txtIdentity);
        _pStep1.Controls.Add(lblI);
        _pStep1.Controls.Add(Spacer(10));
        _pStep1.Controls.Add(txtUsername);
        _pStep1.Controls.Add(lblU);
        _pStep1.Controls.Add(Spacer(16));
        _pStep1.Controls.Add(lblSub);
        _pStep1.Controls.Add(Spacer(4));
        _pStep1.Controls.Add(lblTitle);
    }

    // ── Step 2: New PIN ───────────────────────────────────────────────────

    void BuildStep2Panel()
    {
        _pStep2.Dock      = DockStyle.Fill;
        _pStep2.BackColor = C_White;
        _pStep2.Padding   = new Padding(44, 16, 44, 16);
        _pStep2.Visible   = false;

        var lblTitle = Title("Create a New PIN");
        var lblSub   = Sub("Must be exactly 4 digits.\nNo repeating (1111) or sequential (1234 / 4321) patterns.");

        var lblN = FieldLabel("New PIN");
        txtNewPin.Dock         = DockStyle.Top;
        txtNewPin.Height       = 42;
        txtNewPin.Font         = new Font("Segoe UI", 10.5f);
        txtNewPin.BackColor    = C_Input;
        txtNewPin.ForeColor    = C_Text;
        txtNewPin.BorderStyle  = BorderStyle.FixedSingle;
        txtNewPin.PasswordChar = '●';
        txtNewPin.MaxLength    = 4;
        txtNewPin.PlaceholderText = "● ● ● ●";
        txtNewPin.KeyPress    += NumericOnly;

        var lblC = FieldLabel("Confirm New PIN");
        txtConfirmPin.Dock         = DockStyle.Top;
        txtConfirmPin.Height       = 42;
        txtConfirmPin.Font         = new Font("Segoe UI", 10.5f);
        txtConfirmPin.BackColor    = C_Input;
        txtConfirmPin.ForeColor    = C_Text;
        txtConfirmPin.BorderStyle  = BorderStyle.FixedSingle;
        txtConfirmPin.PasswordChar = '●';
        txtConfirmPin.MaxLength    = 4;
        txtConfirmPin.PlaceholderText = "● ● ● ●";
        txtConfirmPin.KeyPress    += NumericOnly;

        lblErr2.Dock      = DockStyle.Top;
        lblErr2.Height    = 22;
        lblErr2.Font      = new Font("Segoe UI", 9f);
        lblErr2.ForeColor = C_Error;

        var btnReset = PrimaryButton("Reset PIN");
        btnReset.Click += OnStep2Reset;

        var btnBack = SecondaryButton("← Back");
        btnBack.Click += (_, _) => ShowStep(1);

        var btnRow = ButtonRow(btnBack, btnReset);

        _pStep2.Controls.Add(btnRow);
        _pStep2.Controls.Add(Spacer(8));
        _pStep2.Controls.Add(lblErr2);
        _pStep2.Controls.Add(Spacer(6));
        _pStep2.Controls.Add(txtConfirmPin);
        _pStep2.Controls.Add(lblC);
        _pStep2.Controls.Add(Spacer(10));
        _pStep2.Controls.Add(txtNewPin);
        _pStep2.Controls.Add(lblN);
        _pStep2.Controls.Add(Spacer(16));
        _pStep2.Controls.Add(lblSub);
        _pStep2.Controls.Add(Spacer(4));
        _pStep2.Controls.Add(lblTitle);
    }

    // ── Step 3: Confirmation ──────────────────────────────────────────────

    void BuildStep3Panel()
    {
        _pStep3.Dock      = DockStyle.Fill;
        _pStep3.BackColor = C_White;
        _pStep3.Padding   = new Padding(44, 24, 44, 16);
        _pStep3.Visible   = false;

        // Success icon (painted circle with checkmark)
        var icon = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 80,
            BackColor = C_White
        };
        icon.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int cx = icon.Width / 2, r = 28;
            using var b = new SolidBrush(C_Green);
            g.FillEllipse(b, cx - r, 10, r * 2, r * 2);
            using var p  = new Pen(C_White, 3f);
            g.DrawLine(p, cx - 12, 38, cx - 3, 48);
            g.DrawLine(p, cx - 3,  48, cx + 14, 28);
        };

        var lblTitle = Title("PIN Reset Successful");
        var lblSub   = Sub("Your PIN has been updated.\nYou can now sign in with your new PIN.");

        var btnReturn = PrimaryButton("Return to Login");
        btnReturn.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

        _pStep3.Controls.Add(btnReturn);
        _pStep3.Controls.Add(Spacer(20));
        _pStep3.Controls.Add(lblSub);
        _pStep3.Controls.Add(Spacer(8));
        _pStep3.Controls.Add(lblTitle);
        _pStep3.Controls.Add(Spacer(16));
        _pStep3.Controls.Add(icon);
    }

    // ── Step navigation ───────────────────────────────────────────────────

    void ShowStep(int step)
    {
        _pStep1.Visible = step == 1;
        _pStep2.Visible = step == 2;
        _pStep3.Visible = step == 3;

        UpdateDot(lblDot1, step >= 1, step > 1, "1");
        UpdateDot(lblDot2, step >= 2, step > 2, "2");
        UpdateDot(lblDot3, step >= 3, step > 3, "3");
    }

    void UpdateDot(Label dot, bool active, bool done, string number)
    {
        dot.BackColor = done ? C_Green : active ? C_Accent : C_Dim;
        dot.ForeColor = active || done ? C_White : C_Muted;
        dot.Text      = done ? "✓" : number;
        dot.Invalidate();
    }

    // ── Business logic ────────────────────────────────────────────────────

    void OnStep1Continue(object? sender, EventArgs e)
    {
        lblErr1.Text = "";
        var username = txtUsername.Text.Trim().ToLower();
        var identity = txtIdentity.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(identity))
        {
            lblErr1.Text = "⚠  Both fields are required.";
            return;
        }

        // Try to find matching user and verify identity
        _trainer    = null;
        _apprentice = null;
        _company    = null;

        _trainer = _trainerService.GetAll()
            .FirstOrDefault(t => t.Username.ToLower() == username
                && (t.Email.ToLower() == identity || t.Phone.ToLower() == identity));

        if (_trainer == null)
            _apprentice = _apprenticeService.GetAll()
                .FirstOrDefault(a => a.Username.ToLower() == username
                    && a.Email.ToLower() == identity);

        if (_trainer == null && _apprentice == null)
            _company = _companyService.GetAll()
                .FirstOrDefault(c => !string.IsNullOrEmpty(c.AdminUsername)
                    && c.AdminUsername.ToLower() == username
                    && (c.Email.ToLower() == identity || c.Phone.ToLower() == identity));

        if (_trainer == null && _apprentice == null && _company == null)
        {
            lblErr1.Text = "⚠  No account found. Check your username and contact details.";
            return;
        }

        txtNewPin.Clear();
        txtConfirmPin.Clear();
        lblErr2.Text = "";
        ShowStep(2);
        txtNewPin.Focus();
    }

    void OnStep2Reset(object? sender, EventArgs e)
    {
        lblErr2.Text = "";
        var pin     = txtNewPin.Text.Trim();
        var confirm = txtConfirmPin.Text.Trim();

        if (pin.Length != 4)
        {
            lblErr2.Text = "⚠  PIN must be exactly 4 digits.";
            return;
        }
        if (pin != confirm)
        {
            lblErr2.Text = "⚠  PINs do not match.";
            return;
        }
        if (IsWeakPin(pin))
        {
            lblErr2.Text = "⚠  PIN is too simple. Avoid repeating or sequential digits.";
            return;
        }

        // Apply new PIN
        if (_trainer != null)
        {
            _trainer.Pin = pin;
            _trainerService.Update(_trainer);
        }
        else if (_apprentice != null)
        {
            _apprentice.Pin = pin;
            _apprenticeService.Update(_apprentice);
        }
        else if (_company != null)
        {
            _company.AdminPin = pin;
            _companyService.Update(_company);
        }

        ShowStep(3);
    }

    // ── PIN validation ────────────────────────────────────────────────────

    /// Returns true when the PIN is too weak (all same digit, or ascending/descending run).
    static bool IsWeakPin(string pin)
    {
        if (pin.Length != 4) return true;
        if (pin.Distinct().Count() == 1) return true;   // 1111, 2222 …

        bool asc = true, desc = true;
        for (int i = 1; i < 4; i++)
        {
            if (pin[i] - pin[i - 1] != 1) asc  = false;
            if (pin[i - 1] - pin[i] != 1) desc = false;
        }
        return asc || desc;   // 1234, 4321
    }

    static void NumericOnly(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    Label Title(string t) => new()
    {
        Text      = t,
        Dock      = DockStyle.Top,
        Height    = 38,
        Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
        ForeColor = C_Text
    };

    Label Sub(string t) => new()
    {
        Text      = t,
        Dock      = DockStyle.Top,
        Height    = 44,
        Font      = new Font("Segoe UI", 9.5f),
        ForeColor = C_Muted
    };

    Label FieldLabel(string t) => new()
    {
        Text      = t,
        Dock      = DockStyle.Top,
        Height    = 22,
        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        ForeColor = C_Muted
    };

    Button PrimaryButton(string t)
    {
        var b = new Button
        {
            Text      = t,
            Height    = 44,
            Width     = 160,
            BackColor = C_Accent,
            ForeColor = C_White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.None
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    Button SecondaryButton(string t)
    {
        var b = new Button
        {
            Text      = t,
            Height    = 44,
            Width     = 120,
            BackColor = C_White,
            ForeColor = C_Muted,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10.5f),
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.None
        };
        b.FlatAppearance.BorderColor = C_Dim;
        b.FlatAppearance.BorderSize  = 1;
        return b;
    }

    /// Two-button row docked to bottom: secondary on left, primary on right.
    Panel ButtonRow(Button secondary, Button primary)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = C_White };
        primary.Anchor   = AnchorStyles.Right | AnchorStyles.Top;
        secondary.Anchor = AnchorStyles.Left  | AnchorStyles.Top;
        primary.Top   = secondary.Top = 6;
        primary.Left  = row.Width - primary.Width;   // will re-anchor at runtime
        secondary.Left = 0;
        row.SizeChanged += (_, _) => primary.Left = row.Width - primary.Width;
        row.Controls.Add(primary);
        row.Controls.Add(secondary);
        return row;
    }

    Panel Spacer(int h) => new() { Dock = DockStyle.Top, Height = h, BackColor = C_White };
}
