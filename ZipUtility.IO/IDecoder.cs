using System;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IDecoder
    {
        void Decode(
            IBasicInputByteStream sourceStream,
            IBasicOutputByteStream destinationStream,
            ICoderOption option,
            UInt64 unpackedSize,
            UInt64 packedSize,
            IProgress<UInt64>? unpackedCountProgress = null);

        Task<Exception?> DecodeAsync(
            IBasicInputByteStream sourceStream,
            IBasicOutputByteStream destinationStream,
            ICoderOption option,
            UInt64 unpackedSize,
            UInt64 packedSize,
            IProgress<UInt64>? unpackedCountProgress = null,
            CancellationToken cancellationToken = default);
    }
}
