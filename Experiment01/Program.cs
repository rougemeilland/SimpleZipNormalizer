using System;
using System.Linq;
using System.Text;
using Utility.Collections;
using Utility.IO;

namespace Experiment01
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private static void Main(string[] args)
        {
            var baseDirectory = new DirectoryPath(args[0]);
            var file = baseDirectory.GetFile("text.txt");
            using (var writer = file.CreateText())
            {
                for (var count = 0; count < 1024; ++count)
                {
                    writer.Write(new string(RandomSequence.GetAsciiCharSequence().Take(1024).ToArray()));
                }
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
