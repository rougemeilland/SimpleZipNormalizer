using System;

namespace ZipUtility.IO.Compression
{
    public interface ICompressionCoder
    {
        CompressionMethodId CompressionMethodId { get; }
        ICoderOption DefaultOption { get; }
        ICoderOption GetOptionFromGeneralPurposeFlag(Boolean bit1, Boolean bit2);
    }
}
