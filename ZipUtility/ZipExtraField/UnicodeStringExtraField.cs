using System;
using System.Text;
using Utility;
using Utility.IO;
using Utility.Text;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// Info-ZIP Unicode Comment Extra Field と Info-ZIP Unicode Path Extra Field の拡張フィールドの基底クラスです。
    /// </summary>
    public abstract class UnicodeStringExtraField
        : ExtraField
    {
        private static readonly Encoding _utf8Encoding;
        private String? _unicodeString;
        private UInt32 _crc;

        static UnicodeStringExtraField()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8Encoding = Encoding.UTF8.WithFallback(null, null).WithoutPreamble();
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="extraFieldId">
        /// 拡張フィールドの ID です。
        /// </param>
        protected UnicodeStringExtraField(UInt16 extraFieldId)
            : base(extraFieldId)
        {
            _unicodeString = null;
            _crc = 0;
        }

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            if (_unicodeString is null)
                return null;
            try
            {
                // String の内部表現は UTF-16 なので、UTF-8 へのエンコーディングで例外は発生しないはず。
                var unicodeStringBytes = _utf8Encoding.GetBytes(_unicodeString);

                // CRC を計算する
                var (Crc, Length) = unicodeStringBytes.CalculateCrc32();

                // バッファサイズを計算する
                var bufferLength = checked(sizeof(Byte) + sizeof(UInt32) + unicodeStringBytes.Length);
                if (bufferLength > UInt16.MaxValue)
                    return null;

                // バイト列を作成する
                var builder = new ByteArrayBuilder(bufferLength);
                builder.AppendByte(SupportedVersion);
                builder.AppendUInt32LE(_crc);
                builder.AppendBytes(unicodeStringBytes);
                return builder.ToByteArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
        {
            _unicodeString = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                var version = reader.ReadByte();
                if (version != SupportedVersion)
                    return;
                _crc = reader.ReadUInt32LE();
                try
                {
                    _unicodeString = _utf8Encoding.GetString(reader.ReadAllBytes());
                }
                catch (Exception ex)
                {
                    // UTF-8 でデコードできるはずなのに例外が発生するということは、拡張フィールドが壊れている。
                    throw GetBadFormatException(headerType, data, ex);
                }

                if (String.IsNullOrEmpty(_unicodeString))
                    return;
                if (!reader.IsEmpty)
                    throw GetBadFormatException(headerType, data);
                success = true;
            }
            catch (UnexpectedEndOfBufferException ex)
            {
                throw GetBadFormatException(headerType, data, ex);
            }
            finally
            {
                if (!success)
                {
                    _crc = 0;
                    _unicodeString = null;
                }
            }
        }

        /// <summary>
        /// サポートされている拡張フィールドのバージョンを取得します。
        /// </summary>
        protected abstract Byte SupportedVersion { get; }

        /// <summary>
        /// 指定された文字列を UTF-8 エンコーディングでエンコードして、拡張フィールドに格納します。
        /// </summary>
        /// <param name="unicodeString">
        /// 拡張フィールドに格納する文字列です。
        /// </param>
        /// <param name="rawStringBytes">
        /// エントリに格納されている生のバイト列です。
        /// </param>
        /// <remarks>
        /// <list type="bullet">
        /// <item><paramref name="rawStringBytes"/> で指定されたバイト列の CRC が計算され、その CRC 値が拡張フィールドに格納されます。</item>
        /// </list>
        /// </remarks>
        protected void SetUnicodeString(String unicodeString, ReadOnlySpan<Byte> rawStringBytes)
        {
            _unicodeString = unicodeString;
            _crc = rawStringBytes.CalculateCrc32();
        }

        /// <summary>
        /// 拡張フィールドに格納されているバイト列を UTF-8 エンコーディングでデコードします。
        /// </summary>
        /// <param name="rawStringBytes">
        /// エントリに格納されている生のバイト列です。
        /// </param>
        /// <returns>
        /// <paramref name="rawStringBytes"/> から計算された CRC 値と拡張フィールドに格納されている CRC 値が等しい場合は、
        /// 拡張フィールドに格納されているバイト列が UTF-8 エンコーディングでデコードされ、その文字列が返ります。
        /// もし CRC 値が一致しなかった場合は null が返ります。
        /// </returns>
        protected String? GetUnicodeString(ReadOnlySpan<Byte> rawStringBytes)
        {
            if (rawStringBytes.CalculateCrc32() != _crc)
                return null;
            return _unicodeString;
        }
    }
}
