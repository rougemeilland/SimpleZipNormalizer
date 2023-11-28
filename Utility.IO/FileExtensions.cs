using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility.IO
{
    public static class FileExtensions
    {
        private static readonly Object _lockObject;
        private static readonly Regex _simpleFileNamePattern;

        static FileExtensions()
        {
            _lockObject = new Object();
            _simpleFileNamePattern = new Regex(@"^(?<path>.*?)(\s*\([0-9]+\))+$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        public static FileInfo GetFile(this DirectoryInfo directory, String fileName)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException($"'{nameof(fileName)}' を NULL または空にすることはできません。", nameof(fileName));

            return new FileInfo(Path.Combine(directory.FullName, fileName));

        }

        public static DirectoryInfo GetSubDirectory(this DirectoryInfo directory, String directoryName)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentException($"'{nameof(directoryName)}' を NULL または空にすることはできません。", nameof(directoryName));

            return new DirectoryInfo(Path.Combine(directory.FullName, directoryName));
        }

        public static Byte[] ReadAllBytes(this FileInfo file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadAllBytes(file.FullName);
        }

        public static String[] ReadAllLines(this FileInfo file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadAllLines(file.FullName);
        }

        public static IEnumerable<String> ReadLines(this FileInfo file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadLines(file.FullName);
        }

        public static void WriteAllBytes(this FileInfo file, IEnumerable<Byte> data)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            using var stream = file.OpenWrite();
            stream.WriteByteSequence(data);
        }

        public static void WriteAllBytes(this FileInfo file, Byte[] data)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            File.WriteAllBytes(file.FullName, data);
        }

        public static void WriteAllBytes(this FileInfo file, ReadOnlyMemory<Byte> data)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenWrite();
            stream.WriteBytes(data.Span);
        }

        public static void WriteAllBytes(this FileInfo file, ReadOnlySpan<Byte> data)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenWrite();
            stream.WriteBytes(data);
        }

        public static void WriteAllText(this FileInfo file, String text)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (String.IsNullOrEmpty(text))
                throw new ArgumentException($"'{nameof(text)}' を NULL または空にすることはできません。", nameof(text));

            File.WriteAllText(file.FullName, text);
        }

        public static void WriteAllText(this FileInfo file, String text, Encoding encoding)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (String.IsNullOrEmpty(text))
                throw new ArgumentException($"'{nameof(text)}' を NULL または空にすることはできません。", nameof(text));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            File.WriteAllText(file.FullName, text, encoding);
        }

        public static void WriteAllText(this FileInfo file, String[] lines)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            File.WriteAllLines(file.FullName, lines);
        }

        public static void WriteAllText(this FileInfo file, IEnumerable<String> lines)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            File.WriteAllLines(file.FullName, lines);
        }

        public static void WriteAllText(this FileInfo file, String[] lines, Encoding encoding)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            File.WriteAllLines(file.FullName, lines, encoding);
        }

        public static void WriteAllText(this FileInfo file, IEnumerable<String> lines, Encoding encoding)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            File.WriteAllLines(file.FullName, lines, encoding);
        }

        public static (FileInfo File, Boolean AlreadyExists) RenameFile(this FileInfo sourceFile, String newFileName)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));
            if (String.IsNullOrEmpty(newFileName))
                throw new ArgumentException($"'{nameof(newFileName)}' を NULL または空にすることはできません。", nameof(newFileName));

            var sourceFileDirectory = sourceFile.Directory ?? throw new ArgumentException($"{nameof(sourceFile)} is the relative path.", nameof(sourceFile));
            var sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFileName);
            var fileNameMatch = _simpleFileNamePattern.Match(sourceFileNameWithoutExtension);
            if (fileNameMatch.Success)
                sourceFileNameWithoutExtension = fileNameMatch.Groups["path"].Value;
            var sourceFileExtension = Path.GetExtension(newFileName);
            lock (_lockObject)
            {
                var retryCount = 1;
                while (true)
                {
                    var newFile =
                        new FileInfo(
                            Path.Combine(
                                sourceFileDirectory.FullName,
                                $"{sourceFileNameWithoutExtension}{(retryCount <= 1 ? "" : $" ({retryCount})")}{sourceFileExtension}"));
                    if (String.Equals(newFile.FullName, sourceFile.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return (newFile, false);
                    }
                    else if (!newFile.Exists)
                    {
                        File.Move(sourceFile.FullName, newFile.FullName);
                        return (newFile, false);
                    }
                    else if (newFile.Length == sourceFile.Length &&
                            newFile.OpenRead().StreamBytesEqual(sourceFile.OpenRead()))
                    {
                        sourceFile.Delete();
                        return (newFile, true);
                    }
                    else
                    {
                        ++retryCount;
                    }
                }
            }
        }

        public static void SafetyDelete(this FileInfo file)
        {
            try
            {
                if (File.Exists(file.FullName))
                    File.Delete(file.FullName);
            }
            catch (Exception)
            {
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this FileInfo sourceFile, IProgress<UInt64>? progress = null)
            => sourceFile is null
                ? throw new ArgumentNullException(nameof(sourceFile))
                : sourceFile.OpenRead().CalculateCrc24(progress);

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this FileInfo sourceFile, IProgress<UInt64>? progress = null)
            => sourceFile is null
                ? throw new ArgumentNullException(nameof(sourceFile))
                : sourceFile.OpenRead().CalculateCrc32(progress);
    }
}
