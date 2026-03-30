using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms
{
    /// <summary>
    /// Portal for a Company Admin. Provides full CRUD for apprentices and trainers
    /// scoped to the admin's company, including PIN setup and reset.
    /// </summary>
    public class CompanyAdminPortal : Form
    {
        private readonly int _companyId;
        private readonly string _companyName;
        private readonly ApprenticeService _apprenticeService;
        private readonly VocationalTrainerService _trainerService;
        private readonly CompanyService _companyService;
        private readonly GradeService _gradeService;

        private Button _btnNavApprentices = new();
        private Button _btnNavTrainers    = new();
        private Button _btnNavCompany     = new();

        private Panel _pnlApprentices = new();
        private Panel _pnlTrainers    = new();
        private Panel _pnlCompanyInfo = new();
        private Panel _sidebar        = new();

        private DataGridView _dgvApprentices = new();
        private DataGridView _dgvTrainers    = new();

        private static readonly Color C_Sidebar = Color.FromArgb(15, 23, 41);
        private static readonly Color C_SideHov = Color.FromArgb(30, 41, 59);
        private static readonly Color C_Active  = Color.FromArgb(59, 130, 246);
        private static readonly Color C_Bg      = Color.FromArgb(248, 250, 252);
        private static readonly Color C_White   = Color.White;
        private static readonly Color C_Border  = Color.FromArgb(226, 232, 240);
        private static readonly Color C_Muted   = Color.FromArgb(100, 116, 139);
        private static readonly Color C_Green   = Color.FromArgb(16, 185, 129);
        private static readonly Color C_Blue    = Color.FromArgb(59, 130, 246);
        private static readonly Color C_Red     = Color.FromArgb(239, 68, 68);

        /// <summary>
        /// Initializes the portal with the company admin's context and required services.
        /// </summary>
        public CompanyAdminPortal(
            int companyId,
            string companyName,
            ApprenticeService apprenticeService,
            VocationalTrainerService trainerService,
            CompanyService companyService,
            GradeService gradeService)
        {
            _companyId         = companyId;
            _companyName       = companyName;
            _apprenticeService = apprenticeService;
            _trainerService    = trainerService;
            _companyService    = companyService;
            _gradeService      = gradeService;
            BuildUi();
            ShowPanel(_pnlApprentices, _btnNavApprentices);
        }

        // ── UI Construction ───────────────────────────────────────────────────

        private void BuildUi()
        {
            Text          = $"MIA Copilot — {_companyName} Admin";
            Size          = new Size(1020, 680);
            MinimumSize   = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = C_Bg;
            Font          = new Font("Segoe UI", 10f);

            BuildSidebar();

            var content = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(28, 24, 28, 24) };

            BuildApprenticesPanel();
            BuildTrainersPanel();
            BuildCompanyInfoPanel();

            foreach (var p in new[] { _pnlApprentices, _pnlTrainers, _pnlCompanyInfo })
            {
                p.Dock = DockStyle.Fill;
                content.Controls.Add(p);
            }

            Controls.Add(content);
            Controls.Add(_sidebar);
        }

        private void BuildSidebar()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = C_Sidebar };

            var avatarPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = C_Sidebar };
            avatarPanel.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using var avBrush  = new SolidBrush(C_Blue);
                var init = _companyName.Length > 0 ? _companyName[0].ToString().ToUpper() : "A";
                g.FillEllipse(avBrush, 20, 20, 58, 58);
                using var initFont = new Font("Segoe UI", 22f, FontStyle.Bold);
                var sz = g.MeasureString(init, initFont);
                g.DrawString(init, initFont, Brushes.White, 20 + (58 - sz.Width) / 2, 20 + (58 - sz.Height) / 2);

                using var nameFont  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var subFont   = new Font("Segoe UI", 8.5f);
                using var muteBrush = new SolidBrush(C_Muted);
                using var sf        = new System.Drawing.StringFormat { Trimming = System.Drawing.StringTrimming.EllipsisCharacter };
                g.DrawString(_companyName, nameFont, Brushes.White, new RectangleF(88, 28, 132, 22), sf);
                g.DrawString("Company Admin", subFont, muteBrush, 88, 52);
                g.DrawString($"@{Session.Username}", subFont, muteBrush, new RectangleF(88, 70, 132, 18), sf);
            };

            var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

            _btnNavApprentices = NavBtn("🎓  Apprentices");
            _btnNavTrainers    = NavBtn("👨‍🏫  Trainers");
            _btnNavCompany     = NavBtn("🏢  Company Info");

            _btnNavApprentices.Click += (_, _) => ShowPanel(_pnlApprentices, _btnNavApprentices);
            _btnNavTrainers.Click    += (_, _) => ShowPanel(_pnlTrainers,    _btnNavTrainers);
            _btnNavCompany.Click     += (_, _) => ShowPanel(_pnlCompanyInfo, _btnNavCompany);

            var btnLogout = new Button
            {
                Text      = "⎋   Logout",
                Dock      = DockStyle.Bottom,
                Height    = 46,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = C_Sidebar,
                Font      = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0),
                Cursor    = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize         = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = C_SideHov;
            btnLogout.Click += (_, _) => { DialogResult = DialogResult.Retry; Close(); };

            var sepBottom = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

            _sidebar.Controls.Add(btnLogout);
            _sidebar.Controls.Add(sepBottom);
            _sidebar.Controls.Add(_btnNavCompany);
            _sidebar.Controls.Add(_btnNavTrainers);
            _sidebar.Controls.Add(_btnNavApprentices);
            _sidebar.Controls.Add(sep);
            _sidebar.Controls.Add(avatarPanel);
        }

        private void BuildApprenticesPanel()
        {
            _pnlApprentices.BackColor = C_Bg;

            var title = PageTitle("Apprentices", $"Manage apprentices for {_companyName}");

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
            toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var btnAdd    = ActionBtn("➕  Add",    C_Green);
            var btnEdit   = ActionBtn("✏️  Edit",   C_Blue);
            var btnDelete = ActionBtn("🗑  Delete", C_Red);

            btnAdd.Width    = 100;
            btnEdit.Width   = 100;
            btnDelete.Width = 100;

            btnAdd.Location    = new Point(16,  12);
            btnEdit.Location   = new Point(124, 12);
            btnDelete.Location = new Point(232, 12);

            btnAdd.Click    += (_, _) => AddApprentice();
            btnEdit.Click   += (_, _) => EditApprentice();
            btnDelete.Click += (_, _) => DeleteApprentice();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDelete);

            _dgvApprentices = MakeGrid();
            LoadApprentices();

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);
            card.Controls.Add(_dgvApprentices);
            card.Controls.Add(toolbar);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };
            _pnlApprentices.Controls.Add(bottomBar);
            _pnlApprentices.Controls.Add(card);
            _pnlApprentices.Controls.Add(title);
        }

        private void BuildTrainersPanel()
        {
            _pnlTrainers.BackColor = C_Bg;

            var title = PageTitle("Trainers", $"Manage trainers for {_companyName}");

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
            toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var btnAdd    = ActionBtn("➕  Add",    C_Green);
            var btnEdit   = ActionBtn("✏️  Edit",   C_Blue);
            var btnDelete = ActionBtn("🗑  Delete", C_Red);

            btnAdd.Width    = 100;
            btnEdit.Width   = 100;
            btnDelete.Width = 100;

            btnAdd.Location    = new Point(16,  12);
            btnEdit.Location   = new Point(124, 12);
            btnDelete.Location = new Point(232, 12);

            btnAdd.Click    += (_, _) => AddTrainer();
            btnEdit.Click   += (_, _) => EditTrainer();
            btnDelete.Click += (_, _) => DeleteTrainer();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDelete);

            _dgvTrainers = MakeGrid();
            LoadTrainers();

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);
            card.Controls.Add(_dgvTrainers);
            card.Controls.Add(toolbar);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };
            _pnlTrainers.Controls.Add(bottomBar);
            _pnlTrainers.Controls.Add(card);
            _pnlTrainers.Controls.Add(title);
        }

        // ── Data loading ──────────────────────────────────────────────────────

        private void LoadApprentices()
        {
            _dgvApprentices.DataSource = _apprenticeService.GetAll()
                .Where(a => a.CompanyId == _companyId)
                .Select(a => new
                {
                    a.Id,
                    a.FirstName,
                    a.LastName,
                    a.Email,
                    a.Username,
                    StartDate = a.StartDate.ToString("dd.MM.yyyy")
                }).ToList();
            AutoSizeCols(_dgvApprentices, "Email");
        }

        private void LoadTrainers()
        {
            _dgvTrainers.DataSource = _trainerService.GetAll()
                .Where(t => t.CompanyId == _companyId)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Username
                }).ToList();
            AutoSizeCols(_dgvTrainers, "Email");
        }

        // ── Apprentice CRUD ───────────────────────────────────────────────────

        private void AddApprentice()
        {
            var a = new Apprentice { CompanyId = _companyId, StartDate = DateTime.Today };
            if (ShowApprenticeDialog("Add Apprentice", a, isNew: true))
            {
                _apprenticeService.Create(a);
                LoadApprentices();
            }
        }

        private void EditApprentice()
        {
            var a = GetSelectedApprentice();
            if (a == null) return;
            if (ShowApprenticeDialog("Edit Apprentice", a, isNew: false))
            {
                _apprenticeService.Update(a);
                LoadApprentices();
            }
        }

        private void DeleteApprentice()
        {
            var a = GetSelectedApprentice();
            if (a == null) return;
            if (MessageBox.Show($"Delete \"{a.FullName}\"? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _apprenticeService.Delete(a.Id);
                LoadApprentices();
            }
        }

        private Apprentice? GetSelectedApprentice()
        {
            if (_dgvApprentices.SelectedRows.Count == 0) { Info("Select an apprentice first."); return null; }
            var id = (int)_dgvApprentices.SelectedRows[0].Cells["Id"].Value;
            return _apprenticeService.GetById(id);
        }

        // ── Trainer CRUD ──────────────────────────────────────────────────────

        private void AddTrainer()
        {
            var t = new VocationalTrainer { CompanyId = _companyId };
            if (ShowTrainerDialog("Add Trainer", t, isNew: true))
            {
                _trainerService.Create(t);
                LoadTrainers();
            }
        }

        private void EditTrainer()
        {
            var t = GetSelectedTrainer();
            if (t == null) return;
            if (ShowTrainerDialog("Edit Trainer", t, isNew: false))
            {
                _trainerService.Update(t);
                LoadTrainers();
            }
        }

        private void DeleteTrainer()
        {
            var t = GetSelectedTrainer();
            if (t == null) return;
            if (MessageBox.Show($"Delete \"{t.FullName}\"? This cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _trainerService.Delete(t.Id);
                LoadTrainers();
            }
        }

        private VocationalTrainer? GetSelectedTrainer()
        {
            if (_dgvTrainers.SelectedRows.Count == 0) { Info("Select a trainer first."); return null; }
            var id = (int)_dgvTrainers.SelectedRows[0].Cells["Id"].Value;
            return _trainerService.GetById(id);
        }

        // ── Dialogs ───────────────────────────────────────────────────────────

        private bool ShowApprenticeDialog(string title, Apprentice a, bool isNew)
        {
            var ro = new TextBox
            {
                ReadOnly  = true,
                BackColor = Color.FromArgb(240, 244, 248),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Italic)
            };

            using var dlg = new Form
            {
                Text            = title,
                Size            = new Size(420, 420),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                Font            = new Font("Segoe UI", 9.5f),
                BackColor       = Color.White
            };

            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 7,
                Padding     = new Padding(14)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int r = 0; r < 7; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            var txtFirst    = new TextBox { Text = a.FirstName, Dock = DockStyle.Fill };
            var txtLast     = new TextBox { Text = a.LastName,  Dock = DockStyle.Fill };
            var txtEmail    = new TextBox { Text = a.Email,     Dock = DockStyle.Fill };
            var txtUsername = new TextBox { Text = a.Username,  Dock = DockStyle.Fill };
            var txtPin      = new TextBox { Dock = DockStyle.Fill, MaxLength = 4, UseSystemPasswordChar = true };
            var txtCompany  = new TextBox { Text = _companyName, Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 244, 248) };
            var txtRole     = new TextBox { Text = "Apprentice",  Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 244, 248) };

            txtPin.KeyPress += (_, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };

            // Auto-generate username from names; stop if the admin has manually edited the field
            bool usernameEdited = false;
            txtUsername.TextChanged += (_, _) =>
            {
                if (txtUsername.Text != GenerateUsername(txtFirst.Text, txtLast.Text))
                    usernameEdited = true;
            };
            void UpdateUsername()
            {
                if (!usernameEdited) txtUsername.Text = GenerateUsername(txtFirst.Text, txtLast.Text);
            }
            txtFirst.TextChanged += (_, _) => UpdateUsername();
            txtLast.TextChanged  += (_, _) => UpdateUsername();

            table.Controls.Add(new Label { Text = "First Name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            table.Controls.Add(txtFirst, 1, 0);
            table.Controls.Add(new Label { Text = "Last Name:",  TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            table.Controls.Add(txtLast, 1, 1);
            table.Controls.Add(new Label { Text = "Email:",      TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            table.Controls.Add(txtEmail, 1, 2);
            table.Controls.Add(new Label { Text = "Username:",   TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            table.Controls.Add(txtUsername, 1, 3);
            table.Controls.Add(new Label { Text = isNew ? "PIN (4 digits):" : "Reset PIN (optional):", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4);
            table.Controls.Add(txtPin, 1, 4);
            table.Controls.Add(new Label { Text = "Company:",    TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 5);
            table.Controls.Add(txtCompany, 1, 5);
            table.Controls.Add(new Label { Text = "Role:",       TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 6);
            table.Controls.Add(txtRole, 1, 6);

            var btnSave   = new Button { Text = "Save",   DialogResult = DialogResult.OK,     Width = 90 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            var btnPanel  = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            dlg.Controls.Add(table);
            dlg.Controls.Add(btnPanel);
            dlg.AcceptButton = btnSave;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return false;

            var fn  = txtFirst.Text.Trim();
            var ln  = txtLast.Text.Trim();
            var pin = txtPin.Text.Trim();

            var un  = txtUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            { Info("First and last name are required."); return false; }
            if (string.IsNullOrWhiteSpace(un))
            { Info("Username is required."); return false; }
            if (isNew && pin.Length != 4)
            { Info("PIN must be exactly 4 digits."); return false; }
            if (!isNew && pin.Length > 0 && pin.Length != 4)
            { Info("Reset PIN must be exactly 4 digits."); return false; }
            if (!IsUsernameUnique(un, excludeApprenticeId: isNew ? -1 : a.Id))
            { Info($"Username \"{un}\" is already in use. Choose a different username."); return false; }

            a.FirstName = fn;
            a.LastName  = ln;
            a.Email     = txtEmail.Text.Trim();
            a.Username  = un;
            if (!string.IsNullOrEmpty(pin)) a.Pin = pin;
            return true;
        }

        private bool ShowTrainerDialog(string title, VocationalTrainer t, bool isNew)
        {
            using var dlg = new Form
            {
                Text            = title,
                Size            = new Size(420, 460),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                Font            = new Font("Segoe UI", 9.5f),
                BackColor       = Color.White
            };

            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 8,
                Padding     = new Padding(14)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int r = 0; r < 8; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            var txtFirst    = new TextBox { Text = t.FirstName, Dock = DockStyle.Fill };
            var txtLast     = new TextBox { Text = t.LastName,  Dock = DockStyle.Fill };
            var txtEmail    = new TextBox { Text = t.Email,     Dock = DockStyle.Fill };
            var txtPhone    = new TextBox { Text = t.Phone,     Dock = DockStyle.Fill };
            var txtUsername = new TextBox { Text = t.Username,  Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 244, 248) };
            var txtPin      = new TextBox { Dock = DockStyle.Fill, MaxLength = 4, UseSystemPasswordChar = true };
            var txtCompany  = new TextBox { Text = _companyName, Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 244, 248) };
            var txtRole     = new TextBox { Text = "Trainer",    Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 244, 248) };

            txtPin.KeyPress += (_, e) => { if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true; };

            // Auto-generate username whenever first or last name changes
            void UpdateUsername() => txtUsername.Text = GenerateUsername(txtFirst.Text, txtLast.Text);
            txtFirst.TextChanged += (_, _) => UpdateUsername();
            txtLast.TextChanged  += (_, _) => UpdateUsername();

            table.Controls.Add(new Label { Text = "First Name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            table.Controls.Add(txtFirst, 1, 0);
            table.Controls.Add(new Label { Text = "Last Name:",  TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            table.Controls.Add(txtLast, 1, 1);
            table.Controls.Add(new Label { Text = "Email:",      TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            table.Controls.Add(txtEmail, 1, 2);
            table.Controls.Add(new Label { Text = "Phone:",      TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            table.Controls.Add(txtPhone, 1, 3);
            table.Controls.Add(new Label { Text = "Username:",   TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 4);
            table.Controls.Add(txtUsername, 1, 4);
            table.Controls.Add(new Label { Text = isNew ? "PIN (4 digits):" : "Reset PIN (optional):", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 5);
            table.Controls.Add(txtPin, 1, 5);
            table.Controls.Add(new Label { Text = "Company:",    TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 6);
            table.Controls.Add(txtCompany, 1, 6);
            table.Controls.Add(new Label { Text = "Role:",       TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 7);
            table.Controls.Add(txtRole, 1, 7);

            var btnSave   = new Button { Text = "Save",   DialogResult = DialogResult.OK,     Width = 90 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            var btnPanel  = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            dlg.Controls.Add(table);
            dlg.Controls.Add(btnPanel);
            dlg.AcceptButton = btnSave;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return false;

            var fn  = txtFirst.Text.Trim();
            var ln  = txtLast.Text.Trim();
            var un  = txtUsername.Text.Trim();
            var pin = txtPin.Text.Trim();

            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            { Info("First and last name are required."); return false; }
            if (string.IsNullOrWhiteSpace(un))
            { Info("Username is required."); return false; }
            if (isNew && (pin.Length < 4 || pin.Length > 6))
            { Info("PIN must be between 4 and 6 digits."); return false; }
            if (!isNew && pin.Length > 0 && (pin.Length < 4 || pin.Length > 6))
            { Info("Reset PIN must be between 4 and 6 digits."); return false; }
            if (!IsUsernameUnique(un, excludeTrainerId: isNew ? -1 : t.Id))
            { Info($"Username \"{un}\" is already in use. Choose a different username."); return false; }

            t.FirstName = fn;
            t.LastName  = ln;
            t.Email     = txtEmail.Text.Trim();
            t.Phone     = txtPhone.Text.Trim();
            t.Username  = un;
            if (!string.IsNullOrEmpty(pin)) t.Pin = pin;
            return true;
        }

        // ── Company Info panel ────────────────────────────────────────────────

        private void BuildCompanyInfoPanel()
        {
            _pnlCompanyInfo.BackColor = C_Bg;

            var title = PageTitle("Company Info", $"Details for {_companyName}");

            var company = _companyService.GetById(_companyId);

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);

            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                ColumnCount = 2,
                RowCount    = 6,
                Padding     = new Padding(24, 20, 24, 20),
                AutoSize    = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int r = 0; r < 6; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            Label InfoLabel(string t) => new()
            {
                Text      = t,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C_Muted,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label InfoValue(string t) => new()
            {
                Text      = t,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft
            };

            string adminUser = company?.AdminUsername ?? Session.Username;

            table.Controls.Add(InfoLabel("Company"),        0, 0); table.Controls.Add(InfoValue(company?.Name     ?? _companyName),   1, 0);
            table.Controls.Add(InfoLabel("Industry"),       0, 1); table.Controls.Add(InfoValue(company?.Industry ?? "—"),            1, 1);
            table.Controls.Add(InfoLabel("Address"),        0, 2); table.Controls.Add(InfoValue(company?.Address  ?? "—"),            1, 2);
            table.Controls.Add(InfoLabel("Phone"),          0, 3); table.Controls.Add(InfoValue(company?.Phone    ?? "—"),            1, 3);
            table.Controls.Add(InfoLabel("Email"),          0, 4); table.Controls.Add(InfoValue(company?.Email    ?? "—"),            1, 4);
            table.Controls.Add(InfoLabel("Admin Username"), 0, 5); table.Controls.Add(InfoValue(adminUser),                          1, 5);

            card.Controls.Add(table);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };
            _pnlCompanyInfo.Controls.Add(bottomBar);
            _pnlCompanyInfo.Controls.Add(card);
            _pnlCompanyInfo.Controls.Add(title);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowPanel(Panel target, Button navBtn)
        {
            foreach (var p in new[] { _pnlApprentices, _pnlTrainers, _pnlCompanyInfo })
                p.Visible = p == target;
            foreach (Control c in _sidebar.Controls)
                if (c is Button b) { b.BackColor = C_Sidebar; b.ForeColor = Color.FromArgb(148, 163, 184); b.Font = new Font("Segoe UI", 10f); }
            navBtn.BackColor = C_Active;
            navBtn.ForeColor = Color.White;
            navBtn.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        private static Button NavBtn(string text)
        {
            var b = new Button
            {
                Text      = "  " + text,
                Dock      = DockStyle.Top,
                Height    = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.FromArgb(15, 23, 41),
                Font      = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize        = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            return b;
        }

        private static Button ActionBtn(string text, Color color)
        {
            var btn = new Button
            {
                Text      = text,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height    = 36,
                Width     = 134,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static Panel PageTitle(string t, string sub)
        {
            var p = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = C_Bg };
            p.Controls.Add(new Label
            {
                Text      = sub,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                BackColor = C_Bg,
                Location  = new Point(2, 42),
                AutoSize  = true
            });
            p.Controls.Add(new Label
            {
                Text      = t,
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = C_Bg,
                Location  = new Point(0, 4),
                AutoSize  = true
            });
            return p;
        }

        private static DataGridView MakeGrid()
        {
            var d = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                RowHeadersVisible     = false,
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor             = Color.FromArgb(226, 232, 240),
                Font                  = new Font("Segoe UI", 10f),
                RowTemplate           = { Height = 42 }
            };
            d.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            d.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            d.ColumnHeadersHeight                     = 38;
            d.ColumnHeadersHeightSizeMode             = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            d.EnableHeadersVisualStyles               = false;
            d.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(219, 234, 254);
            d.DefaultCellStyle.SelectionForeColor     = Color.FromArgb(15, 23, 42);
            return d;
        }

        private static PaintEventHandler BuildBorderPaintHandler(Panel p) =>
            (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 0.5f);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };

        private static void AutoSizeCols(DataGridView d, string fillCol)
        {
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (DataGridViewColumn col in d.Columns)
                col.AutoSizeMode = col.Name == fillCol
                    ? DataGridViewAutoSizeColumnMode.Fill
                    : DataGridViewAutoSizeColumnMode.AllCells;
        }

        private static void Info(string msg) =>
            MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Returns true when the username is not already taken by any apprentice, trainer, or company admin.
        // excludeApprenticeId / excludeTrainerId let you skip the record currently being edited.
        private bool IsUsernameUnique(string username, int excludeApprenticeId = -1, int excludeTrainerId = -1)
        {
            var lower = username.ToLowerInvariant();
            if (_apprenticeService.GetAll().Any(a => a.Id != excludeApprenticeId && a.Username.ToLower() == lower))
                return false;
            if (_trainerService.GetAll().Any(t => t.Id != excludeTrainerId && t.Username.ToLower() == lower))
                return false;
            if (_companyService.GetAll().Any(c => c.AdminUsername.ToLower() == lower))
                return false;
            return true;
        }

        private static string GenerateUsername(string firstName, string lastName)
            
        {
            string Normalize(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                s = s.Trim().ToLowerInvariant();

                s = s.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue")
                    .Replace("à", "a").Replace("á", "a").Replace("â", "a")
                    .Replace("è", "e").Replace("é", "e").Replace("ê", "e")
                    .Replace("ì", "i").Replace("í", "i").Replace("î", "i")
                    .Replace("ò", "o").Replace("ó", "o").Replace("ô", "o")
                    .Replace("ù", "u").Replace("ú", "u").Replace("û", "u")
                    .Replace("ç", "c");

                var sb = new System.Text.StringBuilder(s.Length);
                foreach (var ch in s)
                {
                    if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                        sb.Append(ch);
                }
                return sb.ToString();
            }

            var fn = Normalize(firstName);
            var ln = Normalize(lastName);

            if (fn.Length == 0 && ln.Length == 0) return "user";
            if (fn.Length == 0) return ln;
            if (ln.Length == 0) return fn;

            var username = $"{fn}.{ln}";
            if (username.Length > 25) username = username.Substring(0, 25);
            return username;
        }
    }
}
