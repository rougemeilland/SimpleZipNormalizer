using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public class NullOutputStream
        : IRandomOutputByteStream<UInt64>
    {
        public UInt64 Length { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public UInt64 Position => throw new NotSupportedException();

        public void Seek(UInt64 offset) => throw new NotSupportedException();

        public Int32 Write(ReadOnlySpan<Byte> buffer) => buffer.Length;

        public Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(buffer.Length);
        }

        public void Flush()
        {
        }

        public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Dispose() => GC.SuppressFinalize(this);

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}
