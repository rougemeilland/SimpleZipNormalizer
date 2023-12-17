using System;

namespace ZipUtility.ExtraFields
{
    internal class Zip64ExtendedInformationExtraFieldForLocalHeader
        : Zip64ExtendedInformationExtraField
    {
        public Zip64ExtendedInformationExtraFieldForLocalHeader()
            : base(ZipEntryHeaderType.LocalHeader)
        {
        }

        public (UInt32 rawSize, UInt32 rawPackedSize) SetValues(UInt64 size, UInt64 packedSize)
            => InternalSetValues(size, packedSize);

        public (UInt64 size, UInt64 packedSize) GetValues(UInt32 rawSize, UInt32 rawPackedSize)
            => InternalGetValues(rawSize, rawPackedSize);
    }
}
