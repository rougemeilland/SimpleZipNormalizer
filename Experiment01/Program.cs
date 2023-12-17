using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility.Collections;
using Utility.IO;
using ZipUtility;
using ZipUtility.ExtraFields;

namespace Experiment01
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static void Main(string[] args)
        {
            using (var writer = new StreamWriter(args[0], false, Encoding.UTF8))
            {
                foreach (var c in RandomSequence.GetAsciiCharSequence().Take(32765-90))
                    writer.Write(c);
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
