using MIACopilot.Models;
using MIACopilot.Services;
using System.Windows.Forms;

namespace MIACopilot.Forms;

/// <summary>
/// Login window — horizontal split layout:
/// left = brand/identity panel, right = login form.
/// </summary>
public class LoginForm : Form
{
    private readonly ApprenticeService        _apprenticeService;
    private readonly VocationalTrainerService _trainerService;
    private readonly CompanyService           _companyService;

    private TextBox txtUsername = new();
    private TextBox txtPin      = new();
    private Label   lblError    = new();

    // Theme colors
    static readonly Color C_Dark   = Color.FromArgb(10, 25, 47);
    static readonly Color C_Accent = Color.FromArgb(0, 180, 216);
    static readonly Color C_White  = Color.White;
    static readonly Color C_Input  = Color.FromArgb(248, 250, 252);
    static readonly Color C_Text   = Color.FromArgb(15, 23, 42);
    static readonly Color C_Muted  = Color.FromArgb(100, 116, 139);
    static readonly Color C_Error  = Color.FromArgb(239, 68, 68);

    // Injects required services and builds the UI.
    public LoginForm(ApprenticeService apprenticeService, VocationalTrainerService trainerService, CompanyService companyService)
    {
        _apprenticeService = apprenticeService;
        _trainerService    = trainerService;
        _companyService    = companyService;
        BuildUI();
    }

    // Builds the full login layout and wires events.
    void BuildUI()
    {
        Text            = "MIA Copilot — Login";
        Size            = new Size(800, 520);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 10f);

        // ── Left: brand panel ──────────────────────────────────────────────
        var left = new Panel { Width = 340, Dock = DockStyle.Left, BackColor = C_Dark };
        left.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Decorative background shapes
            using var b1 = new SolidBrush(Color.FromArgb(25, 0, 180, 216));
            g.FillEllipse(b1, 160, -80, 280, 280);
            using var b2 = new SolidBrush(Color.FromArgb(15, 0, 180, 216));
            g.FillEllipse(b2, -70, 360, 220, 220);

            // Logo text — title block moved up, MIA/Copilot gap closed
            using var f1 = new Font("Segoe UI", 34f, FontStyle.Bold);
            g.DrawString("MIA", f1, Brushes.White, 44, 76);
            using var f2 = new Font("Segoe UI", 34f);
            g.DrawString("Copilot", f2, new SolidBrush(C_Accent), 44, 118);

            // Taglines — follow tightly below the title block
            using var f3 = new Font("Segoe UI", 9.5f);
            using var m  = new SolidBrush(Color.FromArgb(148, 163, 184));
            g.DrawString("Apprentice Management System", f3, m, 44, 176);
            g.DrawString("Track · Learn · Grow", f3, m, 44, 198);

