namespace SimpleZipNormalizer.CUI
{
    public partial class Program
    {
        private static int Main(string[] args)
        {
            using var application = new NormalizerApplication(null, null);
            return application.Run(args);
        }
    }
}
