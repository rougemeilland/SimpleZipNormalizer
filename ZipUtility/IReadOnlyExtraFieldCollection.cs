using System;
using System.Collections.Generic;
using ZipUtility.ZipExtraField;

namespace ZipUtility
{
    /// <summary>
    /// 拡張フィールドのコレクションに対する読み取りのアクセスが可能なインターフェースです。
    /// </summary>
    public interface IReadOnlyExtraFieldCollection
    {
        /// <summary>
        /// 拡張フィールドのコレクションに、指定されたIDの拡張フィールドが含まれているかどうかを調べます。
        /// </summary>
        /// <param name="extraFieldId">
        /// コレクションに含まれているかどうかを調べる対象の拡張フィールド ID です。
        /// </param>
        /// <returns>
        /// 指定されたIDの拡張フィールドがコレクションに含まれていれば true 、そうではないなら false が返ります。
        /// </returns>
        Boolean Contains(UInt16 extraFieldId);

        /// <summary>
        /// コレクションから特定の拡張フィールドのコピーを取得します。
        /// </summary>
        /// <typeparam name="EXTRA_FIELD_T">
        /// コレクションから取得したい拡張フィールドのクラスです。
        /// </typeparam>
        /// <returns>
        /// <typeparamref name="EXTRA_FIELD_T"/> 型パラメタで与えられた拡張フィールドがコレクションに含まれていればそのオブジェクトのコピーが返ります。
        /// 含まれていなかった場合は null が返ります。
        /// </returns>
        EXTRA_FIELD_T? GetExtraField<EXTRA_FIELD_T>() where EXTRA_FIELD_T : class, IExtraField, new();

        /// <summary>
        /// 拡張フィールドのコレクションのバイト配列表現を表すバイトシーケンスを取得します。
        /// </summary>
        /// <returns>
        /// 拡張フィールドのコレクションのバイト配列表現を表す <see cref="ReadOnlyMemory{Byte}">ReadOnlyMemory&lt;<see cref="Byte"/>&gt;</see> オブジェクト。
        /// </returns>
        ReadOnlyMemory<Byte> ToByteArray();

        /// <summary>
        /// 拡張フィールドのコレクションに含まれる拡張フィールドの I Dのシーケンスをします。
        /// </summary>
        /// <returns>
        /// 拡張フィールドの I Dのシーケンスを表す <see cref="IEnumerable{UInt16}">IEnumerable&lt;<see cref="UInt16"/>&gt;</see> オブジェクトです。
        /// </returns>
        IEnumerable<UInt16> EnumerateExtraFieldIds();

        /// <summary>
        /// コレクションに含まれている拡張フィールドの数です。
        /// </summary>
        Int32 Count { get; }
    }
}
