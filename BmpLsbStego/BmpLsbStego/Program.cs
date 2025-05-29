using System;
using System.IO;
using System.Text;

namespace BmpLsbStego
{
    class Program
    {
        const int BMP_HEADER_SIZE = 54;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowHelp();
                return;
            }

            string cmd = args[0].ToLower();
            if (cmd == "embed")
            {
                if (args.Length != 4)
                {
                    ShowHelp();
                    return;
                }

                string inBmp = args[1];
                string outBmp = args[2];
                string secretMsg = args[3];

                Embed(inBmp, outBmp, secretMsg);
                Console.WriteLine("Embedded message into: " + outBmp);
            }
            else if (cmd == "extract")
            {
                if (args.Length != 2)
                {
                    ShowHelp();
                    return;
                }

                string stegoBmp = args[1];
                string recovered = Extract(stegoBmp);
                Console.WriteLine("Recovered message:");
                Console.WriteLine(recovered);
            }
            else
            {
                ShowHelp();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  embed   <in.bmp> <out.bmp> \"secret message\"");
            Console.WriteLine("  extract <stego.bmp>");
        }

        static void Embed(string inputBmp, string outputBmp, string message)
        {
            // BMP у масив
            byte[] allBytes = File.ReadAllBytes(inputBmp);

            byte[] header = new byte[BMP_HEADER_SIZE];
            Array.Copy(allBytes, 0, header, 0, BMP_HEADER_SIZE);

            byte[] pixels = new byte[allBytes.Length - BMP_HEADER_SIZE];
            Array.Copy(allBytes, BMP_HEADER_SIZE, pixels, 0, pixels.Length);

            byte[] msgBytes = Encoding.UTF8.GetBytes(message + "\0");

            // біти у LSB кожний третій байт
            for (int i = 0; i < msgBytes.Length * 8; i++)
            {
                int bytePos = i / 8;
                int bitPos = 7 - (i % 8);
                int bit = (msgBytes[bytePos] >> bitPos) & 1;

                int pixelByteIndex = i * 3;
                if (pixelByteIndex >= pixels.Length)
                    throw new InvalidOperationException("Image too small for message");

                pixels[pixelByteIndex] = (byte)((pixels[pixelByteIndex] & 0xFE) | bit);
            }

            // назад у файл
            using (var fs = new FileStream(outputBmp, FileMode.Create, FileAccess.Write))
            {
                fs.Write(header, 0, header.Length);
                fs.Write(pixels, 0, pixels.Length);
            }
        }

        static string Extract(string stegoBmp)
        {
            byte[] allBytes = File.ReadAllBytes(stegoBmp);

            // пікселі
            byte[] pixels = new byte[allBytes.Length - BMP_HEADER_SIZE];
            Array.Copy(allBytes, BMP_HEADER_SIZE, pixels, 0, pixels.Length);

            var sb = new StringBuilder();
            int currentByte = 0;
            int bitsCollected = 0;

            // LSB
            for (int i = 0; ; i++)
            {
                int pixelByteIndex = i * 3;
                if (pixelByteIndex >= pixels.Length)
                    break;

                int bit = pixels[pixelByteIndex] & 1;
                currentByte = (currentByte << 1) | bit;
                bitsCollected++;

                // 8 біт — в байт
                if (bitsCollected == 8)
                {
                    if (currentByte == 0)
                        break;

                    sb.Append((char)currentByte);
                    bitsCollected = 0;
                    currentByte = 0;
                }
            }

            return sb.ToString();
        }
    }
}