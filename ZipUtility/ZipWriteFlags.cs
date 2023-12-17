using System;

namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブの書き込みにおいて、特殊な動作を指定するフラグの列挙体です。
    /// </summary>
    [Flags]
    public enum ZipWriteFlags
    {
        /// <summary>
        /// 特殊な動作を何も指定しません。
        /// </summary>
        None = 0,

        /// <summary>
        /// ZIP64 EOCDR (ZIP 64 End Of Central Directory Record) を常に付加します。
        /// </summary>
        /// <remarks>
        /// <para>
        /// ZIP64 EOCDR は、以下の何れかの機能を使用する際に ZIP アーカイブに付加される情報です。
        /// <list type="bullet">
        /// <item>ZIP64 拡張機能 (4GB 以上のファイルを扱えるようになる)</item>
        /// <item>強力な暗号化機能 (注意: 本ソフトウェアではサポートされていません)</item>
        /// </list>
        /// </para>
        /// <para>
        /// 既定では、ZIP64 EOCDR は必要がない限り ZIP アーカイブに付加されません。
        /// しかし、<see cref="AlwaysWriteZip64EOCDR"/> フラグを指定することによって、常に ZIP64 EOCDR が ZIP アーカイブに付加されるようになります。
        /// </para>
        /// <para>
        /// <see cref="AlwaysWriteZip64EOCDR"/> フラグを指定することにより、ZIP アーカイブのサイズが数十バイト増加しますが、それ以上の影響はありません。
        /// </para>
        /// <para>
        /// 通常は <see cref="AlwaysWriteZip64EOCDR"/> フラグを指定する必要はありません。しかし、他の ZIP アーカイバソフトウェアとの互換性のテストには役に立つかもしれません。
        /// </para>
        /// </remarks>
        AlwaysWriteZip64EOCDR = 1 << 0,
    }
}
