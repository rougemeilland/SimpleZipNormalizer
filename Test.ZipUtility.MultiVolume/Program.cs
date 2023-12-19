using System;
using System.Linq;
using Utility;
using Utility.Collections;
using Utility.IO;
using ZipUtility;

namespace Test.ZipUtility.MultiVolume
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var baseDirectory = new DirectoryPath(args[0]);

            Test1_2番目以降のローカルヘッダがボリュームの先頭にある場合(baseDirectory, $"{GetFileName(nameof(Test1_2番目以降のローカルヘッダがボリュームの先頭にある場合))}.zip");

            Test2_データディスクリプタがボリュームの先頭にある場合(baseDirectory, $"{GetFileName(nameof(Test2_データディスクリプタがボリュームの先頭にある場合))}.zip");

            // WinRar で開くとエラーとなる。PKZIP および 7-zip は OK。
            Test3_最初のセントラルディレクトリヘッダがボリュームディスクの先頭にある場合(baseDirectory, $"{GetFileName(nameof(Test3_最初のセントラルディレクトリヘッダがボリュームディスクの先頭にある場合))}.zip");

            // WinRar で開くとエラーとなる。PKZIP および 7-zip は OK。
            Test4_ZIP64_EOCDR_がボリュームディスクの先頭にある場合(baseDirectory, $"{GetFileName(nameof(Test4_ZIP64_EOCDR_がボリュームディスクの先頭にある場合))}.zip");

            // WinRar で開くとエラーとなる。PKZIP および 7-zip は OK。
            Test5_ZIP64_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合(baseDirectory, $"{GetFileName(nameof(Test5_ZIP64_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合))}.zip");

            // WinRar で開くとエラーとなる。PKZIP および 7-zip は OK。
            Test6_EOCDR_がボリュームディスクの先頭にある場合(baseDirectory, $"{GetFileName(nameof(Test6_EOCDR_がボリュームディスクの先頭にある場合))}.zip");

            Test7_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合(baseDirectory, $"{GetFileName(nameof(Test7_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合))}.zip");

            Console.WriteLine("終了しました。");
            Console.Beep();
            _ = Console.ReadLine();
        }

        private static string GetFileName(string text)
        {
            var index = text.IndexOf('_');
            if (index < 0)
                return text;
            else
                return text[(index + 1)..];
        }

        private static void Test1_2番目以降のローカルヘッダがボリュームの先頭にある場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = 1024;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 2, checked((ushort)(VOLUME_SIZE - 69)), 0, false);
        }

        private static void Test2_データディスクリプタがボリュームの先頭にある場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = 1024;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 2, checked((ushort)(VOLUME_SIZE - 69)), 0, true);
        }

        private static void Test3_最初のセントラルディレクトリヘッダがボリュームディスクの先頭にある場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = 1024;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 2, 16, checked((ushort)(VOLUME_SIZE - 130)), false);
        }

        private static void Test4_ZIP64_EOCDR_がボリュームディスクの先頭にある場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = (uint.MaxValue + 100UL) * 2;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 2, uint.MaxValue - 97, 0, false);
        }

        private static void Test5_ZIP64_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = (uint.MaxValue + 100UL) * 2;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 4, uint.MaxValue / 2 - 70, 0, false);
        }
        private static void Test6_EOCDR_がボリュームディスクの先頭にある場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = 1024;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 2, VOLUME_SIZE - 180, 0, false);
        }

        private static void Test7_EOCDR_があるボリュームディスクにセントラルディレクトリヘッダが部分的に含まれている場合(DirectoryPath baseDirectory, string fileName)
        {
            const ulong VOLUME_SIZE = 1024;
            DoTest1(baseDirectory, fileName, VOLUME_SIZE, ZipWriteFlags.None, 4, VOLUME_SIZE / 2 - 150, 0, false);
        }

        private static void DoTest1(DirectoryPath baseDirectory, string fileName, ulong volumeSize, ZipWriteFlags flag, int numberOfEntries, ulong contentSize, ushort commentSize, bool useDatadescriptor)
        {
            var zipArchive = baseDirectory.GetFile(fileName);
            using (var zipWriter = zipArchive.CreateAsZipFile(volumeSize))
            {
                zipWriter.Flags = flag;
                for (var count = 1; count <= numberOfEntries; ++count)
                {
                    Console.WriteLine($"書き込み中 {count}/{numberOfEntries}... \"{zipArchive.FullName}\"");
                    var file = zipWriter.CreateEntry($"ファイル{count}.bin", new string(RandomSequence.GetAsciiCharSequence().Take(commentSize).ToArray()));
                    file.IsFile = true;
                    file.CreationTimeUtc = DateTime.Now;
                    file.LastAccessTimeUtc = DateTime.Now;
                    file.LastWriteTimeUtc = DateTime.Now;
                    file.CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                    file.UseDataDescriptor = useDatadescriptor;
                    WriteContentData(file, contentSize);
                }
            }

            try
            {
                using (var reader = zipArchive.OpenAsZipFile(ValidationStringency.Strict))
                {
                    Console.WriteLine($"検査中... \"{zipArchive.FullName}\"");
                    var entries = reader.EnumerateEntries();
                    var count = 0UL;
                    foreach (var entry in entries)
                    {
                        Console.WriteLine($"検査中 ({count + 1}/{numberOfEntries})... \"{zipArchive.FullName}\"");
                        VerifyContentData(entry);
                        checked
                        {
                            ++count;
                        }
                    }
                }

                Console.WriteLine($"検査終了");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    Console.WriteLine($"検査中にエラーが発生しました。: {ex.Message}");
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        private static void WriteContentData(ZipDestinationEntry fileEntry, ulong contentLength)
        {
            const ulong BUFFER_LENGTH = 1024UL * 1024UL;

            if (contentLength < sizeof(uint) + sizeof(ulong))
                throw new ArgumentOutOfRangeException(nameof(contentLength));

            var crcHolder = new ValueHolder<(uint crc, ulong length)>();
            using var outStream1 = fileEntry.GetContentStream();
            var dataLength = checked(contentLength - (sizeof(uint) + sizeof(ulong)));
            outStream1.WriteUInt64LE(dataLength);
            using (var outStream2 = outStream1.WithCrc32Calculation(crcHolder, true))
            {
                var buffer = new byte[BUFFER_LENGTH];
                for (var index = 0; index < buffer.Length; ++index)
                    buffer[index] = unchecked((byte)index);
                var remain = dataLength;
                while (remain > 0)
                {
                    var length = checked((int)remain.Minimum(BUFFER_LENGTH));
                    outStream2.WriteBytes(buffer, 0, length);
                    remain -= checked((uint)length);
                }
            }

            outStream1.WriteUInt32LE(crcHolder.Value.crc);
        }

        private static void VerifyContentData(ZipSourceEntry entry)
        {
            if (entry.IsFile && entry.Size > 0)
            {
                try
                {
                    var crcHolder = new ValueHolder<(uint crc, ulong length)>();
                    using var inStream1 = entry.GetContentStream();
                    var contentLength = inStream1.ReadUInt64LE();
                    using (var inStream2 = inStream1.WithCrc32Calculation(crcHolder, true))
                    {
                        var buffer = new byte[1024 * 1024];
                        var count = 0UL;
                        while (count < contentLength)
                        {
                            var length = inStream2.ReadBytes(buffer.Slice(0, checked((int)(contentLength - count).Minimum((ulong)buffer.Length))));
                            if (length <= 0)
                                throw new Exception($"データが短すぎます。: 期待された長さ=0x{contentLength + sizeof(ulong) + sizeof(uint):x16}, 実際の長さ=0x{count + sizeof(ulong):x16}, entry={entry}");
                            checked
                            {
                                count += (ulong)length;
                            }
                        }
                    }

                    var crc = inStream1.ReadUInt32LE();
                    if (crc != crcHolder.Value.crc)
                        throw new Exception($"データの内容が一致しません。: entry={entry}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    try
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
