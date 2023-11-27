namespace ZipUtility.IO.Compression
{
    public static class CompressionOption
    {
        private class EmptyCoderOption
            : ICoderOption
        {
        }

        static CompressionOption()
        {
            EmptyOption = new EmptyCoderOption();

        }

        public static ICoderOption EmptyOption { get; }

        public static ICoderOption GetDeflateCompressionOption(DeflateCompressionLevel level)
            => new DeflateCompressionOption { CompressionLevel = level };

        public static ICoderOption GetLzmaCompressionOption(System.Boolean useEndOfStreamMarker)
            => new LzmaCompressionOption { UseEndOfStreamMarker = useEndOfStreamMarker };
    }
}
