using System;

namespace ZipUtility
{
    /// <summary>
    /// 圧縮方式が未サポートであることを示す例外のオブジェクトです。
    /// </summary>
    public class CompressionMethodNotSupportedException
        : NotSupportedSpecificationException
    {
        internal CompressionMethodNotSupportedException(ZipEntryCompressionMethodId compresssionMethodId)
            : base($"Compression method '{compresssionMethodId}' is not supported.")
        {
            CompresssionMethodId = compresssionMethodId;
        }

        internal CompressionMethodNotSupportedException(String message, ZipEntryCompressionMethodId compresssionMethodId)
            : base(message)
        {
            CompresssionMethodId = compresssionMethodId;
        }

        internal CompressionMethodNotSupportedException(String message, Exception inner, ZipEntryCompressionMethodId compresssionMethodId)
            : base(message, inner)
        {
            CompresssionMethodId = compresssionMethodId;
        }

        /// <summary>
        /// サポートされていない圧縮方式を示す値です。
        /// </summary>
        public ZipEntryCompressionMethodId CompresssionMethodId { get; }
    }
}
