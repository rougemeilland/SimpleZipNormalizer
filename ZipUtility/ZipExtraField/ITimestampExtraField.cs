using System;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// 一般的な日時 (最終更新日時/最終アクセス日時/作成日時) を取得/設定するインターフェースです。
    /// </summary>
    interface ITimestampExtraField
        : IExtraField
    {
        /// <summary>
        /// 最終更新日時 (UTC) を取得または設定します。
        /// </summary>
        /// <value>
        /// 作成日時である <see cref="DateTime"/> オブジェクトです。
        /// 値が設定されていない場合は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Local"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、自動的に UTC に変換されます。</item>
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、例外が発生します。</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとしました。</item>
        /// </exception>
        DateTime? LastWriteTimeUtc { get; set; }

        /// <summary>
        /// 最終アクセス日時 (UTC) を取得または設定します。
        /// </summary>
        /// <value>
        /// 作成日時である <see cref="DateTime"/> オブジェクトです。
        /// 値が設定されていない場合は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Local"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、自動的に UTC に変換されます。</item>
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、例外が発生します。</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとしました。</item>
        /// </exception>
        DateTime? LastAccessTimeUtc { get; set; }

        /// <summary>
        /// 作成日時 (UTC) を取得または設定します。
        /// </summary>
        /// <value>
        /// 作成日時である <see cref="DateTime"/> オブジェクトです。
        /// 値が設定されていない場合は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Local"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、自動的に UTC に変換されます。</item>
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとした場合は、例外が発生します。</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <item><see cref="DateTime.Kind"/> の値が <see cref="DateTimeKind.Unspecified"/> である <see cref="DateTime"/> 構造体を設定しようとしました。</item>
        /// </exception>
        DateTime? CreationTimeUtc { get; set; }

        /// <summary>
        /// 拡張フィールドが保持する日時の最小単位を取得します。
        /// </summary>
        /// <value>日時の最小単位である <see cref="TimeSpan"/> 値です。</value>
        TimeSpan DateTimePrecision { get; }
    }
}
