#if NETFRAMEWORK
using System;
using System.Drawing;

namespace IdenticonSharp.Compatibility
{
    internal class GraphicsWrapper
    {
        public readonly Image ParentImage;
        public readonly Graphics Graphics;

        public GraphicsWrapper(Image parentImage, Graphics graphics)
        {
            ParentImage = parentImage;
            Graphics = graphics;
        }

        public void Fill(Color color)
        {
            using (Brush brush = new SolidBrush(color))
                Graphics.FillRectangle(brush, 0, 0, ParentImage.Width, ParentImage.Height);
        }

        public void Fill(Color color, RectangleF rectangle)
        {
            using (Brush brush = new SolidBrush(color))
                Graphics.FillRectangle(brush, rectangle);
        }

        public void DrawImage(Image image, Rectangle rectangle) => Graphics.DrawImage(image, rectangle);
    }
}
#endif