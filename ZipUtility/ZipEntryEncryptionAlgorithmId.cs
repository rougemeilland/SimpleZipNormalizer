using System;

namespace ZipUtility
{
    internal enum ZipEntryEncryptionAlgorithmId
        : UInt16
    {
        /// <summary>
        /// DES
        /// </summary>
        Des = 0x6601,

        ///<summary>
        ///RC2
        ///</summary>
        /// <remarks>
        /// 解凍するためのバージョンは 5.2未満
        /// </remarks>
        Rc2_1 = 0x6602,//(version needed to extract < 5.2) 

        /// <summary>
        /// 3DES 168
        /// </summary>
        TripleDes168 = 0x6603,

        /// <summary>
        /// 3DES 112
        /// </summary>
        TripleDes112 = 0x6609,

        /// <summary>
        /// AES 128
        /// </summary>
        Aes128 = 0x660e,

        /// <summary>
        /// AES 192
        /// </summary>
        Aes192 = 0x660f,

        /// <summary>
        /// AES 256
        /// </summary>
        Aes256 = 0x6610,

        ///<summary>
        /// RC2
        ///</summary>
        /// <remarks>
        /// 解凍するためのバージョンは 5.2以上
        /// </remarks>
        Rc2_2 = 0x6702,

        /// <summary>
        /// Blowfish
        /// </summary>
        Blowfish = 0x6720,

        /// <summary>
        /// Twofish
        /// </summary>
        Twofish = 0x6721,

        /// <summary>
        /// RC4
        /// </summary>
        Rc4 = 0x6801,

        /// <summary>
        /// Unknown algorithm
        /// </summary>
        UnknownAlgorithm = 0xffff,
    }
}
