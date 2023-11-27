namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブの検証結果の種別を示す列挙体です。
    /// </summary>
    public enum ZipArchiveValidationResultId
    {
        /// <summary>
        /// ZIP アーカイブは正常です。
        /// </summary>
        Ok = 0,

        /// <summary>
        /// ZIP アーカイブのフォーマットに誤りがあります。おそらく ZIP アーカイブが壊れています。
        /// </summary>
        Corrupted,

        /// <summary>
        /// ZIP アーカイブは暗号化されていますが、復号化をサポートしていません。
        /// </summary>
        Encrypted,

        /// <summary>
        /// ZIP アーカイブは、サポートされていない圧縮方式で圧縮されています。
        /// </summary>
        UnsupportedCompressionMethod,

        /// <summary>
        /// ZIP アーカイブに、サポートされていない機能が使用されています。
        /// </summary>
        UnsupportedFunction,

        /// <summary>
        /// 内部エラーが発生しました。
        /// </summary>
        InternalError,
    }
}
