using System;
using System.Drawing;
using System.Windows.Forms;

using MIACopilot.Models;
using MIACopilot.Services;

namespace MIACopilot.Forms;

/// <summary>
/// Window for viewing and managing work journal entries of one apprentice.
/// </summary>
public class JournalForm : Form
{
    private readonly Apprentice        _apprentice;
    private readonly ApprenticeService _service;

    private DataGridView dgvJournals = new();
    private Button btnAdd    = new();
    private Button btnEdit   = new();
    private Button btnDelete = new();

    // Initializes the form with the selected apprentice and service, builds the UI, and loads data.
    public JournalForm(Apprentice apprentice, ApprenticeService service)
    {
        _apprentice = apprentice;
        _service    = service;
        BuildUI();
        LoadJournals();
    }

    // Builds the journal management UI (toolbar + data grid).
    private void BuildUI()
    {
        Text            = $"📓  Work Journals — {_apprentice.FullName}";
        Size            = new Size(780, 480);
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition   = FormStartPosition.CenterParent;
        Font            = new Font("Segoe UI", 9.5f);
        BackColor       = Color.White;

        // ── Toolbar buttons ────────────────────────────────────────────────
        btnAdd    = new Button { Text = "➕ Add",    BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 110, Height = 34 };
        btnEdit   = new Button { Text = "✏️ Edit",   BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 110, Height = 34 };
        btnDelete = new Button { Text = "🗑 Delete", BackColor = Color.FromArgb(192, 57, 43), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 110, Height = 34 };
        btnAdd.FlatAppearance.BorderSize    = 0;
        btnEdit.FlatAppearance.BorderSize   = 0;
        btnDelete.FlatAppearance.BorderSize = 0;

        btnAdd.Click    += (_, _) => AddJournal();
        btnEdit.Click   += (_, _) => EditJournal();
        btnDelete.Click += (_, _) => DeleteJournal();

        var toolbar = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 52,
            Padding       = new Padding(8, 8, 8, 0),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor     = Color.White
        };
        toolbar.Controls.Add(btnAdd);
        toolbar.Controls.Add(btnEdit);
        toolbar.Controls.Add(btnDelete);

        // ── Journals grid ───────────────────────────────────────────────────
        dgvJournals = new DataGridView
        {
            Dock                  = DockStyle.Fill,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor             = Color.FromArgb(220, 225, 235),
            Font                  = new Font("Segoe UI", 9.5f),
        };
        dgvJournals.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
        dgvJournals.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvJournals.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvJournals.ColumnHeadersHeight = 36;
        dgvJournals.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
        dgvJournals.DefaultCellStyle.SelectionForeColor = Color.White;

        Controls.Add(dgvJournals);
        Controls.Add(toolbar);
    }

    // Loads all work journals of the apprentice into the grid (with shortened content preview).
    private void LoadJournals()
    {
        dgvJournals.DataSource = _apprentice.WorkJournals.Select(j => new
        {
            j.Id,
            Week    = j.WeekNumber,
            Date    = j.Date.ToString("dd.MM.yyyy"),
            j.Title,
            Content = j.Content.Length > 60 ? j.Content[..60] + "…" : j.Content
        }).ToList();
    }

    // Opens the journal detail dialog in "add" mode and saves the new entry.
    private void AddJournal()
    {
        using var form = new JournalDetailForm(null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _service.AddJournal(_apprentice.Id, form.Result!);
            LoadJournals();
        }
    }

    // Opens the journal detail dialog for the selected entry and updates it.
    private void EditJournal()
    {
        if (dgvJournals.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a journal entry.", "No selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id      = (int)dgvJournals.SelectedRows[0].Cells["Id"].Value;
        var journal = _apprentice.WorkJournals.FirstOrDefault(j => j.Id == id);
        if (journal == null) return;

        using var form = new JournalDetailForm(journal);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _service.UpdateJournal(_apprentice.Id, form.Result!);
            LoadJournals();
        }
    }

    // Confirms and deletes the selected journal entry.
    private void DeleteJournal()
    {
        if (dgvJournals.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a journal entry.", "No selection",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)dgvJournals.SelectedRows[0].Cells["Id"].Value;
        if (MessageBox.Show("Delete this journal entry?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _service.DeleteJournal(_apprentice.Id, id);
            LoadJournals();
        }
    }
}