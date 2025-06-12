using System;
using System.Text;
using Palmtree.Application;

namespace SimpleZipNormalizer.WindowsDesktop
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var launcher = new ConsoleApplicationLauncher("zipnorm", Encoding.UTF8);
            launcher.Launch(args);
        }
    }
}
