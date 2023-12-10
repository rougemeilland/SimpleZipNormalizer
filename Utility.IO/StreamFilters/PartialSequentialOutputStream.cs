using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class PartialSequentialOutputStream
        : SequentialOutputByteStreamFilter
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly UInt64 _size;

        private UInt64 _totalCount;

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="ISequentialInputByteStream"/> オブジェクトです。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、現在位置からアクセス可能なバイト数を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        /// <remarks>
        /// このコンストラクタを使用した場合、元になったバイトストリーム上でアクセス可能な開始位置は、コンストラクタ呼び出し時点での <code>baseStream.Position</code> となります。
        /// </remarks>
        public PartialSequentialOutputStream(ISequentialOutputByteStream baseStream, UInt64 size, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;
                _size = size;
                _totalCount = 0;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            var length = _baseStream.Write(buffer[..GetWriteCount(buffer.Length)]);
            ProgressCount(length);
            return length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            var length = await _baseStream.WriteAsync(buffer[..GetWriteCount(buffer.Length)], cancellationToken).ConfigureAwait(false);
            ProgressCount(length);
            return length;
        }

        protected override void FlushCore()
            => _baseStream.Flush();
        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default)
            => _baseStream.FlushAsync(cancellationToken);

        private Int32 GetWriteCount(Int32 bufferLength)
        {
            if (bufferLength <= 0)
                return 0;
            if (_totalCount >= _size)
                throw new IOException("Can not write any more.");
            return (Int32)checked(_size - _totalCount).Minimum(checked((UInt64)bufferLength));
        }

        private void ProgressCount(Int32 length)
        {
            if (length > 0)
            {
                checked
                {
                    _totalCount += (UInt64)length;
                }
            }
        }
    }
}
