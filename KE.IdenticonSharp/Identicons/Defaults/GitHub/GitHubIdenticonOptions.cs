using System;
using KE.IdenticonSharp.Compatibility;

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
            get => _spriteSize * _factor + 2 * _offset;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Size must be greater than 0", nameof(value));
                
                float offsetFactor = 23f / 256f;
                Offset = (int)Math.Round(value * offsetFactor, MidpointRounding.AwayFromZero);

                float factor = (value - 2f * Offset) / SpriteSize;
                Factor = (int)Math.Round(factor, MidpointRounding.AwayFromZero);

                int dif = value - Size;
                Offset += dif / 2;
            }
        }

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
