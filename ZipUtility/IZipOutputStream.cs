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

        /// <summary>
        /// 別々のディスクに分割されてはならない不可分な領域を予約します。
        /// </summary>
        /// <param name="atomicSpaceSize">
        /// 不可分な両機のサイズを示すバイト数です。
        /// </param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <para>
        /// ZIPのすべてのヘッダがそうであるように、途中で別々のディスクに分割されてはならないデータを書き込む前に、あらかじめそのサイズを <paramref name="atomicSpaceSize"/> に指定してこのメソッドを呼び出してください。
        /// </para>
        /// <para>
        /// 次に書き込むヘッダの先頭位置として、このメソッドを呼び出した後の位置を使用してください。何故ならば、このメソッドの呼び出しの後でディスクの番号が変わることがあるからです。
        /// </para>
        /// </item>
        /// <item>
        /// <term>実装時の注意</term>
        /// <description>
        /// <para>
        /// マルチボリュームZIPファイルへ書き込み中にこのメソッドが呼び出された場合、現在書き込み中のディスクの残りのサイズを調べて、<paramref name="atomicSpaceSize"/> 未満であるかどうかを調べてください。
        /// </para>
        /// <para>
        /// ディスクの残りサイズが <paramref name="atomicSpaceSize"/> を超えている場合は、現在書き込み中のディスクを閉じて、次のディスクへ書き込む準備をしてください。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        void ReserveAtomicSpace(UInt64 atomicSpaceSize);
    }
}
