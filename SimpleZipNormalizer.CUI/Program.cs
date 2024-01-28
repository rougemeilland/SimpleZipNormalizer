using System.Text;
using Palmtree;

namespace SimpleZipNormalizer.CUI
{
    public partial class Program
    {
        private static int Main(string[] args)
        {
            var application = new NormalizerApplication(typeof(Program).Assembly.GetAssemblyFileNameWithoutExtension(), Encoding.UTF8);
            return application.Run(args);
        }
    }
}
