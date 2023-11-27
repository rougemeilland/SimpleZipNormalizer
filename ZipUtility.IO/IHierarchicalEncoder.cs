using System;
using Utility.IO;

namespace ZipUtility.IO
{
    public interface IHierarchicalEncoder
    {
        IBasicOutputByteStream GetEncodingStream(IBasicOutputByteStream baseStream, ICoderOption option, UInt64? unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress = null);
    }
}
