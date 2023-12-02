using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// Extended Timestamp Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class ExtendedTimestampExtraField
        : UnixTimestampExtraField
    {
        [Flags]
        private enum Flag
            : Byte
        {
            None = 0,
            LastWriteTime = 1 << 0,
            LastAccessTime = 1 << 1,
            CreationTime = 1 << 2,
        }

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public ExtendedTimestampExtraField()
            : base(ExtraFieldId)
        {
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x5455;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            var flag = Flag.None;
            var lastWriteTimestamp =
                LastWriteTimeUtc is not null
                ? ToUnixTimeStamp(LastWriteTimeUtc.Value)
                : null;
            var lastAccessTimestamp =
                LastAccessTimeUtc is not null
                ? ToUnixTimeStamp(LastAccessTimeUtc.Value)
                : null;
            var creationTimestamp =
                CreationTimeUtc is not null
                ? ToUnixTimeStamp(CreationTimeUtc.Value)
                : null;
            if (lastWriteTimestamp is not null)
                flag |= Flag.LastWriteTime;
            if (lastAccessTimestamp is not null)
                flag |= Flag.LastAccessTime;
            if (creationTimestamp is not null)
                flag |= Flag.CreationTime;
            if (flag == 0)
                return null;

            switch (headerType)
            {
                case ZipEntryHeaderType.LocalFileHeader:
                {
                    var builder = new ByteArrayBuilder(sizeof(Byte) + sizeof(Int32) + sizeof(Int32) + sizeof(Int32));
                    builder.AppendByte((Byte)flag);
                    if (lastWriteTimestamp is not null)
                    {
                        if (lastWriteTimestamp is not null)
                            builder.AppendInt32LE(lastWriteTimestamp.Value);
                    }

                    if (lastAccessTimestamp is not null)
                    {
                        if (lastAccessTimestamp is not null)
                            builder.AppendInt32LE(lastAccessTimestamp.Value);
                    }

                    if (creationTimestamp is not null)
                    {
                        if (creationTimestamp is not null)
                            builder.AppendInt32LE(creationTimestamp.Value);
                    }

                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                {
                    var builder = new ByteArrayBuilder(sizeof(Byte) + sizeof(Int32));
                    builder.AppendByte((Byte)flag);
                    if (lastWriteTimestamp is not null)
                    {
                        if (lastWriteTimestamp is not null)
                            builder.AppendInt32LE(lastWriteTimestamp.Value);
                    }

                    return builder.ToByteArray();
                }
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
        {
            LastWriteTimeUtc = null;
            LastAccessTimeUtc = null;
            CreationTimeUtc = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalFileHeader:
                    {
                        var flag = (Flag)reader.ReadByte();
                        if (flag.HasFlag(Flag.LastWriteTime))
                            LastWriteTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());
                        if (flag.HasFlag(Flag.LastAccessTime))
                            LastAccessTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());
                        if (flag.HasFlag(Flag.CreationTime))
                            CreationTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());

                        break;
                    }
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        var flag = (Flag)reader.ReadByte();
                        if (flag.HasFlag(Flag.LastWriteTime))
                            LastWriteTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());

                        // 本来の仕様では、セントラルディレクトリヘッダの場合に付加されるのは LastWriteTime のみであるが、
                        // それに反して LastAccessTime および CreationTime も付加してしまう実装も存在する模様。
                        // そのような ZIP アーカイブファイル であった場合、LastAccessTime および CreationTime も読み込むこととする。
                        if (!reader.IsEmpty && flag.HasFlag(Flag.LastAccessTime))
                            LastAccessTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());
                        if (!reader.IsEmpty && flag.HasFlag(Flag.CreationTime))
                            CreationTimeUtc = FromUnixTimeStamp(reader.ReadInt32LE());

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
                    LastWriteTimeUtc = null;
                    LastAccessTimeUtc = null;
                    CreationTimeUtc = null;
                }
            }
        }
    }
}
