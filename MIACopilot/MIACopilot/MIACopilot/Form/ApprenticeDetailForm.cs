using System;
using System.Drawing;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing an apprentice.
/// </summary>
public class ApprenticeDetailForm : Form
{
    public Apprentice? Result { get; private set; }

    private readonly Apprentice?              _existing;
    private readonly CompanyService           _companyService;
    private readonly VocationalTrainerService _trainerService;

    // Input controls
    private TextBox         txtFirstName  = new();
    private TextBox         txtLastName   = new();
    private TextBox         txtEmail      = new();
    private DateTimePicker dtpStart       = new();
    private ComboBox        cmbCompany    = new();
    private ComboBox        cmbTrainer    = new();
    private TextBox         txtUsername   = new();
    private Button          btnSave       = new();
    private Button          btnCancel     = new();

    // Initializes the form, builds UI, loads dropdown data, and fills fields or auto-generates username.
    public ApprenticeDetailForm(
        Apprentice? existing,
        CompanyService companyService,
        VocationalTrainerService trainerService)
    {
        _existing       = existing;
        _companyService = companyService;
        _trainerService = trainerService;
        BuildUI();
        LoadDropdowns();
        if (existing != null) FillFields(existing);
        else AutoFillUsername();
    }

    // Builds the UI layout, configures controls, and wires up events (text changes + button clicks).
    private void BuildUI()
    {
        Text            = _existing == null ? "Add Apprentice" : "Edit Apprentice";
        Size            = new Size(420, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 9,
            Padding     = new Padding(16),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        string[] labels = { "First Name*", "Last Name*", "Email*",
                            "Start Date*", "Company*", "Trainer*", "Username*" };
        Control[] inputs = { txtFirstName, txtLastName, txtEmail,
                              dtpStart, cmbCompany, cmbTrainer, txtUsername };

        for (int i = 0; i < labels.Length; i++)
        {
            layout.Controls.Add(new Label
            {
                Text      = labels[i],
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 9.5f)
            }, 0, i);

            if (inputs[i] is TextBox tb) tb.Dock = DockStyle.Fill;
            if (inputs[i] is ComboBox cb) { cb.Dock = DockStyle.Fill; cb.DropDownStyle = ComboBoxStyle.DropDownList; }
            if (inputs[i] is DateTimePicker dtp) { dtp.Dock = DockStyle.Fill; dtp.Format = DateTimePickerFormat.Short; dtp.CustomFormat = "dd.MM.yyyy"; }

            layout.Controls.Add(inputs[i], 1, i);
        }

        // Buttons
        btnSave   = new Button { Text = "💾 Save",   BackColor = Color.FromArgb(39, 174, 96),  ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        btnCancel = new Button { Text = "✖ Cancel", BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        btnSave.FlatAppearance.BorderSize   = 0;
        btnCancel.FlatAppearance.BorderSize = 0;

        txtFirstName.TextChanged += (_, _) => { if (_existing == null) AutoFillUsername(); };
        txtLastName.TextChanged  += (_, _) => { if (_existing == null) AutoFillUsername(); };

        btnSave.Click   += OnSave;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        var btnPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(0, 6, 0, 0)
        };
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnSave);

        layout.SetColumnSpan(btnPanel, 2);
        layout.Controls.Add(btnPanel, 0, 8);

        Controls.Add(layout);
    }

    // Loads companies and trainers from services and binds them to the comboboxes.
    private void LoadDropdowns()
    {
        cmbCompany.DisplayMember = "Name";
        cmbCompany.ValueMember   = "Id";
        cmbCompany.DataSource    = _companyService.GetAll();

        cmbTrainer.DisplayMember = "FullName";
        cmbTrainer.ValueMember   = "Id";
        cmbTrainer.DataSource    = _trainerService.GetAll();
    }

    // Copies values from an existing apprentice into the form controls.
    private void FillFields(Apprentice a)
    {
        txtFirstName.Text         = a.FirstName;
        txtLastName.Text          = a.LastName;
        txtEmail.Text             = a.Email;
        dtpStart.Value            = a.StartDate;
        cmbCompany.SelectedValue  = a.CompanyId;
        cmbTrainer.SelectedValue  = a.VocationalTrainerId;
        txtUsername.Text          = a.Username;
    }

    // Generates the username based on first initial + last name (lowercase, no spaces).
    private void AutoFillUsername()
    {
        var first = txtFirstName.Text.Trim();
        var last  = txtLastName.Text.Trim();
        if (first.Length > 0 && last.Length > 0)
            txtUsername.Text = $"{first[0].ToString().ToLower()}.{last.ToLower().Replace(" ", "")}";
    }

    // Validates required fields, builds the Apprentice result object, and closes with OK.
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

        Result = new Apprentice
        {
            Id                  = _existing?.Id ?? 0,
            FirstName           = txtFirstName.Text.Trim(),
            LastName            = txtLastName.Text.Trim(),
            Email               = txtEmail.Text.Trim(),
            StartDate           = dtpStart.Value,
            CompanyId           = (int)(cmbCompany.SelectedValue ?? 0),
            VocationalTrainerId = (int)(cmbTrainer.SelectedValue ?? 0),
            WorkJournals        = _existing?.WorkJournals ?? new(),
            Username            = txtUsername.Text.Trim(),
            Pin                 = _existing?.Pin ?? "0000"
        };

        DialogResult = DialogResult.OK;
    }
}
