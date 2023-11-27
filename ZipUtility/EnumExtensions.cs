using System;

namespace ZipUtility
{
    static class EnumExtensions
    {
        public static Boolean HasEncryptionFlag(this ZipEntryGeneralPurposeBitFlag flag)
            => (flag &
                    (ZipEntryGeneralPurposeBitFlag.Encrypted |
                     ZipEntryGeneralPurposeBitFlag.EncryptedCentralDirectory |
                     ZipEntryGeneralPurposeBitFlag.StrongEncrypted))
                != ZipEntryGeneralPurposeBitFlag.None;

        public static Int32 GetCompressionOptionValue(this ZipEntryGeneralPurposeBitFlag flag)
            => ((UInt16)flag >> 1) & 3;
    }
}
