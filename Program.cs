using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TextCopy;

namespace sshp
{
    class Program
    {
        private static bool masterPassSet = false;

        private static readonly string masterPass;

        static Program()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "init.io")))
            {
                masterPass = Encoding.UTF8.GetString(Convert.FromBase64String(CryptoLib.DeSerializeObject<string>(AppDomain.CurrentDomain.BaseDirectory)));
            }
            else
            {
                // Request a masterpass from user:
                Console.Write("Enter master pass: ");

                var pass = string.Empty;

                ConsoleKey key;

                ConsoleKeyInfo keyInfo;

                do
                {
                    keyInfo = Console.ReadKey(true);

                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass.Remove(pass.Length - 1); // pass[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                }
                while (keyInfo.Key != ConsoleKey.Enter);

                masterPass = pass;
                masterPassSet = true;
                CryptoLib.SerializeObject<string>(Convert.ToBase64String(Encoding.UTF8.GetBytes(masterPass)), AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            if (args.Length == 0)
            {
                Console.WriteLine("no-input, use one of the switches: [-s], [-a], [-v], [-k]");
            }
            else
            {
                if (args.Length == 1 && args[0] == "-a")
                {
                    Console.WriteLine(About());
                }
                else if (args.Length == 1 && args[0] == "-v")
                {
                    System.Diagnostics.Process.Start("https://emremumcu.com");
                }
                else if (args.Length == 1 && args[0] == "-k")
                {
                    Console.WriteLine(new CryptoLib(masterPass).KeySpace());
                }
                else
                {
                    CreateOutput(args[0]);
                }
            }

            if (masterPassSet)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\b \b");
                Console.WriteLine($"Master pass is SET as: {masterPass}");
                Console.Write("\b \b");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
                Console.Clear();
            }
        }

        static void CreateOutput(string input)
        {
            CryptoLib cl = new CryptoLib(masterPass);
            string output = cl.CreateOutput(input);
            // dotnet add package TextCopy 
            ClipboardService.SetText(output);
            Console.WriteLine("complete");
        }

        public static String About()
        {
            return
                new StringBuilder()
                .Append("Emre Mumcu @2021")
                .Append(Environment.NewLine)
                .Append("https://mumcusoft.com")
                .ToString();
        }
    }

    

    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
