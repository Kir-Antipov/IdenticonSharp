using System;
using System.Linq;
using System.Diagnostics;
using IdenticonSharp.Helpers;
using System.Collections.Generic;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class QRCode
    {
        #region Constants
        public const int MinVersion = 1;

        public const int MaxVersion = 40;

        private static readonly byte[][] CodewordsPerBlock = {
		              // 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40
		    new byte[] { 0,  7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // Low
		    new byte[] { 0, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28 },  // Medium
		    new byte[] { 0, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // Quartile
		    new byte[] { 0, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // High
	    };

        private static readonly byte[][] CorrectionBlocksCount = {
		              // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40
		    new byte[] { 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4,  4,  4,  4,  4,  6,  6,  6,  6,  7,  8,  8,  9,  9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25 },  // Low
		    new byte[] { 0, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5,  5,  8,  9,  9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49 },  // Medium
		    new byte[] { 0, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8,  8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68 },  // Quartile
		    new byte[] { 0, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81 },  // High
	    };
        #endregion

        #region Var
        public int Size { get; }
        public Mask Mask { get; }
        public int Version { get; }
        public CorrectionLevel CorrectionLevel { get; }

        private readonly bool[,] Modules;
        private readonly bool[,] Specials;
        #endregion

        #region Init
        public QRCode(int version, CorrectionLevel correctionLevel, byte[] data, Mask mask)
        {
            if (version < MinVersion || version > MaxVersion)
                throw new ArgumentOutOfRangeException(nameof(version));
            if (correctionLevel is null)
                throw new ArgumentNullException(nameof(correctionLevel));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Version = version;
            Size = ComputeSize(version);
            CorrectionLevel = correctionLevel;
            Modules = new bool[Size, Size];
            Specials = new bool[Size, Size];

            DrawFunctionPatterns();
            DrawCodewords(AddCorrectionBlocksAndStraighten(data));
            Mask = ApplyMask(mask);
        }
        #endregion

        #region Factories
        public static QRCode EncodeText(string text) => EncodeText(text, CorrectionLevel.Medium);
        public static QRCode EncodeText(string text, CorrectionLevel correctionLevel)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (correctionLevel is null)
                throw new ArgumentNullException(nameof(correctionLevel));

            return EncodeSegments(QRSegment.CreateSegments(text), correctionLevel);
        }

        public static QRCode EncodeBytes(byte[] data) => EncodeBytes(data, CorrectionLevel.Medium);
        public static QRCode EncodeBytes(byte[] data, CorrectionLevel correctionLevel)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (correctionLevel is null)
                throw new ArgumentNullException(nameof(correctionLevel));

            return EncodeSegments(new[] { QRSegment.CreateByteSegment(data) }, correctionLevel);
        }

        public static QRCode EncodeSegments(IEnumerable<QRSegment> segments) => 
            EncodeSegments(segments, CorrectionLevel.Medium, MinVersion, MaxVersion, -1, true);

        public static QRCode EncodeSegments(IEnumerable<QRSegment> segments, CorrectionLevel correctionLevel) => 
            EncodeSegments(segments, correctionLevel, MinVersion, MaxVersion, -1, true);

        public static QRCode EncodeSegments(IEnumerable<QRSegment> segments, CorrectionLevel correctionLevel, int minVersion, int maxVersion, Mask mask, bool boostECL)
        {
            if (segments is null)
                throw new ArgumentNullException(nameof(segments));

            if (correctionLevel is null)
                throw new ArgumentNullException(nameof(correctionLevel));

            if (!(MinVersion <= minVersion && minVersion <= maxVersion && maxVersion <= MaxVersion))
                throw new ArgumentOutOfRangeException(nameof(minVersion), nameof(maxVersion));

            List<QRSegment> segmentList = segments.ToList();

            int version;
            int usedBits;
            for (version = minVersion; ; ++version)
            {
                int capacity = ComputeCodewordsCount(version, correctionLevel) * 8; 
                usedBits = QRSegment.ComputeBitLength(segmentList, version);
                if (usedBits != -1 && usedBits <= capacity)
                    break;

                if (version >= maxVersion)
                    throw new ArgumentOutOfRangeException(nameof(segments), "Data length was too long");
            }

            if (boostECL)
                while (correctionLevel.Id != CorrectionLevel.MaxValue)
                {
                    CorrectionLevel check = correctionLevel + 1;
                    if (usedBits <= ComputeCodewordsCount(version, check) * 8)
                        correctionLevel = check;
                    else
                        break;
                }

            BitList bits = new BitList();
            foreach (QRSegment segment in segmentList)
            {
                bits.Add(segment.Mode.ModeBits, 4);
                bits.Add(segment.CharsCount, segment.Mode.GetCharsCountBitsCount(version));
                bits.Add(segment.Data);
            }

            Debug.Assert(bits.Length == usedBits);

            int dataCapacityBits = ComputeCodewordsCount(version, correctionLevel) * 8;

            Debug.Assert(bits.Length <= dataCapacityBits);

            bits.Add(0, Math.Min(4, dataCapacityBits - bits.Length));
            bits.Add(0, (8 - bits.Length % 8) % 8);

            Debug.Assert(bits.Length % 8 == 0);

            for (int padByte = 0b11101100; bits.Length < dataCapacityBits; padByte ^= 0b11111101)
                bits.Add(padByte, 8);

            byte[] dataCodewords = new byte[bits.Length / 8];
            for (int i = 0; i < bits.Length; ++i)
            {
                int index = i >> 3;
                dataCodewords[index] = (byte)(dataCodewords[index] | (bits[i] << (7 - (i & 7))));
            }

            return new QRCode(version, correctionLevel, dataCodewords, mask);
        }
        #endregion

        #region Functions
        public bool this[int x, int y] => x > -1 && x < Size && y > -1 && y < Size && Modules[y, x];

        public void Paint(Action<int, int> painter)
        {
            for (int y = 0; y < Size; ++y)
                for (int x = 0; x < Size; ++x)
                    if (Modules[y, x])
                        painter(x, y);
        }

        public void Paint(Action<int, int, bool> painter)
        {
            for (int y = 0; y < Size; ++y)
                for (int x = 0; x < Size; ++x)
                    painter(x, y, Modules[y, x]);
        }
        #endregion

        #region Draw Helpers
        private void SetSpecial(int x, int y, bool state)
        {
            Modules[y, x] = state;
            Specials[y, x] = true;
        }

        private void DrawFunctionPatterns()
        {
            DrawTimingPatterns();

            DrawFinderPatterns();

            DrawAlignmentPatterns();

            DrawFormatBits(0);

            DrawVersion();
        }

        private void DrawFormatBits(int mask)
        {
            int data = CorrectionLevel.FormatBits << 3 | mask;
            int remainder = data;
            for (int i = 0; i < 10; ++i)
                remainder = (remainder << 1) ^ ((remainder >> 9) * 0b10100110111);
            int bits = (data << 10 | remainder) ^ 0b101010000010010;

            Debug.Assert(bits >> 15 == 0);

            // top left
            for (int i = 0; i < 6; ++i)
                SetSpecial(8, i, bits.GetBit(i));
            SetSpecial(8, 7, bits.GetBit(6));
            SetSpecial(8, 8, bits.GetBit(7));
            SetSpecial(7, 8, bits.GetBit(8));
            for (int i = 9; i < 15; ++i)
                SetSpecial(14 - i, 8, bits.GetBit(i));

            // top right and bottom left
            for (int i = 0; i < 8; ++i)
                SetSpecial(Size - 1 - i, 8, bits.GetBit(i));
            for (int i = 8; i < 15; ++i)
                SetSpecial(8, Size - 15 + i, bits.GetBit(i));
            SetSpecial(8, Size - 8, true);
        }

        private void DrawVersion()
        {
            if (Version < 7)
                return;

            int remainder = Version;
            for (int i = 0; i < 12; ++i)
                remainder = (remainder << 1) ^ ((remainder >> 11) * 0b1111100100101);
            int bits = Version << 12 | remainder;

            Debug.Assert(bits >> 18 == 0);

            for (int i = 0; i < 18; ++i)
            {
                bool bit = bits.GetBit(i);
                int a = Size - 11 + i % 3;
                int b = i / 3;
                SetSpecial(a, b, bit);
                SetSpecial(b, a, bit);
            }
        }

        private void DrawFinderPattern(int x, int y)
        {
            for (int dy = -4; dy <= 4; ++dy)
                for (int dx = -4; dx <= 4; ++dx)
                {
                    int distance = Math.Max(Math.Abs(dx), Math.Abs(dy));  // Tchebychev distance
                    int currentX = x + dx;
                    int currentY = y + dy;
                    // Check white border outside the code
                    if (currentX > -1 && currentX < Size && currentY > -1 && currentY < Size)
                        SetSpecial(currentX, currentY, distance != 2 && distance != 4);
                }
        }

        private void DrawAlignmentPattern(int x, int y)
        {
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                    SetSpecial(x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
        }

        private void DrawTimingPatterns()
        {
            for (int i = 0; i < Size; ++i)
            {
                SetSpecial(6, i, i % 2 == 0);
                SetSpecial(i, 6, i % 2 == 0);
            }
        }

        private void DrawFinderPatterns()
        {
            DrawFinderPattern(3, 3);
            DrawFinderPattern(Size - 4, 3);
            DrawFinderPattern(3, Size - 4);
        }

        private void DrawAlignmentPatterns()
        {
            int[] alignPatPos = ComputeAlignmentPatternPositions(Version);
            int numAlign = alignPatPos.Length;
            for (int i = 0; i < numAlign; i++)
                for (int j = 0; j < numAlign; j++)
                    // Check finders' positions
                    if (!(i == 0 && j == 0 || i == 0 && j == numAlign - 1 || i == numAlign - 1 && j == 0))
                        DrawAlignmentPattern(alignPatPos[i], alignPatPos[j]);
        }

        private void DrawCodewords(byte[] data)
        {
            Debug.Assert(data != null);
            Debug.Assert(data.Length == ComputeRawDataModulesCount(Version) / 8);

            for (int i = 0, anchorX = Size - 1; anchorX > 0; anchorX -= 2)
            {
                if (anchorX == 6)
                    --anchorX;
                for (int anchorY = 0; anchorY < Size; ++anchorY)
                    for (int j = 0; j < 2; ++j)
                    {
                        int x = anchorX - j;
                        bool upward = ((anchorX + 1) & 2) == 0;
                        int y = upward ? Size - 1 - anchorY : anchorY;
                        if (!Specials[y, x] && i < data.Length * 8)
                        {
                            Modules[y, x] = data[i >> 3].GetBit(7 - (i & 7));
                            ++i;
                        }
                    }
            }
        }

        private void DrawMask(Mask mask)
        {
            Debug.Assert(mask != null);

            for (int y = 0; y < Size; ++y)
                for (int x = 0; x < Size; x++)
                    Modules[y, x] ^= mask.Apply(x, y) & !Specials[y, x];
        }
        #endregion

        #region Mask Helpers
        private int ApplyMask(Mask selectedMask)
        {
            if (selectedMask is null)
            {
                int minPenalty = int.MaxValue;
                for (Mask i = Mask.MinValue; ; ++i)
                {
                    DrawMask(i);
                    DrawFormatBits(i);
                    int penalty = ComputeCurrentMaskPenaltyScore();
                    if (penalty < minPenalty)
                    {
                        selectedMask = i;
                        minPenalty = penalty;
                    }
                    DrawMask(i);

                    if (i == Mask.MaxValue)
                        break;
                }
            }

            DrawMask(selectedMask);
            DrawFormatBits(selectedMask);

            return selectedMask;
        }

        private int ComputeCurrentMaskPenaltyScore()
        {
            const int penalty1Rate = 3;
            const int penalty2Rate = 3;
            const int penalty3Rate = 40;
            const int penalty4Rate = 10;

            int result = 0;
            int[] runHistory = new int[7];

            for (int y = 0; y < Size; ++y)
            {
                bool runColor = false;
                int runX = 0;
                Array.Clear(runHistory, 0, runHistory.Length);
                int padRun = Size;
                for (int x = 0; x < Size; ++x)
                {
                    if (Modules[y, x] == runColor)
                    {
                        ++runX;
                        if (runX == 5)
                            result += penalty1Rate;
                        else if (runX > 5)
                            ++result;
                    }
                    else
                    {
                        addFinderHistory(runX + padRun);
                        padRun = 0;
                        if (!runColor)
                            result += countFinderPatterns() * penalty3Rate;
                        runColor = Modules[y, x];
                        runX = 1;
                    }
                }
                result += terminateAndCountFinder(runColor, runX + padRun) * penalty3Rate;
            }

            for (int x = 0; x < Size; ++x)
            {
                bool runColor = false;
                int runY = 0;
                Array.Clear(runHistory, 0, runHistory.Length);
                int padRun = Size;
                for (int y = 0; y < Size; ++y)
                {
                    if (Modules[y, x] == runColor)
                    {
                        ++runY;
                        if (runY == 5)
                            result += penalty1Rate;
                        else if (runY > 5)
                            ++result;
                    }
                    else
                    {
                        addFinderHistory(runY + padRun);
                        padRun = 0;
                        if (!runColor)
                            result += countFinderPatterns() * penalty3Rate;
                        runColor = Modules[y, x];
                        runY = 1;
                    }
                }
                result += terminateAndCountFinder(runColor, runY + padRun) * penalty3Rate;
            }

            for (int y = 0; y < Size - 1; ++y)
                for (int x = 0; x < Size - 1; ++x)
                {
                    bool color = Modules[y, x];
                    if (color == Modules[y, x + 1] &&
                          color == Modules[y + 1, x] &&
                          color == Modules[y + 1, x + 1])
                        result += penalty2Rate;
                }

            int blackCount = Modules.Cast<bool>().Sum(x => x ? 1 : 0);
            int square = Size * Size;
            // 0.45 - 0.05k <= blackCount / square <= 0.55 + 0.05k, k = N
            int k = (Math.Abs(blackCount * 20 - square * 10) + square - 1) / square - 1;

            return result + k * penalty4Rate;

            int countFinderPatterns()
            {
                int n = runHistory[1];

                Debug.Assert(n <= Size * 3);

                bool core = n > 0 && runHistory[2] == n && runHistory[3] == n * 3 && runHistory[4] == n && runHistory[5] == n;
                return (core && runHistory[0] >= n * 4 && runHistory[6] >= n ? 1 : 0)
                     + (core && runHistory[6] >= n * 4 && runHistory[0] >= n ? 1 : 0);
            }

            int terminateAndCountFinder(bool currentRunColor, int currentRunLength)
            {
                if (currentRunColor)
                { 
                    addFinderHistory(currentRunLength);
                    currentRunLength = 0;
                }
                currentRunLength += Size;
                addFinderHistory(currentRunLength);
                return countFinderPatterns();
            }

            void addFinderHistory(int currentRunLength)
            {
                Array.Copy(runHistory, 0, runHistory, 1, runHistory.Length - 1);
                runHistory[0] = currentRunLength;
            }
        }
        #endregion

        #region Compute Helpers
        private static int ComputeSize(int version) => 17 + version * 4;

        private static int[] ComputeAlignmentPatternPositions(int version)
        {
            Debug.Assert(version >= MinVersion && version <= MaxVersion);

            if (version == 1)
                return new int[0];
            else
            {
                int numAlign = version / 7 + 2;
                int step = version == 32 ? 26 : (version * 4 + numAlign * 2 + 1) / (numAlign * 2 - 2) * 2;
                int[] result = new int[numAlign];
                result[0] = 6;
                for (int i = result.Length - 1, pos = ComputeSize(version) - 7; i > 0; --i, pos -= step)
                    result[i] = pos;
                return result;
            }
        }

        private static int ComputeRawDataModulesCount(int version)
        {
            Debug.Assert(version >= MinVersion && version <= MaxVersion);

            int size = ComputeSize(version);
            int result = size * size;   // full
            result -= 8 * 8 * 3;        // finders
            result -= 15 * 2 + 1;       // format
            result -= (size - 16) * 2;  // timing

            if (version > 1) // alignment
            {
                int numAlign = version / 7 + 2;
                result -= (numAlign - 1) * (numAlign - 1) * 25;
                result -= (numAlign - 2) * 2 * 20;  // overlap timing

                if (version > 6)
                    result -= 6 * 3 * 2;  // version
            }
            return result;
        }

        private static int ComputeCodewordsCount(int version, CorrectionLevel correctionLevel)
        {
            Debug.Assert(version >= MinVersion && version <= MaxVersion);
            Debug.Assert(correctionLevel != null);

            return ComputeRawDataModulesCount(version) / 8 - CodewordsPerBlock[correctionLevel.Id][version] * CorrectionBlocksCount[correctionLevel.Id][version];
        }
        #endregion

        #region Correction Helpers
        private byte[] AddCorrectionBlocksAndStraighten(byte[] data)
        {
            Debug.Assert(data != null);
            Debug.Assert(data.Length == ComputeCodewordsCount(Version, CorrectionLevel));

            int blocksCount = CorrectionBlocksCount[CorrectionLevel.Id][Version];
            int correctionBlockLength = CodewordsPerBlock[CorrectionLevel.Id][Version];
            int rawCodewords = ComputeRawDataModulesCount(Version) / 8;
            int shortBlocksCount = blocksCount - rawCodewords % blocksCount;
            int shortBlockLength = rawCodewords / blocksCount;

            byte[][] blocks = new byte[blocksCount][];
            byte[] divisor = ReedSolomonComputeDivisor(correctionBlockLength);
            for (int i = 0, k = 0; i < blocksCount; ++i)
            {
                byte[] currentData = new byte[shortBlockLength - correctionBlockLength + (i < shortBlocksCount ? 0 : 1)];
                Array.Copy(data, k, currentData, 0, currentData.Length);
                k += currentData.Length;
                byte[] block = new byte[shortBlockLength + 1];
                Array.Copy(currentData, block, Math.Min(currentData.Length, block.Length));
                byte[] ecc = ReedSolomonComputeRemainder(currentData, divisor);
                Array.Copy(ecc, 0, block, block.Length - correctionBlockLength, ecc.Length);
                blocks[i] = block;
            }

            byte[] result = new byte[rawCodewords];
            for (int i = 0, k = 0; i < blocks[0].Length; ++i)
                for (int j = 0; j < blocks.Length; ++j)
                    if (i != shortBlockLength - correctionBlockLength || j >= shortBlocksCount)
                    {
                        result[k] = blocks[j][i];
                        ++k;
                    }

            return result;
        }

        private static byte[] ReedSolomonComputeDivisor(int degree)
        {
            Debug.Assert(degree > 0 && degree < 256);

            int root = 1;
            byte[] result = new byte[degree];
            result[degree - 1] = 1;
            for (int i = 0; i < degree; ++i)
            {
                for (int j = 0; j < result.Length; ++j)
                {
                    result[j] = (byte)ReedSolomonMultiply(result[j] & 0xFF, root);
                    if (j + 1 < result.Length)
                        result[j] ^= result[j + 1];
                }
                root = ReedSolomonMultiply(root, 0x02);
            }
            return result;
        }

        private static byte[] ReedSolomonComputeRemainder(byte[] data, byte[] divisor)
        {
            Debug.Assert(data != null && divisor != null);

            byte[] result = new byte[divisor.Length];
            foreach (byte b in data)
            {
                int factor = (b ^ result[0]) & 0xFF;
                Array.Copy(result, 1, result, 0, result.Length - 1);
                result[result.Length - 1] = 0;
                for (int i = 0; i < result.Length; ++i)
                    result[i] = (byte)(result[i] ^ ReedSolomonMultiply(divisor[i] & 0xFF, factor));
            }
            return result;
        }

        private static int ReedSolomonMultiply(int x, int y)
        {
            Debug.Assert(x >> 8 == 0 && y >> 8 == 0);

            int z = 0;
            for (int i = 7; i >= 0; --i)
            {
                z = (z << 1) ^ ((z >> 7) * 0x11D);
                z ^= ((y >> i) & 1) * x;
            }

            Debug.Assert(z >> 8 == 0);

            return z;
        }
        #endregion
}}
