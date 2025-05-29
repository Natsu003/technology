using System;
using System.Numerics;

namespace Lab3_RSA_Signature
{
    public static class PrimeUtils
    {
        public static BigInteger GenerateRandomOddBigInteger(int bits, Random rng)
        {
            byte[] bytes = new byte[bits / 8];
            BigInteger result;
            do
            {
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] |= 0x01; // Зробити число непарним
                result = new BigInteger(bytes);
                result = BigInteger.Abs(result);
            } while (result.GetBitLength() != bits);
            return result;
        }

        public static BigInteger GeneratePseudoPrime(int bits, Random rng, int rounds = 5)
        {
            BigInteger candidate;
            do
            {
                candidate = GenerateRandomOddBigInteger(bits, rng);
            } while (!IsProbablyPrime(candidate, rounds));
            return candidate;
        }

        public static bool IsProbablyPrime(BigInteger n, int k = 5)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;

            BigInteger d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            var rand = new Random();
            for (int i = 0; i < k; i++)
            {
                BigInteger a = RandomBetween(2, n - 2, rand);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                bool pass = false;
                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1)
                    {
                        pass = true;
                        break;
                    }
                }
                if (!pass) return false;
            }
            return true;
        }

        private static BigInteger RandomBetween(BigInteger min, BigInteger max, Random rng)
        {
            BigInteger diff = max - min;
            byte[] bytes = diff.ToByteArray();
            BigInteger result;
            do
            {
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F;
                result = new BigInteger(bytes);
            } while (result > diff);
            return min + result;
        }

        public static int GetBitLength(this BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            int bits = (bytes.Length - 1) * 8;
            byte b = bytes[^1];
            while (b != 0)
            {
                bits++;
                b >>= 1;
            }
            return bits;
        }
    }
}