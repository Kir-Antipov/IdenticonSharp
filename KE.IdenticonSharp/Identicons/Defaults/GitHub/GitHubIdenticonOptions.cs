using System;
#if NETFRAMEWORK
using System.Drawing;
#else
using SixLabors.Primitives;
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Identicons.Defaults.GitHub
{
    public class GitHubIdenticonOptions : IIdenticonOptions
    {
#if NETFRAMEWORK
        public Color Background { get; set; } = Color.FromArgb(240, 240, 240);
#else
        public Color Background { get; set; } = new Color(240, 240, 240);
#endif

        public Size Size
        {
            get => new Size(_spriteSize.Width * _factor + 2 * _offset, _spriteSize.Height * _factor + 2 * _offset);
            set
            {
                if (value.Width < 1 || value.Height < 1)
                    throw new ArgumentException("Width and height must be greater than 0", nameof(value));
                
                float offsetFactor = 23f / 256f;
                Offset = (int)Math.Round((value.Width + value.Height) / 2f * offsetFactor, MidpointRounding.AwayFromZero);

                float factorX = (value.Width - 2f * Offset) / SpriteSize.Width;
                float factorY = (value.Height - 2f * Offset) / SpriteSize.Height;
                Factor = (int)Math.Round((factorX + factorY) / 2f, MidpointRounding.AwayFromZero);

                int dif = value.Width - Size.Width;
                Offset += dif / 2;
            }
        }

        public Size SpriteSize
        {
            get => _spriteSize;
            set
            {
                if (value.Width < 1 || value.Height < 1)
                    throw new ArgumentException("Width and height must be greater than 0", nameof(value));
                _spriteSize = value;
            }
        }
        private Size _spriteSize = new Size(5, 5);

        public int Factor
        {
            get => _factor;
            set
            {
                if (_factor < 1)
                    throw new ArgumentException("Factor must be greater than 0", nameof(value));
                _factor = value;
            }
        }
        private int _factor = 42;

        public int Offset
        {
            get => _offset;
            set
            {
                if (_offset < 1)
                    throw new ArgumentException("Factor must be greater than 0", nameof(value));
                _offset = value;
            }
        }
        private int _offset = 23;

        public IHashProvider HashAlgorithm
        {
            get => _hashAlgorithm;
            set => _hashAlgorithm = value ?? throw new ArgumentNullException(nameof(value));
        }
        private IHashProvider _hashAlgorithm = HashProvider.MD5;
    }
}
