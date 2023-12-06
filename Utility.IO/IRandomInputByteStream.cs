namespace Utility.IO
{
    public interface IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T>
        : IInputByteStream<POSITION_T>
    {
        /// <summary>
        /// バイトストリームの長さを示す値を取得します。
        /// </summary>
        /// <value>
        /// バイトストリームの長さを示す <see cref="UNSIGNED_OFFSET_T"/> 値です。
        /// </value>
        UNSIGNED_OFFSET_T Length { get; }

        /// <summary>
        /// バイトストリームで次に読み込まれる位置を設定します。
        /// </summary>
        /// <param name="position">
        /// 次に読み込まれるストリームの位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </param>
        void Seek(POSITION_T position);
    }
}
