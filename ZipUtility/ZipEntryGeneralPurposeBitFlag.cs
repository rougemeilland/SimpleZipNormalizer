using System;

namespace ZipUtility
{
    [Flags]
    internal enum ZipEntryGeneralPurposeBitFlag
        : UInt16
    {
        None = 0,
        Encrypted = 1 << 0,
        CompresssionOption0 = 1 << 1,
        CompresssionOption1 = 1 << 2,
        HasDataDescriptor = 1 << 3,
        CompressedPatchedData = 1 << 5,
        StrongEncrypted = 1 << 6,
        UseUnicodeEncodingForNameAndComment = 1 << 11,
        EncryptedCentralDirectory = 1 << 13
    }
}
