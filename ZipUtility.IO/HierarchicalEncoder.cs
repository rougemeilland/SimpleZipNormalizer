using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility.IO
{
    public abstract class HierarchicalEncoder
        : IOutputByteStream<UInt64>
    {
        private readonly IBasicOutputByteStream _baseStream;
        private readonly UInt64? _size;
        private readonly ProgressCounterUInt64 _unpackedSizeCounter;

        private Boolean _isDisposed;
        private Boolean _isEndOfWriting;

        public HierarchicalEncoder(IBasicOutputByteStream baseStream, UInt64? size, IProgress<UInt64>? unpackedCountProgress)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isDisposed = false;
            _baseStream = baseStream;
            _size = size;
            _unpackedSizeCounter = new ProgressCounterUInt64(unpackedCountProgress);
            _isEndOfWriting = false;
        }

        public UInt64 Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _unpackedSizeCounter.Value;
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_unpackedSizeCounter.Value <= 0)
                _unpackedSizeCounter.Report();
            if (buffer.Length <= 0)
                return 0;
            var written = WriteToDestinationStream(_baseStream, buffer);
            UpdatePosition(written);
            if (_size is not null && _unpackedSizeCounter.Value >= _size.Value)
            {
                FlushDestinationStream(_baseStream, true);
                _isEndOfWriting = true;
            }

            return written;
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_unpackedSizeCounter.Value <= 0)
                _unpackedSizeCounter.Report();
            if (buffer.Length <= 0)
                return 0;
            var written = await WriteToDestinationStreamAsync(_baseStream, buffer, cancellationToken).ConfigureAwait(false);
            UpdatePosition(written);
            if (_size is not null && _unpackedSizeCounter.Value >= _size.Value)
            {
                await FlushDestinationStreamAsync(_baseStream, true, cancellationToken).ConfigureAwait(false);
                _isEndOfWriting = true;
            }

            return written;
        }

        public void Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            try
            {
                FlushDestinationStream(_baseStream, false);
            }
            catch (IOException)
            {
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                throw new IOException("Stream is closed.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException("Can not flush stream.", ex);
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            try
            {
                await FlushDestinationStreamAsync(_baseStream, false, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException)
            {
                throw;
            }
            catch (ObjectDisposedException ex)
            {
                throw new IOException("Stream is closed.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException("Can not flush stream.", ex);
            }
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
                    try
                    {
                        FlushDestinationStream(_baseStream, true);
                        _isEndOfWriting = true;
                    }
                    catch (Exception)
                    {
                    }

                    _baseStream.Dispose();
                }

                _isDisposed = true;

            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                try
                {
                    await FlushDestinationStreamAsync(_baseStream, true, default).ConfigureAwait(false);
                    _isEndOfWriting = true;
                }
                catch (Exception)
                {
                }

                await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        protected virtual Int32 WriteToDestinationStream(IBasicOutputByteStream destinationStream, ReadOnlySpan<Byte> buffer)
            => destinationStream.Write(buffer);

        protected virtual Task<Int32> WriteToDestinationStreamAsync(IBasicOutputByteStream destinationStream, ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            => destinationStream.WriteAsync(buffer, cancellationToken);

        protected virtual void FlushDestinationStream(IBasicOutputByteStream destinationStream, Boolean isEndOfData)
        {
            if (!_isEndOfWriting)
                destinationStream.Flush();
        }

        protected virtual Task FlushDestinationStreamAsync(IBasicOutputByteStream destinationStream, Boolean isEndOfData, CancellationToken cancellationToken)
            => !_isEndOfWriting
                ? destinationStream.FlushAsync(cancellationToken)
                : Task.CompletedTask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePosition(Int32 written)
        {
            if (written > 0)
                _unpackedSizeCounter.AddValue((UInt32)written);
        }
    }
}
