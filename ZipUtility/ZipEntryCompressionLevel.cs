namespace ZipUtility
{
    /// <summary>
    /// 圧縮率の高さを示す列挙体です。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// 圧縮率が高いほど、圧縮のための計算に時間がかかることに注意してください。
    /// </item>
    /// </list>
    /// </remarks>
    public enum ZipEntryCompressionLevel
    {
        /// <summary>
        /// 通常の圧縮です。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 最大の圧縮率です。
        /// </summary>
        Maximum,

        /// <summary>
        /// 高速な圧縮です。ただし圧縮率は <see cref="Normal"/> より低下します。
        /// </summary>
        Fast,

        /// <summary>
        /// 非常に高速な圧縮です。ただし圧縮率 <see cref="Fast"/> より低下します。
        /// </summary>
        SuperFast
    }
}
