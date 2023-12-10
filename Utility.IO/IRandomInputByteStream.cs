using System;

namespace Utility.IO
{
    public interface IRandomInputByteStream<POSITION_T>
        : ISequentialInputByteStream
    {
        /// <summary>
        /// 現在のストリームの位置を示す値を取得します。
        /// </summary>
        /// <value>
        /// ストリームの位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </value>
        POSITION_T Position { get; }

        /// <summary>
        /// ストリームの最初の位置を示す値を取得します。
        /// </summary>
        /// <value>
        /// ストリームの最初の位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>[実装する際の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="StartOfThisStream"/> が示す値は、いわゆる「ゼロ値」でなければならないことに注意してください。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        POSITION_T StartOfThisStream { get; }

        /// <summary>
        /// ストリームの終端の位置を示す値を取得します。
        /// </summary>
        /// <value>
        /// ストリームの終端の位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <para>
        /// <see cref="StartOfThisStream"/> および <see cref="EndOfThisStream"/>、<see cref="Length"/> の間には、以下の関係が成立します。
        /// </para>
        /// <code>
        /// <see cref="StartOfThisStream"/> + <see cref="Length"/> == <see cref="EndOfThisStream"/>
        /// </code>
        /// </item>
        /// </list>
        /// </remarks>
        POSITION_T EndOfThisStream { get; }

        /// <summary>
        /// ストリームの長さを示す値を取得します。
        /// </summary>
        /// <value>
        /// ストリームの長さをバイト単位で示す <see cref="UInt64"/> 値です。
        /// </value>
        UInt64 Length { get; }

        /// <summary>
        /// ストリームの位置を設定します。
        /// </summary>
        /// <param name="position">
        /// ストリームの位置を示す <typeparamref name="POSITION_T"/> 値です。
        /// </param>
        void Seek(POSITION_T position);
    }
}
