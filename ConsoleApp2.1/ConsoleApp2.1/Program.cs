using System;
using System.Numerics;

namespace Lab2_PRNG
{
    class ParkMillerRNG
    {
        private const long A = 16807;
        private const long M = 2147483647;    // 2^31 - 1
        private const long Q = M / A;          // 127773
        private const long R = M % A;          // 2836
        private long seed;

        public ParkMillerRNG(long seed)
        {
            this.seed = seed % M;
            if (this.seed <= 0) this.seed += M - 1;
        }

        public long Next()
        {
            long t = A * (seed % Q) - R * (seed / Q);
            if (t > 0) seed = t; else seed = t + M;
            return seed;
        }
    }

    static class Primality
    {
        public static bool IsProbablyPrime(BigInteger n, int k = 5)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;

            // write n-1 as 2^r * d
            BigInteger d = n - 1;
            int r = 0;
            while ((d & 1) == 0) { d >>= 1; r++; }

            var rng = new Random();
            for (int i = 0; i < k; i++)
            {
                BigInteger a = RandomBetween(rng, 2, n - 2);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                bool cont = false;
                for (int j = 1; j < r; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1) { cont = true; break; }
                }
                if (cont) continue;
                return false;
            }
            return true;
        }

        private static BigInteger RandomBetween(Random rng, BigInteger min, BigInteger max)
        {
            // returns a in [min, max]
            BigInteger diff = max - min;
            byte[] buf = diff.ToByteArray();
            BigInteger r;
            do
            {
                rng.NextBytes(buf);
                buf[buf.Length - 1] &= 0x7F;
                r = new BigInteger(buf);
            } while (r > diff);
            return min + r;
        }
    }

    class Program
    {
        static void Main()
        {
            var rng = new ParkMillerRNG(DateTime.Now.Ticks);
            Console.WriteLine("Generating 10 pseudorandom numbers + primality check:");
            for (int i = 0; i < 10; i++)
            {
                long x = rng.Next();
                bool prime = Primality.IsProbablyPrime(x);
                Console.WriteLine($"{x,12} → {(prime ? "prime" : "composite")}");
            }
        }
    }
}