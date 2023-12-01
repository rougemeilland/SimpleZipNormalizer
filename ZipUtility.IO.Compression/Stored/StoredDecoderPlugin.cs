using System;
using Utility.IO;

namespace ZipUtility.IO.Compression.Stored
{
    public class StoredDecoderPlugin
        : StoredCoderPlugin, ICompressionHierarchicalDecoder
    {
        private class Decoder
            : HierarchicalDecoder
        {
            public Decoder(IBasicInputByteStream baseStream, UInt64 unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress)
                : base(baseStream, unpackedStreamSize, unpackedCountProgress)
            {
            }
        }

        IInputByteStream<UInt64> IHierarchicalDecoder.GetDecodingStream(
            IBasicInputByteStream baseStream,
            ICoderOption option,
            UInt64 unpackedStreamSize,
            UInt64 packedStreamSize,
            IProgress<UInt64>? unpackedCountProgress)
            => new Decoder(baseStream, unpackedStreamSize, unpackedCountProgress);
    }
}
