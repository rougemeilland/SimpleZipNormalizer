using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// PKWARE Unix Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class UnixExtraFieldType0
        : UnixTimestampExtraField
    {
        private UInt16? _userId;
        private UInt16? _groupId;
        private ReadOnlyMemory<Byte>? _additionalData;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public UnixExtraFieldType0()
            : base(ExtraFieldId)
        {
            _userId = null;
            _groupId = null;
            _additionalData = null;
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x000d;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalFileHeader:
                {
                    var lastAccessTimestamp =
                        LastAccessTimeUtc is not null
                        ? ToUnixTimeStamp(LastAccessTimeUtc.Value)
                        : null;
                    var lastWriteTimestamp =
                        LastWriteTimeUtc is not null
                        ? ToUnixTimeStamp(LastWriteTimeUtc.Value)
                        : null;
                    if (lastAccessTimestamp is null || lastWriteTimestamp is null || _userId is null || _groupId is null || _additionalData is null)
                        return null;

                    var bufferLength = checked(sizeof(Int32) + sizeof(Int32) + sizeof(UInt16) + sizeof(UInt16) + _additionalData.Value.Length);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendInt32LE(lastAccessTimestamp.Value);
                    builder.AppendInt32LE(lastWriteTimestamp.Value);
                    builder.AppendUInt16LE(_userId.Value);
                    builder.AppendUInt16LE(_groupId.Value);
                    builder.AppendBytes(_additionalData.Value.Span);
                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                {
                    return null;
                }
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
        {
            if (headerType != ZipEntryHeaderType.LocalFileHeader)
                return;

            LastAccessTimeUtc = null;
            LastWriteTimeUtc = null;
            _userId = null;
            _groupId = null;
            _additionalData = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalFileHeader:
                    {
                        LastAccessTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());
                        LastWriteTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());
                        _userId = reader.ReadUInt16LE();
                        _groupId = reader.ReadUInt16LE();
                        _additionalData = reader.ReadAllBytes();
                        break;
                    }
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        LastAccessTimeUtc = null;
                        LastWriteTimeUtc = null;
                        _userId = null;
                        _groupId = null;
                        _additionalData = null;
                        break;
                    }
                    default:
                        throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
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
                if (!success)
                {
                    LastAccessTimeUtc = null;
                    LastWriteTimeUtc = null;
                    _userId = null;
                    _groupId = null;
                    _additionalData = null;
                }
            }
        }

        /// <inheritdoc/>
        public override DateTime? CreationTimeUtc
        {
            get => null;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// ユーザー ID を取得または設定します。
        /// </summary>
        public UInt16 UserId
        {
            get => _userId ?? throw new InvalidOperationException();
            set => _userId = value;
        }

        /// <summary>
        /// グループ ID を取得または設定します。
        /// </summary>
        public UInt16 GroupId
        {
            get => _groupId ?? throw new InvalidOperationException();
            set => _groupId = value;
        }

        /// <summary>
        /// ファイルタイプ固有の追加情報を示すバイト列を取得または設定します。
        /// </summary>
        public ReadOnlyMemory<Byte> AdditionalData
        {
            get => _additionalData ?? throw new InvalidOperationException();
            set => _additionalData = value;
        }
    }
}
