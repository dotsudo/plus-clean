namespace Plus.Communication.Encryption.Crypto.RSA
{
    using System;
    using Utilities;

    public sealed class RsaKey
    {
        private RsaKey(BigInteger n, int e, BigInteger d, BigInteger p, BigInteger q, BigInteger dmp1, BigInteger dmq1,
                       BigInteger coeff)
        {
            E = e;
            this.e = e;
            N = n;
            D = d;
            P = p;
            Q = q;
            Dmp1 = dmp1;
            Dmq1 = dmq1;

            //this.GeneratePair(1024, this.e);
        }

        private int E { get; }
        private BigInteger e { get; }
        private BigInteger N { get; }
        private BigInteger D { get; }
        private BigInteger P { get; }
        private BigInteger Q { get; }
        private BigInteger Dmp1 { get; }
        private BigInteger Dmq1 { get; }

        public static RsaKey ParsePrivateKey(string n,
                                             string e,
                                             string d,
                                             string p = null,
                                             string q = null,
                                             string dmp1 = null,
                                             string dmq1 = null,
                                             string coeff = null)
        {
            if (p == null)
            {
                return new RsaKey(new BigInteger(n, 16), Convert.ToInt32(e, 16), new BigInteger(d, 16), 0, 0, 0, 0, 0);
            }

            return new RsaKey(new BigInteger(n, 16),
                Convert.ToInt32(e, 16),
                new BigInteger(d, 16),
                new BigInteger(p, 16),
                new BigInteger(q, 16),
                new BigInteger(dmp1, 16),
                new BigInteger(dmq1, 16),
                new BigInteger(coeff, 16));
        }

        private int GetBlockSize() => (N.bitCount() + 7) / 8;

        public byte[] Sign(byte[] src) => DoEncrypt(DoPrivate, src, Pkcs1PadType.FullByte);

        public byte[] Verify(byte[] src) => DoDecrypt(DoPrivate, src, Pkcs1PadType.FullByte);

        private byte[] DoEncrypt(DoCalculateionDelegate method, byte[] src, Pkcs1PadType type)
        {
            try
            {
                var bl = GetBlockSize();
                var paddedBytes = pkcs1pad(src, bl, type);
                var m = new BigInteger(paddedBytes);
                if (m == 0)
                {
                    return null;
                }

                var c = method(m);
                if (c == 0)
                {
                    return null;
                }

                return c.getBytes();
            }
            catch
            {
                return null;
            }
        }

        private byte[] DoDecrypt(DoCalculateionDelegate method, byte[] src, Pkcs1PadType type)
        {
            try
            {
                var c = new BigInteger(src);
                var m = method(c);
                if (m == 0)
                {
                    return null;
                }

                var bl = GetBlockSize();
                var bytes = pkcs1unpad(m.getBytes(), bl);
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        private byte[] pkcs1pad(byte[] src, int n, Pkcs1PadType type)
        {
            var bytes = new byte[n];
            var i = src.Length - 1;
            while (i >= 0 && n > 11)
            {
                bytes[--n] = src[i--];
            }

            bytes[--n] = 0;
            while (n > 2)
            {
                byte x = 0;
                switch (type)
                {
                    case Pkcs1PadType.FullByte:
                        x = 0xFF;
                        break;
                    case Pkcs1PadType.RandomByte:
                        x = Randomizer.NextByte(1, 255);
                        break;
                }

                bytes[--n] = x;
            }

            bytes[--n] = (byte) type;
            bytes[--n] = 0;
            return bytes;
        }

        private byte[] pkcs1unpad(byte[] src, int n)
        {
            var i = 0;
            while (i < src.Length && src[i] == 0)
            {
                ++i;
            }

            if (src.Length - i != n - 1 || src[i] > 2)
            {
                Console.WriteLine("PKCS#1 unpad: i={0}, expected src[i]==[0,1,2], got src[i]={1}", i, src[i].ToString("X"));
                return null;
            }

            ++i;
            while (src[i] != 0)
            {
                if (++i >= src.Length)
                {
                    Console.WriteLine("PKCS#1 unpad: i={0}, src[i-1]!=0 (={1})", i, src[i - 1].ToString("X"));
                }
            }

            var bytes = new byte[src.Length - i - 1];
            for (var p = 0; ++i < src.Length; p++)
            {
                bytes[p] = src[i];
            }

            return bytes;
        }

        private BigInteger DoPrivate(BigInteger m)
        {
            if (P == 0 && Q == 0)
            {
                return m.modPow(D, N);
            }

            return 0;
        }
    }

    internal delegate BigInteger DoCalculateionDelegate(BigInteger m);

    internal enum Pkcs1PadType
    {
        FullByte = 1,
        RandomByte = 2
    }
}