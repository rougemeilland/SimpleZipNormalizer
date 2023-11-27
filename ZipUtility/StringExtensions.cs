using System;
using Utility;

namespace ZipUtility
{
    /// <summary>
    /// <see cref="String"/> 型の拡張メソッドのクラスです。
    /// </summary>
    public static class StringExtensions
    {
        private const String _headerForUnknownEncoding = "##unknown encoding-";

        /// <summary>
        /// エンコード方法が不明な与えられたバイト列を、識別可能な文字列に変換します。
        /// </summary>
        /// <param name="fullNameBytes"></param>
        /// <returns></returns>
        public static String GetStringByUnknownDecoding(this ReadOnlyMemory<Byte> fullNameBytes)
            => $"{_headerForUnknownEncoding}{fullNameBytes.ToFriendlyString()}";

        /// <summary>
        /// 与えられた文字列が、エンコード方法が不明であるとして変換された文字列であるかどうかを調べます。
        /// </summary>
        /// <param name="text">
        /// 調べる対象の文字列です。
        /// </param>
        /// <returns>
        /// <paramref name="text"/> で与えられた文字列が <see cref="GetStringByUnknownDecoding(ReadOnlyMemory{Byte})"/> で作成された文字列であれば true、そうではないのなら false です。
        /// </returns>
        public static Boolean IsUnknownEncodingText(this String text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            return text.StartsWith(_headerForUnknownEncoding, StringComparison.Ordinal);
        }
    }
}
