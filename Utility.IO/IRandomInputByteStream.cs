using System;

namespace Utility.IO
{
    public interface IRandomInputByteStream<POSITION_T>
        : IInputByteStream<POSITION_T>
    {
        /// <summary>
        /// バイトストリームのバイト単位での長さを示す <see cref="UInt64"/> 値です。
        /// </summary>
        UInt64 Length { get; set; }

        /// <summary>
        /// バイトストリームで次に読み込まれる位置を設定します。
        /// </summary>
        /// <param name="offset">
        /// 次に読み込まれる位置のストリームの先頭からのバイト数を示す <typeparamref name="POSITION_T"/> 値です。
        /// </param>
        void Seek(POSITION_T offset);
    }
}
