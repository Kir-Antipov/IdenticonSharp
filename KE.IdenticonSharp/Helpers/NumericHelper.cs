using System.Collections.Generic;

namespace IdenticonSharp.Helpers
{
    public static class NumericHelper
    {
        public static double Map(this double value, double fromMin, double fromMax, double toMin, double toMax) =>
            (value - fromMin) * (toMax - toMin) / (fromMax - fromMin + toMin);

        public static double Map(this int value, double fromMin, double fromMax, double toMin, double toMax) =>
            Map((double)value, fromMin, fromMax, toMin, toMax);

        public static double Map(this byte value, double fromMin, double fromMax, double toMin, double toMax) =>
            Map((double)value, fromMin, fromMax, toMin, toMax);

        public static string ToNormalizedString(this double value) => value.ToString().Replace(',', '.');
        public static string ToNormalizedString(this decimal value) => value.ToString().Replace(',', '.');
        public static string ToNormalizedString(this double value, string format) => value.ToString(format).Replace(',', '.');
        public static string ToNormalizedString(this decimal value, string format) => value.ToString(format).Replace(',', '.');

        public static bool GetBit(this byte value, int i) => ((value >> i) & 1) != 0;
        public static bool GetBit(this uint value, int i) => ((value >> i) & 1) != 0;
        public static bool GetBit(this ulong value, int i) => ((value >> i) & 1) != 0;
        public static bool GetBit(this ushort value, int i) => ((value >> i) & 1) != 0;
        public static bool GetBit(this int value, int i) => (((uint)value >> i) & 1) != 0;
        public static bool GetBit(this long value, int i) => (((ulong)value >> i) & 1) != 0;
        public static bool GetBit(this sbyte value, int i) => (((byte)value >> i) & 1) != 0;
        public static bool GetBit(this short value, int i) => (((ushort)value >> i) & 1) != 0;

        public static IEnumerable<bool> ToBits(this int value) => ToBits(value, sizeof(int) * 8);
        public static IEnumerable<bool> ToBits(this byte value) => ToBits(value, sizeof(byte) * 8);
        public static IEnumerable<bool> ToBits(this uint value) => ToBits(value, sizeof(uint) * 8);
        public static IEnumerable<bool> ToBits(this sbyte value) => ToBits(value, sizeof(sbyte) * 8);
        public static IEnumerable<bool> ToBits(this short value) => ToBits(value, sizeof(short) * 8);
        public static IEnumerable<bool> ToBits(this ushort value) => ToBits(value, sizeof(ushort) * 8);
        public static IEnumerable<bool> ToBits(this long value, int bitsCount)
        {
            for (int i = bitsCount - 1; i >= 0; --i)
                yield return (value & (0b1L << i)) != 0;
        }
        public static IEnumerable<bool> ToBits(this int value, int bitsCount)
        {
            uint unsigned = (uint)value;
            for (int i = bitsCount - 1; i >= 0; --i)
                yield return (unsigned & (0b1L << i)) != 0;
        }
    }
}
