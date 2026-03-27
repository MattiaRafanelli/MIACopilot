using System;
using System.Drawing;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing a company.
/// </summary>
public class CompanyDetailForm : Form
{
    public Company? Result { get; private set; }

    private readonly Company? _existing;

    private TextBox txtName     = new();
    private TextBox txtAddress  = new();
    private TextBox txtPhone    = new();
    private TextBox txtEmail    = new();
    private TextBox txtIndustry = new();

    // Initializes the dialog, builds the UI, and fills fields if editing an existing company.
    public CompanyDetailForm(Company? existing)
    {
        _existing = existing;
        BuildUI();
        if (existing != null) FillFields(existing);
    }

    // Builds the form layout, input fields, and Save/Cancel buttons.
    private void BuildUI()
    {
        Text            = _existing == null ? "Add Company" : "Edit Company";
        Size            = new Size(420, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        string[] labels  = { "Name*", "Industry*", "Address*", "Phone", "Email" };
        TextBox[] inputs = { txtName, txtIndustry, txtAddress, txtPhone, txtEmail };

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
        layout.Controls.Add(btnPanel, 0, 6);

        Controls.Add(layout);
    }

    // Copies the existing company data into the input fields.
    private void FillFields(Company c)
    {
        txtName.Text     = c.Name;
        txtAddress.Text  = c.Address;
        txtPhone.Text    = c.Phone;
        txtEmail.Text    = c.Email;
        txtIndustry.Text = c.Industry;
    }

    // Validates required fields, creates the Company result, and closes the dialog with OK.
    private void OnSave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtIndustry.Text) ||
            string.IsNullOrWhiteSpace(txtAddress.Text))
        {
            MessageBox.Show("Please fill in all required fields (*).",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new Company
        {
            Id       = _existing?.Id ?? 0,
            Name     = txtName.Text.Trim(),
            Address  = txtAddress.Text.Trim(),
            Phone    = txtPhone.Text.Trim(),
            Email    = txtEmail.Text.Trim(),
            Industry = txtIndustry.Text.Trim()
        };
        DialogResult = DialogResult.OK;
    }
}
