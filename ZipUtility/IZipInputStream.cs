using System;
using Utility.IO;

namespace ZipUtility
{
    internal interface IZipInputStream
        : IRandomInputByteStream<ZipStreamPosition>
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
        /// 物理的なディスク番号とオフセット値から仮想的なファイル上の位置を取得します。
        /// </summary>
        /// <param name="diskNumber">
        /// 物理的なディスク番号を示す <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="offset">
        /// 物理的なディスクファイル上のオフセットを示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <returns>
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </returns>
        ZipStreamPosition GetPosition(UInt32 diskNumber, UInt64 offset);

        /// <summary>
        /// 仮想的なファイルのうち、最後の物理的なディスクの先頭を指す位置です。
        /// </summary>
        /// <value>
        /// 最後の物理的なディスクの先頭を指す <see cref="ZipStreamPosition"/> 値です。
        /// </value>
        ZipStreamPosition LastDiskStartPosition { get; }

        /// <summary>
        /// 仮想的なファイルのうち、最後の物理的なディスクのサイズです。
        /// </summary>
        /// <value>
        /// 最後の物理的なディスクのサイズを示す <see cref="UInt64"/> 値です。
        /// </value>
        UInt64 LastDiskSize { get; }
    }
}
