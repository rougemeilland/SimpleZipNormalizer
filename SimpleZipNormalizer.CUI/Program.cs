using System.Text;
using Palmtree;
using Palmtree.IO.Console;

namespace SimpleZipNormalizer.CUI
{
    public partial class Program
    {
        private static int Main(string[] args)
        {
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;
            var application = new NormalizerApplication(typeof(Program).Assembly.GetAssemblyFileNameWithoutExtension(), Encoding.UTF8);
            return application.Run(args);
        }
    }
}
