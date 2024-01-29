using System;
using System.Text;
using Palmtree;
using Palmtree.IO;

namespace SimpleZipNormalizer.WindowsDesktop
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = typeof(Program).Assembly.GetBaseDirectory().FullName;
            var launcher = new ConsoleApplicationLauncher("zipnorm", Encoding.UTF8);
            launcher.Launch(args);
        }
    }
}
