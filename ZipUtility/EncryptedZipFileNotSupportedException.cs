using System;

namespace ZipUtility
{
    /// <summary>
    /// 暗号化された ZIP ファイルをサポートしていないことを示す例外オブジェクトです。
    /// </summary>
    public class EncryptedZipFileNotSupportedException
        : NotSupportedSpecificationException
    {
        internal EncryptedZipFileNotSupportedException(String required)
            : base($"Encrypted ZIP file is not supported.: required-function=\"{required}\"")
        {
            Required = required;
        }

        internal EncryptedZipFileNotSupportedException(String message, String required)
            : base(message)
        {
            Required = required;
        }

        internal EncryptedZipFileNotSupportedException(String message, Exception inner, String required)
            : base(message, inner)
        {
            Required = required;
        }

        /// <summary>
        /// 必要とされている暗号化の種類を示す文字列です。
        /// </summary>
        public String Required { get; }
    }
}
