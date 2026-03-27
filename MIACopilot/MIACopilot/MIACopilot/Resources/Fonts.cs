using System.Drawing;

namespace MIACopilot.Resources;

public static class AppFonts
{
    public static Font Body(float size = 9.5f) => new("Segoe UI", size, FontStyle.Regular);
    public static Font BodyBold(float size = 9.5f) => new("Segoe UI", size, FontStyle.Bold);

    public static Font Title(float size = 16f) => new("Segoe UI", size, FontStyle.Bold);
    public static Font Section(float size = 12.5f) => new("Segoe UI", size, FontStyle.Bold);
}