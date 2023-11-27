namespace ZipUtility.ZipFileHeader
{
    class ZipFileLastDiskHeader
    {
        private ZipFileLastDiskHeader(ZipFileEOCDR eocdr, ZipFileZip64EOCDL? zip64EOCDL)
        {
            EOCDR = eocdr;
            Zip64EOCDL = zip64EOCDL;
        }

        public ZipFileEOCDR EOCDR { get; }
        public ZipFileZip64EOCDL? Zip64EOCDL { get; }

        public static ZipFileLastDiskHeader Parse(IZipInputStream zipInputStream)
        {
            using var lastDiskInputStream = zipInputStream.AsRandomPartial(zipInputStream.LastDiskStartPosition, zipInputStream.LastDiskSize);
            var eocdr = ZipFileEOCDR.Find(lastDiskInputStream);
            var zip64EOCDL = ZipFileZip64EOCDL.Find(lastDiskInputStream, eocdr);
            return new ZipFileLastDiskHeader(eocdr, zip64EOCDL);
        }
    }
}
