using System;
using System.Threading.Tasks;
using System.Threading;
using Utility.IO;
using Utility;

namespace ZipUtility.IO
{
    public static class StreamExtensions
    {
        private class StreamSizeCounter
        {
            public StreamSizeCounter()
            {
                TotalCount = 0;
            }

            public UInt64 TotalCount { get; private set; }

            public void AddToCounter(Int32 count)
            {
                checked
                {
                    TotalCount += (UInt64)count;
                }
            }
        }

        private class CountFilterInputStream
            : IBasicInputByteStream
        {
            private readonly IBasicInputByteStream _baseStream;
            private readonly StreamSizeCounter _counter;
            private Boolean _isDisposed;

            public CountFilterInputStream(IBasicInputByteStream baseStream, StreamSizeCounter counter)
            {
                _baseStream = baseStream;
                _isDisposed = false;
                _counter = counter;
            }

            public Int32 Read(Span<Byte> buffer)
            {
                var length = _baseStream.Read(buffer);
                _counter.AddToCounter(length);
                return length;
            }

            public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                _counter.AddToCounter(length);
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
                        _baseStream.Dispose();
                    _isDisposed = true;
                }
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                    _isDisposed = true;
                }
            }
        }

        private class CountFilterOutputStream
            : IBasicOutputByteStream
        {
            private readonly IBasicOutputByteStream _baseStream;
            private readonly StreamSizeCounter _counter;
            private Boolean _isDisposed;

            public CountFilterOutputStream(IBasicOutputByteStream baseStream, StreamSizeCounter counter)
            {
                _baseStream = baseStream;
                _isDisposed = false;
                _counter = counter;
            }

            public Int32 Write(ReadOnlySpan<Byte> buffer)
            {
                var length = _baseStream.Write(buffer);
                _counter.AddToCounter(length);
                return length;
            }

            public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                var length = await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                _counter.AddToCounter(length);
                return length;
            }

            public void Flush() => _baseStream.Flush();
            public Task FlushAsync(CancellationToken cancellationToken = default) => _baseStream.FlushAsync(cancellationToken);

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
                        _baseStream.Dispose();
                    _isDisposed = true;
                }
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                    _isDisposed = true;
                }
            }
        }

        public static (IBasicInputByteStream baseStream, IProgress<UInt64>? unpackedCountProgress) CreateProgressFilter(this IBasicInputByteStream baseStream, IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress)
        {
            if (progress is null)
                return (baseStream, null);
            var packedCounter = new StreamSizeCounter();
            return
                (
                    baseStream:
                        new CountFilterInputStream(baseStream, packedCounter),
                    unpackedCountProgress:
                        SafetyProgress.CreateIncreasingProgress<UInt64, (UInt64 unpackedCount, UInt64 packedCount)>(
                            progress,
                            unpackedCount => (unpackedCount, packedCounter.TotalCount))
                );
        }

        public static (IBasicOutputByteStream baseStream, IProgress<UInt64>? unpackedCountProgress) CreateProgressFilter(
            this IBasicOutputByteStream baseStream,
            IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress)
        {
            if (progress is null)
                return (baseStream, null);
            var packedCounter = new StreamSizeCounter();
            return
                (
                    baseStream:
                        new CountFilterOutputStream(baseStream, packedCounter),
                    unpackedCountProgress:
                        SafetyProgress.CreateIncreasingProgress<UInt64, (UInt64 unpackedCount, UInt64 packedCount)>(
                            progress,
                            unpackedCount => (unpackedCount, packedCounter.TotalCount))
                );
        }
    }
}
