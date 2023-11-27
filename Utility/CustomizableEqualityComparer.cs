using System;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// デリゲートによって与えられた比較方法により <see cref="IEqualityComparer{T}"/> を実装するクラスです。
    /// </summary>
    /// <typeparam name="VALUE_T">
    /// 比較対象の型です。
    /// </typeparam>
    public class CustomizableEqualityComparer<VALUE_T>
        : IEqualityComparer<VALUE_T>
    {
        private readonly Func<VALUE_T, VALUE_T, Boolean> _equalityComparer;
        private readonly Func<VALUE_T, Int32> _hashCalculater;

        /// <summary>
        /// 値の比較方法を示すデリゲートによって初期化されるコンストラクタです。
        /// </summary>
        /// <param name="equalityComparer">
        /// 与えられた値が等値かどうかを返すデリゲートです。
        /// このデリゲートは、二つの <typeparamref name="VALUE_T"/> 値をパラメタで受け取り、それらの値が等値かどうかを示す <see cref="Boolean"/> 値を返す必要があります。
        /// 第一パラメタの値と第二パラメタの値が等値である場合は true、そうではない場合は false を返す必要があります。
        /// 型パラメタ <typeparamref name="VALUE_T"/> がクラスである場合でも、 このデリゲートのパラメタに null が与えられることはありません。
        /// </param>
        /// <param name="hashCalculater">
        /// 与えられた値のハッシュコードを返すデリゲートです。
        /// このデリゲート、は一つの <typeparamref name="VALUE_T"/> 値をパラメタで受け取り、そのハッシュコードを示す <see cref="Int32"/> 値を返す必要があります。
        /// 型パラメタ <typeparamref name="VALUE_T"/> がクラスである場合でも、 このデリゲートのパラメタに null が与えられることはありません。
        /// </param>
        public CustomizableEqualityComparer(Func<VALUE_T, VALUE_T, Boolean> equalityComparer, Func<VALUE_T, Int32> hashCalculater)
        {
            _equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
            _hashCalculater = hashCalculater ?? throw new ArgumentNullException(nameof(hashCalculater));
        }

        public Boolean Equals(VALUE_T? x, VALUE_T? y)
            => x is null
                ? y is null
                : y is null
                ? false
                : _equalityComparer(x, y);

        public Int32 GetHashCode(VALUE_T obj)
            => obj is null
                ? throw new ArgumentNullException(nameof(obj))
                : _hashCalculater(obj);
    }
}
