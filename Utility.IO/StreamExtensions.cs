using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO
{
    public static class StreamExtensions
    {
        #region private class

        private class ReverseByteSequenceByByteStream
            : ReverseByteSequenceByByteStreamEnumerable<UInt64>
        {
            public ReverseByteSequenceByByteStream(IRandomInputByteStream<UInt64> inputStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen)
                : base(inputStream, offset, count, progress, leaveOpen)
            {
            }

            protected override UInt64 AddPositionAndDistance(UInt64 position, UInt64 distance)
                => checked(position + distance);

            protected override Int32 GetDistanceBetweenPositions(UInt64 position1, UInt64 distance2)
                => checked((Int32)(position1 - distance2));

            protected override UInt64 SubtractBufferSizeFromPosition(UInt64 position, UInt32 distance)
                => checked(position - distance);
        }

        private class BufferedInputStreamUInt64
            : BufferedInputStream<UInt64>
        {
            public BufferedInputStreamUInt64(IInputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public BufferedInputStreamUInt64(IInputByteStream<UInt64> baseStream, Int32 bufferSize, Boolean leaveOpen)
                : base(baseStream, bufferSize, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class BufferedOutputStreamUInt64
            : BufferedOutputStream<UInt64>
        {
            public BufferedOutputStreamUInt64(IOutputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public BufferedOutputStreamUInt64(IOutputByteStream<UInt64> baseStream, Int32 bufferSize, Boolean leaveOpen)
                : base(baseStream, bufferSize, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class BufferedRandomInputStreamUInt64
            : BufferedRandomInputStream<UInt64>
        {

            public BufferedRandomInputStreamUInt64(IRandomInputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public BufferedRandomInputStreamUInt64(IRandomInputByteStream<UInt64> baseStream, Int32 bufferSize, Boolean leaveOpen)
                : base(baseStream, bufferSize, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);

            protected override UInt64 GetDistanceBetweenPositions(UInt64 x, UInt64 y)
                => checked(x - y);
        }

        private class BufferedRandomOutputStreamUint64
            : BufferedRandomOutputStream<UInt64>
        {
            public BufferedRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public BufferedRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, Int32 bufferSize, Boolean leaveOpen)
                : base(baseStream, bufferSize, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);

            protected override UInt64 GetDistanceBetweenPositions(UInt64 x, UInt64 y) => x - y;
        }

        private class PartialInputStreamUint64
            : PartialInputStream<UInt64, UInt64>
        {
            public PartialInputStreamUint64(IInputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialInputStreamUint64(IInputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialInputStreamUint64(IInputByteStream<UInt64> baseStream, UInt64? size, Boolean leaveOpen)
                : base(baseStream, size, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class PartialOutputStreamUint64
            : PartialOutputStream<UInt64, UInt64>
        {
            public PartialOutputStreamUint64(IOutputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialOutputStreamUint64(IOutputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialOutputStreamUint64(IOutputByteStream<UInt64> baseStream, UInt64? size, Boolean leaveOpen)
                : base(baseStream, size, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class PartialRandomInputStreamUint64
            : PartialRandomInputStream<UInt64, UInt64>
        {
            private readonly IRandomInputByteStream<UInt64> _baseStream;

            public PartialRandomInputStreamUint64(IRandomInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
                : base(baseStream, leaveOpen)
            {
                _baseStream = baseStream;
            }

            public PartialRandomInputStreamUint64(IRandomInputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
                _baseStream = baseStream;
            }

            public PartialRandomInputStreamUint64(IRandomInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, offset, size, leaveOpen)
            {
                _baseStream = baseStream;
            }

            public PartialRandomInputStreamUint64(IRandomInputByteStream<UInt64> baseStream, UInt64? offset, UInt64? size, Boolean leaveOpen = false)
                : base(baseStream, offset, size, leaveOpen)
            {
                _baseStream = baseStream;
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 EndBasePositionValue => _baseStream.Length;

            protected override (Boolean Success, UInt64 Position) AddBasePosition(UInt64 x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, 0);
                }
            }

            protected override (Boolean Success, UInt64 Position) AddPosition(UInt64 x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, 0);
                }
            }

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenBasePositions(UInt64 x, UInt64 y)
                => x >= y ? (true, x - y) : (false, 0);

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenPositions(UInt64 x, UInt64 y)
                => x >= y ? (true, x - y) : (false, 0);
        }

        private class PartialRandomOutputStreamUint64
            : PartialRandomOutputStream<UInt64, UInt64>
        {
            public PartialRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, Boolean leaveOpen)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, UInt64 offset, UInt64 size, Boolean leaveOpen)
                : base(baseStream, offset, size, leaveOpen)
            {
            }

            public PartialRandomOutputStreamUint64(IRandomOutputByteStream<UInt64> baseStream, UInt64? offset, UInt64? size, Boolean leaveOpen)
                : base(baseStream, offset, size, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 ZeroBasePositionValue => 0;

            protected override (Boolean Success, UInt64 Position) AddBasePosition(UInt64 x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, 0);
                }
            }

            protected override (Boolean Success, UInt64 Position) AddPosition(UInt64 x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, 0);
                }
            }

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenBasePositions(UInt64 x, UInt64 y)
                => x >= y ? (true, x - y) : (false, 0);

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenPositions(UInt64 x, UInt64 y)
                => x >= y ? (true, x - y) : (false, 0);
        }

        private class Crc32CalculationInputStream
            : IInputByteStream<UInt64>
        {
            private readonly IBasicInputByteStream _baseStream;
            private readonly ICrcCalculationState<UInt32, UInt64> _session;
            private readonly ValueHolder<(UInt32 Crc, UInt64 Length)> _valueHolder;
            private readonly Boolean _leaveOpen;

            private Boolean _isDisposed;
            private UInt64 _position;

            public Crc32CalculationInputStream(IBasicInputByteStream baseStream, ICrcCalculationState<UInt32, UInt64> session, ValueHolder<(UInt32 Crc, UInt64 Length)> valueHolder, Boolean leaveOpen)
            {
                _baseStream = baseStream;
                _session = session;
                _valueHolder = valueHolder;
                _leaveOpen = leaveOpen;
            }

            public UInt64 Position
            {
                get
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);

                    return _position;
                }
            }

            public Int32 Read(Span<Byte> buffer)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var length = _baseStream.Read(buffer);
                if (length > 0)
                {
                    _position += (UInt32)length;
                    _session.Put(buffer[..length]);
                }

                return length;
            }

            public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (length > 0)
                {
                    _position += (UInt32)length;
                    _session.Put(buffer.Span[..length]);
                }

                return length;
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public async ValueTask DisposeAsync()
            {
                await DisposeAsyncCore().ConfigureAwait(false);
                Dispose(disposing: false);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        _valueHolder.Value = _session.GetResult();
                        if (!_leaveOpen)
                            _baseStream.Dispose();
                    }

                    _isDisposed = true;
                }
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    _valueHolder.Value = _session.GetResult();
                    if (!_leaveOpen)
                        await _baseStream.DisposeAsync().ConfigureAwait(false);
                    _isDisposed = true;
                }
            }
        }

        private class Crc32CalculationOutputStream
            : IOutputByteStream<UInt64>
        {
            private readonly IBasicOutputByteStream _baseStream;
            private readonly ICrcCalculationState<UInt32, UInt64> _session;
            private readonly ValueHolder<(UInt32 Crc, UInt64 Length)> _valueHolder;
            private readonly Boolean _leaveOpen;

            private Boolean _isDisposed;
            private UInt64 _position;

            public Crc32CalculationOutputStream(IBasicOutputByteStream baseStream, ICrcCalculationState<UInt32, UInt64> session, ValueHolder<(UInt32 Crc, UInt64 Length)> valueHolder, Boolean leaveOpen)
            {
                _baseStream = baseStream;
                _session = session;
                _valueHolder = valueHolder;
                _leaveOpen = leaveOpen;
            }

            public UInt64 Position
                => _isDisposed
                    ? throw new ObjectDisposedException(GetType().FullName)
                    : _position;

            public Int32 Write(ReadOnlySpan<Byte> buffer)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var length = _baseStream.Write(buffer);
                if (length > 0)
                {
                    _position += (UInt32)length;
                    _session.Put(buffer[..length]);
                }

                return length;
            }

            public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var length = await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (length > 0)
                {
                    _position += (UInt32)length;
                    _session.Put(buffer.Span[..length]);
                }

                return length;
            }

            public void Flush()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

            }

            public Task FlushAsync(CancellationToken cancellationToken = default)
                => _isDisposed
                    ? throw new ObjectDisposedException(GetType().FullName)
                : Task.CompletedTask;

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public async ValueTask DisposeAsync()
            {
                await DisposeAsyncCore().ConfigureAwait(false);
                Dispose(disposing: false);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (!_leaveOpen)
                                _baseStream.Dispose();
                        }

                        _isDisposed = true;
                    }
                    finally
                    {
                        _valueHolder.Value = _session.GetResult();
                    }
                }
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    try
                    {
                        if (!_leaveOpen)
                            await _baseStream.DisposeAsync().ConfigureAwait(false);
                        _isDisposed = true;
                    }
                    finally
                    {
                        _valueHolder.Value = _session.GetResult();
                    }
                }
            }
        }

        #endregion

        private const Int32 _COPY_TO_DEFAULT_BUFFER_SIZE = 81920;
        private const Int32 _WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE = 81920;

        #region AsInputByteStream

        public static IInputByteStream<UInt64> AsInputByteStream(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.CanSeek
                    ? new RandomInputByteStreamByStream(baseStream, leaveOpen)
                    : new SequentialInputByteStreamByStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsOutputByteStream

        public static IOutputByteStream<UInt64> AsOutputByteStream<BASE_STREAM_T>(this BASE_STREAM_T baseStream, Boolean leaveOpen = false)
            where BASE_STREAM_T : Stream
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.CanSeek
                    ? new RandomOutputByteStreamByStream<BASE_STREAM_T>(baseStream, _ => { }, leaveOpen)
                    : new SequentialOutputByteStreamByStream<BASE_STREAM_T>(baseStream, _ => { }, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> AsOutputByteStream<BASE_STREAM_T>(this BASE_STREAM_T baseStream, Action<BASE_STREAM_T> finishAction, Boolean leaveOpen = false)
            where BASE_STREAM_T : Stream
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.CanSeek
                    ? new RandomOutputByteStreamByStream<BASE_STREAM_T>(baseStream, finishAction, leaveOpen)
                    : new SequentialOutputByteStreamByStream<BASE_STREAM_T>(baseStream, finishAction, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsStream

        public static Stream AsStream(this IBasicInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new StreamByInputByteStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static Stream AsStream(this IBasicOutputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new StreamByOutputByteStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsByteStream

        public static IInputByteStream<UInt64> AsByteStream(this IEnumerable<Byte> baseSequence)
            => baseSequence is null
                ? throw new ArgumentNullException(nameof(baseSequence))
                : (IInputByteStream<UInt64>)new SequentialInputByteStreamBySequence(baseSequence);

        public static IInputByteStream<UInt64> AsByteStream(this IInputBitStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IInputByteStream<UInt64>)new SequentialInputByteStreamByBitStream(baseStream, BitPackingDirection.MsbToLsb, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> AsByteStream(this IInputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IInputByteStream<UInt64>)new SequentialInputByteStreamByBitStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> AsByteStream(this IOutputBitStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IOutputByteStream<UInt64>)new SequentialOutputByteStreamByBitStream(baseStream, BitPackingDirection.MsbToLsb, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> AsByteStream(this IOutputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IOutputByteStream<UInt64>)new SequentialOutputByteStreamByBitStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsBitStream

        public static IInputBitStream AsBitStream(this IInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IInputBitStream)new SequentialInputBitStreamByByteStream(baseStream, BitPackingDirection.Default, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputBitStream AsBitStream(this IInputByteStream<UInt64> baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IInputBitStream)new SequentialInputBitStreamByByteStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputBitStream AsBitStream(this IEnumerable<Byte> baseSequence, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => baseSequence is null
                ? throw new ArgumentNullException(nameof(baseSequence))
                : (IInputBitStream)new SequentialInputBitStreamBySequence(baseSequence, bitPackingDirection);

        public static IOutputBitStream AsBitStream(this IOutputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IOutputBitStream)new SequentialOutputBitStreamByByteStream(baseStream, BitPackingDirection.Default, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputBitStream AsBitStream(this IOutputByteStream<UInt64> baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IOutputBitStream)new SequentialOutputBitStreamByByteStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region WithPartial

        public static IInputByteStream<UInt64> WithPartial(this IInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomInputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomInputStreamUint64(baseRandomAccessStream, leaveOpen)
                    : new PartialInputStreamUint64(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithPartial(this IInputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomInputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomInputStreamUint64(baseRandomAccessStream, size, leaveOpen)
                    : new PartialInputStreamUint64(baseStream, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithPartial(this IInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomInputByteStream<UInt64> baseRandomAccessStream
                    ? throw new NotSupportedException()
                    : (IInputByteStream<UInt64>)new PartialRandomInputStreamUint64(baseRandomAccessStream, offset, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithPartial(this IInputByteStream<UInt64> baseStream, UInt64? offset, UInt64? size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomInputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomInputStreamUint64(baseRandomAccessStream, offset, size, leaveOpen)
                    : offset is not null
                    ? throw new NotSupportedException()
                    : (IInputByteStream<UInt64>)new PartialInputStreamUint64(baseStream, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithPartial(this IOutputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomOutputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomOutputStreamUint64(baseRandomAccessStream, leaveOpen)
                    : new PartialOutputStreamUint64(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithPartial(this IOutputByteStream<UInt64> baseStream, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomOutputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomOutputStreamUint64(baseRandomAccessStream, size, leaveOpen)
                    : new PartialOutputStreamUint64(baseStream, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithPartial(this IOutputByteStream<UInt64> baseStream, UInt64 offset, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomOutputByteStream<UInt64> baseRandomAccessStream
                    ? throw new NotSupportedException()
                    : (IOutputByteStream<UInt64>)new PartialRandomOutputStreamUint64(baseRandomAccessStream, offset, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithPartial(this IOutputByteStream<UInt64> baseStream, UInt64? offset, UInt64? size, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is IRandomOutputByteStream<UInt64> baseRandomAccessStream
                    ? new PartialRandomOutputStreamUint64(baseRandomAccessStream, offset, size, leaveOpen)
                    : offset is not null
                    ? throw new NotSupportedException()
                    : (IOutputByteStream<UInt64>)new PartialOutputStreamUint64(baseStream, size, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region WithCache

        public static IBasicInputByteStream WithCache(this IBasicInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStreamUInt64(baseRandomAccessStream, leaveOpen),
                        IInputByteStream<UInt64> baseByteSteram
                            => new BufferedInputStreamUInt64(baseByteSteram, leaveOpen),
                        _
                            => new BufferedBasicInputStream(baseStream, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IBasicInputByteStream WithCache(this IBasicInputByteStream baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStreamUInt64(baseRandomAccessStream, cacheSize, leaveOpen),
                        IInputByteStream<UInt64> baseByteSteram
                            => new BufferedInputStreamUInt64(baseByteSteram, cacheSize, leaveOpen),
                        _
                            => new BufferedBasicInputStream(baseStream, cacheSize, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithCache(this IInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStreamUInt64(baseRandomAccessStream, leaveOpen),
                        _
                            => new BufferedInputStreamUInt64(baseStream, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithCache(this IInputByteStream<UInt64> baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStreamUInt64(baseRandomAccessStream, cacheSize, leaveOpen),
                        _
                            => new BufferedInputStreamUInt64(baseStream, cacheSize, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IBasicOutputByteStream WithCache(this IBasicOutputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStreamUint64(baseRandomAccessStream, leaveOpen),
                        IOutputByteStream<UInt64> baseByteStream
                            => new BufferedOutputStreamUInt64(baseByteStream, leaveOpen),
                        _
                            => new BufferedBasicOutputStream(baseStream, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IBasicOutputByteStream WithCache(this IBasicOutputByteStream baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStreamUint64(baseRandomAccessStream, cacheSize, leaveOpen),
                        IOutputByteStream<UInt64> baseByteStream
                            => new BufferedOutputStreamUInt64(baseByteStream, cacheSize, leaveOpen),
                        _
                            => new BufferedBasicOutputStream(baseStream, cacheSize, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithCache(this IOutputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStreamUint64(baseRandomAccessStream, leaveOpen),
                        _
                            => new BufferedOutputStreamUInt64(baseStream, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithCache(this IOutputByteStream<UInt64> baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStreamUint64(baseRandomAccessStream, cacheSize, leaveOpen),
                        _
                            => new BufferedOutputStreamUInt64(baseStream, cacheSize, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region WithCrc32Calculation

        public static IBasicInputByteStream WithCrc32Calculation(this IBasicInputByteStream baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> value, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                return new Crc32CalculationInputStream(baseStream, Crc32.CreateCalculationState(), value, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputByteStream<UInt64> WithCrc32Calculation(this IInputByteStream<UInt64> baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> value, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                return new Crc32CalculationInputStream(baseStream, Crc32.CreateCalculationState(), value, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IBasicOutputByteStream WithCrc32Calculation(this IBasicOutputByteStream baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> value, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                return new Crc32CalculationOutputStream(baseStream, Crc32.CreateCalculationState(), value, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputByteStream<UInt64> WithCrc32Calculation(this IOutputByteStream<UInt64> baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> value, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                return new Crc32CalculationOutputStream(baseStream, Crc32.CreateCalculationState(), value, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region GetByteSequence

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence(progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, count);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, count, progress: progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : InternalGetByteSequence(baseStream, null, null, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : InternalGetByteSequence(baseStream, null, null, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomInputByteStream<UInt64> byteStream
                    ? throw new NotSupportedException()
                    : offset > byteStream.Length
                    ? throw new ArgumentOutOfRangeException(nameof(offset))
                    : InternalGetByteSequence(byteStream, offset, byteStream.Length - offset, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomInputByteStream<UInt64> byteStream
                    ? throw new NotSupportedException()
                    : offset > byteStream.Length
                    ? throw new ArgumentOutOfRangeException(nameof(offset))
                    : InternalGetByteSequence(byteStream, offset, byteStream.Length - offset, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream
                    ? throw new NotSupportedException()
                    : checked(offset + count) > baseRamdomAccessStream.Length
                    ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.")
                    : InternalGetByteSequence(baseRamdomAccessStream, offset, count, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream
                    ? throw new NotSupportedException()
                    : checked(offset + count) > baseRamdomAccessStream.Length
                    ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.")
                    : InternalGetByteSequence(baseRamdomAccessStream, offset, count, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region GetReverseByteSequence

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(progress: progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, count);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, count, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, 0, baseRamdomAccessStream.Length, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, 0, baseRamdomAccessStream.Length, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (!offset.IsBetween(0UL, baseRamdomAccessStream.Length))
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, offset, baseRamdomAccessStream.Length - offset, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (!offset.IsBetween(0UL, baseRamdomAccessStream.Length))
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, offset, baseRamdomAccessStream.Length - offset, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, offset, count, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this IInputByteStream<UInt64> baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return new ReverseByteSequenceByByteStream(baseRamdomAccessStream, offset, count, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region StreamBytesEqual

        public static Boolean StreamBytesEqual(this Stream stream1, Stream stream2, Boolean leaveOpen = false)
        {
            if (stream1 is null)
                throw new ArgumentNullException(nameof(stream1));
            if (stream2 is null)
                throw new ArgumentNullException(nameof(stream2));

            try
            {
                using var byteStream1 = stream1.AsInputByteStream(true);
                using var byteStream2 = stream2.AsInputByteStream(true);
                return byteStream1.StreamBytesEqual(byteStream2, null, leaveOpen);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this Stream stream1, Stream stream2, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            if (stream1 is null)
                throw new ArgumentNullException(nameof(stream1));
            if (stream2 is null)
                throw new ArgumentNullException(nameof(stream2));

            try
            {
                using var byteStream1 = stream1.AsInputByteStream(true);
                using var byteStream2 = stream2.AsInputByteStream(true);
                return byteStream1.StreamBytesEqual(byteStream2, progress, leaveOpen);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this IBasicInputByteStream stream1, IBasicInputByteStream stream2, Boolean leaveOpen = false)
        {
            try
            {
                if (stream1 is null)
                    throw new ArgumentNullException(nameof(stream1));
                if (stream2 is null)
                    throw new ArgumentNullException(nameof(stream2));

                return InternalStreamBytesEqual(stream1, stream2, null);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this IBasicInputByteStream stream1, IBasicInputByteStream stream2, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (stream1 is null)
                    throw new ArgumentNullException(nameof(stream1));
                if (stream2 is null)
                    throw new ArgumentNullException(nameof(stream2));

                return InternalStreamBytesEqual(stream1, stream2, progress);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        #endregion

        #region CopyTo

        public static void CopyTo(this Stream source, Stream destination, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            using var sourceByteStream = source.AsInputByteStream(true);
            using var destinationByteStream = destination.AsOutputByteStream(true);
            sourceByteStream.InternalCopyTo(destinationByteStream, _COPY_TO_DEFAULT_BUFFER_SIZE, progress);
        }

        public static void CopyTo(this IBasicInputByteStream source, IBasicOutputByteStream destination, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            source.InternalCopyTo(destination, _COPY_TO_DEFAULT_BUFFER_SIZE, progress);
        }

        public static void CopyTo(this Stream source, Stream destination, Int32 bufferSize, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            using var sourceByteStream = source.AsInputByteStream(true);
            using var destinationByteStream = destination.AsOutputByteStream(true);
            sourceByteStream.InternalCopyTo(destinationByteStream, bufferSize, progress);
        }

        public static void CopyTo(this IBasicInputByteStream source, IBasicOutputByteStream destination, Int32 bufferSize, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            source.InternalCopyTo(destination, bufferSize, progress);
        }

        #endregion

        #region Read

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : stream.Read(buffer, 0, buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer, Int32 offset)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : !offset.IsBetween(0, buffer.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : stream.Read(
                    buffer,
                    offset,
                    buffer.Length - offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this Stream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif
            return
                (UInt32)stream.Read(
                    buffer,
                    (Int32)offset,
                    buffer.Length - (Int32)offset)
                .Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer, Range range)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            return
                isOk
                ? stream.Read(buffer, offset, count)
                : throw new ArgumentOutOfRangeException(nameof(range));
        }

#if false
        public static Int32 Read(this Stream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.Read(Byte[], Int32, Int32)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this Stream stream, Byte[] buffer, UInt32 offset, UInt32 count)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : checked(offset + count) > (UInt32)buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : (UInt32)stream.Read(
                    buffer,
                    (Int32)offset,
                    (Int32)count).Maximum(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Memory<Byte> buffer)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : stream.Read(buffer.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this IBasicInputByteStream stream, Byte[] buffer)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : stream.Read(buffer.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this IBasicInputByteStream stream, Byte[] buffer, Int32 offset)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : !offset.IsBetween(0, buffer.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : stream.Read(buffer.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this IBasicInputByteStream stream, Byte[] buffer, UInt32 offset)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset > buffer.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : (UInt32)stream.Read(buffer.AsSpan(offset)).Maximum(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this IBasicInputByteStream stream, Byte[] buffer, Range range)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            return
                isOk
                ? stream.Read(buffer.AsSpan(offset, count))
                : throw new ArgumentOutOfRangeException(nameof(range));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this IBasicInputByteStream stream, Byte[] buffer, Int32 offset, Int32 count)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : stream.Read(buffer.AsSpan(offset, count));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this IBasicInputByteStream stream, Byte[] buffer, UInt32 offset, UInt32 count)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : (UInt32)stream.Read(buffer.AsSpan(offset, count));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this IBasicInputByteStream stream, Memory<Byte> buffer)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : stream.Read(buffer.Span);

        #endregion

        #region ReadByteOrNull

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte? ReadByteOrNull(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[1];
            return
                sourceStream.Read(buffer) > 0
                ? buffer[0]
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte? ReadByteOrNull(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[1];
            return
                sourceStream.Read(buffer) > 0
                ? buffer[0]
                : null;
        }

        #endregion

        #region ReadByte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte ReadByte(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            Span<Byte> buffer = stackalloc Byte[1];
            return
                sourceStream.Read(buffer) > 0
                ? buffer[0]
                : throw new UnexpectedEndOfStreamException();
        }

        #endregion

        #region ReadBytes

        public static ReadOnlyMemory<Byte> ReadBytes(this Stream sourceStream, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this Stream sourceStream, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Int32 offset)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : !offset.IsBetween(0, buffer.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : InternalReadBytes(
                    offset,
                    buffer.Length - offset,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));

        public static UInt32 ReadBytes(this Stream sourceStream, Byte[] buffer, UInt32 offset)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset > (UInt32)buffer.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : (UInt32)InternalReadBytes(
                    (Int32)offset,
                    buffer.Length - (Int32)offset,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count)).Maximum(0);

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Range range)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            return
                isOk
                ? InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count))
                : throw new ArgumentOutOfRangeException(nameof(range));
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Int32 offset, Int32 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));

        public static UInt32 ReadBytes(this Stream sourceStream, Byte[] buffer, UInt32 offset, UInt32 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : checked(offset) + count > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : (UInt32)InternalReadBytes(
                    (Int32)offset,
                    (Int32)count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count)).Maximum(0);

        public static Int32 ReadBytes(this Stream sourceStream, Memory<Byte> buffer)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : InternalReadBytes(
                    buffer,
                    _buffer => sourceStream.Read(_buffer.Span));

        public static Int32 ReadBytes(this Stream sourceStream, Span<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var processedCount = 0;
            while (!buffer.IsEmpty)
            {
                var length = sourceStream.Read(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                processedCount += length;
            }

            return processedCount;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this IBasicInputByteStream sourceStream, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this IBasicInputByteStream sourceStream, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer, Int32 offset)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : !offset.IsBetween(0, buffer.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : InternalReadBytes(
                    offset,
                    buffer.Length - offset,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));

        public static UInt32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer, UInt32 offset)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset > (UInt32)buffer.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : (UInt32)InternalReadBytes(
                    (Int32)offset,
                    buffer.Length - (Int32)offset,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count))).Maximum(0);

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer, Range range)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            return
                isOk
                ? InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)))
                : throw new ArgumentOutOfRangeException(nameof(range));
        }

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer, Int32 offset, Int32 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));

        public static UInt32 ReadBytes(this IBasicInputByteStream sourceStream, Byte[] buffer, UInt32 offset, UInt32 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : (UInt32)InternalReadBytes(
                    (Int32)offset,
                    (Int32)count,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count))).Maximum(0);

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Memory<Byte> buffer)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : InternalReadBytes(
                    buffer,
                    _buffer => sourceStream.Read(_buffer.Span));

        public static Int32 ReadBytes(this IBasicInputByteStream sourceStream, Span<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var processedCount = 0;
            while (!buffer.IsEmpty)
            {
                var length = sourceStream.Read(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                processedCount += length;
            }

            return processedCount;
        }

        #endregion

        #region ReadByteSequence

        public static IEnumerable<Byte> ReadByteSequence(this Stream sourceStream, Int64 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : InternalReadByteSequence(
                    (UInt64)count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });

        public static IEnumerable<Byte> ReadByteSequence(this Stream sourceStream, UInt64 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : InternalReadByteSequence(
                    count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });

        public static IEnumerable<Byte> ReadByteSequence(this IBasicInputByteStream sourceStream, Int64 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : InternalReadByteSequence(
                    (UInt64)count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });

        public static IEnumerable<Byte> ReadByteSequence(this IBasicInputByteStream sourceStream, UInt64 count)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : InternalReadByteSequence(
                    count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });

        #endregion

        #region ReadAllBytes

        public static ReadOnlyMemory<Byte> ReadAllBytes(this Stream sourceStream)
            => sourceStream is null
                ? throw new ArgumentNullException(nameof(sourceStream))
                : InternalReadAllBytes(sourceStream.Read);

        public static ReadOnlyMemory<Byte> ReadAllBytes(this IBasicInputByteStream sourceByteStream)
            => sourceByteStream is null
                ? throw new ArgumentNullException(nameof(sourceByteStream))
                : InternalReadAllBytes((_buffer, _offset, _count) => sourceByteStream.Read(_buffer.AsSpan(_offset, _count)));

        #endregion

        #region ReadInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt16LE();
        }

        #endregion

        #region ReadUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt16LE();
        }

        #endregion

        #region ReadInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt32LE();
        }

        #endregion

        #region ReadUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt32LE();
        }

        #endregion

        #region ReadInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt64LE();
        }

        #endregion

        #region ReadUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64LE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt64LE();
        }

        #endregion

        #region ReadSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToSingleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleLE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToSingleLE();
        }

        #endregion

        #region ReadDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDoubleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleLE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDoubleLE();
        }

        #endregion

        #region ReadDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDecimalLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalLE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDecimalLE();
        }

        #endregion

        #region ReadInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt16BE();
        }

        #endregion

        #region ReadUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt16BE();
        }

        #endregion

        #region ReadInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt32BE();
        }

        #endregion

        #region ReadUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt32BE();
        }

        #endregion

        #region ReadInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToInt64BE();
        }

        #endregion

        #region ReadUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64BE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToUInt64BE();
        }

        #endregion

        #region ReadSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToSingleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleBE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToSingleBE();
        }

        #endregion

        #region ReadDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDoubleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleBE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDoubleBE();
        }

        #endregion

        #region ReadDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDecimalBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalBE(this IBasicInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            return
                sourceStream.ReadBytes(buffer) != buffer.Length
                ? throw new UnexpectedEndOfStreamException()
                : buffer.ToDecimalBE();
        }

        #endregion

        #region Write

#if false
        public static Int32 Write(this Stream stream, Byte[] buffer)
        {
            throw new NotImplementedException(); // equivalent to System.IO.Stream.Write(ReadOnlyMemory<Byte>)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this Stream stream, Byte[] buffer, Int32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            var count = buffer.Length - offset;
            stream.Write(buffer, offset, count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this Stream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            var count = buffer.Length - (Int32)offset;
            stream.Write(buffer, (Int32)offset, count);
            return (UInt32)count;
        }

#if false

        public static Int32 Write(this Stream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.Write(Byte[], Int32, Int32)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this Stream stream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            stream.Write(buffer, (Int32)offset, (Int32)count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this Stream stream, ReadOnlyMemory<Byte> buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            stream.Write(buffer.Span);
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this IBasicOutputByteStream stream, Byte[] buffer, Int32 offset)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : !offset.IsBetween(0, buffer.Length)
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : stream.Write(buffer.AsSpan(offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this IBasicOutputByteStream stream, Byte[] buffer, UInt32 offset)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset > buffer.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : (UInt32)stream.Write(buffer.AsSpan((Int32)offset)).Maximum(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this IBasicOutputByteStream stream, Byte[] buffer, Int32 offset, Int32 count)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : offset < 0
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : checked(offset + count) > buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : stream.Write(buffer.AsSpan(offset, count));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this IBasicOutputByteStream stream, Byte[] buffer, UInt32 offset, UInt32 count)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
                : buffer is null
                ? throw new ArgumentNullException(nameof(buffer))
                : checked(offset + count) > (UInt32)buffer.Length
                ? throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.")
                : (UInt32)stream.Write(buffer.AsSpan(offset, count)).Maximum(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this IBasicOutputByteStream stream, ReadOnlyMemory<Byte> buffer)
            => stream is null
                ? throw new ArgumentNullException(nameof(stream))
            : stream.Write(buffer.Span);

        #endregion

        #region WriteByte

#if false
        public static void WriteByte(this Stream stream, Byte value)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.WriteByte(Byte)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(this IBasicOutputByteStream stream, Byte value)
        {
            Span<Byte> buffer = stackalloc Byte[1];
            buffer[0] = value;
            var length = stream.Write(buffer);
            if (length <= 0)
                throw new InternalLogicalErrorException();
        }

        #endregion

        #region WriteBytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            destinationStream.Write(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Int32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            destinationStream.Write(buffer, offset, buffer.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, UInt32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            destinationStream.Write(buffer, (Int32)offset, buffer.Length - (Int32)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Range range)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            destinationStream.Write(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            destinationStream.Write(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            destinationStream.Write(buffer, (Int32)offset, (Int32)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, ReadOnlyMemory<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            destinationStream.Write(buffer.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, ReadOnlySpan<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            destinationStream.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, IEnumerable<ReadOnlyMemory<Byte>> buffers)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffers is null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                destinationStream.Write(buffer.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            InternalWriteBytes(0, buffer.Length, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer, Int32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            InternalWriteBytes(0, buffer.Length, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer, UInt32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            InternalWriteBytes((Int32)offset, buffer.Length - (Int32)offset, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer, Range range)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalWriteBytes(offset, count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            InternalWriteBytes(offset, count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            InternalWriteBytes((Int32)offset, (Int32)count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, ReadOnlyMemory<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            var spanOfBuffer = buffer.Span;
            while (!spanOfBuffer.IsEmpty)
            {
                var length = destinationStream.Write(spanOfBuffer);
                if (length <= 0)
                    throw new IOException("Can not write any more");
                spanOfBuffer = spanOfBuffer[length..];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, ReadOnlySpan<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            while (!buffer.IsEmpty)
            {
                var length = destinationStream.Write(buffer);
                if (length <= 0)
                    throw new IOException("Can not write any more");
                buffer = buffer[length..];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this IBasicOutputByteStream destinationStream, IEnumerable<ReadOnlyMemory<Byte>> buffers)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffers is null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteByteSequence

        public static void WriteByteSequence(this Stream destinationStream, IEnumerable<Byte> sequence)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            using var enumerator = sequence.GetEnumerator();
            var buffer = new Byte[_WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE];
            var isEndOfSequence = false;
            while (!isEndOfSequence)
            {
                var index = 0;
                while (index < buffer.Length)
                {
                    if (!enumerator.MoveNext())
                    {
                        isEndOfSequence = true;
                        break;
                    }

                    buffer[index++] = enumerator.Current;
                }

                if (index > 0)
                    destinationStream.Write(buffer, 0, index);
            }
        }

        public static void WriteByteSequence(this IBasicOutputByteStream destinationStream, IEnumerable<Byte> sequence)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            using var enumerator = sequence.GetEnumerator();
            var buffer = new Byte[_WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE];
            var isEndOfSequence = false;
            while (!isEndOfSequence)
            {
                var index = 0;
                while (index < buffer.Length)
                {
                    if (!enumerator.MoveNext())
                    {
                        isEndOfSequence = true;
                        break;
                    }

                    buffer[index++] = enumerator.Current;
                }

                if (index > 0)
                    destinationStream.WriteBytes(buffer.AsSpan(0, index));
            }
        }

        #endregion

        #region WriteInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LE(this Stream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LE(this IBasicOutputByteStream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LE(this Stream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LE(this IBasicOutputByteStream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LE(this Stream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LE(this IBasicOutputByteStream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LE(this Stream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LE(this IBasicOutputByteStream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LE(this Stream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LE(this IBasicOutputByteStream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LE(this Stream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LE(this IBasicOutputByteStream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLE(this Stream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLE(this IBasicOutputByteStream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLE(this Stream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLE(this IBasicOutputByteStream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalLE(this Stream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalLE(this IBasicOutputByteStream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BE(this Stream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BE(this IBasicOutputByteStream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BE(this Stream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BE(this IBasicOutputByteStream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BE(this Stream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BE(this IBasicOutputByteStream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BE(this Stream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BE(this IBasicOutputByteStream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BE(this Stream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BE(this IBasicOutputByteStream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BE(this Stream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BE(this IBasicOutputByteStream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleBE(this Stream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleBE(this IBasicOutputByteStream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleBE(this Stream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleBE(this IBasicOutputByteStream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalBE(this Stream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalBE(this IBasicOutputByteStream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region CalculateCrc24

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc24(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc24(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc24(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc24(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this IBasicInputByteStream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc24(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this IBasicInputByteStream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc24(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this IBasicInputByteStream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc24(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this IBasicInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc24(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        #endregion

        #region CalculateCrc32

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc32(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc32(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc32(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.AsInputByteStream(true).InternalCalculateCrc32(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this IBasicInputByteStream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc32(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this IBasicInputByteStream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc32(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this IBasicInputByteStream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc32(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this IBasicInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                return
                    inputStream is null
                    ? throw new ArgumentNullException(nameof(inputStream))
                    : inputStream.InternalCalculateCrc32(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalGetByteSequence<POSITION_T>(IInputByteStream<POSITION_T> baseStream, POSITION_T? offset, UInt64? count, IProgress<UInt64>? progress, Boolean leaveOpen)
            where POSITION_T : struct
        {
            const Int32 BUFFER_SIZE = 80 * 1024;

            try
            {
                if (baseStream is IRandomInputByteStream<POSITION_T> randomAccessStream)
                {
                    if (offset is not null)
                        randomAccessStream.Seek(offset.Value);
                }
                else
                {
                    if (offset is not null)
                        throw new ArgumentException($"{nameof(offset)} must be null if {nameof(baseStream)} is sequential.", nameof(offset));
                }

                try
                {
                    progress?.Report(0);
                }
                catch (Exception)
                {
                }

                var buffer = new Byte[BUFFER_SIZE];
                var processedCount = 0UL;
                while (true)
                {
                    try
                    {
                        var readCount = buffer.Length;
                        if (count is not null)
                            readCount = (Int32)((UInt64)readCount).Minimum(count.Value - processedCount);
                        if (readCount <= 0)
                            break;
                        var length = baseStream.Read(buffer, 0, readCount);
                        if (length <= 0)
                            break;
                        for (var index = 0; index < length; ++index)
                            yield return buffer[index];
                        processedCount += (UInt32)length;
                    }
                    finally
                    {
                        try
                        {
                            progress?.Report(processedCount);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            finally
            {
                if (!leaveOpen)
                    baseStream.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalStreamBytesEqual(this IBasicInputByteStream stream1, IBasicInputByteStream stream2, IProgress<UInt64>? progress)
        {
            const Int32 bufferSize = 81920;

            if (bufferSize % sizeof(UInt64) != 0)
                throw new InternalLogicalErrorException();

            try
            {
                progress?.Report(0);
            }
            catch (Exception)
            {
            }

            var buffer1 = new Byte[bufferSize];
            var buffer2 = new Byte[bufferSize];
            var processedCount = 0UL;
            while (true)
            {
                try
                {
                    // まず両方のストリームから bufferSize バイトだけ読み込みを試みる
                    var bufferCount1 = stream1.ReadBytes(buffer1);
                    var bufferCount2 = stream2.ReadBytes(buffer2);
                    processedCount += (UInt32)bufferCount1;

                    if (bufferCount1 != bufferCount2)
                    {
                        // 実際に読み込めたサイズが異なっている場合はどちらかだけがEOFに達したということなので、ストリームの内容が異なると判断しfalseを返す。
                        return false;
                    }

                    // この時点で bufferCount1 == bufferCount2 (どちらのストリームも読み込めたサイズは同じ)

                    if (!buffer1.SequenceEqual(0, buffer2, 0, bufferCount1))
                    {
                        // バッファの内容が一致しなかった場合は false を返す。
                        return false;
                    }

                    if (bufferCount1 < buffer1.Length)
                    {
                        // どちらのストリームも同時にEOFに達したがそれまでに読み込めたデータはすべて一致していた場合
                        // 全てのデータが一致したと判断して true を返す。
                        return true;
                    }
                }
                finally
                {
                    try
                    {
                        progress?.Report(processedCount);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyTo(this IBasicInputByteStream source, IBasicOutputByteStream destination, Int32 bufferSize, IProgress<UInt64>? progress)
        {
            try
            {
                progress?.Report(0);
            }
            catch (Exception)
            {
            }

            var buffer = new Byte[bufferSize];
            var processedCount = 0UL;
            while (true)
            {
                try
                {
                    var length = source.ReadBytes(buffer);
                    if (length <= 0)
                        break;
                    destination.WriteBytes(buffer.AsSpan(0, length).AsReadOnly());
                    processedCount += (UInt32)length;
                }
                finally
                {
                    try
                    {
                        progress?.Report(processedCount);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            destination.Flush();
        }

        #region InternalReadBytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalReadBytes(Int32 offset, Int32 count, Func<Int32, Int32, Int32> reader)
        {
            var index = 0;
            while (count > 0)
            {
                var length = reader(offset + index, count);
                if (length <= 0)
                    break;
                index += length;
                count -= length;
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalReadBytes(Memory<Byte> buffer, Func<Memory<Byte>, Int32> reader)
        {
            var index = 0;
            while (!buffer.IsEmpty)
            {
                var length = reader(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                index += length;
            }

            return index;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalReadByteSequence(UInt64 count, Func<Int32, IEnumerable<Byte>?> reader)
        {
            var byteArrayChain = Array.Empty<Byte>().AsEnumerable();
            while (count > 0)
            {
                var length = (Int32)count.Minimum((UInt32)Int32.MaxValue);
                var data = reader(length);
                if (data is null)
                    break;
                byteArrayChain = byteArrayChain.Concat(data);
                count -= (UInt32)length;
            }

            return byteArrayChain;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlyMemory<Byte> InternalReadAllBytes(Func<Byte[], Int32, Int32, Int32> reader)
        {
            const Int32 BUFFER_SIZE = 80 * 2024;
            var buffers = new Queue<Byte[]>();
            var dataLength = 0;
            while (true)
            {
                var partialBuffer = new Byte[BUFFER_SIZE];
                var length = reader(partialBuffer, 0, partialBuffer.Length);
                if (length <= 0)
                    break;
                if (length < partialBuffer.Length)
                    Array.Resize(ref partialBuffer, length);
                buffers.Enqueue(partialBuffer);
                dataLength += length;
            }

            if (buffers.Count <= 0)
            {
                return ReadOnlyMemory<Byte>.Empty;
            }
            else if (buffers.Count == 1)
            {
                return buffers.Dequeue();
            }
            else
            {
                var buffer = new Byte[dataLength];
                var destinationWindow = buffer.AsMemory();
                while (buffers.Count > 0)
                {
                    var partialBuffer = buffers.Dequeue();
                    partialBuffer.CopyTo(destinationWindow);
                    destinationWindow = destinationWindow[partialBuffer.Length..];
                }
#if DEBUG
                if (!destinationWindow.IsEmpty)
                    throw new Exception();
#endif
                return buffer;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalWriteBytes(Int32 offset, Int32 count, Func<Int32, Int32, Int32> writer)
        {
            var index = 0;
            while (count > 0)
            {
                var length = writer(offset + index, count);
                if (length <= 0)
                    break;
                index += length;
                count -= length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc24(this IBasicInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress)
            => InternalCalculateCrc(inputStream, bufferSize, progress, Crc24.CreateCalculationState());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc32(this IBasicInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress)
            => InternalCalculateCrc(inputStream, bufferSize, progress, Crc32.CreateCalculationState());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc(IBasicInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, ICrcCalculationState<UInt32, UInt64> session)
        {
            try
            {
                progress?.Report(0);
            }
            catch (Exception)
            {
            }

            var buffer = new Byte[bufferSize];
            var processedCount = 0UL;
            while (true)
            {
                try
                {
                    var length = inputStream.Read(buffer);
                    if (length <= 0)
                        break;
                    session.Put(buffer, 0, length);
                    processedCount += (UInt32)length;
                }
                finally
                {
                    try
                    {
                        progress?.Report(processedCount);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return session.GetResult();
        }
    }
}
