using System;

namespace ZipUtility
{
    /// <summary>
    /// ホスト OS の種類を示す列挙体です。
    /// </summary>
    public enum ZipEntryHostSystem
        : Byte
    {
        /// <summary>
        /// MS-DOS and OS/2 (FAT / VFAT / FAT32 file systems)
        /// </summary>
        FAT = 0,

        /// <summary>
        /// Amiga
        /// </summary>
        Amiga = 1,

        /// <summary>
        /// OpenVMS
        /// </summary>
        OpenVMS = 2,

        /// <summary>
        /// UNIX
        /// </summary>
        UNIX = 3,

        /// <summary>
        /// VM/CMS
        /// </summary>
        VM_CMS = 4,

        /// <summary>
        /// Atari ST
        /// </summary>
        Atari_ST = 5,

        /// <summary>
        /// OS/2 H.P.F.S.
        /// </summary>
        OS2_HPFS = 6,

        /// <summary>
        /// Macintosh
        /// </summary>
        Macintosh = 7,

        /// <summary>
        /// Z-System
        /// </summary>
        Z_System = 8,

        /// <summary>
        /// CP/M
        /// </summary>
        CP_M = 9,

        /// <summary>
        /// Windows NTFS
        /// </summary>
        Windows_NTFS = 10,

        /// <summary>
        /// MVS (OS/390 - Z/OS)
        /// </summary>
        MVS = 11,

        /// <summary>
        /// VSE
        /// </summary>
        VSE = 12,

        /// <summary>
        /// Acorn Risc
        /// </summary>
        AcornRisc = 13,

        /// <summary>
        /// VFAT
        /// </summary>
        VFAT = 14,

        /// <summary>
        /// alternate MVS
        /// </summary>
        alternateMVS = 15,

        /// <summary>
        /// BeOS
        /// </summary>
        BeOS = 16,

        /// <summary>
        /// Tandem
        /// </summary>
        Tandem = 17,

        /// <summary>
        /// OS/400
        /// </summary>
        OS400 = 18,

        /// <summary>
        /// OS X (Darwin)
        /// </summary>
        OSX = 19,
    }
}
