namespace ZipUtility
{
    internal static class ZipEntryCompressionMethodIdExtensions
    {
        public static ZipEntryCompressionMethod GetCompressionMethod(this ZipEntryCompressionMethodId compressionMethodId, ZipEntryGeneralPurposeBitFlag flag)
            => ZipEntryCompressionMethod.GetCompressionMethod(compressionMethodId, flag);

        public static ZipEntryCompressionMethod GetCompressionMethod(this ZipEntryCompressionMethodId compressionMethodId, ZipEntryCompressionLevel level)
            => ZipEntryCompressionMethod.GetCompressionMethod(
                compressionMethodId,
                level switch
                {
                    ZipEntryCompressionLevel.Maximum => ZipEntryGeneralPurposeBitFlag.CompresssionOption0,
                    ZipEntryCompressionLevel.Fast => ZipEntryGeneralPurposeBitFlag.CompresssionOption1,
                    ZipEntryCompressionLevel.SuperFast => ZipEntryGeneralPurposeBitFlag.CompresssionOption0 | ZipEntryGeneralPurposeBitFlag.CompresssionOption1,
                    _ => ZipEntryGeneralPurposeBitFlag.None,
                });
    }
}
