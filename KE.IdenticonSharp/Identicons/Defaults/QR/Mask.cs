using System;
using System.Linq;
using System.Collections.Generic;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class Mask
    {
        #region Constants
        public const int MinValue = 0;
        public const int MaxValue = 7;

        public static readonly Mask Zero = new Mask(MinValue, (x, y) => (x + y) % 2 == 0);
        public static readonly Mask One = new Mask(1, (x, y) => y % 2 == 0);
        public static readonly Mask Two = new Mask(2, (x, y) => x % 3 == 0);
        public static readonly Mask Three = new Mask(3, (x, y) => (x + y) % 3 == 0);
        public static readonly Mask Four = new Mask(4, (x, y) => (x / 3 + y / 2) % 2 == 0);
        public static readonly Mask Five = new Mask(5, (x, y) => x * y % 2 + x * y % 3 == 0);
        public static readonly Mask Six = new Mask(6, (x, y) => (x * y % 2 + x * y % 3) % 2 == 0);
        public static readonly Mask Seven = new Mask(MaxValue, (x, y) => ((x + y) % 2 + x * y % 3) % 2 == 0);

        private static readonly Dictionary<int, Mask> Masks = GetAllMasks().ToDictionary(x => x.Id);
        #endregion

        #region Var
        public int Id { get; }
        private readonly Func<int, int, bool> _mask;
        #endregion

        #region Init
        private Mask(int id, Func<int, int, bool> mask)
        {
            Id = id;
            _mask = mask;
        }
        #endregion

        #region Functions
        public bool Apply(int x, int y) => _mask(x, y);

        public static Mask operator +(Mask mask, int shift)
        {
            const int count = MaxValue + 1;
            return Get((mask.Id + shift + count) % count);
        }
        public static Mask operator -(Mask mask, int shift) => mask + -shift;

        public static Mask operator ++(Mask mask) => mask + 1;
        public static Mask operator --(Mask mask) => mask + -1;

        public static implicit operator int(Mask mask) => mask?.Id ?? -1;
        public static implicit operator Mask(int id) => id >= MinValue && id <= MaxValue ? Get(id) : null;

        public override string ToString() => $"{Id}";
        public override int GetHashCode() => Id;
        public override bool Equals(object obj) => obj is Mask o && o.Id == Id;

        public static IEnumerable<Mask> GetAllMasks() => new[] { Zero, One, Two, Three, Four, Five, Six, Seven };
        public static Mask Get(int index) => Masks.TryGetValue(index, out var mask) ? mask : throw new KeyNotFoundException();
        #endregion
    }
}
