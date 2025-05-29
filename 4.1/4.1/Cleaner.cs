using System;
using System.IO;

namespace _4_1
{
    static class Cleaner
    {
        private static readonly string LogFile = Path.Combine(
            Path.GetTempPath(),
            "keylog.txt"
        );

        private const string Password = "secret";

        public static void Run()
        {
            Console.Write("Enter admin password: ");
            if (Console.ReadLine() != Password)
            {
                Console.WriteLine("Wrong password.");
                return;
            }

            if (!File.Exists(LogFile))
            {
                Console.WriteLine("Log file not found at: " + LogFile);
                return;
            }

            long len = new FileInfo(LogFile).Length;
            using (var fs = new FileStream(LogFile, FileMode.Open, FileAccess.Write))
            {
                byte[] zeros = new byte[len];
                fs.Write(zeros, 0, zeros.Length);
                fs.Flush();
            }
            File.Delete(LogFile);
            Console.WriteLine("Log destroyed.");
        }
    }
}