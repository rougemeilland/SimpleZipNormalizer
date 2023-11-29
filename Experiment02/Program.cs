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
            var zipFile = new FilePath(args[0]);
            using (var zipWriter = zipFile.CreateAsZipFile(ZipEntryNameEncodingProvider.CreateInstance()))
            {
                var now1 = DateTime.Now;
                var entry1 = zipWriter.CreateEntry("諸/", new byte[] { 0xee, 0x8d, (int)'/' }, "黑", new byte[] { 0xee, 0xec }, Encoding.GetEncoding("shift_jis"), Array.Empty<Encoding>());
                entry1.IsDirectory = true;
                entry1.LastWriteTimeUtc = now1;
                entry1.LastAccessTimeUtc = now1;
                entry1.CreationTimeUtc = now1;
                var now2 = DateTime.Now;
                var entry2 = zipWriter.CreateEntry("諸/馞.txt", new byte[] { 0xee, 0x8d, (int)'/', 0xee, 0xde, (int)'.', (int)'t', (int)'x', (int)'t' }, "魵", new byte[] { 0xee, 0xe2 }, Encoding.GetEncoding("shift_jis"), Array.Empty<Encoding>());
                entry2.IsFile = true;
                entry2.LastWriteTimeUtc = now2;
                entry2.LastAccessTimeUtc = now2;
                entry2.CreationTimeUtc = now2;
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
