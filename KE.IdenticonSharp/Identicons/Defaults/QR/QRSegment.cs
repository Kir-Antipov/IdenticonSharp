using System;
using System.Linq;
using System.Text;
using IdenticonSharp.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class QRSegment
    {
        #region Constants
        private static readonly Regex NumericRegex = new Regex("[0-9]+", RegexOptions.Compiled);
        private static readonly Regex AlphaNumericRegex = new Regex("[0-9A-Z $%*+./:-]+", RegexOptions.Compiled);
        #endregion

        #region Var
        public Mode Mode { get; }

        public int CharsCount { get; }

        public BitList Data => _data.Clone();
        private readonly BitList _data;
        #endregion

        #region Init
        public QRSegment(Mode mode, int charsCount, BitList data)
        {
            Mode = mode ?? throw new ArgumentNullException(nameof(mode));
            CharsCount = charsCount >= 0 ? charsCount : throw new ArgumentOutOfRangeException(nameof(charsCount));
            _data = (data ?? throw new ArgumentNullException(nameof(data))).Clone();
        }
        #endregion

        #region Factories
        public static QRSegment CreateByteSegment(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            BitList bits = new BitList();
            bits.AddRange(data);
            return new QRSegment(Mode.Byte, data.Length, bits);
        }

        public static QRSegment CreateNumericSegment(string digits)
        {
            if (digits is null)
                throw new ArgumentNullException(nameof(digits));

            var numbers = digits
                .Select(x => x >= '0' && x <= '9' ? x - '0' : throw new ArgumentException("String contains non-numeric characters", nameof(digits)))
                .Bucket(3)
                .Select(x => {
                    int[] currentDigits = x.ToArray();
                    return new { Count = currentDigits.Length, Number = x.Aggregate(0, (a, b) => a * 10 + b) };
                });

            BitList bits = new BitList();
            foreach (var num in numbers)
                bits.Add(num.Number, num.Count * 3 + 1);

            return new QRSegment(Mode.Numeric, digits.Length, bits);
        }

        public static QRSegment CreateAlphaNumericSegment(string text)
        {
            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var numbers = text
                .Select(x => {
                    int i = charset.IndexOf(x);
                    return i != -1 ? i : throw new ArgumentException("String contains unencodable characters in alphanumeric mode", nameof(text));
                })
                .Bucket(2)
                .Select(x => {
                    int[] currentDigits = x.ToArray();
                    return new { Count = currentDigits.Length, Number = x.Aggregate(0, (a, b) => a * charset.Length + b) };
                });

            BitList bits = new BitList();
            foreach (var num in numbers)
                bits.Add(num.Number, num.Count == 2 ? 11 : 6);

            return new QRSegment(Mode.AlphaNumeric, text.Length, bits);
        }

        public static QRSegment CreateECISegment(int assignValue)
        {
            if (assignValue < 0)
                throw new ArgumentOutOfRangeException(nameof(assignValue));

            BitList bits = new BitList();
            if (assignValue < 1 << 7)
                bits.Add(assignValue, 8);
            else if (assignValue < 1 << 14)
            {
                bits.Add(2, 2);
                bits.Add(assignValue, 14);
            }
            else if (assignValue < 1_000_000)
            {
                bits.Add(6, 3);
                bits.Add(assignValue, 21);
            }
            else
                throw new ArgumentOutOfRangeException(nameof(assignValue));

            return new QRSegment(Mode.ECI, 0, bits);
        }

        public static IEnumerable<QRSegment> CreateSegments(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            char[] chars = text.ToCharArray();

            var numerics = NumericRegex.Matches(text).Cast<Match>()
                .Select(match => new {
                    Match = match,
                    Computer = (Func<int, int>)ComputeNumericBitLength,
                    Creator = (Func<string, QRSegment>)CreateNumericSegment,
                    BitLength = ComputeNumericBitLength(match.Length),
                    Type = 0
                });
            var alphaNumerics = AlphaNumericRegex.Matches(text).Cast<Match>()
                .Where(x => chars.Skip(x.Index).Take(x.Length).Count(c => c < '0' || c > '9') > x.Length / 2)
                .Select(match => new {
                    Match = match,
                    Computer = (Func<int, int>)ComputeAlphaNumericBitLength,
                    Creator = (Func<string, QRSegment>)CreateAlphaNumericSegment,
                    BitLength = ComputeAlphaNumericBitLength(match.Length),
                    Type = 1 });

            var suitable = numerics.Concat(alphaNumerics)
                .Where(x => Encoding.UTF8.GetByteCount(chars, x.Match.Index, x.Match.Length) * 8 > x.BitLength)
                .OrderBy(x => x.Match.Index)
                .ThenBy(x => x.Type)
                .GetEnumerator();
            suitable.MoveNext();

            for (int i = 0; i < text.Length; suitable.MoveNext())
            {
                var current = suitable.Current;
                if (current == null)
                {
                    yield return CreateByteSegment(Encoding.UTF8.GetBytes(chars, i, text.Length - i));
                    i = text.Length;
                }
                else
                {
                    if (i <= current.Match.Index)
                    {
                        if (i < current.Match.Index)
                            yield return CreateByteSegment(Encoding.UTF8.GetBytes(chars, i, current.Match.Index - i));
                        yield return current.Creator(text.Substring(current.Match.Index, current.Match.Length));
                        i = current.Match.Index + current.Match.Length;
                    }
                    else if (i < current.Match.Index + current.Match.Length)
                    {
                        int count = current.Match.Index + current.Match.Length - i;
                        if (current.Computer(count) < Encoding.UTF8.GetByteCount(chars, i, count))
                        {
                            yield return current.Creator(text.Substring(i, count));
                            i = current.Match.Index + current.Match.Length;
                        }
                    }
                }
            }
        }
        #endregion

        #region Functions
        private static int ComputeNumericBitLength(int charsCount)
        {
            int bitLength = 4 + 14; // type + max charsCount
            bitLength += charsCount / 3 * 10; // 10 bits per 3 chars

            int remainder = charsCount % 3;
            bitLength += remainder == 2 ? 7 : remainder == 1 ? 4 : 0; // 7 bits per 2, 4 bits for 1

            return bitLength;
        }

        private static int ComputeAlphaNumericBitLength(int charsCount)
        {
            int bitLength = 4 + 13; // type + max charsCount
            bitLength += charsCount / 2 * 11; // 11 bits per 2 chars
            bitLength += charsCount == 0 ? 0 : 6; // 6 bits per 1

            return bitLength;
        }

        public static int ComputeBitLength(IEnumerable<QRSegment> segments, int version)
        {
            if (segments is null)
                throw new ArgumentNullException(nameof(segments));

            if (version < QRCode.MinVersion || version > QRCode.MaxVersion)
                throw new ArgumentOutOfRangeException(nameof(version));

            long result = segments.Sum(segment => {
                int charsCountBits = segment.Mode.GetCharsCountBitsCount(version);
                if (segment.CharsCount >= 1 << charsCountBits)
                    return int.MaxValue;
                return 4L + charsCountBits + segment.Data.Length;
            });

            return result > int.MaxValue ? -1 : (int)result;
        }
        #endregion
    }
}
