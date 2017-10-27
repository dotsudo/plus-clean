namespace Plus.Communication.Encryption.Crypto.Prng
{
    using System.Collections.Generic;

    public sealed class Arc4
    {
        private const int Poolsize = 256;
        private readonly byte[] _bytes;
        private int _i;
        private int _j;

        internal Arc4(byte[] key)
        {
            _bytes = new byte[Poolsize];
            Initialize(key);
        }

        private void Initialize(IReadOnlyList<byte> key)
        {
            _i = 0;
            _j = 0;

            for (_i = 0; _i < Poolsize; ++_i)
            {
                _bytes[_i] = (byte) _i;
            }

            for (_i = 0; _i < Poolsize; ++_i)
            {
                _j = (_j + _bytes[_i] + key[_i % key.Count]) & (Poolsize - 1);
                Swap(_i, _j);
            }

            _i = 0;
            _j = 0;
        }

        private void Swap(int a, int b)
        {
            var t = _bytes[a];
            _bytes[a] = _bytes[b];
            _bytes[b] = t;
        }

        private byte Next()
        {
            _i = ++_i & (Poolsize - 1);
            _j = (_j + _bytes[_i]) & (Poolsize - 1);
            Swap(_i, _j);
            return _bytes[(_bytes[_i] + _bytes[_j]) & 255];
        }

        private void Encrypt(ref byte[] src)
        {
            for (var k = 0; k < src.Length; k++)
            {
                src[k] ^= Next();
            }
        }

        internal void Decrypt(ref byte[] src)
        {
            Encrypt(ref src);
        }
    }
}