using System;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IEncoder
    {
        void Encode(
            IBasicInputByteStream sourceStream,
            IBasicOutputByteStream destinationStream,
            ICoderOption option,
            UInt64? sourceSize,
            IProgress<UInt64>? unpackedCountProgress = null);

        Task<Exception?> EncodeAsync(
            IBasicInputByteStream sourceStream,
            IBasicOutputByteStream destinationStream,
            ICoderOption option,
            UInt64? sourceSize,
            IProgress<UInt64>? unpackedCountProgress = null,
            CancellationToken cancellationToken = default);
    }
}