            // Supported roles
            using var f4 = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            g.DrawString("Admin · Trainer · Apprentice", f4, m, 44, 440);
        };

        // ── Right: login form ──────────────────────────────────────────────
        var right = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = C_White,
            Padding = new Padding(52, 24, 52, 36)   // reduced top padding → "Welcome back" moves up
        };

        var lblTitle = new Label
        {
            Text      = "Welcome back",
            Font      = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = C_Text,
            Dock      = DockStyle.Top,
            Height    = 46
        };

        var lblSub = new Label
        {
            Text      = "Sign in with your username and PIN",
            Font      = new Font("Segoe UI", 9.5f),
            ForeColor = C_Muted,
            Dock      = DockStyle.Top,
            Height    = 30
        };

        var lblU = FieldLabel("Username");
        txtUsername = StyledInput("Your username");

        var lblP = FieldLabel("PIN");
        txtPin = StyledInput("● ● ● ●");
        txtPin.PasswordChar = '●';
        txtPin.MaxLength    = 4;
        txtPin.KeyPress    += (_, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
        };

        lblError = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 9f),
            ForeColor = C_Error,
            Dock      = DockStyle.Top,
            Height    = 22
        };

        var btnLogin = new Button
        {
            Text      = "Sign In",
            Dock      = DockStyle.Top,
            Height    = 48,
            BackColor = C_Accent,
            ForeColor = C_White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += OnLogin;

        var lnkForgotPin = new LinkLabel
        {
            Text      = "Forgot PIN?",
            Dock      = DockStyle.Top,
            Height    = 22,
            Font      = new Font("Segoe UI", 9f),
            LinkColor = C_Accent,
            TextAlign = ContentAlignment.MiddleCenter
        };
        lnkForgotPin.LinkClicked += OnForgotPin;

        // Keyboard navigation
        txtUsername.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) txtPin.Focus(); };
        txtPin.KeyDown      += (_, e) => { if (e.KeyCode == Keys.Enter) OnLogin(null, EventArgs.Empty); };

        // Layout stack
        right.Controls.Add(btnLogin);
        right.Controls.Add(Spacer(6));
        right.Controls.Add(lnkForgotPin);
        right.Controls.Add(Spacer(4));
        right.Controls.Add(lblError);
        right.Controls.Add(Spacer(8));
        right.Controls.Add(txtPin);
        right.Controls.Add(lblP);
        right.Controls.Add(Spacer(14));
        right.Controls.Add(txtUsername);
        right.Controls.Add(lblU);
        right.Controls.Add(Spacer(24));
        right.Controls.Add(lblSub);
        right.Controls.Add(lblTitle);

        Controls.Add(right);
        Controls.Add(left);
        AcceptButton = btnLogin;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    Panel Spacer(int h) => new() { Dock = DockStyle.Top, Height = h, BackColor = C_White };

    Label FieldLabel(string t) => new()
    {
        Text      = t,
        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        ForeColor = C_Muted,
        Dock      = DockStyle.Top,
        Height    = 22
    };

    TextBox StyledInput(string placeholder) => new()
    {
        Dock            = DockStyle.Top,
        Height          = 42,
        Font            = new Font("Segoe UI", 10.5f),
        BackColor       = C_Input,
        ForeColor       = C_Text,
        BorderStyle     = BorderStyle.FixedSingle,
        PlaceholderText = placeholder
    };

    // ── Authentication logic ──────────────────────────────────────────────

    void OnLogin(object? sender, EventArgs e)
    {
        lblError.Text = "";
        var user = txtUsername.Text.Trim().ToLower();
        var pin  = txtPin.Text.Trim();

        if (string.IsNullOrEmpty(user) || pin.Length != 4)
        {
            lblError.Text = "⚠  Please enter username and 4-digit PIN.";
            return;
        }

        // Super admin shortcut
        if (user == "admin" && pin == "0000")
        {
            Session.Role     = UserRole.SuperAdmin;
            Session.UserId   = 0;
            Session.Username = "admin";
            Session.FullName = "Administrator";
            DialogResult     = DialogResult.OK;
            return;
        }

        // Trainer login
        var trainer = _trainerService.GetAll()
            .FirstOrDefault(t => t.Username.ToLower() == user && t.Pin == pin);
        if (trainer != null)
        {
            Session.Role     = UserRole.Trainer;
            Session.UserId   = trainer.Id;
            Session.Username = trainer.Username;
            Session.FullName = trainer.FullName;
            DialogResult     = DialogResult.OK;
            return;
        }

        // Apprentice login
        var apprentice = _apprenticeService.GetAll()
            .FirstOrDefault(a => a.Username.ToLower() == user && a.Pin == pin);
        if (apprentice != null)
        {
            Session.Role     = UserRole.Apprentice;
            Session.UserId   = apprentice.Id;
            Session.Username = apprentice.Username;
            Session.FullName = apprentice.FullName;
            DialogResult     = DialogResult.OK;
            return;
        }

        // Company Admin login
        var company = _companyService.GetAll()
            .FirstOrDefault(c => c.AdminUsername.ToLower() == user && c.AdminPin == pin
                              && !string.IsNullOrEmpty(c.AdminUsername));
        if (company != null)
        {
            Session.Role      = UserRole.CompanyAdmin;
            Session.UserId    = 0;
            Session.CompanyId = company.Id;
            Session.Username  = company.AdminUsername;
            Session.FullName  = company.Name + " Admin";
            DialogResult      = DialogResult.OK;
            return;
        }

        // Failure
        lblError.Text = "⚠  Invalid username or PIN. Please try again.";
        txtPin.Clear();
        txtPin.Focus();
    }

    void OnForgotPin(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        lblError.Text = "";
        using var form = new PinResetForm(_apprenticeService, _trainerService, _companyService);
        form.ShowDialog(this);
    }
}