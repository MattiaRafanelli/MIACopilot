using MIACopilot.Models;

namespace MIACopilot.Forms;

/// <summary>
/// Dialog for adding or editing a single grade entry.
/// </summary>
public class GradeDetailForm : Form
{
    public Grade? Result { get; private set; }

    private readonly Grade? _existing;
    private readonly int    _apprenticeId;

    private TextBox        txtSubject  = new();
    private NumericUpDown  nudGrade    = new();
    private ComboBox       cmbType     = new();
    private DateTimePicker dtpDate     = new();
    private TextBox        txtNotes    = new();
    private Label          lblPreview  = new();

    private static readonly string[] GradeTypes =
        { "Short Test (15-30 min)", "Long Test (45-60 min)", "Final Exam (90+ min)", "Project", "Presentation", "Practical Work" };

    // Initializes the form, builds the UI, fills fields if editing, and updates the preview.
    public GradeDetailForm(Grade? existing, int apprenticeId)
    {
        _existing     = existing;
        _apprenticeId = apprenticeId;
        BuildUI();
        if (existing != null) FillFields(existing);
        UpdatePreview();
    }

    // Builds the grade form UI, configures controls, and wires Save/Cancel actions.
    private void BuildUI()
    {
        Text            = _existing == null ? "Add Grade" : "Edit Grade";
        Size            = new Size(400, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 8,
            Padding     = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Configures the grade spinner (1.0–6.0, step 0.1) and updates preview on change.
        nudGrade.DecimalPlaces = 1;
        nudGrade.Increment     = 0.1m;
        nudGrade.Minimum       = 1.0m;
        nudGrade.Maximum       = 6.0m;
        nudGrade.Value         = 4.0m;
        nudGrade.Dock          = DockStyle.Fill;
        nudGrade.Font          = new Font("Segoe UI", 10f, FontStyle.Bold);
        nudGrade.ValueChanged  += (_, _) => UpdatePreview();

        // Configures the grade type dropdown.
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.Items.AddRange(GradeTypes);
        cmbType.SelectedIndex = 0;
        cmbType.Dock          = DockStyle.Fill;

        // Configures the date picker.
        dtpDate.Dock         = DockStyle.Fill;
        dtpDate.Format       = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd.MM.yyyy";

        // Configures subject and notes inputs.
        txtSubject.Dock = DockStyle.Fill;
        txtNotes.Dock   = DockStyle.Fill;

        // Displays a live preview of grade value and status.
        lblPreview = new Label
        {
            Text      = "4.0  ✅ Passed",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 128, 185),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        string[] labels    = { "Subject*", "Grade (1–6)*", "Type*", "Date*", "Preview", "Notes" };
        Control[] controls = { txtSubject, nudGrade, cmbType, dtpDate, lblPreview, txtNotes };

        for (int i = 0; i < labels.Length; i++)
        {
            layout.Controls.Add(new Label
            {
                Text      = labels[i],
                TextAlign = ContentAlignment.MiddleRight,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 9.5f)
            }, 0, i);
            layout.Controls.Add(controls[i], 1, i);
        }

        // Creates Save and Cancel buttons and wires their actions.
        var btnSave   = new Button { Text = "💾 Save",   BackColor = Color.FromArgb(39, 174, 96),  ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        var btnCancel = new Button { Text = "✖ Cancel", BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 34 };
        btnSave.FlatAppearance.BorderSize   = 0;
        btnCancel.FlatAppearance.BorderSize = 0;
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
        layout.Controls.Add(btnPanel, 0, 7);

        Controls.Add(layout);
    }

    // Updates the preview label to reflect the current grade value and status.
    private void UpdatePreview()
    {
        double v = (double)nudGrade.Value;
        string status = v >= 5.0 ? "🌟 Excellent"
                      : v >= 4.0 ? "✅ Passed"
                      : v >= 3.0 ? "⚠️ Sufficient"
                      :            "❌ Failed";

        lblPreview.Text      = $"{nudGrade.Value:0.0}  {status}";
        lblPreview.ForeColor = v >= 5.0 ? Color.FromArgb(39, 174, 96)
                             : v >= 4.0 ? Color.FromArgb(41, 128, 185)
                             : v >= 3.0 ? Color.FromArgb(230, 126, 34)
                             :            Color.FromArgb(192, 57, 43);
    }

    // Copies existing grade data into the form fields.
    private void FillFields(Grade g)
    {
        txtSubject.Text  = g.Subject;
        nudGrade.Value   = (decimal)g.Value;
        dtpDate.Value    = g.Date;
        txtNotes.Text    = g.Notes;
        var idx = Array.IndexOf(GradeTypes, g.Type);
        cmbType.SelectedIndex = idx >= 0 ? idx : 0;
    }

    // Validates input, creates the Grade result, and closes the dialog with OK.
    private void OnSave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSubject.Text))
        {
            MessageBox.Show("Please enter a subject.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new Grade
        {
            Id           = _existing?.Id ?? 0,
            ApprenticeId = _apprenticeId,
            Subject      = txtSubject.Text.Trim(),
            Value        = (double)nudGrade.Value,
            Type         = cmbType.SelectedItem?.ToString() ?? "Test",
            Date         = dtpDate.Value,
            Notes        = txtNotes.Text.Trim()
        };
        DialogResult = DialogResult.OK;
    }
}
