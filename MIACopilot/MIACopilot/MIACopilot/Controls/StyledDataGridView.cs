using System.Drawing;
using System.Windows.Forms;
using MIACopilot.Resources;

namespace MIACopilot.Controls;

public class StyledDataGridView : DataGridView
{
    public StyledDataGridView()
    {
        Dock = DockStyle.Fill;
        ReadOnly = true;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        BackgroundColor = AppColors.Surface;
        BorderStyle = BorderStyle.None;
        RowHeadersVisible = false;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = AppColors.GridLine;

        Font = AppFonts.Body(9.5f);

        ColumnHeadersDefaultCellStyle.BackColor = AppColors.GridHeader;
        ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        ColumnHeadersDefaultCellStyle.Font = AppFonts.BodyBold(9.5f);
        ColumnHeadersHeight = 36;

        AlternatingRowsDefaultCellStyle.BackColor = AppColors.SurfaceAlt;

        DefaultCellStyle.SelectionBackColor = AppColors.AccentBlue;
        DefaultCellStyle.SelectionForeColor = Color.White;
    }
}