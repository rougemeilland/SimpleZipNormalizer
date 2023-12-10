using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public interface ISequentialInputByteStream
        : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// バイトストリームからデータを同期的に読み込みます。
        /// </summary>
        /// <param name="buffer">
        /// 読み込んだデータを格納するための <see cref="Span{Byte}">Span&lt;<see cref="Byte"/>&gt;</see> です。
        /// </param>
        /// <returns>
        /// <see cref="Int32"/> を返します。
        /// 戻り値が正の値である場合、 それは実際に読み込まれたデータの長さ (バイト数) を示します。この値は <paramref name="buffer"/> の長さを超えることはありません。
        /// 戻り値が 0 である場合、それはバイトストリームの終端に達したことを示します。
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        Int32 Read(Span<Byte> buffer);

        /// <summary>
        /// バイトストリームからデータを非同期的に読み込みます。
        /// </summary>
        /// <param name="buffer">
        /// 読み込んだデータを格納するための <see cref="Memory{Byte}">Memory&lt;<see cref="Byte"/>&gt;</see> です。
        /// </param>
        /// <param name="cancellationToken">
        /// 読み込みの中断を検出するための <see cref="CancellationToken"/> です。
        /// </param>
        /// <returns>
        /// <see cref="Int32"/> を返します。
        /// 戻り値が正の値である場合、 それは実際に読み込まれたデータの長さ (バイト数) を示します。この値は <paramref name="buffer"/> の長さを超えることはありません。
        /// 戻り値が 0 である場合、それはバイトストリームの終端に達したことを示します。
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// 読み込みが中断されました。
        /// </exception>
        Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default);
    }
}
