using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public class NullOutputStream
        : RandomOutputByteStream<UInt64>
    {
        protected override UInt64 PositionCore => throw new NotSupportedException();
        protected override UInt64 StartOfThisStreamCore => 0;
        protected override UInt64 LengthCore { get => throw new NotSupportedException(); set { } }
        protected override void SeekCore(UInt64 position) { }
        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer) => buffer.Length;

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(buffer.Length);
        }

        protected override void FlushCore() { }
        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
