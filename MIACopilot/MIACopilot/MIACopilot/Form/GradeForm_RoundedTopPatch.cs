using System.Drawing;
using System.Drawing.Drawing2D;

namespace MIACopilot.Forms
{
    /// <summary>
    /// Provides missing RoundedTop helper methods for GradeForm.
    /// Supports int and float based calls.
    /// </summary>
    public partial class GradeForm
    {
        /// <summary>
        /// Rounded top corners using a Rectangle.
        /// </summary>
        private GraphicsPath RoundedTop(Rectangle bounds, int radius)
        {
            int d = radius * 2;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom);
            path.AddLine(bounds.Right, bounds.Bottom, bounds.X, bounds.Bottom);
            path.AddLine(bounds.X, bounds.Bottom, bounds.X, bounds.Y + radius);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Rounded top corners using int coordinates.
        /// </summary>
        private GraphicsPath RoundedTop(int x, int y, int width, int height, int radius)
        {
            return RoundedTop(new Rectangle(x, y, width, height), radius);
        }

        /// <summary>
        /// ✅ Rounded top corners using float coordinates.
        /// This matches calls like RoundedTop(x, y, w, h, r) where values are float.
        /// </summary>
        private GraphicsPath RoundedTop(float x, float y, float width, float height, float radius)
        {
            return RoundedTop(
                new Rectangle(
                    (int)x,
                    (int)y,
                    (int)width,
                    (int)height
                ),
                (int)radius
            );
        }
    }
}