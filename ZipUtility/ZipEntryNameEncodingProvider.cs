using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility;
using Utility.Linq;
using Utility.Text;

namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブエントリのエンコーディング方法を提供するクラスです。
    /// </summary>
    public class ZipEntryNameEncodingProvider
    {
        private class EncodingProvider
            : IZipEntryNameEncodingProvider
        {
            private static readonly IDictionary<Int32, Int32> _codePagePriority;
            private readonly IEnumerable<Encoding> _sourceEncodings;
            private readonly String _alternativeText;

            static EncodingProvider()
            {
                var unicodeCodePages =
                    Encoding.GetEncodings()
                    .Where(info => info.Name.IsAnyOf("utf-7", "utf-8", "utf-16", "utf-16BE", "utf-32", "utf-32BE", "unicodeFFFE"))
                    .Select(info => info.CodePage)
                    .ToArray();

                _codePagePriority =
                    Encoding.GetEncodings()
                    .Select(info => info.GetEncoding().WithFallback(null, null).WithoutPreamble())
                    .OrderBy(encoding =>
                        String.Equals(encoding.WebName, "us-ascii", StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : unicodeCodePages.Contains(encoding.CodePage)
                        ? 3
                        : encoding.CodePage.IsAnyOf(Console.InputEncoding.CodePage, Console.OutputEncoding.CodePage)
                        ? 1
                        : 2)
                    .ThenByDescending(encoding => encoding.GetMaxByteCount(1))
                    .Select((encoding, index) => new { index, encoding })
                    .ToDictionary(item => item.encoding.CodePage, item => item.index);
            }

            public EncodingProvider(IEnumerable<Encoding> supportedEncodings, String alternativeText)
            {
                _sourceEncodings = (supportedEncodings ?? throw new ArgumentNullException(nameof(supportedEncodings))).ToList();
                _alternativeText = alternativeText ?? throw new ArgumentNullException(nameof(alternativeText));
            }

            IEnumerable<Encoding> IZipEntryNameEncodingProvider.SupportedEncodings
                => _sourceEncodings
                    .Select(encoding =>
                        String.IsNullOrEmpty(_alternativeText)
                        ? encoding.WithoutPreamble()
                        : encoding.WithFallback(_alternativeText, _alternativeText).WithoutPreamble());

            IEnumerable<Encoding> IZipEntryNameEncodingProvider.GetBestEncodings(
                ReadOnlyMemory<Byte> entryFullNameBytes,
                String? entryFullName,
                ReadOnlyMemory<Byte> entryCommentBytes,
                String? entryComment)
            {
                var enumerable =
                    _sourceEncodings
                    .Select(encoding =>
                    {
                        try
                        {
                            return new
                            {
                                encoding,
                                reencodedEntryFullNameBytes = encoding.GetReadOnlyBytes(encoding.GetString(entryFullNameBytes)),
                                encodedEntryFullNameBytes = entryFullName is not null ? encoding.GetReadOnlyBytes(entryFullName) : (ReadOnlyMemory<Byte>?)null,
                                decodedEntryFullName = encoding.GetString(entryFullNameBytes),
                                reencodedEntryCommentBytes = encoding.GetReadOnlyBytes(encoding.GetString(entryCommentBytes)),
                                encodedEntryCommentBytes = entryComment is not null ? encoding.GetReadOnlyBytes(entryComment) : (ReadOnlyMemory<Byte>?)null,
                                decodedEntryComment = encoding.GetString(entryCommentBytes),
                                score = 0,
                            };
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    })
                    .WhereNotNull()
                    .Where(item =>
                        entryFullNameBytes.Span.SequenceEqual(item.reencodedEntryFullNameBytes.Span)
                        && entryCommentBytes.Span.SequenceEqual(item.reencodedEntryCommentBytes.Span))
                    .ToList();

                if (!String.IsNullOrEmpty(entryFullName) || !String.IsNullOrEmpty(entryComment))
                {
                    // entryFullName または entryComment の何れかが指定された場合

                    // 与えられた文字列とバイト列が完全に一対一でマッピングできるエンコーディングを探す
                    var matchedEncodings =
                        enumerable
                        .Where(item =>
                            (entryFullName is null || item.decodedEntryFullName == entryFullName)
                            && (entryComment is null || item.decodedEntryComment == entryComment)
                            && (item.encodedEntryFullNameBytes is null || item.encodedEntryFullNameBytes.Value.Span.SequenceEqual(entryFullNameBytes.Span))
                            && (item.encodedEntryCommentBytes is null || item.encodedEntryCommentBytes.Value.Span.SequenceEqual(entryCommentBytes.Span)))
                        .OrderBy(item => _codePagePriority[item.encoding.CodePage])
                        .ToList();

                    if (matchedEncodings.Any())
                    {
                        return
                            matchedEncodings
                            .Select(item =>
                                String.IsNullOrEmpty(_alternativeText)
                                ? item.encoding.WithoutPreamble()
                                : item.encoding.WithFallback(_alternativeText, _alternativeText).WithoutPreamble())
                            .ToList();
                    }
                }

                // 完全に一致するエンコーディングが見つからなかった場合

                // 一致した文字数によるスコアづけによって優先順位を決定する
                return
                    enumerable
                    // 
                    .Select(item =>
                    {
                        var score = 0;
                        if (entryFullName is not null)
                            score += CalculateMatchCount(entryFullName, item.decodedEntryFullName);
                        if (entryComment is not null)
                            score += CalculateMatchCount(entryComment, item.decodedEntryComment);
                        return new { item.encoding, score };
                    })
                    .WhereNotNull()
                    .OrderByDescending(item => item.score)
                    .ThenBy(item => _codePagePriority[item.encoding.CodePage])
                    .Select(item =>
                        String.IsNullOrEmpty(_alternativeText)
                        ? item.encoding.WithoutPreamble()
                        : item.encoding.WithFallback(_alternativeText, _alternativeText).WithoutPreamble())
                    .ToList();

                static Int32 CalculateMatchCount(String s1, String s2)
                {
                    var count = 0;
                    var s2List = s2.ToList();
                    foreach (var c in s1)
                    {
                        if (s2List.Remove(c))
                            ++count;
                    }

                    return count;
                }
            }
        }

        private static readonly IEnumerable<Encoding> _allEncodings;

        static ZipEntryNameEncodingProvider()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _allEncodings =
                Encoding.GetEncodings()
                .Select(info => info.GetEncoding().WithFallback("--found bad character at encoding--", "--found bad character at decoding--").WithoutPreamble())
                .ToArray();
        }

        /// <summary>
        /// <see cref="IZipEntryNameEncodingProvider"/> を実装するオブジェクトを作成します。
        /// </summary>
        /// <param name="allowedEncodingNames">
        /// ZIPエントリのエンコーディングの候補としたいコードページの名前のコレクションです。
        /// 空のコレクションを与えた場合、実装されているすべてのエンコーディングが候補になります。
        /// </param>
        /// <param name="excludedEncodingNames">
        /// ZIPエントリのエンコーディングの候補から除外したいコードページの名前のコレクションです。
        /// 空のコレクションを与えた場合、どのエンコーディングも除外されません。
        /// </param>
        /// <param name="alternativeText">
        /// エンコードあるいはデコードの際に変換できない文字を見つけた場合に使用される代替文字列です。省略時の値はエンコーディング依存です。
        /// </param>
        /// <returns>
        /// 作成された <see cref="IZipEntryNameEncodingProvider"/> をサポートするオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 以下の引数の何れかが null です。
        /// <list type="bullet">
        /// <item><paramref name="allowedEncodingNames"/></item>
        /// <item><paramref name="excludedEncodingNames"/></item>
        /// <item><paramref name="alternativeText"/></item>
        /// </list>
        /// </exception>
        /// <example>
        /// <code>
        /// var provider = ZipEntryNameEncodingProvider.Create(new[] { "shift_jis, "utf-8"}, new string[0]);
        /// </code>
        /// <code>
        /// var provider = ZipEntryNameEncodingProvider.Create(new string[0], new string[] { "IBM437" });
        /// </code>
        /// </example>
        public static IZipEntryNameEncodingProvider Create(IEnumerable<String> allowedEncodingNames, IEnumerable<String> excludedEncodingNames, String alternativeText = "")
        {
            if (allowedEncodingNames is null)
                throw new ArgumentNullException(nameof(allowedEncodingNames));
            if (excludedEncodingNames is null)
                throw new ArgumentNullException(nameof(excludedEncodingNames));
            if (alternativeText is null)
                throw new ArgumentNullException(nameof(alternativeText));

            var allowedEncodingNameArray = allowedEncodingNames.ToArray().AsReadOnlyMemory();
            var excludedEncodingNamesArray = excludedEncodingNames.ToArray().AsReadOnlyMemory();

            var encodings = _allEncodings;
            if (allowedEncodingNameArray.Length > 0)
                encodings = encodings.Where(encoding => allowedEncodingNameArray.Span.IndexOf(encoding.WebName) >= 0);
            if (excludedEncodingNamesArray.Length > 0)
                encodings = encodings.Where(encoding => excludedEncodingNamesArray.Span.IndexOf(encoding.WebName) >= 0);

            return new EncodingProvider(encodings, alternativeText);
        }
    }
}
