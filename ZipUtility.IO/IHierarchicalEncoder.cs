using System;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IHierarchicalEncoder
    {
        ISequentialOutputByteStream GetEncodingStream(
            ISequentialOutputByteStream baseStream,
            ICoderOption option,
            IProgress<UInt64>? unpackedCountProgress,
            Boolean leaveOpen = false);
    }
}
