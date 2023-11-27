using System;

namespace ZipUtility.IO.Compression
{
    public class LzmaCompressionOption
        : ICoderOption
    {
        public Boolean UseEndOfStreamMarker { get; set; }
    }
}
