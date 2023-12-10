using System;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IHierarchicalDecoder
    {
        ISequentialInputByteStream GetDecodingStream(
            ISequentialInputByteStream baseStream,
            ICoderOption option,
            UInt64 unpackedStreamSize,
            UInt64 packedStreamSize,
            IProgress<UInt64>? unpackedCountProgress,
            Boolean leaveOpen = false);
    }
}
