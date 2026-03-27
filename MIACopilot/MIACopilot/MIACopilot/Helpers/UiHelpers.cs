using System.Drawing;
using System.Windows.Forms;
using MIACopilot.Resources;

namespace MIACopilot.Helpers;

/// <summary>
/// Collection of small UI helper methods to create
/// consistently styled WinForms controls.
/// </summary>
public static class UiHelpers
{
    /// <summary>
    /// Creates an empty spacer panel with a fixed height.
    /// Useful for vertical spacing in stacked layouts.
    /// </summary>
    public static Panel Spacer(int height) => new()
    {
        Height    = height,
        Dock      = DockStyle.Top,
        BackColor = Color.Transparent
    };

    /// <summary>
    /// Creates a label with default secondary text color
    /// and configurable text alignment.
    /// The label fills its parent container.
    /// </summary>
    public static Label MakeLabel(
        string text,
        ContentAlignment align = ContentAlignment.MiddleLeft)
        => new()
        {
            Text      = text,
            ForeColor = AppColors.TextSecondary,
            TextAlign = align,
            AutoSize  = false,
            Dock      = DockStyle.Fill
        };

    /// <summary>
    /// Creates a solid-style button with unified font,
    /// color and flat appearance.
    /// Used for primary and secondary actions.
    /// </summary>
    public static Button MakeSolidButton(
        string text,
        Color backColor,
        int width  = 92,
        int height = 34)
    {
        var btn = new Button
        {
            Text      = text,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Width     = width,
            Height    = height,
            Font      = AppFonts.BodyBold(9f),
            Cursor    = Cursors.Hand
        };

        // Remove default button border
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}
