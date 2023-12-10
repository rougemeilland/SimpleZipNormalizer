using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public interface ISequentialOutputByteStream
        : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// データを同期的にバイトストリームに書き込みます。
        /// </summary>
        /// <param name="buffer">
        /// 書き込むデータを示す <see cref="ReadOnlySpan{Byte}">ReadOnlySpan&lt;<see cref="Byte"/>&gt;</see> です。
        /// </param>
        /// <returns>
        /// 実際に書き込むことができたバイト数を示す <see cref="Int32"/> 値です。
        /// </returns>
        /// <remarks>
        /// <para>
        /// このメソッドの仕様は、.NETの <see cref="System.IO.Stream.Write(ReadOnlySpan{Byte})"/> よりも、UNIX系のシステムコールの write に似ています。
        /// このメソッドは <paramref name="buffer"/> で与えられたデータのすべてを書き込むとは限りません。
        /// (ただし、<paramref name="buffer"/> が空ではない場合は、最低でも1バイトは書き込みます。)
        /// </para>
        /// <para>
        /// このメソッドを呼び出した場合は必ず戻り値を確認してください。
        /// そして、戻り値が <paramref name="buffer"/> の長さより小さかった場合は、
        /// 続きのデータを書き込むために再度このメソッドを呼び出してください。
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        Int32 Write(ReadOnlySpan<Byte> buffer);

        /// <summary>
        /// データを非同期的にバイトストリームに書き込みます。
        /// </summary>
        /// <param name="buffer">
        /// 書き込むデータを示す <see cref="ReadOnlyMemory{Byte}">ReadOnlyMemory&lt;<see cref="Byte"/>&gt;</see> です。
        /// </param>
        /// <param name="cancellationToken">
        /// 書き込みの中断を検出するための <see cref="CancellationToken"/> です。
        /// </param>
        /// <returns>
        /// 実際に書き込むことができたバイト数を示す <see cref="Int32"/> です。
        /// </returns>
        /// <remarks>
        /// <para>
        /// このメソッドの仕様は、.NETの <see cref="System.IO.Stream.Write(ReadOnlySpan{Byte})"/> よりも、UNIX系のシステムコールの write に似ています。
        /// このメソッドは <paramref name="buffer"/> で与えられたデータのすべてを書き込むとは限りません。
        /// (ただし、<paramref name="buffer"/> が空ではない場合は、最低でも1バイトは書き込みます。)
        /// </para>
        /// <para>
        /// このメソッドを呼び出した場合は必ず戻り値を確認してください。
        /// そして、戻り値が <paramref name="buffer"/> の長さより小さかった場合は、
        /// 続きのデータを書き込むために再度このメソッドを呼び出してください。
        /// </para>
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// 書き込みが中断されました。
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// もしデータがキャッシュされている場合は、可能な限り出力先に同期的に書き出します。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 呼び出す場合の注意: 実装するクラスによっては、このメソッドは何もしないことがあります。このメソッドがキャッシュされているデータを必ず出力先に書き出すことを期待しないでください。
        /// </para>
        /// <para>
        /// 実装する場合の注意: キャッシュされているデータを出力先に書き出す必要がない (または書き出すことができない) 場合は、何も行わないでください。
        /// 特に、常に <see cref="NotSupportedException"/> 例外や <see cref="NotImplementedException"/> 例外を発生させるような実装はしないでください。
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        void Flush();

        /// <summary>
        /// もしデータがキャッシュされている場合は、可能な限り出力先に非同期的に書き出します。
        /// </summary>
        /// <param name="cancellationToken">
        /// 書き込みの中断を検出するための <see cref="CancellationToken"/> です。
        /// </param>
        /// <remarks>
        /// <para>
        /// 呼び出す場合の注意: 実装するクラスによっては、このメソッドは何もしないことがあります。このメソッドがキャッシュされているデータを必ず出力先に書き出すことを期待しないでください。
        /// </para>
        /// <para>
        /// 実装する場合の注意: キャッシュされているデータを出力先に書き出す必要がない (または書き出すことができない) 場合は、何も行わないでください。
        /// 特に、常に <see cref="NotSupportedException"/> 例外や <see cref="NotImplementedException"/> 例外を発生させるような実装はしないでください。
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// バイトストリームが既に破棄されています。
        /// </exception>
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}
