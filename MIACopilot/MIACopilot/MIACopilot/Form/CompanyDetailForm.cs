using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing a company.
/// When adding, auto-generates the Company Admin username and requires a 4-digit PIN.
/// </summary>
public class CompanyDetailForm : Form
{
    public Company? Result { get; private set; }

    private readonly Company?               _existing;
    private readonly bool                   _isNew;
    private readonly IReadOnlyList<Company> _allCompanies;
    private bool                            _usernameManuallyEdited;

    private TextBox txtName        = new();
    private TextBox txtAddress     = new();
    private TextBox txtPhone       = new();
    private TextBox txtEmail       = new();
    private TextBox txtIndustry    = new();
    private TextBox txtAdminUser   = new();  // readonly display
    private TextBox txtAdminPin    = new();

    // Initializes the dialog, builds the UI, and fills fields if editing an existing company.
    // allCompanies is used to validate AdminUsername uniqueness on save.
    public CompanyDetailForm(Company? existing, IReadOnlyList<Company>? allCompanies = null)
    {
        _existing     = existing;
        _isNew        = existing == null;
        _allCompanies = allCompanies ?? Array.Empty<Company>();
        BuildUI();
        if (existing != null) FillFields(existing);
    }

    // Builds the form layout, input fields, and Save/Cancel buttons.
    private void BuildUI()
    {
        Text            = _isNew ? "Add Company" : "Edit Company";
        Size            = new Size(440, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int r = 0; r < 7; r++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12)); // spacer
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); // buttons

        // Company Admin username — pre-filled by auto-generation, but editable
        txtAdminUser = new TextBox
        {
            Dock      = DockStyle.Fill,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        // Flag manual edits only when the text differs from the auto-generated value
        txtAdminUser.TextChanged += (_, _) =>
        {
            if (txtAdminUser.Text != GenerateCompanyAdminUsername(txtName.Text))
                _usernameManuallyEdited = true;
        };

        // Admin PIN
        txtAdminPin = new TextBox { Dock = DockStyle.Fill, MaxLength = 4 };
        txtAdminPin.KeyPress += (_, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };

        string[]  labels = { "Name*", "Industry", "Address", "Phone", "Email", "Admin Username", _isNew ? "Admin PIN*" : "Reset Admin PIN" };
        TextBox[] inputs = { txtName, txtIndustry, txtAddress, txtPhone, txtEmail, txtAdminUser, txtAdminPin };

        for (int i = 0; i < labels.Length; i++)
        {
            layout.Controls.Add(new Label
            {
                Text      = labels[i],
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill
            }, 0, i);
            inputs[i].Dock = DockStyle.Fill;
            layout.Controls.Add(inputs[i], 1, i);
        }

        // Auto-generate Admin username whenever company name changes,
        // but stop if the admin has already overridden the field manually.
        txtName.TextChanged += (_, _) =>
        {
            if (!_usernameManuallyEdited)
                txtAdminUser.Text = GenerateCompanyAdminUsername(txtName.Text);
        };

        var btnSave   = new Button { Text = "💾 Save",   BackColor = Color.FromArgb(39, 174, 96),  ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        var btnCancel = new Button { Text = "✖ Cancel", BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        btnSave.FlatAppearance.BorderSize   = 0;
        btnCancel.FlatAppearance.BorderSize = 0;

        btnSave.Click   += OnSave;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 0, 0) };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnSave);
        layout.SetColumnSpan(btnPanel, 2);
        layout.Controls.Add(btnPanel, 0, 8);

        Controls.Add(layout);
    }

    // Copies the existing company data into the input fields.
    private void FillFields(Company c)
    {
        txtName.Text      = c.Name;
        txtAddress.Text   = c.Address;
        txtPhone.Text     = c.Phone;
        txtEmail.Text     = c.Email;
        txtIndustry.Text  = c.Industry;
        txtAdminUser.Text = c.AdminUsername;
        // PIN left blank on edit — only fill if admin wants to reset
    }

    // Validates required fields, creates the Company result, and closes the dialog with OK.
    private void OnSave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Company Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pin = txtAdminPin.Text.Trim();

        if (_isNew && pin.Length != 4)
        {
            MessageBox.Show("Admin PIN must be exactly 4 digits.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!_isNew && pin.Length > 0 && pin.Length != 4)
        {
            MessageBox.Show("Reset PIN must be exactly 4 digits (or leave empty to keep current).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var adminUsername = txtAdminUser.Text.Trim();
        if (string.IsNullOrWhiteSpace(adminUsername))
            adminUsername = GenerateCompanyAdminUsername(txtName.Text);

        // Validate admin username uniqueness (skip the company currently being edited)
        var duplicate = _allCompanies.FirstOrDefault(c =>
            c.Id != (_existing?.Id ?? 0) &&
            string.Equals(c.AdminUsername, adminUsername, StringComparison.OrdinalIgnoreCase));
        if (duplicate != null)
        {
            MessageBox.Show(
                $"Admin username \"{adminUsername}\" is already used by \"{duplicate.Name}\".\nChoose a different username.",
                "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new Company
        {
            Id            = _existing?.Id ?? 0,
            Name          = txtName.Text.Trim(),
            Address       = txtAddress.Text.Trim(),
            Phone         = txtPhone.Text.Trim(),
            Email         = txtEmail.Text.Trim(),
            Industry      = txtIndustry.Text.Trim(),
            AdminUsername = adminUsername.Trim(),
            AdminPin      = pin.Length == 4 ? pin : (_existing?.AdminPin ?? "")
        };
        DialogResult = DialogResult.OK;
    }

    /// <summary>
    /// Generates the Company Admin username: "Admin" + cleaned company name.
    /// Removes spaces, hyphens, umlauts, and non-alphanumeric characters.
    /// </summary>
    public static string GenerateCompanyAdminUsername(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName)) return "Admin";

        var clean = companyName.Trim()
            .Replace("ä", "a").Replace("ö", "o").Replace("ü", "u")
            .Replace("Ä", "A").Replace("Ö", "O").Replace("Ü", "U")
            .Replace("ß", "ss");

        var sb = new StringBuilder("Admin");
        foreach (char c in clean)
            if (char.IsLetterOrDigit(c)) sb.Append(c);

        return sb.ToString();
    }
}
