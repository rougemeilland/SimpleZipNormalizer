using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// Info-ZIP UNIX Extra Field (type 2) の拡張フィールドのクラスです。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// この拡張フィールドの設定値は、以下の拡張クラスの設定値より優先されます。
    /// <list type="bullet">
    /// <item><see cref="UnixExtraFieldType1"/></item>
    /// <item><see cref="UnixExtraFieldType0"/></item>
    /// </list>
    /// </item>
    /// <item>
    /// この拡張フィールドの設定値より、以下の拡張クラスの設定値が優先されます。
    /// <list type="bullet">
    /// <item><see cref="NewUnixExtraField"/> (16bit を超える UID/GID をサポート)</item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    public abstract class UnixExtraFieldType2
        : ExtraField
    {
        private UInt16? _userId;
        private UInt16? _groupId;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public UnixExtraFieldType2()
            : base(ExtraFieldId)
        {
            _userId = null;
            _groupId = null;
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x7855;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalFileHeader:
                {
                    if (_userId is null || _groupId is null)
                        return null;

                    var builder = new ByteArrayBuilder(sizeof(UInt16) + sizeof(UInt16));
                    builder.AppendUInt16LE(_userId.Value);
                    builder.AppendUInt16LE(_groupId.Value);
                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                    return ReadOnlyMemory<Byte>.Empty;
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
        {
            _userId = null;
            _groupId = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalFileHeader:
                        _userId = reader.ReadUInt16LE();
                        _groupId = reader.ReadUInt16LE();
                        break;
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                        _userId = null;
                        _groupId = null;
                        break;
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
                    _userId = null;
                    _groupId = null;
                }
            }
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
    }
}
