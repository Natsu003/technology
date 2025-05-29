using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;

namespace ConsoleApp1_1
{
    class Program
    {
        static void Main()
        {
            const string fileName = "test50kb.bin";
            if (!File.Exists(fileName))
                File.WriteAllBytes(fileName, new byte[50 * 1024]);

            byte[] key = Encoding.UTF8.GetBytes("studenK8");

            var sw = Stopwatch.StartNew();
            byte[] cipher = DesEncrypt(File.ReadAllBytes(fileName), key);
            sw.Stop();
            File.WriteAllBytes("des_enc.bin", cipher);
            Console.WriteLine($"DES Encrypt: {sw.Elapsed.TotalSeconds:F4} s");

            sw.Restart();
            byte[] plain = DesDecrypt(cipher, key);
            sw.Stop();
            File.WriteAllBytes("des_dec.bin", plain);
            Console.WriteLine($"DES Decrypt: {sw.Elapsed.TotalSeconds:F4} s");
        }

        static byte[] DesEncrypt(byte[] data, byte[] key)
        {
            using var des = DES.Create();
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        static byte[] DesDecrypt(byte[] data, byte[] key)
        {
            using var des = DES.Create();
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}
