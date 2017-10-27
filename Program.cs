namespace Plus
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using Core;
    using log4net.Config;

    internal static class Program
    {
        private const int MfBycommand = 0x00000000;
        private const int ScClose = 0xF060;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), ScClose, MfBycommand);

            XmlConfigurator.Configure();

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += PlusEnvironment.MyHandler;

            PlusEnvironment.Initialize();

            while (true)
            {
                if (Console.ReadKey(true).Key != ConsoleKey.Enter)
                {
                    continue;
                }

                Console.Write("plus> ");
                var input = Console.ReadLine();

                if (input == null || input.Length <= 0)
                {
                    continue;
                }

                var s = input.Split(' ')[0];
                ConsoleCommands.InvokeCommand(s);
            }
        }
    }
}