using System.Drawing;
using System.Windows.Forms;
using MIACopilot.Resources;

namespace MIACopilot.Controls;

public class NavigationButton : Button
{
    public bool IsActive { get; private set; }

    // Initializes a styled navigation button with default colors, font, and hover/active behavior.
    public NavigationButton()
    {
        Height = 44;
        Width = 170;
        FlatStyle = FlatStyle.Flat;
        ForeColor = Color.White;
        BackColor = AppColors.NavBackground;
        Font = AppFonts.BodyBold(10f);
        Cursor = Cursors.Hand;
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(12, 0, 0, 0);

        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = AppColors.NavHover;
        FlatAppearance.MouseDownBackColor = AppColors.NavActive;
    }

    // Sets the active state of the button and updates its background color accordingly.
    public void SetActive(bool active)
    {
        IsActive = active;
        BackColor = active ? AppColors.NavActive : AppColors.NavBackground;
    }
}