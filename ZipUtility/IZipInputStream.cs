using System;
using Utility.IO;

namespace ZipUtility
{
    internal interface IZipInputStream
        : IRandomInputByteStream<ZipStreamPosition, UInt64>
    {
        /// <summary>
        /// この仮想ファイルがマルチボリュームZIPファイルであるかどうかの値を取得します。
        /// </summary>
        /// <value>
        /// この仮想ファイルがマルチボリュームZIPファイルかどうかを示す <see cref="Boolean"/> 値です。
        /// マルチボリュームZIPであるならtrue、そうではないのならfalseです。
        /// </value>
        Boolean IsMultiVolumeZipStream { get; }

        /// <summary>
        /// 現在アクセス中のボリュームディスクディスク番号とボリュームディスクファイルのオフセット値から仮想的なファイル上の位置を取得します。
        /// </summary>
        /// <param name="diskNumber">
        /// ボリュームディスク番号を示す <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="offsetOnTheDisk">
        /// ボリュームディスクファイル上のオフセットを示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <returns>
        /// <para>
        /// null ではない場合、それは仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </para>
        /// <para>
        /// null である場合、パラメタ <paramref name="diskNumber"/> および <paramref name="offsetOnTheDisk"/> の値が無効であることを示します。
        /// </para>
        /// </returns>
        ZipStreamPosition? GetPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk);

        /// <summary>
        /// 最初のボリュームディスクの先頭を示す位置を取得します。
        /// </summary>
        /// <value>
        /// 最初のボリュームディスクの先頭を指す <see cref="ZipStreamPosition"/> 値です。
        /// </value>
        ZipStreamPosition FirstDiskStartPosition { get; }

        /// <summary>
        /// 最後のボリュームディスクの先頭を指す位置を取得します。
        /// </summary>
        /// <value>
        /// 最後のボリュームディスクの先頭を指す <see cref="ZipStreamPosition"/> 値です。
        /// </value>
        ZipStreamPosition LastDiskStartPosition { get; }

        /// <summary>
        /// 最後のボリュームディスクファイルのサイズを取得します。
        /// </summary>
        /// <value>
        /// 最後のボリュームディスクファイルのサイズを示す <see cref="UInt64"/> 値です。
        /// </value>
        UInt64 LastDiskSize { get; }

        /// <summary>
        /// 指定された長さの不可分なデータをボリュームディスクをまたがずに読み込めるかどうかを調べます。
        /// </summary>
        /// <param name="minimumAtomicDataSize">
        /// 不可分なデータの最低限のバイト数です。
        /// </param>
        /// <returns>
        /// <paramref name="minimumAtomicDataSize"/> で指定された長さのデータをボリュームディスクファイルをまたがずに読み込めるなら true、そうではないのなら false を返します。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>ZIPの各種ヘッダを読み込むために <see cref="LockVolumeDisk"/> を呼び出す場合、その前に必ず <see cref="CheckIfCanAtomicRead(UInt64)"/> を呼び出してください。</item>
        /// <item>パラメタ <paramref name="minimumAtomicDataSize"/> には、読み込む予定の ZIP のヘッダのサイズを指定してください。もしZIPのヘッダの長さが不定長である場合は、その最低限の長さを指定してください。</item>
        /// <item>
        /// <term>[実装する場合の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="CheckIfCanAtomicRead(UInt64)"/> が呼び出された場合、アクセス対象のボリュームディスクファイルを変更することなくパラメタ <paramref name="minimumAtomicDataSize"/> で指定された長さのデータを読み込むことが出来るかどうか調べてください。
        /// </para>
        /// <para>
        /// 更に、現在のディスクのアクセス位置がボリュームディスクファイルのちょうど終端である場合は、次のボリュームディスクファイルの先頭へアクセス位置を移動してください。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        Boolean CheckIfCanAtomicRead(UInt64 minimumAtomicDataSize);

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
        /// <item>ボリュームディスクの終端を超えて読み込みを行おうとした場合</item>
        /// <item><see cref="IRandomInputByteStream{POSITION_T, UNSIGNED_OFFSET_T}.Seek(POSITION_T)"/> によって別のボリュームディスクへのアクセスを試みた場合。</item>
        /// </list>
        /// </item>
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> および <see cref="UnlockVolumeDisk"/> は、主に ZIP の各種ヘッダを読み込む前後に使用します。
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
        /// <item>ボリュームディスクの終端を超えて読み込みを行おうとした場合</item>
        /// <item><see cref="IRandomInputByteStream{POSITION_T, UNSIGNED_OFFSET_T}.Seek(POSITION_T)"/> によって別のボリュームディスクへのアクセスを試みた場合。</item>
        /// </list>
        /// </item>
        /// <item>
        /// <para>
        /// <see cref="LockVolumeDisk"/> および <see cref="UnlockVolumeDisk"/> は、主に ZIP の各種ヘッダを読み込む前後に使用します。
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
