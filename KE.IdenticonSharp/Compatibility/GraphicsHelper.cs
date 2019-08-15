using System;

#if NETFRAMEWORK
using System.Drawing;
#else
using SixLabors.Primitives;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Compatibility
{
    internal static class GraphicsHelper
    {
#if NETFRAMEWORK
        public static void Mutate(this Image image, Action<GraphicsWrapper> mutator)
        {
            using (Graphics g = Graphics.FromImage(image))
                mutator(new GraphicsWrapper(image, g));
        }
#else
        public static void DrawImage(this IImageProcessingContext<Rgba32> context, Image image, Rectangle rectangle)
        {
            using (Image resized = image.Clone(x => x.Resize(rectangle.Width, rectangle.Height)))
                context.DrawImage(resized, new Point(rectangle.X, rectangle.Y), 1f);
        }
#endif
    }
}
