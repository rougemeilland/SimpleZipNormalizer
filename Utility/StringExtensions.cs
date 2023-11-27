using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility
{
    public static class StringExtensions
    {
        private static readonly Regex _localFilePathReplacePattern1;
        private static readonly Regex _localFilePathReplacePattern2;

        static StringExtensions()
        {
            // '?' を最低一つは含む、連続する '!' または '?' を検出する正規表現
            _localFilePathReplacePattern1 = new Regex(@"[!\?]*\?[!\?]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // Windows でファイル名に使用できない文字を検出する正規表現
            _localFilePathReplacePattern2 = new Regex(@"[:\*\?""<>\|]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        #region ChunkAsString

        public static IEnumerable<String> ChunkAsString(this IEnumerable<Char> source, Int32 count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var sb = new StringBuilder();
            foreach (var c in source)
            {
                _ = sb.Append(c);
                if (sb.Length >= count)
                {
                    yield return sb.ToString();
                    _ = sb.Clear();
                }
            }
        }

        #endregion

        #region Slice

        public static ReadOnlyMemory<Char> Slice(this String sourceString, Int32 offset)
            => sourceString is null
                ? throw new ArgumentNullException(nameof(sourceString))
                : !offset.IsBetween(0, sourceString.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : (ReadOnlyMemory<Char>)sourceString[offset..].ToCharArray();

        public static ReadOnlyMemory<Char> Slice(this String sourceString, UInt32 offset)
            => sourceString.Slice(checked((Int32)offset));

        public static ReadOnlyMemory<Char> Slice(this String sourceString, Range range)
        {
            if (sourceString is null)
                throw new ArgumentNullException(nameof(sourceString));
            var sourceArray = sourceString.ToCharArray();
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            return
                !isOk
                ? throw new ArgumentOutOfRangeException(nameof(range))
                : (ReadOnlyMemory<Char>)sourceString.Substring(offset, count).ToCharArray();
        }

        public static ReadOnlyMemory<Char> Slice(this String sourceString, Int32 offset, Int32 count)
            => sourceString is null
                ? throw new ArgumentNullException(nameof(sourceString))
                : offset < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : checked(count + offset) > sourceString.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceString)}.")
                : (ReadOnlyMemory<Char>)sourceString.Substring(offset, count).ToCharArray();

        public static ReadOnlyMemory<Char> Slice(this String sourceString, UInt32 offset, UInt32 count)
            => sourceString.Slice(checked((Int32)offset), checked((Int32)count));

        #endregion

        public static IEnumerable<FileInfo> EnumerateFilesFromArgument(this IEnumerable<String> args)
            => args is null
                ? throw new ArgumentNullException(nameof(args))
                : args
                .SelectMany(arg =>
                {
                    var file = TryParseAsFilePath(arg);
                    if (file is not null)
                        return new[] { file };
                    var directory = TryParseAsDirectoryPath(arg);
                    return
                        directory is not null
                        ? directory.EnumerateFiles("*", SearchOption.AllDirectories)
                        : Array.Empty<FileInfo>();
                });

        public static String? GetLeadingCommonPart(this String? s1, String? s2, Boolean ignoreCase = false)
        {
            if (s1 is null)
                return s2;
            if (s2 is null)
                return s1;
            if (s1.Length == 0 || s2.Length == 0)
                return "";
            if (s1.Length > s2.Length)
                (s2, s1) = (s1, s2);
#if DEBUG
            if (s1.Length > s2.Length)
                throw new Exception();
#endif
            var found =
                s1
                .Zip(s2, (c1, c2) => new { c1, c2 })
                .Select((item, index) => new { item.c1, item.c2, index })
                .FirstOrDefault(item => !CharacterEqual(item.c1, item.c2, ignoreCase));
            return found is not null ? s1[..found.index] : s1;
        }

        public static String? GetTrailingCommonPart(this String? s1, String? s2, Boolean ignoreCase = false)
        {
            if (s1 is null)
                return s2;
            if (s2 is null)
                return s1;
            if (s1.Length == 0 || s2.Length == 0)
                return "";
            if (s1.Length > s2.Length)
                (s2, s1) = (s1, s2);
#if DEBUG
            if (s1.Length > s2.Length)
                throw new Exception();
#endif
            var found =
                s1.Reverse()
                .Zip(s2.Reverse(), (c1, c2) => new { c1, c2 })
                .Select((item, index) => new { item.c1, item.c2, index })
                .FirstOrDefault(item => !CharacterEqual(item.c1, item.c2, ignoreCase));
            return found is not null ? s1.Substring(s1.Length - found.index, found.index) : s1;
        }

        #region IsNoneOf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf(this String s, String s1, String s2, StringComparison stringComparison = StringComparison.Ordinal)
            => !String.Equals(s, s1, stringComparison)
                && !String.Equals(s, s2, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf(this String s, String s1, String s2, String s3, StringComparison stringComparison = StringComparison.Ordinal)
            => !String.Equals(s, s1, stringComparison)
                && !String.Equals(s, s2, stringComparison)
                && !String.Equals(s, s3, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf(this String s, String s1, String s2, String s3, String s4, StringComparison stringComparison = StringComparison.Ordinal)
            => !String.Equals(s, s1, stringComparison)
                && !String.Equals(s, s2, stringComparison)
                && !String.Equals(s, s3, stringComparison)
                && !String.Equals(s, s4, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf(this String s, String s1, String s2, String s3, String s4, String s5, StringComparison stringComparison = StringComparison.Ordinal)
            => !String.Equals(s, s1, stringComparison)
                && !String.Equals(s, s2, stringComparison)
                && !String.Equals(s, s3, stringComparison)
                && !String.Equals(s, s4, stringComparison)
                && !String.Equals(s, s5, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf(this String s, String s1, String s2, String s3, String s4, String s5, String s6, StringComparison stringComparison = StringComparison.Ordinal)
            => !String.Equals(s, s1, stringComparison)
                && !String.Equals(s, s2, stringComparison)
                && !String.Equals(s, s3, stringComparison)
                && !String.Equals(s, s4, stringComparison)
                && !String.Equals(s, s5, stringComparison)
                && !String.Equals(s, s6, stringComparison);

        #endregion

        #region IsAnyOf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf(this String s, String s1, String s2, StringComparison stringComparison = StringComparison.Ordinal)
            => String.Equals(s, s1, stringComparison)
                || String.Equals(s, s2, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf(this String s, String s1, String s2, String s3, StringComparison stringComparison = StringComparison.Ordinal)
            => String.Equals(s, s1, stringComparison)
                || String.Equals(s, s2, stringComparison)
                || String.Equals(s, s3, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf(this String s, String s1, String s2, String s3, String s4, StringComparison stringComparison = StringComparison.Ordinal)
            => String.Equals(s, s1, stringComparison)
                || String.Equals(s, s2, stringComparison)
                || String.Equals(s, s3, stringComparison)
                || String.Equals(s, s4, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf(this String s, String s1, String s2, String s3, String s4, String s5, StringComparison stringComparison = StringComparison.Ordinal)
            => String.Equals(s, s1, stringComparison)
                || String.Equals(s, s2, stringComparison)
                || String.Equals(s, s3, stringComparison)
                || String.Equals(s, s4, stringComparison)
                || String.Equals(s, s5, stringComparison);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf(this String s, String s1, String s2, String s3, String s4, String s5, String s6, StringComparison stringComparison = StringComparison.Ordinal)
            => String.Equals(s, s1, stringComparison)
                || String.Equals(s, s2, stringComparison)
                || String.Equals(s, s3, stringComparison)
                || String.Equals(s, s4, stringComparison)
                || String.Equals(s, s5, stringComparison)
                || String.Equals(s, s6, stringComparison);

        #endregion

        public static String GetString(this Encoding encoding, ReadOnlyMemory<Byte> bytes)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            return encoding.GetString(bytes.Span);
        }

        public static ReadOnlyMemory<Byte> GetReadOnlyBytes(this Encoding encoding, String s)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return encoding.GetBytes(s).AsReadOnly();
        }

        public static String GetRelativeLocalFilePath(this String entryPath)
            => String.Join(
                Path.DirectorySeparatorChar,
                entryPath.Split('/', '\\')
                .Select(element => element.ReplaceConsecutiveExclamationAndQuestionMarks().ReplaceIllegalCharacters()));

        private static String ReplaceIllegalCharacters(this String input)
            => _localFilePathReplacePattern2.Replace(
                input,
                m =>
                    m.Value switch
                    {
                        @":" => "：",
                        @"*" => "＊",
                        @"?" => "？",
                        @"""" => "”",
                        @"<" => "＜",
                        @">" => "＞",
                        @"|" => "｜",
                        _ => m.Value,
                    });
        public static String ReplaceConsecutiveExclamationAndQuestionMarks(this String element)
            => _localFilePathReplacePattern1.Replace(
                element,
                m =>
                    String.Concat(
                        m.Value
                        .Select(c =>
                            c switch
                            {
                                '!' => '！',
                                '?' => '？',
                                _ => c,
                            })));

        private static FileInfo? TryParseAsFilePath(String path)
        {
            try
            {
                var file = new FileInfo(path);
                return file.Exists ? file : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DirectoryInfo? TryParseAsDirectoryPath(String path)
        {
            try
            {
                var directory = new DirectoryInfo(path);
                return directory.Exists ? directory : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean CharacterEqual(Char c1, Char c2, Boolean ignoreCase)
            => ignoreCase ?
                Char.ToUpperInvariant(c1) == Char.ToUpperInvariant(c2)
                : c1 == c2;
    }
}
