using System;
using System.IO;
using System.Security.AccessControl;
using ZipUtility;
using ZipUtility.ZipExtraField;

namespace Experiment01
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if true

            using var reader = new FileInfo(args[0]).OpenAsZipFile(ZipEntryNameEncodingProvider.Create(Array.Empty<string>(), Array.Empty<string>()));
            Console.WriteLine($"zip-comment:{reader.Comment}");
            foreach (var entry in reader.GetEntries())
            {
                Console.WriteLine("-----");
                {
                    Console.WriteLine($"{entry.FullName}: utf8 bit={(entry.EntryTextEncoding == ZipEntryTextEncoding.UTF8 ? "ON" : "OFF")}");
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<XceedUnicodeExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(local): 0x554e_name=\"{extraField.FullName}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<XceedUnicodeExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(central): 0x554e_name=\"{extraField.FullName}\", 0x554e_comment=\"{extraField.Comment ?? ""}\"");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<NtfsExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(local): 0x000a_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x000a_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x000a_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<NtfsExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(central): 0x000a_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x000a_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x000a_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<ExtendedTimestampExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(local): 0x5455_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x5455_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x5455_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<ExtendedTimestampExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(central): 0x5455_m={extraField.LastWriteTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<UnicodePathExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(lcoal): 0x7075_name=\"{extraField.GetFullName(entry.FullNameBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<UnicodePathExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(central): 0x7075_name=\"{extraField.GetFullName(entry.FullNameBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<UnicodeCommentExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(lcoal): 0x6375_comment=\"{extraField.GetComment(entry.CommentBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<UnicodeCommentExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{entry.FullName}-(central): 0x6375_comment=\"{extraField.GetComment(entry.CommentBytes.Span)}\"");
                    }
                }
            }
#else
            var sourceString = new string('あ', 1000);
            var uncompressedData = Encoding.UTF8.GetBytes(sourceString);
            var r = Compress(ZipEntryCompressionMethodId.Deflate, uncompressedData);
            if (r is null)
                throw new Exception();
            var uncompressedData2 = Decompress(r.Value.compressionMethodId, r.Value.compressedData);
            if (uncompressedData2 is null)
                throw new Exception();
            if (!uncompressedData.SequenceEqual(uncompressedData))
                throw new Exception();
            var s = Encoding.UTF8.GetString(uncompressedData2.Value.Span);
            if (s != sourceString)
                throw new Exception();
            Console.WriteLine(s);
#endif
        }
    }
}
