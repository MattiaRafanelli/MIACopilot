using System.Drawing;
using System.Windows.Forms;
using MIACopilot.Resources;

namespace MIACopilot.Helpers;

public static class UiHelpers
{
    public static Panel Spacer(int height) => new()
    {
        Height = height,
        Dock = DockStyle.Top,
        BackColor = Color.Transparent
    };

    public static Label MakeLabel(string text, ContentAlignment align = ContentAlignment.MiddleLeft)
        => new()
        {
            Text = text,
            ForeColor = AppColors.TextSecondary,
            TextAlign = align,
            AutoSize = false,
            Dock = DockStyle.Fill
        };

    public static Button MakeSolidButton(string text, Color backColor, int width = 92, int height = 34)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Width = width,
            Height = height,
            Font = AppFonts.BodyBold(9f),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}
