using System;
using Utility.IO;

namespace ZipUtility.IO.Compression.Stored
{
    public class StoredEncoderPlugin
        : StoredCoderPlugin, ICompressionHierarchicalEncoder
    {
        private class Encoder
            : HierarchicalEncoder
        {
            public Encoder(ISequentialOutputByteStream baseStream, IProgress<UInt64>? unpackedCountProgress, Boolean leaveOpen)
                : base(baseStream, unpackedCountProgress, leaveOpen)
            {
            }
        }

        ISequentialOutputByteStream IHierarchicalEncoder.GetEncodingStream(
            ISequentialOutputByteStream baseStream,
            ICoderOption option,
            IProgress<UInt64>? unpackedCountProgress,
            Boolean leaveOpen)
            => new Encoder(baseStream, unpackedCountProgress, leaveOpen);
    }
}
