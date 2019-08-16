using System;
using System.IO;
using System.Linq;
using IdenticonSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System.Collections.Generic;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using IdenticonSharp.Identicons.Defaults.Animal;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace Preview
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string[] values = { "Microsoft", "GitHub", "Visual Studio", "IdenticonSharp", "Roslyn", "C#", "F#", "VB" };

            var provider = IdenticonManager.Create<AnimalIdenticonOptions>(options => {
                options.Size = 129;
            });

            var images = values.Select(x => Sign(provider.Create(x), x));
            var union = Unite(images);
            union.Save(args.FirstOrDefault() ?? GetDefaultFilename());
        }

        private static string GetDefaultFilename() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "union.png");

        private static Image Sign(Image image, string text, Font font) => Sign(image, text, font, Rgba32.White, new Rgba32(120, 120, 120));
        private static Image Sign(Image image, string text) => Sign(image, text, SystemFonts.CreateFont("Segoe UI", (int)(image.Height * 0.11f)), Rgba32.White, new Rgba32(120, 120, 120));
        private static Image Sign(Image image, string text, Rgba32 forecolor, Rgba32 backcolor) => Sign(image, text, SystemFonts.CreateFont("Segoe UI", (int)(image.Height * 0.11f)), backcolor, forecolor);

        private static Image Sign(Image image, string text, Font font, Rgba32 backcolor, Rgba32 forecolor)
        {
            const float offsetFactor = 0.15f;
            const float factor = 1f + 2f * offsetFactor;
            const int divider = 6;

            SizeF size = TextMeasurer.Measure(text, new RendererOptions(font));
            size.Height = (int)size.Height + divider - (int)size.Height % divider;

            SizeF indent = new SizeF(size.Width * factor, size.Height * factor);
            Size resultImageSize = Size.Ceiling(new SizeF(Math.Max(image.Width, indent.Width), image.Height + indent.Height));

            Image result = new Image(resultImageSize.Width, resultImageSize.Height);
            result.Mutate(context => {
                context.Fill(backcolor);
                context.DrawImage(image, 1f);
                context.DrawText(text, font, forecolor, Point.Round(new PointF((result.Width - size.Width) / 2f, image.Height + indent.Height - size.Height)));
            });
            return result;
        }

        private static Image Unite(Image[] images) => Unite(images, Rgba32.White);
        private static Image Unite(IEnumerable<Image> images) => Unite(images.ToArray(), Rgba32.White);
        private static Image Unite(IEnumerable<Image> images, Rgba32 backcolor) => Unite(images.ToArray(), backcolor);

        private static Image Unite(Image[] images, Rgba32 backcolor)
        {
            const float offsetFactor = 0.1f;

            int rows = (int)Math.Ceiling(Math.Sqrt(images.Length));
            while (images.Length % rows != 0)
                --rows;
            int columns = images.Length / rows;
            if (rows > columns)
                (rows, columns) = (columns, rows);

            int maxWidth = images.Max(x => x.Width);
            int maxHeight = images.Max(x => x.Height);
            int offsetX = (int)(maxWidth * offsetFactor);
            int offsetY = (int)(maxHeight * offsetFactor);

            int resultWidth = maxWidth * columns + offsetX * (columns + 1);
            int resultHeight = maxHeight * rows + offsetY * (rows + 1);

            Image result = new Image(resultWidth, resultHeight);
            result.Mutate(context =>
            {
                context.Fill(backcolor);
                for (int y = 0, i = 0; y < rows; ++y)
                    for (int x = 0; x < columns && i < images.Length; ++x, ++i)
                    {
                        int posX = (x + 1) * offsetX + maxWidth * x + (maxWidth - images[i].Width) / 2;
                        int posY = (y + 1) * offsetY + maxHeight * y + (maxHeight - images[i].Height) / 2;
                        context.DrawImage(images[i], new Point(posX, posY), 1f);
                    }
            });

            return result;
        }
    }
}
