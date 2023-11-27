using System;

namespace ZipUtility
{
    /// <summary>
    /// サポートされていない仕様が要求されたことを示す例外オブジェクトです。
    /// </summary>
    public class NotSupportedSpecificationException
        : Exception
    {
        internal NotSupportedSpecificationException()
            : base("Not supported function required to access the ZIP file.")
        {
        }

        internal NotSupportedSpecificationException(String message)
            : base(message)
        {
        }

        internal NotSupportedSpecificationException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
