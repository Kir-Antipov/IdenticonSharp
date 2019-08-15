using System;
using System.Linq;
using System.Collections;
using IdenticonSharp.Helpers;
using System.Collections.Generic;

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class BitList : IEnumerable<int>, ICloneable
    {
        #region Var
        public int Length => Data.Count;
        private readonly List<bool> Data = new List<bool>();
        #endregion

        #region Functions
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= Data.Count)
                    throw new IndexOutOfRangeException();

                return Data[index] ? 1 : 0;
            }
        }

        public void Add(int value) => Add(value, sizeof(int));
        public void Add(int value, int length) => AddRange(value.ToBits(length));
        public void Add(BitList list) => Data.AddRange(list.Data);

        public void AddRange(IEnumerable<bool> bits) => Data.AddRange(bits);
        public void AddRange(IEnumerable<byte> bits) => Data.AddRange(bits.SelectMany(x => x.ToBits()));

        public BitList Clone()
        {
            BitList cloned = new BitList();
            cloned.AddRange(Data);
            return cloned;
        }
        object ICloneable.Clone() => Clone();

        public IEnumerator<int> GetEnumerator()
        {
            foreach (bool bit in Data)
                yield return bit ? 1 : 0;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
