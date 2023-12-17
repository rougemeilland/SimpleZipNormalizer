namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// 拡張フィールドを取得する際に参照可能なパラメタのインターフェースです。
    /// </summary>
    public interface IExtraFieldDecodingParameter
    {
        /// <summary>
        /// 拡張フィールドを解析する際に適用する厳密度を取得します。
        /// </summary>
        ValidationStringency Stringency { get; }
    }
}
