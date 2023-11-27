using System;
using System.IO;
using Utility;
using ZipUtility;
using ZipUtility.ZipExtraField;

namespace Experiment.Xceed.CSharp002
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                PrintZipDetail(arg);
                Console.WriteLine();
                Console.WriteLine("-----------------------------");
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Completed.");
            _ = Console.ReadLine();
        }

        private static void PrintZipDetail(string zipFilePath)
        {
            var zipfile = new FileInfo(zipFilePath);
            using var zipReader = zipfile.OpenAsZipFile(ZipEntryNameEncodingProvider.Create(Array.Empty<string>(), Array.Empty<string>()));
            Console.WriteLine($"{zipfile.Name}: zip-comment={zipReader.Comment}");
            foreach (var entry in zipReader.GetEntries())
            {
                {
                    Console.WriteLine($"{entry.FullName}: utf8 bit={(entry.EntryTextEncoding == ZipEntryTextEncoding.UTF8 ? "ON" : "OFF")}");
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<XceedUnicodeExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (local): 0x554e_name=\"{extraField.FullName}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<XceedUnicodeExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (central): 0x554e_name=\"{extraField.FullName}\", 0x554e_comment=\"{extraField.Comment ?? ""}\"");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<NtfsExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (local): 0x000a_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x000a_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x000a_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<NtfsExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (central): 0x000a_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x000a_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x000a_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<ExtendedTimestampExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (local): 0x5455_m={extraField.LastWriteTimeUtc?.ToLocalTime()}, 0x5455_a={extraField.LastAccessTimeUtc?.ToLocalTime()}, 0x5455_c={extraField.CreationTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<ExtendedTimestampExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (central): 0x5455_m={extraField.LastWriteTimeUtc?.ToLocalTime()}");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<UnicodePathExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (local): 0x7075_name=\"{extraField.GetFullName(entry.FullNameBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<UnicodePathExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (central): 0x7075_name=\"{extraField.GetFullName(entry.FullNameBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.LocalHeaderExtraFields.GetExtraField<UnicodeCommentExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (local): 0x6375_comment=\"{extraField.GetComment(entry.CommentBytes.Span)}\"");
                    }
                }

                {
                    var extraField = entry.CentralDirectoryHeaderExtraFields.GetExtraField<UnicodeCommentExtraField>();
                    if (extraField is not null)
                    {
                        Console.WriteLine($"{zipfile.Name}/{entry.FullName} (central): 0x6375_comment=\"{extraField.GetComment(entry.CommentBytes.Span)}\"");
                    }
                }
            }
        }
    }
}
