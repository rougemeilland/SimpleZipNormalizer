using System;
using System.IO;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO.StreamFilters
{
    internal class DotNetStreamBySequentialInputByteStream
        : Stream
    {
        private readonly ISequentialInputByteStream _basicStream;
        private readonly Boolean _leaveOpen;
        private readonly IRandomInputByteStream<UInt64>? _randomAccessStream;

        private Boolean _isDisposed;

        public DotNetStreamBySequentialInputByteStream(ISequentialInputByteStream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _basicStream = baseStream;
                _leaveOpen = leaveOpen;
                _isDisposed = false;
                _randomAccessStream = baseStream as IRandomInputByteStream<UInt64>;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public override Boolean CanSeek => _randomAccessStream is not null;
        public override Boolean CanRead => true;
        public override Boolean CanWrite => false;

        public override Int64 Length
        {
            get
            {
                if (_randomAccessStream is null)
                    throw new NotSupportedException();
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return checked((Int64)_randomAccessStream.Length);
            }
        }

        public override void SetLength(Int64 value) => throw new NotSupportedException();

        public override Int64 Position
        {
            get
            {
                if (_randomAccessStream is null)
                    throw new NotSupportedException();
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return checked((Int64)_randomAccessStream.Position);
            }

            set
            {
                if (_randomAccessStream is null)
                    throw new NotSupportedException();
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _randomAccessStream.Seek(checked((UInt64)value));
            }
        }

        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            if (_randomAccessStream is null)
                throw new NotSupportedException();
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            UInt64 absoluteOffset;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                        throw new ArgumentOutOfRangeException(nameof(offset));

                    absoluteOffset = checked((UInt64)offset);
                    break;
                case SeekOrigin.Current:
                    try
                    {
                        absoluteOffset = _randomAccessStream.Position.AddAsUInt(offset);
                    }
                    catch (OverflowException ex)
                    {
                        throw new ArgumentOutOfRangeException($"Invalid {nameof(offset)} value", ex);
                    }

                    break;
                case SeekOrigin.End:
                    try
                    {
                        absoluteOffset = _randomAccessStream.Length.AddAsUInt(offset);
                    }
                    catch (OverflowException ex)
                    {
                        throw new ArgumentOutOfRangeException($"Invalid {nameof(offset)} value", ex);
                    }

                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(SeekOrigin)} value : {nameof(origin)}=\"{origin}\"", nameof(origin));
            }

            if (absoluteOffset > Int64.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _randomAccessStream.Seek(absoluteOffset);
            return checked((Int64)absoluteOffset);
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _basicStream.Read(buffer.AsSpan(offset, count));
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count) => throw new NotSupportedException();

        public override void Flush() { }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                        _basicStream.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    _basicStream.Dispose();
                _isDisposed = true;
            }

            await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}
