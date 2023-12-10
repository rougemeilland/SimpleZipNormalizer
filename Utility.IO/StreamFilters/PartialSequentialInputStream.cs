using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    /// <summary>
    /// バイトストリームの部分的な範囲のアクセスのみを可能にするクラスです。
    /// </summary>
    internal class PartialSequentialInputStream
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly UInt64 _size;

        private UInt64 _totalCount;

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(開始位置と長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="ISequentialInputByteStream"/> オブジェクトです。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、最初の位置からアクセス可能なバイト数を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialSequentialInputStream(ISequentialInputByteStream baseStream, UInt64 size, Boolean leaveOpen)
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

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = _baseStream.Read(buffer[..actualCount]);
            ProgressCount(length);
            return length;
        }

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = await _baseStream.ReadAsync(buffer[..actualCount], cancellationToken).ConfigureAwait(false);
            ProgressCount(length);
            return length;
        }

        private Int32 GetReadCount(Int32 bufferLength)
        {
            if (bufferLength <= 0)
                return 0;
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
