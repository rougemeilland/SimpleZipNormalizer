namespace Utility.IO
{
    public interface IRandomOutputByteStream<POSITION_T, UNSIGNED_OFFSET_T>
        : IOutputByteStream<POSITION_T>
    {
        /// <summary>
        /// バイトストリームの長さを取得または設定します。
        /// </summary>
        /// <value>
        /// バイトストリームの長さを示す <see cref="UNSIGNED_OFFSET_T"/> 値です。
        /// </value>
        UNSIGNED_OFFSET_T Length { get; set; }

        /// <summary>
        /// バイトストリームで次に書き込まれる位置を設定します。
        /// </summary>
        /// <param name="offset">
        /// 次に書き込まれるストリームの位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </param>
        void Seek(POSITION_T offset);
    }
}
