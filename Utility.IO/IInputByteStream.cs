namespace Utility.IO
{
    public interface IInputByteStream<POSITION_T>
        : IBasicInputByteStream
    {
        /// <summary>
        /// 次にデータが読み込まれる予定のストリームの先頭からのバイト単位での位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </summary>
        POSITION_T Position { get; }
    }
}
