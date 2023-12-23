using System;
using System.Collections.Generic;
using System.Linq;
using Palmtree;
using Palmtree.Collections;
using Palmtree.IO;
using Palmtree.IO.Compression.Archive.Zip;

namespace Test.ZipUtility.SingleVolume
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var baseDirectory = new DirectoryPath(args[0]);
            Test1(baseDirectory, "test_small", 100UL, false);
            Test1(baseDirectory, "test_huge", 1024UL * 1024 * 1024 * 2, false);
            Test1(baseDirectory, "test_small_dd", 100UL, true);
            Test1(baseDirectory, "test_huge_dd", 1024UL * 1024 * 1024 * 2, true);
            Console.WriteLine("終了しました。");
            Console.Beep();
            _ = Console.ReadLine();
        }

        private static void Test1(DirectoryPath baseDirectory, string fileName, ulong contentLength, bool useDatadescriptor)
        {
            var zipArchive = baseDirectory.GetFile($"{fileName}.zip");
            using (var zipWriter = zipArchive.CreateAsZipFile())
            {
                var dir = zipWriter.CreateEntry("ディレクトリ/", "これはディレクトリです。");
                dir.IsDirectory = true;
                dir.CreationTimeUtc = DateTime.Now;
                dir.LastAccessTimeUtc = DateTime.Now;
                dir.LastWriteTimeUtc = DateTime.Now;

                Console.WriteLine($"書き込み中1... \"{zipArchive.FullName}\"");
                var file1 = zipWriter.CreateEntry("ディレクトリ/ファイル1.bin", "これはファイルその1です。");
                file1.IsFile = true;
                file1.CreationTimeUtc = DateTime.Now;
                file1.LastAccessTimeUtc = DateTime.Now;
                file1.LastWriteTimeUtc = DateTime.Now;
                file1.CompressionMethodId = ZipEntryCompressionMethodId.Deflate;
                file1.CompressionLevel = ZipEntryCompressionLevel.Maximum;
                file1.UseDataDescriptor = useDatadescriptor;
                WriteContentData(file1, contentLength);

                Console.WriteLine($"書き込み中2... \"{zipArchive.FullName}\"");
                var file2 = zipWriter.CreateEntry("ファイル2.bin", "これはファイルその2です。");
                file2.IsFile = true;
                file2.CreationTimeUtc = DateTime.Now;
                file2.LastAccessTimeUtc = DateTime.Now;
                file2.LastWriteTimeUtc = DateTime.Now;
                file2.CompressionMethodId = ZipEntryCompressionMethodId.Deflate;
                file2.CompressionLevel = ZipEntryCompressionLevel.Maximum;
                file2.UseDataDescriptor = useDatadescriptor;
                WriteContentData(file2, contentLength);

                Console.WriteLine($"書き込み中3... \"{zipArchive.FullName}\"");
                var file3 = zipWriter.CreateEntry("ファイル3.bin", "これはファイルその3です。");
                file3.IsFile = true;
                file3.CreationTimeUtc = DateTime.Now;
                file3.LastAccessTimeUtc = DateTime.Now;
                file3.LastWriteTimeUtc = DateTime.Now;
                file3.CompressionMethodId = ZipEntryCompressionMethodId.Deflate;
                file3.CompressionLevel = ZipEntryCompressionLevel.Maximum;
                file3.UseDataDescriptor = useDatadescriptor;
                WriteContentData(file3, 100);
            }

            Console.WriteLine($"検査中... \"{zipArchive.FullName}\"");
            var result = zipArchive.ValidateAsZipFile(ValidationStringency.Strict);
            Console.WriteLine($"検査終了: {result.ResultId}, \"{result.Message}\"");
        }

        private static void WriteContentData(ZipDestinationEntry fileEntry, ulong contentLength)
        {
            const ulong maximumLength = 1024UL * 1024UL;

            using var outStream1 = fileEntry.GetContentStream();
            var remain = contentLength;
            while (remain > 0)
            {
                var length = checked((int)remain.Minimum(maximumLength));
                RandomSequence.GetByteSequence().Take(length).AsByteStream().CopyTo(outStream1);
                remain -= checked((uint)length);
            }
        }
    }
}
