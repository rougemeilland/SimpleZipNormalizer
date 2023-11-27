using System;

namespace ZipUtility
{
    /// <summary>
    /// エントリのデータの圧縮方式です。
    /// </summary>
    public enum ZipEntryCompressionMethodId
        : UInt16
    {
        /// <summary>
        /// 非圧縮です。
        /// </summary>
        Stored = 0,

        /// <summary>
        /// Deflate 方式です。
        /// </summary>
        Deflate = 8,

        /// <summary>
        /// Deflate64 方式です。
        /// </summary>
        Deflate64 = 9,

        /// <summary>
        /// BZIP2 方式です。
        /// </summary>
        BZIP2 = 12,

        /// <summary>
        /// LZMA 方式です。
        /// </summary>
        LZMA = 14,

        /// <summary>
        /// PPMd 方式 (version I, Rev 1) です。
        /// </summary>
        PPMd = 98,

        /// <summary>
        /// 不明あるいはサポートされていない圧縮方式です。
        /// </summary>
        Unknown = 0xffff,
    }
}
