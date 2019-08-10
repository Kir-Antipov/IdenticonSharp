using System;
using System.Security.Cryptography;

namespace IdenticonSharp.Identicons
{
    public abstract class HashProvider : IHashProvider
    {
        #region Defaults
        public static HashProvider MD5 { get; } = Create<MD5CryptoServiceProvider>();
        public static HashProvider SHA1 { get; } = Create<SHA1CryptoServiceProvider>();
        public static HashProvider SHA256 { get; } = Create<SHA256CryptoServiceProvider>();
        public static HashProvider SHA384 { get; } = Create<SHA384CryptoServiceProvider>();
        public static HashProvider SHA512 { get; } = Create<SHA512CryptoServiceProvider>();
        #endregion

        #region Functions
        public abstract byte[] ComputeHash(byte[] input);

        public static HashProvider Create(Func<byte[], byte[]> hashAlgorithm)
        {
            if (hashAlgorithm is null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            return new HashWrapper(hashAlgorithm);
        }
        public static HashProvider Create(HashAlgorithm hashAlgorithm)
        {
            if (hashAlgorithm is null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            return new HashWrapper(hashAlgorithm.ComputeHash);
        }

        public static HashProvider Create<T>() where T : HashAlgorithm, new() => Create(new T());
        #endregion

        #region Wrapper
        private class HashWrapper : HashProvider
        {
            private readonly Func<byte[], byte[]> ComputeHashImpl;

            public HashWrapper(Func<byte[], byte[]> hashAlgorithm) => ComputeHashImpl = hashAlgorithm;

            public override byte[] ComputeHash(byte[] input) => ComputeHashImpl(input);
        }
        #endregion
    }
}
