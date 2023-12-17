namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// エントリのヘッダの種類を示す列挙体です。
    /// </summary>
    public enum ZipEntryHeaderType
    {
        /// <summary>
        /// 不明なヘッダです。
        /// </summary>
        Unknown,

        /// <summary>
        /// ローカルヘッダです。
        /// </summary>
        LocalHeader,

        /// <summary>
        /// セントラルディレクトリヘッダです。
        /// </summary>
        CentralDirectoryHeader,
    }
}
