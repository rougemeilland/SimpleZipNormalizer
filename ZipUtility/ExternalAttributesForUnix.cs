using System;

namespace ZipUtility
{
    /// <summary>
    /// ホスト OS が UNIX 系 OS である場合の external attributes のフラグの値です。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>MS-DOS系OSとの互換のために残してあるフラグも含んでいますが、設定するかどうかは任意です。ただし、ディレクトリエントリの場合は <see cref="DOS_DIRECTORY"/> も設定することを推奨します。</item>
    /// </list>
    /// </remarks>
    [Flags]
    internal enum ExternalAttributesForUnix
        : UInt32
    {
        /// <summary>
        /// ファイルタイプのマスク
        /// </summary>
        UNX_IFMT = 0xf0000000,

        /// <summary>
        /// ソケット
        /// </summary>
        UNX_IFSOCK = 0xc0000000,

        /// <summary>
        /// シンボリックリンク
        /// </summary>
        UNX_IFLNK = 0xa0000000,

        /// <summary>
        /// 通常のファイル
        /// </summary>
        UNX_IFREG = 0x80000000,

        /// <summary>
        /// ブロック型デバイス
        /// </summary>
        UNX_IFBLK = 0x60000000,

        /// <summary>
        /// ディレクトリ
        /// </summary>
        UNX_IFDIR = 0x40000000,

        /// <summary>
        /// キャラクタ型デバイス
        /// </summary>
        UNX_IFCHR = 0x20000000,

        /// <summary>
        /// パイプ
        /// </summary>
        UNX_IFIFO = 0x10000000,

        UNX_ISUID = 0x08000000,
        UNX_ISGID = 0x04000000,
        UNX_ISVTX = 0x02000000,
        UNX_ENFMT = UNX_ISGID,

        /// <summary>
        /// 所有者から読み込み可能
        /// </summary>
        UNX_IRUSR = 0x01000000,

        /// <summary>
        /// 所有者から書き込み可能
        /// </summary>
        UNX_IWUSR = 0x00800000,

        /// <summary>
        /// 所有者から実行/検索可能
        /// </summary>
        UNX_IXUSR = 0x00400000,

        /// <summary>
        /// 同じグループのユーザから読み込み可能
        /// </summary>
        UNX_IRGRP = 0x00200000,

        /// <summary>
        /// 同じグループのユーザから書き込み可能
        /// </summary>
        UNX_IWGRP = 0x00100000,

        /// <summary>
        /// 同じグループのユーザから実行/検索可能
        /// </summary>
        UNX_IXGRP = 0x00080000,

        /// <summary>
        /// 他のユーザから読み込み可能
        /// </summary>
        UNX_IROTH = 0x00040000,

        /// <summary>
        /// 他のユーザから書き込み可能
        /// </summary>
        UNX_IWOTH = 0x00020000,

        /// <summary>
        /// 他のユーザから実行/検索可能
        /// </summary>
        UNX_IXOTH = 0x00010000,

        /// <summary>
        /// 更新されたファイル (MS-DOS互換)
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
