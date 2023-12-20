using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZipUtility
{
    internal class VolumeDiskStreamCache<STREAM_T>
        : IDisposable, IAsyncDisposable
        where STREAM_T : IDisposable, IAsyncDisposable
    {
        private readonly Int32 _capacity;
        private readonly Func<UInt32, STREAM_T> _newStreamGetter;
        private readonly LinkedList<(UInt32 diskNumber, STREAM_T stream)> _streams;
        private Boolean _isDisposed;

        public VolumeDiskStreamCache(Int32 capacity, Func<UInt32, STREAM_T> newStreamGetter)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _newStreamGetter = newStreamGetter ?? throw new ArgumentNullException(nameof(newStreamGetter));
            _streams = new LinkedList<(UInt32 diskNumber, STREAM_T stream)>();
            _isDisposed = false;
        }

        public STREAM_T GetStream(UInt32 diskNumber)
        {
            var node = _streams.First;
            while (node != null)
            {
                if (node.Value.diskNumber == diskNumber)
                {
                    var stream = node.Value.stream;
                    _streams.Remove(node);
                    _ = _streams.AddFirst((diskNumber, stream));
                    return stream;
                }

                node = node.Next;
            }

            var newStream = _newStreamGetter(diskNumber);
            _ = _streams.AddFirst((diskNumber, newStream));
            while (_streams.Count > _capacity && _streams.Last is not null)
            {
                _streams.Last.Value.stream.Dispose();
                _streams.RemoveLast();
            }

            return newStream;
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
                    while (_streams.First != null)
                    {
                        _streams.First.Value.stream.Dispose();
                        _streams.RemoveFirst();
                    }
                }

                _isDisposed = true;
            }
        }

        protected async virtual Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                while (_streams.First != null)
                {
                    await _streams.First.Value.stream.DisposeAsync().ConfigureAwait(false);
                    _streams.RemoveFirst();
                }

                _isDisposed = true;
            }
        }
    }
}
