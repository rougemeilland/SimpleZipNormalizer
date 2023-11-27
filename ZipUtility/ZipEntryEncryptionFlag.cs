using System;

namespace ZipUtility
{
    [Flags]
    internal enum ZipEntryEncryptionFlag
        : UInt16
    {
        None = 0,
        RequiredPassword = 1 << 0,
        RequiredCertification = 1 << 1,
    }
}
