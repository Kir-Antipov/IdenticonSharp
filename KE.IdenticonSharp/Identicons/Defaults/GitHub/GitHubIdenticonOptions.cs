using System;
using IdenticonSharp.Compatibility;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Identicons.Defaults.GitHub
{
    public class GitHubIdenticonOptions : IIdenticonOptions
    {
        public Color Background { get; set; } = ColorHelper.FromRgb(240, 240, 240);

        public int Size
        {
            get => _requestedSize;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Size must be greater than 0", nameof(value));
                
                float offsetFactor = 23f / 256f;
                Border = (int)Math.Round(value * offsetFactor, MidpointRounding.AwayFromZero);

                float factor = (value - 2f * Border) / SpriteSize;
                Scale = (int)Math.Round(factor, MidpointRounding.AwayFromZero);

                int dif = value - RealSize;
                Border += dif / 2;

                _requestedSize = value;
            }
        }
        private int _requestedSize = 5 * 42 + 2 * 23;

        public int RealSize => _spriteSize * _scale + 2 * _border;

        public int SpriteSize
        {
            get => _spriteSize;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Size must be greater than 0", nameof(value));
                _spriteSize = value;
            }
        }
        private int _spriteSize = 5;

        public int Scale
        {
            get => _scale;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Scale must be greater than 0", nameof(Scale));
                _scale = value;
            }
        }
        private int _scale = 42;

        public int Border
        {
            get => _border;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Border must be greater than 0", nameof(Border));
                _border = value;
            }
        }
        private int _border = 23;

        public IHashProvider HashAlgorithm
        {
            get => _hashAlgorithm;
            set => _hashAlgorithm = value ?? throw new ArgumentNullException(nameof(HashAlgorithm));
        }
        private IHashProvider _hashAlgorithm = HashProvider.MD5;
    }
}
