using System;
using System.Linq;
using System.Collections.Generic;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class CorrectionLevel
    {
        #region Constants
        public const int MinValue = 0;
        public const int MaxValue = 3;

        public static readonly CorrectionLevel Low  = new CorrectionLevel(MinValue, nameof(Low), 0b01, 0.07f);

        public static readonly CorrectionLevel Medium = new CorrectionLevel(1, nameof(Medium), 0b00, 0.15f);

        public static readonly CorrectionLevel Quartile = new CorrectionLevel(2, nameof(Quartile), 0b11, 0.25f);

        public static readonly CorrectionLevel High = new CorrectionLevel(MaxValue, nameof(High), 0b10, 0.30f);

        private static readonly Dictionary<int, CorrectionLevel> LevelsByIndex = GetAllCorrectionLevels().ToDictionary(x => x.Id);
        private static readonly Dictionary<string, CorrectionLevel> LevelsByName = GetAllCorrectionLevels().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Var
        public int Id { get; }
        public string Name { get; }
        public int FormatBits { get; }
        public float MaxDamage { get; }
        #endregion

        #region Init
        private CorrectionLevel(int id, string name, int formatBits, float maxDamage)
        {
            Id = id;
            FormatBits = formatBits;
            MaxDamage = maxDamage;
            Name = name;
        }
        #endregion

        #region Functions
        public static CorrectionLevel operator+(CorrectionLevel level, int shift)
        {
            const int count = MaxValue + 1;
            return Get((level.Id + shift + count) % count);
        }
        public static CorrectionLevel operator -(CorrectionLevel level, int shift) => level + -shift;

        public static CorrectionLevel operator ++(CorrectionLevel level) => level + 1;
        public static CorrectionLevel operator --(CorrectionLevel level) => level + -1;

        public override string ToString() => Name;
        public override int GetHashCode() => Id;
        public override bool Equals(object obj) => obj is CorrectionLevel o && o.Id == Id;

        public static IEnumerable<CorrectionLevel> GetAllCorrectionLevels() => new[] { Low, Medium, Quartile, High };
        public static CorrectionLevel Get(string name) => LevelsByName.TryGetValue(name, out var level) ? level : throw new KeyNotFoundException();
        public static CorrectionLevel Get(int index) => LevelsByIndex.TryGetValue(index, out var level) ? level : throw new KeyNotFoundException();
        #endregion
    }
}
