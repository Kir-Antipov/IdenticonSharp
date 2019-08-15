using System;
using System.Linq;
using System.Collections.Generic;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class Mode
    {
        #region Constants
        public static readonly Mode Numeric = new Mode(nameof(Numeric), 0b0001, 10, 12, 14);
        public static readonly Mode AlphaNumeric = new Mode(nameof(AlphaNumeric), 0b0010, 9, 11, 13);
        public static readonly Mode Byte = new Mode(nameof(Byte), 0b0100, 8, 16, 16);
        public static readonly Mode Kanji = new Mode(nameof(Kanji), 0b1000, 8, 10, 12);
        public static readonly Mode ECI = new Mode(nameof(ECI), 0b0111, 0, 0, 0);
        #endregion

        #region Var
        public string Name { get; }
        public int ModeBits { get; }

        public int[] BitsCount => _bitsCount.ToArray();
        private readonly int[] _bitsCount;
        #endregion

        #region Init
        private Mode(string name, int mode, params int[] bitsCount)
        {
            Name = name;
            ModeBits = mode;
            _bitsCount = bitsCount;
        }
        #endregion

        #region Functions
        public int GetCharsCountBitsCount(int version)
        {
            if (version < QRCode.MinVersion || version > QRCode.MaxVersion)
                throw new ArgumentOutOfRangeException(nameof(version));

            return BitsCount[(version + 7) / 17];
        }

        public override string ToString() => Name;

        public static IEnumerable<Mode> GetAllModes() => new[] { Numeric, AlphaNumeric, Byte, Kanji, ECI };
        #endregion
    }
}
