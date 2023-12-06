using System;
using Utility.IO;

namespace ZipUtility
{
    internal interface IZipOutputStream
        : IOutputByteStream<ZipStreamPosition>
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
        /// <item>ZIPの各種ヘッダを書き込むために <see cref="LockVolumeDisk"/> を呼び出す場合、その前に必ず <see cref="ReserveAtomicSpace(UInt64)"/> を呼び出してください。</item>
        /// <item>パラメタ <paramref name="atomicSpaceSize"/> には、書き込む予定の ZIP のヘッダのサイズを指定してください。</item>
        /// <item>
        /// <term>[実装する場合の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="ReserveAtomicSpace(UInt64)"/> が呼び出された場合、現在書き込み中のボリュームディスクの上限サイズを超えることなくパラメタ <paramref name="atomicSpaceSize"/> で指定された長さのデータを書き込むことが出来るかどうか調べてください。
        /// </para>
        /// <para>
        /// もし、パラメタ <paramref name="atomicSpaceSize"/> で指定された長さのデータを書き込むことによって現在のボリュームディスクの上限を超えてしまうことが予想される場合は、現在のボリュームディスクを閉じて次のボリュームディスクを開いてください。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        void ReserveAtomicSpace(UInt64 atomicSpaceSize);

        /// <summary>
        /// ボリュームディスク間をまたぐアクセスを禁止します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> を呼び出してから <see cref="UnlockVolumeDisk"/> を呼び出すまでの間、以下のようなボリュームディスクをまたぐアクセスをしようとすると、<see cref="InvalidOperationException"/> 例外が発生します。
        /// </para>
        /// <list type="bullet">
        /// <item>ボリュームディスクのサイズの上限を超えて書き込みを行おうとした場合</item>
        /// </list>
        /// </item>
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> および <see cref="UnlockVolumeDisk"/> は、主に ZIP の各種ヘッダを書き込み前後に使用します。
        /// </para>
        /// <para>
        /// その理由は、ZIP の各種ヘッダは複数のボリュームディスクにまたがってはならない仕様であり、もしそれらのヘッダが複数のボリュームディスクにまたがっているように見えるようならそのZIPアーカイブは破損していると判断しなければならないからです。
        /// </para>
        /// </item>
        /// <item>
        /// <term>[実装する場合の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="LockVolumeDisk"/> が呼び出されてから <see cref="UnlockVolumeDisk"/> が呼び出されるまでの間、アクセス対象のボリュームディスクファイルを変更してはなりません。
        /// </para>
        /// <para>
        /// もし、その間にアクセス対象のボリュームディスクファイルを変更しなければならない状況に陥った場合は、<see cref="InvalidOperationException"/> 例外を発生させなければなりません。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        void LockVolumeDisk();

        /// <summary>
        /// <see cref="LockVolumeDisk"/> によって禁止された、ボリュームディスク間をまたぐアクセス禁止を解除します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> を呼び出してから <see cref="UnlockVolumeDisk"/> を呼び出すまでの間、以下のようなボリュームディスクをまたぐアクセスをしようとすると、<see cref="InvalidOperationException"/> 例外が発生します。
        /// </para>
        /// <list type="bullet">
        /// <item>ボリュームディスクのサイズの上限を超えて書き込みを行おうとした場合</item>
        /// </list>
        /// </item>
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> および <see cref="UnlockVolumeDisk"/> は、主に ZIP の各種ヘッダを書き込む前後に使用します。
        /// </para>
        /// <para>
        /// その理由は、ZIP の各種ヘッダは複数のボリュームディスクにまたがってはならない仕様であり、もしそれらのヘッダが複数のボリュームディスクにまたがっているように見えるようならそのZIPアーカイブは破損していると判断しなければならないからです。
        /// </para>
        /// </item>
        /// <item>
        /// <term>[実装する場合の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="LockVolumeDisk"/> が呼び出されてから <see cref="UnlockVolumeDisk"/> が呼び出されるまでの間、アクセス対象のボリュームディスクファイルを変更してはなりません。
        /// </para>
        /// <para>
        /// もし、その間にアクセス対象のボリュームディスクファイルを変更しなければならない状況に陥った場合は、<see cref="InvalidOperationException"/> 例外を発生させなければなりません。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        void UnlockVolumeDisk();

    }
}
