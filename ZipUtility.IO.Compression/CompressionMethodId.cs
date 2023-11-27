namespace ZipUtility.IO.Compression
{
    public enum CompressionMethodId
    {
        Unknown = -1,
        Stored = 0,
        Deflate = 1,
        Deflate64 = 2,
        BZIP2 = 3,
        LZMA = 4,
        PPMd = 5,
    }
}
