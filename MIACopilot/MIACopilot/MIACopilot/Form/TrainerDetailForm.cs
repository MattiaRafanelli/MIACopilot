using System;
using System.Drawing;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing a vocational trainer.
/// </summary>
public class TrainerDetailForm : Form
{
    public VocationalTrainer? Result { get; private set; }

    private readonly VocationalTrainer? _existing;
    private readonly CompanyService     _companyService;

    private TextBox  txtFirstName = new();
    private TextBox  txtLastName  = new();
    private TextBox  txtEmail     = new();
    private TextBox  txtPhone     = new();
    private ComboBox cmbCompany   = new();
    private TextBox  txtUsername  = new();

    public TrainerDetailForm(VocationalTrainer? existing, CompanyService companyService)
    {
        _existing       = existing;
        _companyService = companyService;
        BuildUI();
        LoadDropdown();
        if (existing != null) FillFields(existing);
        else AutoFillUsername();
    }

    private void BuildUI()
    {
        Text            = _existing == null ? "Add Trainer" : "Edit Trainer";
        Size            = new Size(420, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        string[] labels  = { "First Name*", "Last Name*", "Email*", "Phone", "Company*", "Username*" };
        Control[] inputs = { txtFirstName, txtLastName, txtEmail, txtPhone, cmbCompany, txtUsername };

        for (int i = 0; i < labels.Length; i++)
        {
            layout.Controls.Add(new Label
            {
                Text = labels[i], TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill
            }, 0, i);

            if (inputs[i] is TextBox tb)  tb.Dock = DockStyle.Fill;
            if (inputs[i] is ComboBox cb) { cb.Dock = DockStyle.Fill; cb.DropDownStyle = ComboBoxStyle.DropDownList; }
            layout.Controls.Add(inputs[i], 1, i);
        }

        // Name → auto-update username for new entries
        txtFirstName.TextChanged += (_, _) => { if (_existing == null) AutoFillUsername(); };
        txtLastName.TextChanged  += (_, _) => { if (_existing == null) AutoFillUsername(); };

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
        layout.Controls.Add(btnPanel, 0, 7);

        Controls.Add(layout);
    }

    private void LoadDropdown()
    {
        cmbCompany.DisplayMember = "Name";
        cmbCompany.ValueMember   = "Id";
        cmbCompany.DataSource    = _companyService.GetAll();
    }

    private void FillFields(VocationalTrainer t)
    {
        txtFirstName.Text        = t.FirstName;
        txtLastName.Text         = t.LastName;
        txtEmail.Text            = t.Email;
        txtPhone.Text            = t.Phone;
        cmbCompany.SelectedValue = t.CompanyId;
        txtUsername.Text         = t.Username;
    }

    private void AutoFillUsername()
    {
        var first = txtFirstName.Text.Trim();
        var last  = txtLastName.Text.Trim();
        if (first.Length > 0 && last.Length > 0)
            txtUsername.Text = $"{first[0].ToString().ToLower()}.{last.ToLower().Replace(" ", "")}";
    }

    private void OnSave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
            string.IsNullOrWhiteSpace(txtLastName.Text)  ||
            string.IsNullOrWhiteSpace(txtEmail.Text)     ||
            string.IsNullOrWhiteSpace(txtUsername.Text))
        {
            MessageBox.Show("Please fill in all required fields (*).",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new VocationalTrainer
        {
            Id        = _existing?.Id ?? 0,
            FirstName = txtFirstName.Text.Trim(),
            LastName  = txtLastName.Text.Trim(),
            Email     = txtEmail.Text.Trim(),
            Phone     = txtPhone.Text.Trim(),
            CompanyId = (int)(cmbCompany.SelectedValue ?? 0),
            Username  = txtUsername.Text.Trim(),
            Pin       = _existing?.Pin ?? "0000"   // preserve existing PIN or default
        };
        DialogResult = DialogResult.OK;
    }
}
