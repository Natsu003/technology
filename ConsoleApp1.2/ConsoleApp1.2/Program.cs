using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;

namespace RSAExample
{
    class Program
    {
        static void Main()
        {
            const string fileName = "test50kb.bin";
            // 1) Generate a 50 KB test file if it doesn’t exist
            if (!File.Exists(fileName))
                File.WriteAllBytes(fileName, new byte[50 * 1024]);

            byte[] data = File.ReadAllBytes(fileName);

            // Key generation
            var swKey = Stopwatch.StartNew();
            using RSA rsa = RSA.Create(2048);
            swKey.Stop();
            Console.WriteLine($"RSA KeyGen:   {swKey.Elapsed.TotalSeconds:F4} s");

            int keySizeBytes = rsa.KeySize / 8;
            int maxDataLen = keySizeBytes - 42; // OAEP SHA-1 overhead

            // Encryption
            var swEnc = Stopwatch.StartNew();
            byte[] encrypted;
            using (var msEnc = new MemoryStream())
            {
                for (int offset = 0; offset < data.Length; offset += maxDataLen)
                {
                    int chunkSize = Math.Min(maxDataLen, data.Length - offset);
                    byte[] chunk = rsa.Encrypt(data.AsSpan(offset, chunkSize).ToArray(),
                                               RSAEncryptionPadding.OaepSHA1);
                    msEnc.Write(chunk, 0, chunk.Length);
                }
                encrypted = msEnc.ToArray();
            }
            swEnc.Stop();
            File.WriteAllBytes("rsa_enc.bin", encrypted);
            Console.WriteLine($"RSA Encrypt:  {swEnc.Elapsed.TotalSeconds:F4} s");

            // Decryption
            var swDec = Stopwatch.StartNew();
            byte[] decrypted;
            using (var msDec = new MemoryStream())
            {
                for (int offset = 0; offset < encrypted.Length; offset += keySizeBytes)
                {
                    byte[] chunk = encrypted.AsSpan(offset, keySizeBytes).ToArray();
                    byte[] decChunk = rsa.Decrypt(chunk, RSAEncryptionPadding.OaepSHA1);
                    msDec.Write(decChunk, 0, decChunk.Length);
                }
                decrypted = msDec.ToArray();
            }
            swDec.Stop();
            File.WriteAllBytes("rsa_dec.bin", decrypted);
            Console.WriteLine($"RSA Decrypt:  {swDec.Elapsed.TotalSeconds:F4} s");
        }
    }
}