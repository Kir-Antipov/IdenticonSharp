﻿using System;
using System.IO;

#if NETFRAMEWORK
using System.Drawing;
using System.Drawing.Imaging;
#else
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Helpers
{
    public static class DrawingHelper
    {
#if NETFRAMEWORK
        public static void Save(this Image image, Stream stream)
        {
            using (Bitmap bit = new Bitmap(image))
                bit.Save(stream, ImageFormat.Png);
        }

        public static Bitmap Resize(this Image image, int width, int height)
        {
            using (image)
                return new Bitmap(image, new Size(width, height));
        }
#else
        public static void Save(this Image image, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
                image.Save(stream);
        }

        public static void Save(this Image image, Stream stream) => image.Save(stream, new PngEncoder());

        public static Image Resize(this Image image, int width, int height)
        {
            image.Mutate(context => context.Resize(width, height));
            return image;
        }
#endif

        public static byte[] GetBytes(this Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream);
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        public static string ToBase64(this Image image) => Convert.ToBase64String(GetBytes(image));
        public static string ToBase64Link(this Image image) => $"data:image/png;base64,{Convert.ToBase64String(GetBytes(image))}";
    }
}
