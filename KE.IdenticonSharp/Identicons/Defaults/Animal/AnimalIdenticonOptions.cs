using System;
using System.IO;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Identicons.Defaults.Animal
{
    public class AnimalIdenticonOptions : IIdenticonOptions
    {
        Color IIdenticonOptions.Background
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public int Size
        {
            get => _size;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Size must be greater than 0", nameof(Size));
                _size = value;
            }
        }
        private int _size = 1;

        public string CachePath
        {
            get => Path.Combine(Environment.CurrentDirectory, _cachePath);
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(CachePath));
                _cachePath = value;
            }
        }
        private string _cachePath = "animals";

        public bool SvgImageAsLink { get; set; }

        public IHashProvider HashAlgorithm
        {
            get => _hashAlgorithm;
            set => _hashAlgorithm = value ?? throw new ArgumentNullException(nameof(HashAlgorithm));
        }
        private IHashProvider _hashAlgorithm = HashProvider.SHA1;
    }
}
