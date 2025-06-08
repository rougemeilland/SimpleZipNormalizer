using System;
using System.Text;
using Palmtree.Application;
using Palmtree.IO.Console;

namespace SimpleZipNormalizer.WindowsDesktop
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;
            var launcher = new ConsoleApplicationLauncher("zipnorm", Encoding.UTF8);
            launcher.Launch(args);
        }
    }
}
