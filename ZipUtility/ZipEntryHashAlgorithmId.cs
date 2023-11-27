using System;

namespace ZipUtility
{
    internal enum ZipEntryHashAlgorithmId
        : UInt16
    {
        None = 0x0000,
        Crc32 = 0x0001,
        Md5 = 0x8003,
        Sha1 = 0x8004,
        Ripemd160 = 0x8007,
        Sha256 = 0x800c,
        Sha384 = 0x800d,
        Sha512 = 0x800e,
    }
}
