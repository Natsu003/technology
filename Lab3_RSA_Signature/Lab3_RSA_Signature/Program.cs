using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Lab3_RSA_Signature
{
    class RSAKey
    {
        public BigInteger n, e, d;

        // Демо-режим
        public RSAKey(bool demo = true)
        {
            if (demo)
            {
                BigInteger p = 61, q = 53;
                Console.WriteLine("Using fixed primes p=61, q=53 for demo.");
                n = p * q;
                var phi = (p - 1) * (q - 1);
                e = 17;
                d = ModInverse(e, phi);
                Console.WriteLine($"n = {n}, e = {e}, d = {d}\n");
                return;
            }
            throw new NotImplementedException("Full keygen not implemented in demo mode.");
        }

        public BigInteger Sign(BigInteger hashMod) => BigInteger.ModPow(hashMod, d, n);
        public BigInteger Verify(BigInteger sig) => BigInteger.ModPow(sig, e, n);

        private static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, x0 = 0, x1 = 1, q, t;
            if (m == 1) return 0;
            while (a > 1)
            {
                q = a / m; t = m;
                m = a % m; a = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }
            return x1 < 0 ? x1 + m0 : x1;
        }
    }

    static class FileHasher
    {
        public static BigInteger GetSHA1Hash(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            byte[] hash = SHA1.Create().ComputeHash(data);
            // повертаємо беззнаковий BigInteger у великому порядку
            return new BigInteger(hash, isUnsigned: true, isBigEndian: true);
        }
    }

    class Program
    {
        static void Main()
        {
            const string file = "example.txt";

            // Тестовий файл
            if (!File.Exists(file))
            {
                Console.WriteLine("Generating 'example.txt'...");
                string line = "This is a test line for the example file.\n";
                int repeat = (100 * 1024) / line.Length + 1;
                File.WriteAllText(file, string.Concat(Enumerable.Repeat(line, repeat)));
                Console.WriteLine($"'{file}' created ({new FileInfo(file).Length} bytes).\n");
            }

            //RSA у демо-режимі
            var rsa = new RSAKey(demo: true);

            //Обчислення SHA-1 і звужування хешу mod n
            Console.WriteLine("Hashing file...");
            var hash = FileHasher.GetSHA1Hash(file);
            var hashMod = hash % rsa.n;
            if (hashMod < 0) hashMod += rsa.n;
            Console.WriteLine($"Original hash mod n: {hashMod}\n");

            //Підпис
            Console.WriteLine("Signing...");
            var signature = rsa.Sign(hashMod);
            File.WriteAllText("signature.txt", signature.ToString());
            Console.WriteLine($"Signature: {signature}\n(saved in 'signature.txt')\n");

            //Перевірка
            Console.WriteLine("Verifying...");
            var sigFromFile = BigInteger.Parse(File.ReadAllText("signature.txt"));
            var recoveredMod = rsa.Verify(sigFromFile);
            Console.WriteLine($"Recovered hash mod n: {recoveredMod}\n");

            Console.WriteLine($"Valid signature: {hashMod == recoveredMod}");
        }
    }
}