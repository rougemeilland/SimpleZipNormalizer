using System.IO.Compression;
using Utility.IO;

namespace ZipUtility.IO.Compression.Deflate
{
    public class DeflateDecoderPlugin
        : DeflateCoderPlugin, ICompressionHierarchicalDecoder
    {
        private class Decoder
            : HierarchicalDecoder
        {
            public Decoder(IBasicInputByteStream baseStream, UInt64 unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress)
                : base(GetBaseStream(baseStream, unpackedStreamSize), unpackedStreamSize, unpackedCountProgress)
            {
            }

            protected override Int32 ReadFromSourceStream(IBasicInputByteStream sourceStream, Span<Byte> buffer)
            {
                try
                {
                    return base.ReadFromSourceStream(sourceStream, buffer);
                }
                catch (Exception ex)
                {
                    throw new DataErrorException("Detected data error", ex);
                }
            }

            protected override async Task<Int32> ReadFromSourceStreamAsync(IBasicInputByteStream sourceStream, Memory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                try
                {
                    return await base.ReadFromSourceStreamAsync(sourceStream, buffer, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new DataErrorException("Detected data error", ex);
                }
            }

            private static IBasicInputByteStream GetBaseStream(IBasicInputByteStream baseStream, UInt64 unpackedStreamSize)
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

#if true
                return
                    new DeflateStream(baseStream.AsStream(), CompressionMode.Decompress)
                    .AsInputByteStream();
#else
                return
                    new DeflateDecoderStream(
                        baseStream.WithCache(),
                        unpackedStreamSize,
                        null,
                        false);
#endif
            }
        }

        public IBasicInputByteStream GetDecodingStream(IBasicInputByteStream baseStream, ICoderOption option, UInt64 unpackedStreamSize, UInt64 packedStreamSize, IProgress<UInt64>? unpackedCountProgress = null)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (option is null)
                throw new ArgumentNullException(nameof(option));
            if (option is not DeflateCompressionOption)
                throw new ArgumentException($"Illegal {nameof(option)} data", nameof(option));

            return new Decoder(baseStream, unpackedStreamSize, unpackedCountProgress);
        }
    }
}
