using System;
using System.Drawing;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing a single work journal entry.
/// </summary>
public class JournalDetailForm : Form
{
    public WorkJournal? Result { get; private set; }

    private readonly WorkJournal? _existing;

    private NumericUpDown  nudWeek   = new();
    private DateTimePicker dtpDate   = new();
    private TextBox        txtTitle  = new();
    private RichTextBox    rtbContent = new();

    public JournalDetailForm(WorkJournal? existing)
    {
        _existing = existing;
        BuildUI();
        if (existing != null) FillFields(existing);
    }

    private void BuildUI()
    {
        Text            = _existing == null ? "Add Journal Entry" : "Edit Journal Entry";
        Size            = new Size(520, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // content
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        // Week
        nudWeek.Dock    = DockStyle.Fill;
        nudWeek.Minimum = 1;
        nudWeek.Maximum = 200;

        // Date
        dtpDate.Dock         = DockStyle.Fill;
        dtpDate.Format       = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd.MM.yyyy";

        // Title
        txtTitle.Dock = DockStyle.Fill;

        // Content
        rtbContent.Dock       = DockStyle.Fill;
        rtbContent.ScrollBars = RichTextBoxScrollBars.Vertical;
        rtbContent.BorderStyle = BorderStyle.FixedSingle;

        layout.Controls.Add(new Label { Text = "Week Number*",  TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
        layout.Controls.Add(nudWeek, 1, 0);
        layout.Controls.Add(new Label { Text = "Date*",         TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        layout.Controls.Add(dtpDate, 1, 1);
        layout.Controls.Add(new Label { Text = "Title*",        TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
        layout.Controls.Add(txtTitle, 1, 2);
        layout.Controls.Add(new Label { Text = "Content*",      TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
        layout.Controls.Add(rtbContent, 1, 3);

        // Buttons
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
        layout.Controls.Add(btnPanel, 0, 5);

        Controls.Add(layout);
    }

    private void FillFields(WorkJournal j)
    {
        nudWeek.Value    = j.WeekNumber;
        dtpDate.Value    = j.Date;
        txtTitle.Text    = j.Title;
        rtbContent.Text  = j.Content;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
            string.IsNullOrWhiteSpace(rtbContent.Text))
        {
            MessageBox.Show("Please fill in all required fields (*).",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new WorkJournal
        {
            Id          = _existing?.Id ?? 0,
            ApprenticeId = _existing?.ApprenticeId ?? 0,
            WeekNumber  = (int)nudWeek.Value,
            Date        = dtpDate.Value,
            Title       = txtTitle.Text.Trim(),
            Content     = rtbContent.Text.Trim()
        };
        DialogResult = DialogResult.OK;
    }
}
