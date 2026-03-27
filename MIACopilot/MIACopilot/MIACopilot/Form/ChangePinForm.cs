namespace MIACopilot.Forms;

/// <summary>Dialog for changing a 4-digit PIN.</summary>
public class ChangePinForm : Form
{
    public string NewPin { get; private set; } = "";

    private TextBox txtOld  = new();
    private TextBox txtNew  = new();
    private TextBox txtConf = new();
    private Label   lblErr  = new();

    public ChangePinForm()
    {
        Text            = "Change PIN";
        Size            = new Size(340, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.White;
        Font            = new Font("Segoe UI", 10f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(24, 16, 24, 16)
        };

        void AddRow(string label, TextBox tb)
        {
            tb.PasswordChar = '●';
            tb.MaxLength    = 4;
            tb.Dock         = DockStyle.Fill;
            tb.Font         = new Font("Segoe UI", 12f);
            tb.Height       = 38;
            tb.KeyPress    += (_, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };

            layout.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139), Dock = DockStyle.Fill, Height = 24 });
            layout.Controls.Add(tb);
        }

        AddRow("Current PIN", txtOld);
        AddRow("New PIN (4 digits)", txtNew);
        AddRow("Confirm new PIN", txtConf);

        lblErr = new Label { Text = "", ForeColor = Color.FromArgb(239, 68, 68), Font = new Font("Segoe UI", 8.5f), Dock = DockStyle.Fill, Height = 22 };
        layout.Controls.Add(lblErr);

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var btnCancel = new Button { Text = "Cancel", Width = 90, Height = 36, BackColor = Color.FromArgb(226, 232, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f) };
        var btnSave   = new Button { Text = "Save",   Width = 90, Height = 36, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnSave.FlatAppearance.BorderSize   = 0;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btnSave.Click   += OnSave;
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnSave);
        layout.Controls.Add(btnPanel);

        Controls.Add(layout);
        AcceptButton = btnSave;
    }

    void OnSave(object? sender, EventArgs e)
    {
        if (txtOld.Text != Models.Session.Role.ToString() && txtOld.TextLength != 4)
        { lblErr.Text = "⚠  Current PIN must be 4 digits."; return; }
        if (txtNew.TextLength != 4)
        { lblErr.Text = "⚠  New PIN must be exactly 4 digits."; return; }
        if (txtNew.Text != txtConf.Text)
        { lblErr.Text = "⚠  New PINs do not match."; return; }

        NewPin       = txtNew.Text;
        DialogResult = DialogResult.OK;
    }
}
