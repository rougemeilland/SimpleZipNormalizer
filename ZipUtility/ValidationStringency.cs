namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブに対する検証の厳格さを示す列挙体です。
    /// </summary>
    public enum ValidationStringency
    {
        /// <summary>
        /// あまり一般的ではない実装も許容します。
        /// </summary>
        Lazy = -1,

        /// <summary>
        /// 通常の検証を行います。
        /// </summary>
        /// <remarks>
        /// <see href="https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT">APPNOTE</see> よりもデファクトスタンダードを優先します。
        /// </remarks>
        Normal = 0,

        /// <summary>
        /// <see href="https://pkware.cachefly.net/webdocs/casestudies/APPNOTE.TXT">APPNOTE</see> に基づいて厳格に検証を行います。
        /// </summary>
        Strict = 1,
    }
}
