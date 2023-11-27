using System;

namespace ZipUtility
{
    /// <summary>
    /// 拡張フィールドのコレクションに対するアクセスが可能なインターフェースです。
    /// </summary>
    public interface IExtraFieldCollection
        : IReadOnlyExtraFieldCollection, IWriteOnlyExtraFieldCollection
    {
        /// <summary>
        /// 拡張フィールドのコレクションから、指定された ID の拡張フィールドを削除します。
        /// </summary>
        /// <param name="extraFieldId">
        /// 削除する拡張フィールドの ID です。
        /// </param>
        void Delete(UInt16 extraFieldId);

        /// <summary>
        /// 拡張フィールドのコレクションの要素をすべて消去します。
        /// </summary>
        void Clear();
    }
}
