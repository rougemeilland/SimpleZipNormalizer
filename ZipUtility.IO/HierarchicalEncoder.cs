using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using Utility.IO.StreamFilters;

namespace ZipUtility.IO
{
    public abstract class HierarchicalEncoder
        : SequentialOutputByteStreamFilter
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly ProgressCounterUInt64 _unpackedSizeCounter;

        private Boolean _isDisposed;
        private Boolean _isEndOfWriting;

        public HierarchicalEncoder(ISequentialOutputByteStream baseStream, IProgress<UInt64>? unpackedCountProgress, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isDisposed = false;
            _baseStream = baseStream;
            _unpackedSizeCounter = new ProgressCounterUInt64(unpackedCountProgress);
            _isEndOfWriting = false;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            _unpackedSizeCounter.ReportIfInitial();
            if (buffer.Length <= 0)
                return 0;
            var written = WriteToDestinationStream(_baseStream, buffer);
            if (written > 0)
                _unpackedSizeCounter.AddValue(checked((UInt32)written));
            return written;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            _unpackedSizeCounter.ReportIfInitial();
            if (buffer.Length <= 0)
                return 0;
            var written = await WriteToDestinationStreamAsync(_baseStream, buffer, cancellationToken).ConfigureAwait(false);
            if (written > 0)
                _unpackedSizeCounter.AddValue(checked((UInt32)written));
            return written;
        }

        protected override void FlushCore()
        {
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

        protected override async Task FlushAsyncCore(CancellationToken cancellationToken = default)
        {
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

        protected override void Dispose(Boolean disposing)
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

                _unpackedSizeCounter.Report();
                _isDisposed = true;

            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
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
                _unpackedSizeCounter.Report();
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        protected virtual Int32 WriteToDestinationStream(ISequentialOutputByteStream destinationStream, ReadOnlySpan<Byte> buffer)
            => destinationStream.Write(buffer);

        protected virtual Task<Int32> WriteToDestinationStreamAsync(ISequentialOutputByteStream destinationStream, ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            => destinationStream.WriteAsync(buffer, cancellationToken);

        protected virtual void FlushDestinationStream(ISequentialOutputByteStream destinationStream, Boolean isEndOfData)
        {
            if (!_isEndOfWriting)
                destinationStream.Flush();
        }

        protected virtual Task FlushDestinationStreamAsync(ISequentialOutputByteStream destinationStream, Boolean isEndOfData, CancellationToken cancellationToken)
            => !_isEndOfWriting
                ? destinationStream.FlushAsync(cancellationToken)
                : Task.CompletedTask;
    }
}
