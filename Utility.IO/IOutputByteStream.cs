namespace Utility.IO
{
    public interface IOutputByteStream<POSITION_T>
        : IBasicOutputByteStream
    {
        /// <summary>
        /// 次にデータが書き込まれる予定のストリームの先頭からのバイト単位での位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </summary>
        POSITION_T Position { get; }
    }
}
