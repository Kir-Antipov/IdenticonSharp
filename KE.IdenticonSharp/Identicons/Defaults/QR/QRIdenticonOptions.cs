using System;
using IdenticonSharp.Compatibility;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class QRIdenticonOptions : IIdenticonOptions
    {
        public Color Foreground { get; set; } = ColorHelper.FromRgb(0, 0, 0);
        public Color Background { get; set; } = ColorHelper.FromRgb(255, 255, 255);

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
        private int _scale = 1;

        public int Border
        {
            get => _border;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Border must be greater or equal than 0", nameof(Border));
                _border = value;
            }
        }
        private int _border = 4;

        public Image CenterImage { get; set; } = null;

        public CorrectionLevel CorrectionLevel
        {
            get => _correctionLevel;
            set => _correctionLevel = value ?? throw new ArgumentNullException(nameof(CorrectionLevel));
        }
        private CorrectionLevel _correctionLevel = CorrectionLevel.Medium;
    }
}
