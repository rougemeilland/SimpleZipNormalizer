using System;
using Utility;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// Info-ZIP New Unix Extra Field の拡張フィールドのクラスです。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// この拡張フィールドの設定値は、以下の拡張クラスの設定値より優先されます。
    /// <list type="bullet">
    /// <item><see cref="UnixExtraFieldType2"/></item>
    /// <item><see cref="UnixExtraFieldType1"/></item>
    /// <item><see cref="UnixExtraFieldType0"/></item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    public class NewUnixExtraField
        : ExtraField
    {
        private const Byte _supportedVersion = 1;
        private UInt32? _uid;
        private UInt32? _gid;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public NewUnixExtraField()
            : base(ExtraFieldId)
        {
            _uid = null;
            _gid = null;
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x7875;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType, IExtraFieldEncodingParameter parameter)
        {

            switch (headerType)
            {
                case ZipEntryHeaderType.LocalHeader:
                {
                    if (_uid is null || _gid is null)
                        return null;

                    Span<Byte> uidBytes = stackalloc Byte[sizeof(UInt32)];
                    var sizeOfUID = FromUInt32LEToBytes(_uid.Value, uidBytes);
                    uidBytes = uidBytes[..sizeOfUID];

                    Span<Byte> gidBytes = stackalloc Byte[sizeof(UInt32)];
                    var sizeOfGID = FromUInt32LEToBytes(_gid.Value, gidBytes);
                    gidBytes = gidBytes[..sizeOfGID];

                    var bufferLength = checked(sizeof(Byte) + sizeof(Byte) + sizeOfUID + sizeof(Byte) + sizeOfGID);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendByte(_supportedVersion);
                    builder.AppendByte(sizeOfUID);
                    builder.AppendBytes(uidBytes);
                    builder.AppendByte(sizeOfGID);
                    builder.AppendBytes(gidBytes);
                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                {
                    return ReadOnlyMemory<Byte>.Empty;
                }
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, IExtraFieldDecodingParameter parameter)
        {
            _uid = null;
            _gid = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalHeader:
                    {
                        var version = reader.ReadByte();
                        if (version != _supportedVersion)
                            return;
                        var uidSize = reader.ReadByte();
                        if (uidSize > sizeof(UInt32))
                            return;
                        _uid = FromBytesToUInt32LE(reader.ReadBytes(uidSize));
                        var gidSize = reader.ReadByte();
                        if (gidSize > sizeof(UInt32))
                            return;
                        _gid = FromBytesToUInt32LE(reader.ReadBytes(gidSize));
                        break;
                    }
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        _uid = null;
                        _gid = null;
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
                    _uid = null;
                    _gid = null;
                }
            }
        }

        /// <summary>
        /// UID を示すバイト値です。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see cref="UID"/> の長さはターゲットとなる OS に依存します。(現在のリビジョンでは 32bit 整数です)</item>
        /// <item><see cref="UID"/> はリトルエンディアンで格納されています。</item>
        /// </list>
        /// </remarks>
        public UInt32 UID
        {
            get => _uid ?? throw new InvalidOperationException();
            set => _uid = value;
        }

        /// <summary>
        /// GID を示す値です。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see cref="GID"/> の長さはターゲットとなる OS に依存します。(現在のリビジョンでは 32bit 整数です)</item>
        /// <item><see cref="GID"/> はリトルエンディアンで格納されています。</item>
        /// </list>
        /// </remarks>
        public UInt32 GID
        {
            get => _gid ?? throw new InvalidOperationException();
            set => _gid = value;
        }

        private static Byte FromUInt32LEToBytes(UInt32 value, Span<Byte> buffer)
        {
            if ((UInt16)(value >> 16) == 0)
            {
                buffer[0] = (Byte)(value >> (8 * 0));
                buffer[1] = (Byte)(value >> (8 * 1));
                return sizeof(UInt16);
            }
            else
            {
                buffer[0] = (Byte)(value >> (8 * 0));
                buffer[1] = (Byte)(value >> (8 * 1));
                buffer[2] = (Byte)(value >> (8 * 2));
                buffer[3] = (Byte)(value >> (8 * 3));
                return sizeof(UInt32);
            }
        }

        private static UInt32 FromBytesToUInt32LE(ReadOnlyMemory<Byte> buffer)
        {
            if (buffer.Length > sizeof(UInt32) || buffer.IsEmpty)
                throw new InternalLogicalErrorException();

            var data = buffer.Span;
            var value = 0U;
            for (var index = data.Length - 1; index >= 0; --index)
            {
                value <<= 8;
                value |= data[index];
            }

            return value;
        }
    }
}
