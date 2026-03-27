using System.Drawing;

namespace MIACopilot.Resources;

public static class AppColors
{
    // Surfaces
    public static readonly Color AppBackground = Color.FromArgb(245, 247, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 252);

    // Text
    public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
    public static readonly Color TextSecondary = Color.FromArgb(51, 65, 85);

    // Navigation
    public static readonly Color NavBackground = Color.FromArgb(30, 41, 59);
    public static readonly Color NavHover = Color.FromArgb(51, 65, 85);
    public static readonly Color NavActive = Color.FromArgb(71, 85, 105);

    // Accents
    public static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentGreen = Color.FromArgb(22, 163, 74);
    public static readonly Color AccentRed = Color.FromArgb(220, 38, 38);
    public static readonly Color AccentIndigo = Color.FromArgb(99, 102, 241);

    // Grid
    public static readonly Color GridLine = Color.FromArgb(226, 232, 240);
    public static readonly Color GridHeader = Color.FromArgb(52, 73, 94);
}
