using System;
using System.Text;
using Palmtree;
using Palmtree.IO.Console;

namespace SimpleZipNormalizer.WindowsDesktop
{
    internal static class Program
    {
        private class DesctopApplication
            : WindowsDesktopApplication
        {
            private readonly Func<int> _main;

            public DesctopApplication(Func<int> main)
            {
                _main = main;
            }

            protected override int Main()
            {
                var exitCode = _main();
                TinyConsole.Beep();
                _ = TinyConsole.ReadLine();
                return exitCode;
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using var application = new NormalizerApplication("Zipnorm for Desktop", Encoding.UTF8);
            var desktopApplication = new DesctopApplication(() => _ = application.Run(args));

            desktopApplication.Run();
        }
    }
}
