﻿namespace Plus.Communication.Encryption.KeyExchange
{
    using System;
    using Utilities;

    public sealed class DiffieHellman
    {
        public readonly int Bitlength = 32;

        private BigInteger _privateKey;

        public DiffieHellman()
        {
            Initialize();
        }

        public DiffieHellman(int b)
        {
            Bitlength = b;
            Initialize();
        }

        public DiffieHellman(BigInteger prime, BigInteger generator)
        {
            Prime = prime;
            Generator = generator;
            Initialize(true);
        }

        public BigInteger Prime { get; private set; }
        public BigInteger Generator { get; private set; }
        public BigInteger PublicKey { get; private set; }

        private void Initialize(bool ignoreBaseKeys = false)
        {
            PublicKey = 0;
            var rand = new Random();
            while (PublicKey == 0)
            {
                if (!ignoreBaseKeys)
                {
                    Prime = BigInteger.genPseudoPrime(Bitlength, 10, rand);
                    Generator = BigInteger.genPseudoPrime(Bitlength, 10, rand);
                }
                var bytes = new byte[Bitlength / 8];
                Randomizer.NextBytes(bytes);
                _privateKey = new BigInteger(bytes);
                if (Generator > Prime)
                {
                    var temp = Prime;
                    Prime = Generator;
                    Generator = temp;
                }
                PublicKey = Generator.modPow(_privateKey, Prime);
                if (!ignoreBaseKeys)
                {
                    break;
                }
            }
        }

        public BigInteger CalculateSharedKey(BigInteger m) => m.modPow(_privateKey, Prime);
    }
}