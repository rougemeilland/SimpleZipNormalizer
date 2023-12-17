using System;

namespace ZipUtility.ExtraFields
{
    internal class Zip64ExtendedInformationExtraFieldForCentraHeader
        : Zip64ExtendedInformationExtraField
    {
        public Zip64ExtendedInformationExtraFieldForCentraHeader()
            : base(ZipEntryHeaderType.CentralDirectoryHeader)
        {
        }

        public (UInt32 rawSize, UInt32 rawPackedSize, UInt32 rawRelatiiveHeaderOffset, UInt16 rawDiskStartNumber) SetValues(UInt64 size, UInt64 packedSize, UInt64 relatiiveHeaderOffset, UInt32 diskStartNumber)
            => InternalSetValues(size, packedSize, relatiiveHeaderOffset, diskStartNumber);

        public (UInt64 size, UInt64 packedSize, UInt64 relatiiveHeaderOffset, UInt32 diskStartNumber) GetValues(UInt32 rawSize, UInt32 rawPackedSize, UInt32 rawRelatiiveHeaderOffset, UInt16 rawDiskStartNumber)
            => InternalGetValues(rawSize, rawPackedSize, rawRelatiiveHeaderOffset, rawDiskStartNumber);
    }
}
