using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public interface IOutputBitStream
        : IDisposable, IAsyncDisposable
    {
        void Write(Boolean bit);
        Task WriteAsync(Boolean bit, CancellationToken cancellationToken = default);
        void Write(TinyBitArray bitArray);
        Task WriteAsync(TinyBitArray bitArray, CancellationToken cancellationToken = default);
        void Flush();
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
}
