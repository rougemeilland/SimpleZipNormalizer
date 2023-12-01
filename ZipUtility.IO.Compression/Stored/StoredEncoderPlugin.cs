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
            public Encoder(IBasicOutputByteStream baseStream, UInt64? unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress)
                : base(baseStream, unpackedStreamSize, unpackedCountProgress)
            {
            }
        }

        IOutputByteStream<UInt64> IHierarchicalEncoder.GetEncodingStream(
            IBasicOutputByteStream baseStream,
            ICoderOption option,
            UInt64? unpackedStreamSize,
            IProgress<UInt64>? unpackedCountProgress)
            => new Encoder(baseStream, unpackedStreamSize, unpackedCountProgress);
    }
}
