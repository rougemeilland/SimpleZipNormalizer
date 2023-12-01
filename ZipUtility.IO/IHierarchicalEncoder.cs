using System;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IHierarchicalEncoder
    {
        IOutputByteStream<UInt64> GetEncodingStream(
            IBasicOutputByteStream baseStream,
            ICoderOption option,
            UInt64? unpackedStreamSize,
            IProgress<UInt64>? unpackedCountProgress = null);
    }
}
