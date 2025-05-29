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

        public RSAKey(int bits = 512)
        {
            var rng = new Random();
            BigInteger p = PrimeUtils.GeneratePseudoPrime(bits / 2, rng);
            BigInteger q = PrimeUtils.GeneratePseudoPrime(bits / 2, rng);
            n = p * q;
            var phi = (p - 1) * (q - 1);
            e = 65537;
            d = ModInverse(e, phi);
        }

        public BigInteger Sign(BigInteger hash) => BigInteger.ModPow(hash, d, n);
        public BigInteger Verify(BigInteger sig) => BigInteger.ModPow(sig, e, n);

        private static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;
            if (m == 1) return 0;
            while (a > 1)
            {
                q = a / m;
                t = m;
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
        public static BigInteger GetSHA1Hash(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var fs = File.OpenRead(filePath);
            var hashBytes = sha1.ComputeHash(fs);
            return new BigInteger(hashBytes.Reverse().ToArray());
        }
    }

    class Program
    {
        static void Main()
        {
            //Створення файлу
            string file = "example.txt";
            if (!File.Exists(file))
            {
                Console.WriteLine("Generating test file 'example.txt'...");
                string line = "This is a test line for the example file.\n";
                int targetSize = 100 * 1024; // 100 KB
                int repeatCount = targetSize / line.Length + 1;
                File.WriteAllText(file, string.Concat(Enumerable.Repeat(line, repeatCount)));
                Console.WriteLine($"File '{file}' created ({new FileInfo(file).Length} bytes).");
            }

            // Генерація RSA ключів
            var rsa = new RSAKey(512);

            // Обчислення хешу та створення підпису
            var hash = FileHasher.GetSHA1Hash(file);
            var signature = rsa.Sign(hash);
            File.WriteAllText("signature.txt", signature.ToString());

            // Перевірка підпису
            var hash2 = FileHasher.GetSHA1Hash(file);
            var sigFromFile = BigInteger.Parse(File.ReadAllText("signature.txt"));
            var decryptedHash = rsa.Verify(sigFromFile);

            Console.WriteLine($"Original hash:   {hash}");
            Console.WriteLine($"Decrypted hash:  {decryptedHash}");
            Console.WriteLine($"Signature valid: {hash == decryptedHash}");
        }
    }
}