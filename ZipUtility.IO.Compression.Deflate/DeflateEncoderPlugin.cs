using System.IO.Compression;
using Utility.IO;

namespace ZipUtility.IO.Compression.Deflate
{
    public class DeflateEncoderPlugin
        : DeflateCoderPlugin, ICompressionHierarchicalEncoder
    {
        private class Encoder
            : HierarchicalEncoder
        {
            public Encoder(IBasicOutputByteStream baseStream, CompressionLevel level, UInt64? unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress)
                : base(GetBaseStream(baseStream, level), unpackedStreamSize, unpackedCountProgress)
            {
            }

            private static IBasicOutputByteStream GetBaseStream(IBasicOutputByteStream baseStream, CompressionLevel level)
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

#if true
                return new DeflateStream(baseStream.AsStream(), level).AsOutputByteStream();
#else
                return
                    new DeflateEncoderStream(
                        baseStream.WithCache(),
                        new DeflateEncoderProperties
                        {
                            Level = level,
                        },
                        null,
                        false);
#endif
            }

            protected override void FlushDestinationStream(IBasicOutputByteStream destinationStream, Boolean isEndOfData)
            {
                if (isEndOfData)
                    destinationStream.Dispose();
            }
        }

        public IBasicOutputByteStream GetEncodingStream(IBasicOutputByteStream baseStream, ICoderOption option, UInt64? unpackedStreamSize, IProgress<UInt64>? unpackedCountProgress = null)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (option is null)
                throw new ArgumentNullException(nameof(option));
            if (option is not DeflateCompressionOption deflateOption)
                throw new ArgumentException($"Illegal {nameof(option)} data", nameof(option));
            if (deflateOption.CompressionLevel is < DeflateCompressionLevel.Minimum or > DeflateCompressionLevel.Maximum)
                throw new ArgumentException($"Illegal {nameof(option)}.CompressionLevel value", nameof(option));

            var level = deflateOption.CompressionLevel switch
            {
                DeflateCompressionLevel.Level0 or DeflateCompressionLevel.Level1 or DeflateCompressionLevel.Level2 or DeflateCompressionLevel.Level3 => CompressionLevel.Fastest,
                DeflateCompressionLevel.Level7 or DeflateCompressionLevel.Level8 or DeflateCompressionLevel.Level9 => CompressionLevel.SmallestSize,
                _ => CompressionLevel.Optimal,
            };
            return new Encoder(baseStream, level, unpackedStreamSize, unpackedCountProgress);
        }
    }
}
