using System;

namespace ZipUtility
{
    /// <summary>
    /// ホスト OS が Windows / MS-DOS 系 OS である場合の external attributes のフラグの値です。
    /// </summary>
    [Flags]
    public enum ExternalAttributesForDos
        : UInt32
    {
        /// <summary>
        /// 更新されたファイル
        /// </summary>
        DOS_ARCHIVE = 0x00000020,

        /// <summary>
        /// ディレクトリ (MS-DOS互換)
        /// </summary>
        DOS_DIRECTORY = 0x00000010,

        /// <summary>
        /// システムファイル (MS-DOS互換)
        /// </summary>
        DOS_SYSTEM = 0x00000004,

        /// <summary>
        /// 隠しファイル (MS-DOS互換)
        /// </summary>
        DOS_HIDDEN = 0x00000002,

        /// <summary>
        /// 書き込み禁止 (MS-DOS互換)
        /// </summary>
        DOS_READONLY = 0x00000001,
    }
}
