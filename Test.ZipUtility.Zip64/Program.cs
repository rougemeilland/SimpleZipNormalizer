using System;
using System.Linq;
using Utility;
using Utility.Collections;
using Utility.IO;
using ZipUtility;

namespace Test.ZipUtility.Zip64
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var baseDirectory = new DirectoryPath(args[0]);

#if true
            // ボリューム数 = UInt16.MaxValue - 3 / UInt16.MaxValue - 2 / UInt16.MaxValue -1 / UInt16.MaxValue / UInt16.MaxValue + 1
            foreach (var value in new[] { ushort.MaxValue - 2U, ushort.MaxValue - 1U, ushort.MaxValue, ushort.MaxValue + 1U, ushort.MaxValue + 2U, ushort.MaxValue + 3U })
            {
                const ulong VOLUME_SIZE = 1024;

                var targetDirectory = baseDirectory.GetSubDirectory($"total volume number=0x{value:x8}");
                targetDirectory.Create();
                DoTest1(targetDirectory, $"total volume number=0x{value:x8}.zip", VOLUME_SIZE, ZipWriteFlags.None, 2, ((ulong)value - 1) * VOLUME_SIZE / 2, _ => 0, false);
            }
#endif

#if true
            // 最初のセントラルディレクトリヘッダのボリューム番号 = UInt16.MaxValue -1 / UInt16.MaxValue / UInt16.MaxValue + 1
            foreach (var value in new[] { ushort.MaxValue - 1U, ushort.MaxValue, ushort.MaxValue + 1U })
            {
                const ulong VOLUME_SIZE = 1024;

                var targetDirectory = baseDirectory.GetSubDirectory($"volume number of first central directory header=0x{value:x8}");
                targetDirectory.Create();
                DoTest1(targetDirectory, $"volume number of first central directory header=0x{value:x8}.zip", VOLUME_SIZE, ZipWriteFlags.None, 2, (ulong)value * VOLUME_SIZE / 2, _ => 512, false);
            }
#endif

#if true
            // 最後のボリューム内のセントラルディレクトリヘッダの数 = UInt16.MaxValue -1 / UInt16.MaxValue / UInt16.MaxValue + 1
            foreach (var value in new[] { ushort.MaxValue - 1U, ushort.MaxValue, ushort.MaxValue + 1U })
            {
                var VOLUME_SIZE = checked(15717828UL - 108UL * value);

                var targetDirectory = baseDirectory.GetSubDirectory($"number of central directories on last volume=0x{value:x8}");
                targetDirectory.Create();
                DoTest1(targetDirectory, $"number of central directories on last volume=0x{value:x8}.zip", VOLUME_SIZE, ZipWriteFlags.None, ushort.MaxValue + 2, 64, _ => 0, false);
            }
#endif

#if true
            // セントラルディレクトリヘッダの合計数 = UInt16.MaxValue -1 / UInt16.MaxValue / UInt16.MaxValue + 1
            foreach (var value in new[] { ushort.MaxValue - 1U, ushort.MaxValue, ushort.MaxValue + 1U })
            {
                const ulong VOLUME_SIZE = 1024;

                var targetDirectory = baseDirectory.GetSubDirectory($"toatl number of central directories=0x{value:x8}");
                targetDirectory.Create();
                DoTest1(targetDirectory, $"toatl number of central directories=0x{value:x8}.zip", VOLUME_SIZE, ZipWriteFlags.None, value, 16, _ => 0, false);
            }
#endif

#if true
            // セントラルディレクトリヘッダの合計サイズ = UInt32.MaxValue -1 / UInt32.MaxValue / UInt32.MaxValue + 1
            foreach (var value in new[] { uint.MaxValue - 1UL, uint.MaxValue, uint.MaxValue + 1UL })
            {
                const ulong VOLUME_SIZE = 512 * 1024 * 1024;
                const int ENTRIES = ushort.MaxValue - 109;

                var targetDirectory = baseDirectory.GetSubDirectory($"total size of all central directories=0x{value:x16}");
                targetDirectory.Create();
                var commentLengthOfLastEntry = checked((ushort)(value - 0xffff5aa2 - 0x4a4b));
                DoTest1(targetDirectory, $"total size of all central directories=0x{value:x16}.zip", VOLUME_SIZE, ZipWriteFlags.None, ENTRIES, 16, index => index < ENTRIES - 1 ? ushort.MaxValue : commentLengthOfLastEntry, false);
            }
#endif

#if true
            // 最初のセントラルディレクトリヘッダのオフセット = UInt32.MaxValue -1 / UInt32.MaxValue / UInt32.MaxValue + 1
            foreach (var value in new[] { uint.MaxValue - 1UL, uint.MaxValue, uint.MaxValue + 1UL })
            {
                const ulong VOLUME_SIZE = ulong.MaxValue;

                var targetDirectory = baseDirectory.GetSubDirectory($"first central directory offset=0x{value:x16}");
                targetDirectory.Create();
                DoTest1(targetDirectory, $"first central directory offset=0x{value:x16}.zip", VOLUME_SIZE, ZipWriteFlags.None, 1, value - 0x40UL, _ => 0, false);
            }
#endif

            Console.WriteLine("終了しました。");
            Console.Beep();
            _ = Console.ReadLine();
        }

        private static void DoTest1(DirectoryPath baseDirectory, string fileName, ulong volumeSize, ZipWriteFlags flag, ulong numberOfEntries, ulong contentSize, Func<ulong, ushort> commentSizeGetter, bool useDatadescriptor)
        {
            var zipArchive = baseDirectory.GetFile(fileName);
            using (var zipWriter = zipArchive.CreateAsZipFile(volumeSize))
            {
                zipWriter.Flags = flag;
                var step = (numberOfEntries / 100).Maximum(1UL);
                for (var count = 0UL; count < numberOfEntries; ++count)
                {
                    if (count % step == 0 || count == numberOfEntries - 1)
                        Console.WriteLine($"書き込み中 {count + 1}/{numberOfEntries}... \"{zipArchive.FullName}\"");
                    var file = zipWriter.CreateEntry($"ファイル{count + 1}.bin", new string(RandomSequence.GetAsciiCharSequence().Take(commentSizeGetter(count)).ToArray()));
                    file.IsFile = true;
                    file.CreationTimeUtc = DateTime.Now;
                    file.LastAccessTimeUtc = DateTime.Now;
                    file.LastWriteTimeUtc = DateTime.Now;
                    file.CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                    file.UseDataDescriptor = useDatadescriptor;
                    WriteContentData(file, contentSize);
                }

                Console.WriteLine($"書き込み中... \"{zipArchive.FullName}\"");
            }

            try
            {
                using (var reader = zipArchive.OpenAsZipFile(ValidationStringency.Strict))
                {
                    Console.WriteLine($"検査中... \"{zipArchive.FullName}\"");
                    var entries = reader.EnumerateEntries();
                    var step = checked((ulong)(numberOfEntries / 100).Maximum(1UL));
                    var count = 0UL;
                    foreach (var entry in entries)
                    {
                        if (count % step == 0 || count == numberOfEntries - 1)
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
