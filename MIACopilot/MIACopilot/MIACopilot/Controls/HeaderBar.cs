using System.Drawing;
using System.Windows.Forms;
using MIACopilot.Helpers;
using MIACopilot.Resources;

namespace MIACopilot.Controls;

public class HeaderBar : UserControl
{
    public Label TitleLabel { get; } = new();
    public TextBox SearchBox { get; } = new();
    public Button SearchButton { get; } = UiHelpers.MakeSolidButton("Search", AppColors.NavHover, 88);

    private readonly FlowLayoutPanel _rightActions = new();

    // Builds the header bar layout with title, search box, search button, and right-aligned action buttons.
    public HeaderBar()
    {
        Dock = DockStyle.Top;
        Height = 60;
        BackColor = AppColors.Surface;
        Padding = new Padding(12, 10, 12, 8);

        TitleLabel.Text = "Title";
        TitleLabel.Font = AppFonts.Section(14f);
        TitleLabel.ForeColor = AppColors.TextPrimary;
        TitleLabel.AutoSize = true;
        TitleLabel.Location = new Point(8, 14);

        SearchBox.Width = 230;
        SearchBox.Height = 28;
        SearchBox.BorderStyle = BorderStyle.FixedSingle;
        SearchBox.Font = AppFonts.Body(10f);
        SearchBox.Location = new Point(240, 16);

        SearchButton.Location = new Point(478, 14);

        _rightActions.Dock = DockStyle.Right;
        _rightActions.AutoSize = true;
        _rightActions.FlowDirection = FlowDirection.RightToLeft;
        _rightActions.WrapContents = false;
        _rightActions.BackColor = AppColors.Surface;
        _rightActions.Padding = new Padding(0, 0, 0, 0);

        Controls.Add(TitleLabel);
        Controls.Add(SearchBox);
        Controls.Add(SearchButton);
        Controls.Add(_rightActions);
    }

    // Updates the displayed title text.
    public void SetTitle(string title) => TitleLabel.Text = title;

    // Adds a button to the right-aligned action area.
    public void AddRightButton(Button button)
    {
        button.Margin = new Padding(6, 1, 0, 0);
        _rightActions.Controls.Add(button);
    }

    // Removes all buttons from the right-aligned action area.
    public void ClearRightButtons() => _rightActions.Controls.Clear();
}
