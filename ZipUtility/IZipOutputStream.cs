using System;
using Utility.IO;

namespace ZipUtility
{
    internal interface IZipOutputStream
        : IRandomOutputByteStream<ZipStreamPosition>
    {
        /// <summary>
        /// この仮想ファイルが複数の物理ファイルから構成されるマルチボリュームZIPファイルであるかどうかの値です。
        /// </summary>
        /// <value>
        /// この仮想ファイルがマルチボリュームZIPファイルかどうかを示す <see cref="Boolean"/> 値です。
        /// マルチボリュームZIPであるならtrue、そうではないのならfalseです。
        /// </value>
        Boolean IsMultiVolumeZipStream { get; }

        /// <summary>
        /// この仮想ファイルがマルチボリュームZIPファイルである場合、1ボリュームあたりの最大ファイルサイズです。
        /// マルチボリュームではない場合は <see cref="UInt64.MaxValue"/> が返ります。
        /// </summary>
        UInt64 MaximumDiskSize { get; }
    }
}
