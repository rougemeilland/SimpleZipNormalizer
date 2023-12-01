using System;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IHierarchicalDecoder
    {
        IInputByteStream<UInt64> GetDecodingStream(
            IBasicInputByteStream baseStream,
            ICoderOption option,
            UInt64 unpackedStreamSize,
            UInt64 packedStreamSize,
            IProgress<UInt64>? unpackedCountProgress = null);
    }
}
