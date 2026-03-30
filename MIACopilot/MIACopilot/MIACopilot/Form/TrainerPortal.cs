using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms
{
    /// <summary>
    /// Portal for a logged-in vocational trainer. Provides profile management
    /// and read/write access to the apprentices assigned to this trainer.
    /// </summary>
    public class TrainerPortal : Form
    {
        // ── Dependencies ──────────────────────────────────────────────────────
        private readonly VocationalTrainer _trainer;
        private readonly ApprenticeService _apprenticeService;
        private readonly CompanyService _companyService;
        private readonly VocationalTrainerService _trainerService;
        private readonly GradeService _gradeService;

        // ── Sidebar nav buttons ───────────────────────────────────────────────
        private Button _btnNavProfile = new();
        private Button _btnNavApprentices = new();

        // ── Content panels ────────────────────────────────────────────────────
        private Panel _pnlProfile = new();
        private Panel _pnlApprentices = new();
        private Panel _pnlGrades = new();
        private Panel _pnlJournals = new();

        // ── Apprentice panel grid ─────────────────────────────────────────────
        private DataGridView _dgvApprentices = new();

        // ── Grades view grid ──────────────────────────────────────────────────
        private DataGridView _dgvGrades = new();

        // ── Journals view grid ────────────────────────────────────────────────
        private DataGridView _dgvJournals = new();

        // ── Sidebar ───────────────────────────────────────────────────────────
        private Panel _sidebar = new();

        // ── Currently selected apprentice (for grades / journals views) ────────
        private Apprentice? _selectedApprentice;

        // ── Color palette (matches ApprenticePortal) ──────────────────────────
        private static readonly Color C_Sidebar = Color.FromArgb(15, 23, 41);
        private static readonly Color C_SideHov = Color.FromArgb(30, 41, 59);
        private static readonly Color C_Active  = Color.FromArgb(59, 130, 246);
        private static readonly Color C_Bg      = Color.FromArgb(248, 250, 252);
        private static readonly Color C_White   = Color.White;
        private static readonly Color C_Border  = Color.FromArgb(226, 232, 240);
        private static readonly Color C_Text    = Color.FromArgb(15, 23, 42);
        private static readonly Color C_Muted   = Color.FromArgb(100, 116, 139);
        private static readonly Color C_Green   = Color.FromArgb(16, 185, 129);
        private static readonly Color C_Blue    = Color.FromArgb(59, 130, 246);
        private static readonly Color C_Orange  = Color.FromArgb(245, 158, 11);
        private static readonly Color C_Red     = Color.FromArgb(239, 68, 68);

        // ─────────────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the TrainerPortal with the logged-in trainer and all required services,
        /// builds the UI, and opens the profile panel by default.
        /// </summary>
        /// <param name="trainer">The currently logged-in vocational trainer.</param>
        /// <param name="apprenticeService">Service for apprentice data access.</param>
        /// <param name="companyService">Service for company data access.</param>
        /// <param name="trainerService">Service for trainer data access.</param>
        /// <param name="gradeService">Service for grade data access.</param>
        public TrainerPortal(
            VocationalTrainer trainer,
            ApprenticeService apprenticeService,
            CompanyService companyService,
            VocationalTrainerService trainerService,
            GradeService gradeService)
        {
            _trainer           = trainer           ?? throw new ArgumentNullException(nameof(trainer));
            _apprenticeService = apprenticeService ?? throw new ArgumentNullException(nameof(apprenticeService));
            _companyService    = companyService    ?? throw new ArgumentNullException(nameof(companyService));
            _trainerService    = trainerService    ?? throw new ArgumentNullException(nameof(trainerService));
            _gradeService      = gradeService      ?? throw new ArgumentNullException(nameof(gradeService));

            BuildUi();
            ShowPanel(_pnlProfile, _btnNavProfile);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI Construction
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the overall form layout: sidebar, content area, and all sub-panels.
        /// </summary>
        private void BuildUi()
        {
            Text          = $"MIA Copilot — {_trainer.FullName}";
            Size          = new Size(1020, 680);
            MinimumSize   = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = C_Bg;
            Font          = new Font("Segoe UI", 10f);

            BuildSidebar();

            var content = new Panel { Dock = DockStyle.Fill, BackColor = C_Bg, Padding = new Padding(28, 24, 28, 24) };

            BuildProfilePanel();
            BuildApprenticesPanel();
            BuildGradesPanel();
            BuildJournalsPanel();

            foreach (var p in new[] { _pnlProfile, _pnlApprentices, _pnlGrades, _pnlJournals })
            {
                p.Dock = DockStyle.Fill;
                content.Controls.Add(p);
            }

            Controls.Add(content);
            Controls.Add(_sidebar);
        }

        /// <summary>
        /// Builds the dark left sidebar with navigation buttons and a logout button at the bottom.
        /// </summary>
        private void BuildSidebar()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = C_Sidebar };

            // ── Avatar / name area ──────────────────────────────────────────
            var avatarPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = C_Sidebar };
            avatarPanel.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Avatar circle
                using var avBrush = new SolidBrush(C_Blue);
                g.FillEllipse(avBrush, 20, 20, 58, 58);
                using var initFont = new Font("Segoe UI", 20f, FontStyle.Bold);
                var init = $"{(_trainer.FirstName.Length > 0 ? _trainer.FirstName[0] : '?')}{(_trainer.LastName.Length > 0 ? _trainer.LastName[0] : '?')}".ToUpper();
                var sz   = g.MeasureString(init, initFont);
                g.DrawString(init, initFont, Brushes.White, 20 + (58 - sz.Width) / 2, 20 + (58 - sz.Height) / 2);

                // Name and role — clipped with ellipsis for long names
                using var nameFont  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var subFont   = new Font("Segoe UI", 8.5f);
                using var muteBrush = new SolidBrush(C_Muted);
                using var sf        = new System.Drawing.StringFormat { Trimming = System.Drawing.StringTrimming.EllipsisCharacter };
                g.DrawString(_trainer.FullName,        nameFont, Brushes.White, new RectangleF(88, 28, 132, 22), sf);
                g.DrawString("Vocational Trainer",     subFont,  muteBrush,    88, 52);
                g.DrawString($"@{_trainer.Username}", subFont,  muteBrush,    new RectangleF(88, 70, 132, 18), sf);
            };

            var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(30, 41, 59) };

            _btnNavProfile     = NavBtn("👤  My Profile");
            _btnNavApprentices = NavBtn("🎓  Apprentices");

            _btnNavProfile.Click     += (_, _) => ShowPanel(_pnlProfile,     _btnNavProfile);
            _btnNavApprentices.Click += (_, _) => ShowPanel(_pnlApprentices, _btnNavApprentices);

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
            _sidebar.Controls.Add(_btnNavApprentices);
            _sidebar.Controls.Add(_btnNavProfile);
            _sidebar.Controls.Add(sep);
            _sidebar.Controls.Add(avatarPanel);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Profile Panel
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the "My Profile" panel with editable fields for the trainer's own data
        /// and a "Change PIN" button.
        /// </summary>
        private void BuildProfilePanel()
        {
            _pnlProfile.BackColor = C_Bg;

            var title = PageTitle("My Profile", "Your personal information");

            // Banner
            var banner = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = C_Sidebar };
            banner.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Avatar
                using var avBrush = new SolidBrush(C_Blue);
                g.FillEllipse(avBrush, 28, 30, 72, 72);
                using var initFont = new Font("Segoe UI", 24f, FontStyle.Bold);
                var init = $"{(_trainer.FirstName.Length > 0 ? _trainer.FirstName[0] : '?')}{(_trainer.LastName.Length > 0 ? _trainer.LastName[0] : '?')}".ToUpper();
                var sz   = g.MeasureString(init, initFont);
                g.DrawString(init, initFont, Brushes.White, 28 + (72 - sz.Width) / 2, 30 + (72 - sz.Height) / 2);

                // Name, role, username
                using var nameFont = new Font("Segoe UI", 14f, FontStyle.Bold);
                using var roleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                using var subFont  = new Font("Segoe UI", 8.5f);
                using var accentBr = new SolidBrush(C_Blue);
                using var muteBr   = new SolidBrush(Color.FromArgb(148, 163, 184));
                g.DrawString(_trainer.FullName,         nameFont, Brushes.White, 116, 38);
                g.DrawString("● VOCATIONAL TRAINER",    roleFont, accentBr,     118, 66);
                g.DrawString($"@{_trainer.Username}",  subFont,  muteBr,       118, 88);
            };

            // ── Card ───────────────────────────────────────────────────────
            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);

            // View-mode value labels
            var lblFirstVal = ProfileVal(_trainer.FirstName, 22);
            var lblLastVal  = ProfileVal(_trainer.LastName,  58);
            var lblEmailVal = ProfileVal(_trainer.Email,     94);
            var lblPhoneVal = ProfileVal(_trainer.Phone,    130);

            // Edit-mode text boxes (hidden initially)
            var txtFirst = new TextBox { Location = new Point(200, 20),  Size = new Size(380, 28), Font = new Font("Segoe UI", 10.5f), Visible = false };
            var txtLast  = new TextBox { Location = new Point(200, 56),  Size = new Size(380, 28), Font = new Font("Segoe UI", 10.5f), Visible = false };
            var txtEmail = new TextBox { Location = new Point(200, 92),  Size = new Size(380, 28), Font = new Font("Segoe UI", 10.5f), Visible = false };
            var txtPhone = new TextBox { Location = new Point(200, 128), Size = new Size(380, 28), Font = new Font("Segoe UI", 10.5f), Visible = false };

            // Row headers
            (string lbl, int y)[] rows =
            {
                ("First Name", 22),
                ("Last Name",  58),
                ("Email",      94),
                ("Phone",     130),
            };
            foreach (var (lbl, y) in rows)
            {
                card.Controls.Add(new Label
                {
                    Text      = lbl,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = C_Muted,
                    Location  = new Point(24, y),
                    Size      = new Size(170, 28),
                    TextAlign = ContentAlignment.MiddleLeft
                });
            }

            // ── Buttons ────────────────────────────────────────────────────
            var btnEdit = new Button
            {
                Text      = "✏  Edit Profile",
                Location  = new Point(24, 180),
                Size      = new Size(152, 36),
                BackColor = C_Blue,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;

            var btnSave = new Button
            {
                Text      = "💾  Save",
                Location  = new Point(24, 180),
                Size      = new Size(110, 36),
                BackColor = C_Green,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnSave.FlatAppearance.BorderSize = 0;

            var btnCancelEdit = new Button
            {
                Text      = "✖  Cancel",
                Location  = new Point(142, 180),
                Size      = new Size(110, 36),
                BackColor = C_Muted,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnCancelEdit.FlatAppearance.BorderSize = 0;

            var btnChangePin = new Button
            {
                Text      = "🔑  Change PIN",
                Location  = new Point(24, 226),
                Size      = new Size(160, 38),
                BackColor = C_Blue,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnChangePin.FlatAppearance.BorderSize = 0;
            btnChangePin.Click += (_, _) => ChangeTrainerPin();

            // ── Local helpers for profile edit mode ────────────────────────
            void EnterEditMode()
            {
                txtFirst.Text = _trainer.FirstName;
                txtLast.Text  = _trainer.LastName;
                txtEmail.Text = _trainer.Email;
                txtPhone.Text = _trainer.Phone;
                lblFirstVal.Visible = lblLastVal.Visible = lblEmailVal.Visible = lblPhoneVal.Visible = false;
                txtFirst.Visible    = txtLast.Visible    = txtEmail.Visible    = txtPhone.Visible    = true;
                btnEdit.Visible     = false;
                btnSave.Visible     = btnCancelEdit.Visible = true;
            }

            void ExitEditMode()
            {
                lblFirstVal.Visible = lblLastVal.Visible = lblEmailVal.Visible = lblPhoneVal.Visible = true;
                txtFirst.Visible    = txtLast.Visible    = txtEmail.Visible    = txtPhone.Visible    = false;
                btnEdit.Visible     = true;
                btnSave.Visible     = btnCancelEdit.Visible = false;
            }

            btnEdit.Click       += (_, _) => EnterEditMode();
            btnCancelEdit.Click += (_, _) => ExitEditMode();
            btnSave.Click       += (_, _) =>
            {
                var fn = txtFirst.Text.Trim();
                var ln = txtLast.Text.Trim();
                if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
                {
                    MessageBox.Show("First and last name are required.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _trainer.FirstName = fn;
                _trainer.LastName  = ln;
                _trainer.Email     = txtEmail.Text.Trim();
                _trainer.Phone     = txtPhone.Text.Trim();
                _trainerService.Update(_trainer);
                lblFirstVal.Text = fn;
                lblLastVal.Text  = ln;
                lblEmailVal.Text = _trainer.Email;
                lblPhoneVal.Text = _trainer.Phone;
                ExitEditMode();
                banner.Invalidate();
                MessageBox.Show("Profile updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[]
            {
                lblFirstVal, lblLastVal, lblEmailVal, lblPhoneVal,
                txtFirst, txtLast, txtEmail, txtPhone,
                btnEdit, btnSave, btnCancelEdit, btnChangePin
            });

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };

            _pnlProfile.Controls.Add(bottomBar);
            _pnlProfile.Controls.Add(card);
            _pnlProfile.Controls.Add(banner);
            _pnlProfile.Controls.Add(title);
        }

        /// <summary>
        /// Shows the Change PIN dialog and updates the trainer's PIN via the trainer service on confirmation.
        /// </summary>
        private void ChangeTrainerPin()
        {
            using var dlg = new Form
            {
                Text            = "Change PIN",
                Size            = new Size(340, 220),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                Font            = new Font("Segoe UI", 9.5f)
            };

            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 3,
                Padding     = new Padding(14)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int r = 0; r < 3; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            var txtOldPin  = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill };
            var txtNewPin  = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill };
            var txtNewPin2 = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Fill };

            table.Controls.Add(new Label { Text = "Current PIN:",   TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            table.Controls.Add(txtOldPin,  1, 0);
            table.Controls.Add(new Label { Text = "New PIN:",       TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            table.Controls.Add(txtNewPin,  1, 1);
            table.Controls.Add(new Label { Text = "Confirm PIN:",   TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            table.Controls.Add(txtNewPin2, 1, 2);

            var btnOk     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Width = 80 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
            var btnPanel  = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(8)
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnOk);

            dlg.Controls.Add(table);
            dlg.Controls.Add(btnPanel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            if (txtOldPin.Text != _trainer.Pin)
            {
                MessageBox.Show("Current PIN is incorrect.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var newPin = txtNewPin.Text.Trim();
            if (newPin.Length != 4 || !newPin.All(char.IsDigit))
            {
                MessageBox.Show("New PIN must be exactly 4 digits.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPin != txtNewPin2.Text.Trim())
            {
                MessageBox.Show("The two new PINs do not match.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _trainer.Pin = newPin;
            _trainerService.Update(_trainer);
            MessageBox.Show("PIN changed successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Apprentices Panel
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the apprentice management panel with a toolbar (Add, Edit, Delete, Grades, Journals)
        /// and a DataGridView listing the trainer's own apprentices.
        /// </summary>
        private void BuildApprenticesPanel()
        {
            _pnlApprentices.BackColor = C_Bg;

            var title = PageTitle("Apprentices", "Read-only view of your assigned apprentices");

            // ── Grid ───────────────────────────────────────────────────────
            _dgvApprentices = MakeGrid();
            LoadApprentices();

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);
            card.Controls.Add(_dgvApprentices);

            // Bottom action bar — Grades/Journals right-aligned
            var actionBar = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = C_White };
            actionBar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, 0, actionBar.Width, 0);
            };

            var btnGrades   = ActionBtn("📊  Grades",   C_Orange);
            var btnJournals = ActionBtn("📓  Journals", C_Muted);
            btnGrades.Width   = 110;
            btnJournals.Width = 120;

            var actionFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(8, 8, 8, 8)
            };
            btnGrades.Click   += (_, _) => OpenGradesView();
            btnJournals.Click += (_, _) => OpenJournalsView();
            actionFlow.Controls.Add(btnGrades);
            actionFlow.Controls.Add(btnJournals);
            actionBar.Controls.Add(actionFlow);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };

            _pnlApprentices.Controls.Add(bottomBar);
            _pnlApprentices.Controls.Add(actionBar);
            _pnlApprentices.Controls.Add(card);
            _pnlApprentices.Controls.Add(title);
        }

        /// <summary>
        /// Loads all apprentices assigned to this trainer and binds them to the apprentice grid.
        /// </summary>
        private void LoadApprentices()
        {
            var rows = _apprenticeService
                .GetAll()
                .Where(a => a.VocationalTrainerId == _trainer.Id)
                .Select(a => new
                {
                    a.Id,
                    a.FirstName,
                    a.LastName,
                    a.Email,
                    StartDate = a.StartDate.ToString("dd.MM.yyyy"),
                    Company   = _companyService.GetById(a.CompanyId)?.Name ?? "—"
                })
                .ToList();

            _dgvApprentices.DataSource = rows;
            AutoSizeCols(_dgvApprentices, "Email");
        }

        /// <summary>
        /// Opens a dialog to create a new apprentice and assigns it to this trainer's company.
        /// </summary>
        private void AddApprentice()
        {
            var newApprentice = new Apprentice
            {
                VocationalTrainerId = _trainer.Id,
                CompanyId           = _trainer.CompanyId,
                StartDate           = DateTime.Today,
                Pin                 = "0000"
            };

            if (ShowApprenticeDialog("Add Apprentice", newApprentice))
            {
                _apprenticeService.Create(newApprentice);
                LoadApprentices();
                MessageBox.Show("Apprentice created successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Opens an edit dialog pre-filled with the selected apprentice's data and persists changes.
        /// </summary>
        private void EditApprentice()
        {
            var apprentice = GetSelectedApprentice();
            if (apprentice == null) return;

            if (ShowApprenticeDialog("Edit Apprentice", apprentice))
            {
                _apprenticeService.Update(apprentice);
                LoadApprentices();
                MessageBox.Show("Apprentice updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Asks for confirmation and deletes the selected apprentice.
        /// </summary>
        private void DeleteApprentice()
        {
            var apprentice = GetSelectedApprentice();
            if (apprentice == null) return;

            var result = MessageBox.Show(
                $"Delete apprentice \"{apprentice.FullName}\"? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _apprenticeService.Delete(apprentice.Id);
                LoadApprentices();
            }
        }

        /// <summary>
        /// Stores the selected apprentice and navigates to the read-only Grades view.
        /// </summary>
        private void OpenGradesView()
        {
            var apprentice = GetSelectedApprentice();
            if (apprentice == null) return;

            _selectedApprentice = apprentice;
            LoadGradesForApprentice(apprentice);
            ShowPanel(_pnlGrades, null);
        }

        /// <summary>
        /// Stores the selected apprentice and navigates to the read-only Journals view.
        /// </summary>
        private void OpenJournalsView()
        {
            var apprentice = GetSelectedApprentice();
            if (apprentice == null) return;

            _selectedApprentice = apprentice;
            LoadJournalsForApprentice(apprentice);
            ShowPanel(_pnlJournals, null);
        }

        /// <summary>
        /// Retrieves the currently selected apprentice from the grid, or shows an info message if none is selected.
        /// Returns null if nothing is selected or the apprentice cannot be found.
        /// </summary>
        private Apprentice? GetSelectedApprentice()
        {
            if (_dgvApprentices.SelectedRows.Count == 0)
            {
                Info("Select an apprentice first.");
                return null;
            }

            var id = (int)_dgvApprentices.SelectedRows[0].Cells["Id"].Value;
            return _apprenticeService.GetById(id);
        }

        /// <summary>
        /// Shows an inline dialog to create or edit an apprentice's fields (FirstName, LastName, Email, Username).
        /// Mutates the passed apprentice object on confirmation.
        /// Returns true if the user confirmed with valid input.
        /// </summary>
        /// <param name="dialogTitle">Title displayed in the dialog window.</param>
        /// <param name="apprentice">The apprentice object to populate or update.</param>
        private bool ShowApprenticeDialog(string dialogTitle, Apprentice apprentice)
        {
            using var dlg = new Form
            {
                Text            = dialogTitle,
                Size            = new Size(380, 280),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                Font            = new Font("Segoe UI", 9.5f)
            };

            var table = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 4,
                Padding     = new Padding(12)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int r = 0; r < 4; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            var txtFirst    = new TextBox { Text = apprentice.FirstName, Dock = DockStyle.Fill };
            var txtLast     = new TextBox { Text = apprentice.LastName,  Dock = DockStyle.Fill };
            var txtEmail    = new TextBox { Text = apprentice.Email,     Dock = DockStyle.Fill };
            var txtUsername = new TextBox { Text = apprentice.Username,  Dock = DockStyle.Fill };

            table.Controls.Add(new Label { Text = "First Name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            table.Controls.Add(txtFirst, 1, 0);
            table.Controls.Add(new Label { Text = "Last Name:",  TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            table.Controls.Add(txtLast, 1, 1);
            table.Controls.Add(new Label { Text = "Email:",      TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            table.Controls.Add(txtEmail, 1, 2);
            table.Controls.Add(new Label { Text = "Username:",   TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            table.Controls.Add(txtUsername, 1, 3);

            var btnOk     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Width = 80 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
            var btnPanel  = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(8)
            };
            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnOk);

            dlg.Controls.Add(table);
            dlg.Controls.Add(btnPanel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return false;

            var fn = txtFirst.Text.Trim();
            var ln = txtLast.Text.Trim();
            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            {
                Info("First and last name are required.");
                return false;
            }

            apprentice.FirstName = fn;
            apprentice.LastName  = ln;
            apprentice.Email     = txtEmail.Text.Trim();
            apprentice.Username  = txtUsername.Text.Trim();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Grades View (read-only)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the read-only grades view panel with a back button and a DataGridView.
        /// </summary>
        private void BuildGradesPanel()
        {
            _pnlGrades.BackColor = C_Bg;

            var title = PageTitle("Grades", "Read-only view of the selected apprentice's grades");

            // ── Back toolbar ───────────────────────────────────────────────
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
            toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var btnBack = ActionBtn("← Back", C_Muted);
            btnBack.Width    = 110;
            btnBack.Location = new Point(16, 12);
            btnBack.Click   += (_, _) =>
            {
                _selectedApprentice = null;
                ShowPanel(_pnlApprentices, _btnNavApprentices);
            };

            toolbar.Controls.Add(btnBack);

            // ── Grid ───────────────────────────────────────────────────────
            _dgvGrades = MakeGrid();

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);
            card.Controls.Add(_dgvGrades);
            card.Controls.Add(toolbar);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };

            _pnlGrades.Controls.Add(bottomBar);
            _pnlGrades.Controls.Add(card);
            _pnlGrades.Controls.Add(title);
        }

        /// <summary>
        /// Loads grades for the given apprentice and binds them to the grades grid.
        /// Also updates the page title subtitle to show the apprentice's name.
        /// </summary>
        /// <param name="apprentice">The apprentice whose grades should be displayed.</param>
        private void LoadGradesForApprentice(Apprentice apprentice)
        {
            var grades = _gradeService.GetByApprentice(apprentice.Id);

            _dgvGrades.DataSource = grades
                .OrderByDescending(g => g.Date)
                .Select(g => new
                {
                    g.Id,
                    g.Subject,
                    Grade  = g.FormattedValue,
                    g.Type,
                    Date   = g.Date.ToString("dd.MM.yyyy"),
                    Status = g.Category,
                    g.Notes
                })
                .ToList();

            AutoSizeCols(_dgvGrades, "Notes");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Journals View (read-only)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the read-only journals view panel with a back button and a DataGridView.
        /// </summary>
        private void BuildJournalsPanel()
        {
            _pnlJournals.BackColor = C_Bg;

            var title = PageTitle("Work Journals", "Read-only view of the selected apprentice's journals");

            // ── Back toolbar ───────────────────────────────────────────────
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = C_White };
            toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(C_Border);
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            var btnBack = ActionBtn("← Back", C_Muted);
            btnBack.Width    = 110;
            btnBack.Location = new Point(16, 12);
            btnBack.Click   += (_, _) =>
            {
                _selectedApprentice = null;
                ShowPanel(_pnlApprentices, _btnNavApprentices);
            };

            toolbar.Controls.Add(btnBack);

            // ── Grid ───────────────────────────────────────────────────────
            _dgvJournals = MakeGrid();

            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_White };
            card.Paint += BuildBorderPaintHandler(card);
            card.Controls.Add(_dgvJournals);
            card.Controls.Add(toolbar);

            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = C_Blue };

            _pnlJournals.Controls.Add(bottomBar);
            _pnlJournals.Controls.Add(card);
            _pnlJournals.Controls.Add(title);
        }

        /// <summary>
        /// Loads work journals for the given apprentice and binds them to the journals grid.
        /// </summary>
        /// <param name="apprentice">The apprentice whose journals should be displayed.</param>
        private void LoadJournalsForApprentice(Apprentice apprentice)
        {
            var fresh    = _apprenticeService.GetById(apprentice.Id);
            var journals = fresh?.WorkJournals ?? apprentice.WorkJournals;

            _dgvJournals.DataSource = journals
                .OrderByDescending(j => j.Date)
                .Select(j => new
                {
                    j.Id,
                    Week    = j.WeekNumber,
                    Date    = j.Date.ToString("dd.MM.yyyy"),
                    j.Title,
                    Preview = j.Content.Length > 80 ? j.Content[..80] + "…" : j.Content
                })
                .ToList();

            AutoSizeCols(_dgvJournals, "Preview");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Switches the visible content panel and highlights the active sidebar navigation button.
        /// Passing null for navBtn leaves all sidebar buttons in the inactive state.
        /// </summary>
        /// <param name="target">The panel to make visible.</param>
        /// <param name="navBtn">The sidebar button that should be styled as active, or null.</param>
        private void ShowPanel(Panel target, Button? navBtn)
        {
            foreach (var p in new[] { _pnlProfile, _pnlApprentices, _pnlGrades, _pnlJournals })
                p.Visible = p == target;

            foreach (Control c in _sidebar.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = C_Sidebar;
                    b.ForeColor = Color.FromArgb(148, 163, 184);
                    b.Font      = new Font("Segoe UI", 10f);
                }
            }

            if (navBtn != null)
            {
                navBtn.BackColor = C_Active;
                navBtn.ForeColor = Color.White;
                navBtn.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
            }
        }

        /// <summary>
        /// Creates a styled sidebar navigation button with hover highlighting.
        /// </summary>
        /// <param name="text">The label text for the button.</param>
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
            b.FlatAppearance.BorderSize         = 0;
            b.FlatAppearance.MouseOverBackColor  = Color.FromArgb(30, 41, 59);
            return b;
        }

        /// <summary>
        /// Creates a styled toolbar action button with the specified label and background color.
        /// </summary>
        /// <param name="text">The button label text.</param>
        /// <param name="color">The background color of the button.</param>
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

        /// <summary>
        /// Creates a page title header panel containing a bold main title and a muted subtitle.
        /// </summary>
        /// <param name="t">The main title text.</param>
        /// <param name="sub">The subtitle/description text.</param>
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

        /// <summary>
        /// Creates a consistently styled read-only DataGridView for use across the portal.
        /// </summary>
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
            var bg = Color.FromArgb(248, 250, 252);
            d.ColumnHeadersDefaultCellStyle.BackColor          = bg;
            d.ColumnHeadersDefaultCellStyle.ForeColor          = Color.FromArgb(100, 116, 139);
            d.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Bold);
            d.ColumnHeadersDefaultCellStyle.SelectionBackColor = bg;
            d.ColumnHeadersDefaultCellStyle.Padding            = new Padding(10, 0, 0, 0);
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            d.ColumnHeadersHeight      = 44;
            d.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            d.DefaultCellStyle.SelectionBackColor       = Color.FromArgb(239, 246, 255);
            d.DefaultCellStyle.SelectionForeColor       = Color.FromArgb(15, 23, 42);
            d.DefaultCellStyle.Padding                  = new Padding(10, 0, 10, 0);
            return d;
        }

        /// <summary>
        /// Returns a PaintEventHandler that draws a thin border rectangle around the given panel.
        /// </summary>
        /// <param name="p">The panel to draw the border around.</param>
        private static PaintEventHandler BuildBorderPaintHandler(Panel p)
        {
            return (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 0.5f);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
        }

        /// <summary>
        /// Configures column auto-sizing on a DataGridView, using Fill for one named column
        /// and AllCells sizing for the rest.
        /// </summary>
        /// <param name="d">The DataGridView to configure.</param>
        /// <param name="fillCol">The name of the column that should use Fill auto-sizing.</param>
        private static void AutoSizeCols(DataGridView d, string fillCol)
        {
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (DataGridViewColumn col in d.Columns)
            {
                col.AutoSizeMode = col.Name == fillCol
                    ? DataGridViewAutoSizeColumnMode.Fill
                    : DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        /// <summary>
        /// Creates a styled read-only value label used in the profile card at a given Y position.
        /// </summary>
        /// <param name="text">The display text for the label.</param>
        /// <param name="y">The vertical offset within the card.</param>
        private static Label ProfileVal(string text, int y)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 10.5f),
                ForeColor = C_Text,
                Location  = new Point(200, y),
                Size      = new Size(400, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// Shows a simple informational message box.
        /// </summary>
        /// <param name="msg">The message to display.</param>
        private static void Info(string msg)
        {
            MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
