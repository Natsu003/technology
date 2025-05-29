using System;
using System.Diagnostics;
using System.Text;

namespace Lab2_Hash
{
    static class MD5Custom
    {
        // Shift amounts
        private static readonly int[] S = {
             7,12,17,22, 7,12,17,22, 7,12,17,22, 7,12,17,22,
             5, 9,14,20, 5, 9,14,20, 5, 9,14,20, 5, 9,14,20,
             4,11,16,23, 4,11,16,23, 4,11,16,23, 4,11,16,23,
             6,10,15,21, 6,10,15,21, 6,10,15,21, 6,10,15,21
        };

        // Constants
        private static readonly uint[] K = new uint[64];

        static MD5Custom()
        {
            for (int i = 0; i < 64; i++)
                K[i] = (uint)(Math.Floor(Math.Abs(Math.Sin(i + 1)) * Math.Pow(2, 32)));
        }

        public static byte[] ComputeHash(byte[] input)
        {
            // pad
            int origLen = input.Length;
            ulong bitLen = (ulong)origLen * 8;
            int padLen = (56 - (origLen + 1) % 64 + 64) % 64;
            var data = new byte[origLen + 1 + padLen + 8];
            Array.Copy(input, data, origLen);
            data[origLen] = 0x80;
            for (int i = 0; i < 8; i++)
                data[data.Length - 8 + i] = (byte)(bitLen >> (8 * i));

            // init
            uint A = 0x67452301, B = 0xEFCDAB89, C = 0x98BADCFE, D = 0x10325476;

            // process each 512-bit chunk
            for (int off = 0; off < data.Length; off += 64)
            {
                uint[] M = new uint[16];
                for (int j = 0; j < 16; j++)
                    M[j] = BitConverter.ToUInt32(data, off + j * 4);

                uint a = A, b = B, c = C, d = D;
                for (int i = 0; i < 64; i++)
                {
                    uint F, g;
                    if (i < 16) { F = (b & c) | (~b & d); g = (uint)i; }
                    else if (i < 32) { F = (d & b) | (~d & c); g = (uint)((5 * i + 1) % 16); }
                    else if (i < 48) { F = b ^ c ^ d; g = (uint)((3 * i + 5) % 16); }
                    else { F = c ^ (b | ~d); g = (uint)((7 * i) % 16); }

                    uint tmp = d;
                    d = c;
                    c = b;
                    uint sum = a + F + K[i] + M[g];
                    b = b + RotateLeft(sum, S[i]);
                    a = tmp;
                }

                A += a; B += b; C += c; D += d;
            }

            // output little-endian
            var hash = new byte[16];
            Array.Copy(BitConverter.GetBytes(A), 0, hash, 0, 4);
            Array.Copy(BitConverter.GetBytes(B), 0, hash, 4, 4);
            Array.Copy(BitConverter.GetBytes(C), 0, hash, 8, 4);
            Array.Copy(BitConverter.GetBytes(D), 0, hash, 12, 4);
            return hash;
        }

        private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
    }

    static class SHA1Custom
    {
        public static byte[] ComputeHash(byte[] input)
        {
            // pad
            ulong bitLen = (ulong)input.Length * 8;
            int padLen = (56 - (input.Length + 1) % 64 + 64) % 64;
            var data = new byte[input.Length + 1 + padLen + 8];
            Array.Copy(input, data, input.Length);
            data[input.Length] = 0x80;
            for (int i = 0; i < 8; i++)
                data[data.Length - 1 - i] = (byte)(bitLen >> (8 * i));

            // init
            uint h0 = 0x67452301;
            uint h1 = 0xEFCDAB89;
            uint h2 = 0x98BADCFE;
            uint h3 = 0x10325476;
            uint h4 = 0xC3D2E1F0;

            // process each 512-bit chunk
            for (int off = 0; off < data.Length; off += 64)
            {
                uint[] W = new uint[80];
                for (int t = 0; t < 16; t++)
                    W[t] = (uint)(
                        (data[off + 4 * t] << 24) |
                        (data[off + 4 * t + 1] << 16) |
                        (data[off + 4 * t + 2] << 8) |
                        (data[off + 4 * t + 3])
                    );
                for (int t = 16; t < 80; t++)
                    W[t] = RotateLeft(W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16], 1);

                uint a = h0, b = h1, c = h2, d = h3, e = h4;
                for (int t = 0; t < 80; t++)
                {
                    uint f, k;
                    if (t < 20) { f = (b & c) | ((~b) & d); k = 0x5A827999; }
                    else if (t < 40) { f = b ^ c ^ d; k = 0x6ED9EBA1; }
                    else if (t < 60) { f = (b & c) | (b & d) | (c & d); k = 0x8F1BBCDC; }
                    else { f = b ^ c ^ d; k = 0xCA62C1D6; }

                    uint temp = RotateLeft(a, 5) + f + e + k + W[t];
                    e = d;
                    d = c;
                    c = RotateLeft(b, 30);
                    b = a;
                    a = temp;
                }

                h0 += a; h1 += b; h2 += c; h3 += d; h4 += e;
            }

            // output
            var hash = new byte[20];
            Array.Copy(ToBigEndian(h0), 0, hash, 0, 4);
            Array.Copy(ToBigEndian(h1), 0, hash, 4, 4);
            Array.Copy(ToBigEndian(h2), 0, hash, 8, 4);
            Array.Copy(ToBigEndian(h3), 0, hash, 12, 4);
            Array.Copy(ToBigEndian(h4), 0, hash, 16, 4);
            return hash;
        }

        private static byte[] ToBigEndian(uint x)
        {
            return new byte[]{
                (byte)(x >> 24),
                (byte)(x >> 16),
                (byte)(x >>  8),
                (byte) x
            };
        }

        private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
    }

    class Program
    {
        static void Main()
        {
            var text = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

            var sw = Stopwatch.StartNew();
            var md5 = MD5Custom.ComputeHash(text);
            sw.Stop();
            Console.WriteLine($"MD5   : {sw.Elapsed.TotalMilliseconds:F3} ms → {BitConverter.ToString(md5).Replace("-", "")}");

            sw.Restart();
            var sha1 = SHA1Custom.ComputeHash(text);
            sw.Stop();
            Console.WriteLine($"SHA-1 : {sw.Elapsed.TotalMilliseconds:F3} ms → {BitConverter.ToString(sha1).Replace("-", "")}");
        }
    }
}