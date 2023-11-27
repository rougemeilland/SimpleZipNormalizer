using System;
using System.Collections.Generic;
using System.Text;

namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブエントリのエンコーディング方法を提供します。
    /// </summary>
    public interface IZipEntryNameEncodingProvider
    {
        /// <summary>
        /// 与えられたエントリ名とコメントのバイト列が、与えられたエントリ名とコメントの文字列にデコードされるために最適なエンコーディングを推測します。
        /// </summary>
        /// <param name="entryFullNameBytes">
        /// エントリ名のバイト列です。
        /// </param>
        /// <param name="entryFullName">
        /// エントリ名の文字列または null です。
        /// </param>
        /// <param name="entryCommentBytes">
        /// コメントのバイト列です。
        /// </param>
        /// <param name="entryComment">
        /// コメントの文字列または null です。
        /// </param>
        /// <returns>
        /// 推測された最適なエンコーディングのコレクションです。このコレクションはより適した順番に並んでいます。また、最適なエンコーディングがひとつも見つからなかった場合は空のコレクションが返ります。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>必ずしも正確なエンコーディングが推測されるとは限らないことに注意してください。
        /// 特に、<paramref name="entryFullName"/> または <paramref name="entryComment"/> が null または空文字である場合は推測の精度が低下します。</item>
        /// <item>
        /// より適したエンコーディングの判断は、以下の優先順位で行われます。
        /// <list type="number">
        /// <item><paramref name="entryFullNameBytes"/> および <paramref name="entryCommentBytes"/> からデコードした文字列をエンコードしたバイト列が、それぞれ <paramref name="entryFullName"/> および <paramref name="entryComment"/> と一致していること。</item>
        /// <item><paramref name="entryFullNameBytes"/> および <paramref name="entryCommentBytes"/> からデコードした文字列が、どれだけ <paramref name="entryFullName"/> および <paramref name="entryComment"/> に似ているか。</item>
        /// </list>
        /// </item>
        /// </list>
        /// </remarks>
        IEnumerable<Encoding> GetBestEncodings(ReadOnlyMemory<Byte> entryFullNameBytes, String? entryFullName, ReadOnlyMemory<Byte> entryCommentBytes, String? entryComment);

        /// <summary>
        /// このインスタンスでサポートされているエンコーディングのコレクションを返します。
        /// </summary>
        IEnumerable<Encoding> SupportedEncodings { get; }
    }
}
