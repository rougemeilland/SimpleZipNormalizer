using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility.Collections;
using Utility.IO;
using Utility.Text;
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
                {
                    var now = DateTime.Now;
                    var entry = zipWriter.CreateEntry("諸/", new byte[] { 0xee, 0x8d, (int)'/' }, "黑", new byte[] { 0xee, 0xec }, Encoding.GetEncoding("shift_jis"), Array.Empty<Encoding>());
                    entry.IsDirectory = true;
                    entry.LastWriteTimeUtc = now;
                    entry.LastAccessTimeUtc = now;
                    entry.CreationTimeUtc = now;
                }

                {
                    var now = DateTime.Now;
                    var entry = zipWriter.CreateEntry("諸/馞1.bin", new byte[] { 0xee, 0x8d, (int)'/', 0xee, 0xde, (int)'1', (int)'.', (int)'b', (int)'i', (int)'n' }, "魵", new byte[] { 0xee, 0xe2 }, Encoding.GetEncoding("shift_jis"), Array.Empty<Encoding>());
                    entry.IsFile = true;
                    entry.LastWriteTimeUtc = now;
                    entry.LastAccessTimeUtc = now;
                    entry.CreationTimeUtc = now;
                    entry.CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                    using var outStream = entry.GetContentStream();
                    using var writer = new StreamWriter(outStream.AsStream(), Encoding.ASCII);
                    var totalLength = (ulong)uint.MaxValue + 1;
                    var text = new string('*', 1024);
                    for (var count = 0UL; count < totalLength; count += (uint)text.Length)
                        writer.Write(text);
                }

                {
                    var now = DateTime.Now;
                    var entry = zipWriter.CreateEntry("諸/馞2.bin", new byte[] { 0xee, 0x8d, (int)'/', 0xee, 0xde, (int)'2', (int)'.', (int)'b', (int)'i', (int)'n' }, "魵", new byte[] { 0xee, 0xe2 }, Encoding.GetEncoding("shift_jis"), Array.Empty<Encoding>());
                    entry.IsFile = true;
                    entry.LastWriteTimeUtc = now;
                    entry.LastAccessTimeUtc = now;
                    entry.CreationTimeUtc = now;
                    entry.CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                    using var outStream = entry.GetContentStream();
                    using var writer = new StreamWriter(outStream.AsStream(), Encoding.ASCII);
                    var totalLength = (ulong)uint.MaxValue + 1;
                    var text = new string('*', 1024);
                    for (var count = 0UL; count < totalLength; count += (uint)text.Length)
                        writer.Write(text);
                }
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
