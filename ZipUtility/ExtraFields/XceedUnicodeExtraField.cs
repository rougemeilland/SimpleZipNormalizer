using System;
using System.Text;
using Utility;
using Utility.Text;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// Xceed unicode extra field の拡張ヘッダのクラスです。
    /// </summary>
    public class XceedUnicodeExtraField
        : ExtraField
    {
        private static readonly Encoding _unicodeEncoding;
        private static readonly UInt32 _signature;
        private String? _fullName;
        private String? _comment;

        static XceedUnicodeExtraField()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _unicodeEncoding = Encoding.Unicode.WithFallback(null, null).WithoutPreamble();
            _signature = Signature.MakeUInt32LESignature(0x4e, 0x55, 0x43, 0x58);
        }

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public XceedUnicodeExtraField()
            : base(ExtraFieldId)
        {
            Encoding = _unicodeEncoding;
            _fullName = null;
            _comment = null;
        }

        /// <summary>
        /// 拡張ヘッダの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x554e;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType, IExtraFieldEncodingParameter parameter)
        {
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalHeader:
                {
                    // FullName が未設定または空文字列ならば、この拡張フィールドは設定不要とする
                    if (String.IsNullOrEmpty(_fullName))
                        return null;

                    // String オブジェクトは必ず UTF-16 エンコーディングに成功するので例外チェックはしない (String の内部表現が UTF-16 であるため)
                    var fullNameBytes = _unicodeEncoding.GetBytes(_fullName ?? "");
                    if ((fullNameBytes.Length >> 1) > UInt16.MaxValue)
                        return null;

                    var bufferLength = checked(sizeof(UInt32) + sizeof(UInt16) + fullNameBytes.Length + 1 >> 1 << 1);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendUInt32LE(_signature);
                    builder.AppendUInt16LE((UInt16)(fullNameBytes.Length >> 1));
                    builder.AppendBytes(fullNameBytes);

                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                {
                    // FullName と Comment がともに未設定または空文字列ならば、この拡張フィールドは設定不要とする
                    if (String.IsNullOrEmpty(_fullName) && String.IsNullOrEmpty(_comment))
                        return null;

                    // String オブジェクトは必ず UTF-16 エンコーディングに成功するので例外チェックはしない (String の内部表現が UTF-16 であるため)
                    var fullNameBytes = _unicodeEncoding.GetBytes(_fullName ?? "");

                    // String オブジェクトは必ず UTF-16 エンコーディングに成功するので例外チェックはしない (String の内部表現が UTF-16 であるため)
                    var commentBytes = _unicodeEncoding.GetBytes(_comment ?? "");

                    var bufferLength = checked(sizeof(UInt32) + sizeof(UInt16) + fullNameBytes.Length + 1 >> 1 << 1 + commentBytes.Length + 1 >> 1 << 1);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendUInt32LE(_signature);
                    builder.AppendUInt16LE((UInt16)(fullNameBytes.Length >> 1));
                    builder.AppendUInt16LE((UInt16)(commentBytes.Length >> 1));
                    builder.AppendBytes(fullNameBytes);
                    builder.AppendBytes(commentBytes);

                    return builder.ToByteArray();
                }
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, IExtraFieldDecodingParameter parameter)
        {
            _fullName = null;
            _comment = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalHeader:
                    {
                        var signature = reader.ReadUInt32LE();
                        if (signature != _signature)
                            return;
                        var fullNameCount = reader.ReadUInt16LE();
                        var fullNameBytes = reader.ReadBytes((UInt16)(fullNameCount * 2));
                        try
                        {
                            _fullName = _unicodeEncoding.GetString(fullNameBytes);
                        }
                        catch (Exception ex)
                        {
                            // UTF-16 でデコードできなかった場合
                            throw GetBadFormatException(headerType, data, ex);
                        }

                        _comment = null;
                        break;
                    }

                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        var signature = reader.ReadUInt32LE();
                        if (signature != _signature)
                            return;
                        var fullNameCount = reader.ReadUInt16LE();
                        var commentCount = reader.ReadUInt16LE();
                        var fullNameBytes = reader.ReadBytes((UInt16)(fullNameCount * 2));
                        var commentBytes = reader.ReadBytes((UInt16)(commentCount * 2));
                        try
                        {
                            _fullName = _unicodeEncoding.GetString(fullNameBytes);
                        }
                        catch (Exception ex)
                        {
                            // UTF-16 でデコードできなかった場合
                            throw GetBadFormatException(headerType, data, ex);
                        }

                        try
                        {
                            _comment = _unicodeEncoding.GetString(commentBytes);
                        }
                        catch (Exception ex)
                        {
                            // UTF-16 でデコードできなかった場合
                            throw GetBadFormatException(headerType, data, ex);
                        }

                        break;
                    }

                    default:
                        throw new ArgumentException($"Unexpected {nameof(ZipEntryHeaderType)} value", nameof(headerType));
                }

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
                // 復帰時に success フラグが立っていない場合は、すべての設定値を初期値に戻す。
                if (!success)
                {
                    _fullName = null;
                    _comment = null;
                }
            }
        }

        /// <summary>
        /// 拡張ヘッダが保持しているエントリ名およびコメントのエンコーディングを取得します。
        /// </summary>
        /// <remarks>
        /// これは常に UTF-16 です。
        /// </remarks>
        public Encoding Encoding { get; }

        /// <summary>
        /// エントリの名前の文字列です。
        /// </summary>
        public String? FullName
        {
            get => _fullName;
            set
            {
                // 長さの上限は UInt16.MaxValue
                if (value is not null && value.Length > UInt16.MaxValue)
                    throw new ArgumentException($"The maximum length of characters that can be set in the {nameof(FullName)} property is {UInt16.MaxValue} characters.: {nameof(value)}.{nameof(value.Length)}={value.Length}", nameof(value));

                // 少なくとも、セントラルディレクトリヘッダの長さ (8 + FullName.Length * 2 + Comment.Length * 2) は UInt16.Maxvalue 以下でなければならない。
                if (_comment is not null && value is not null && checked(value.Length + _comment.Length) > (UInt16.MaxValue - 8) / 2)
                    throw new ArgumentException($"The total length of the {nameof(FullName)} and {nameof(Comment)} properties must be less than or equal to {(UInt16.MaxValue - 8) / 2} characters.: {nameof(value)}.{nameof(value.Length)}={value.Length}, {nameof(Comment)}.{nameof(_comment.Length)}={_comment.Length}", nameof(value));

                _fullName = value;
            }
        }

        /// <summary>
        /// エントリの名前の
        /// </summary>
        public String? Comment
        {
            get => _comment;
            set
            {
                // 長さの上限は UInt16.MaxValue
                if (value is not null && value.Length > UInt16.MaxValue)
                    throw new ArgumentException($"The maximum length of characters that can be set in the {nameof(Comment)} property is {UInt16.MaxValue} characters.: {nameof(value)}.{nameof(value.Length)}={value.Length}", nameof(value));

                // 少なくとも、セントラルディレクトリヘッダの長さ (8 + FullName.Length * 2 + Comment.Length * 2) は UInt16.Maxvalue 以下でなければならない。
                if (_fullName is not null && value is not null && checked(_fullName.Length + value.Length) > (UInt16.MaxValue - 8) / 2)
                    throw new ArgumentException($"The total length of the {nameof(FullName)} and {nameof(Comment)} properties must be less than or equal to {(UInt16.MaxValue - 8) / 2} characters.: {nameof(FullName)}.{nameof(_fullName.Length)}={_fullName.Length}, {nameof(Comment)}.{nameof(value.Length)}={value.Length}", nameof(value));

                _comment = value;
            }
        }
    }
}
