using System;

namespace ZipUtility
{
    /// <summary>
    /// ZIP ファイルのフォーマットに誤りを検出したことを示す例外オブジェクトです。
    /// </summary>
    public class BadZipFileFormatException
        : Exception
    {
        internal BadZipFileFormatException()
            : base("ZIPファイルのフォーマットに誤りを見つけました。")
        {
        }

        internal BadZipFileFormatException(String message)
            : base(message)
        {
        }

        internal BadZipFileFormatException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
