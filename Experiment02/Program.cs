using System;
using System.Text;
using Utility.IO;
using ZipUtility;

namespace Experiment02
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        static void Main(string[] args)
        {
            var entryNameProvider = ZipEntryNameEncodingProvider.CreateInstance();
            foreach (var file in args.EnumerateFilesFromArgument(true))
            {
                if (string.Equals(file.Extension, ".zip", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(file.Extension, ".001", StringComparison.OrdinalIgnoreCase))
                {
                    var result = file.ValidateAsZipFile(entryNameProvider);
                    Console.WriteLine($"\"{file.FullName}\": {result.ResultId}, \"{result.Message}\"");
                }
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
