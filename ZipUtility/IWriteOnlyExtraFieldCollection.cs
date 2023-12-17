using ZipUtility.ExtraFields;

namespace ZipUtility
{
    /// <summary>
    /// 拡張フィールドのコレクションに対するアクセスが可能なインターフェースです。
    /// </summary>
    public interface IWriteOnlyExtraFieldCollection
    {
        /// <summary>
        /// 拡張フィールドをコレクションに追加します。
        /// </summary>
        /// <typeparam name="EXTRA_FIELD_T">
        /// 追加する拡張フィールドのクラスです。このクラスは <see cref="IExtraField"/> を実装している必要があります。
        /// </typeparam>
        /// <param name="extraField">
        /// 追加する拡張フィールドのオブジェクトです。
        /// </param>
        void AddExtraField<EXTRA_FIELD_T>(EXTRA_FIELD_T extraField) where EXTRA_FIELD_T : IExtraField;
    }
}
