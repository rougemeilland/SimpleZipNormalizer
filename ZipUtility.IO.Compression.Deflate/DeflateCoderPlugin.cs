namespace ZipUtility.IO.Compression.Deflate
{
    public abstract class DeflateCoderPlugin
        : ICompressionCoder
    {
        public CompressionMethodId CompressionMethodId => CompressionMethodId.Deflate;

        public ICoderOption DefaultOption
            => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Normal);

        public ICoderOption GetOptionFromGeneralPurposeFlag(Boolean bit1, Boolean bit2)
            => bit2
                ? bit1
                    ? CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.SuperFast)
                    : CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Fast)
                : bit1
                    ? CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Maximum)
                    : CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Normal);
    }
}
