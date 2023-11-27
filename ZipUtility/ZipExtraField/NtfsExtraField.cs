using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// PKWARE Win95/WinNT Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class NtfsExtraField
        : TimestampExtraField
    {
        // info-ZIP extrafield.txt ではローカルヘッダにのみ設定することになっている。
        // PKWARE の APPNOTE ではどちらのヘッダに設定するかは規定されていない。
        // 7-zip の実装では、セントラルディレクトリヘッダにのみ設定されている模様。
        // => 設定はローカルヘッダにのみ行い、取得はどちらのヘッダからも行うことにする。

        private const UInt16 _subTag0001Id = 0x0001;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public NtfsExtraField()
            : base(ExtraFieldId)
        {
            LastWriteTimeUtc = null;
            LastAccessTimeUtc = null;
            CreationTimeUtc = null;
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x000a;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalFileHeader:
                {
                    var dataOfSubTag0001 = GetDataForSubTag0001();
                    if (dataOfSubTag0001 is null)
                        return null;

                    var bufferLength = checked(sizeof(UInt32) + sizeof(UInt16) + sizeof(UInt16) + dataOfSubTag0001.Value.Length);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendUInt32LE(0); //Reserved
                    builder.AppendUInt16LE(_subTag0001Id);
                    builder.AppendUInt16LE((UInt16)dataOfSubTag0001.Value.Length);
                    builder.AppendBytes(dataOfSubTag0001.Value.Span);
                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                    return null;
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
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        _ = reader.ReadUInt32LE(); //Reserved
                        while (!reader.IsEmpty)
                        {
                            var subTagId = reader.ReadUInt16LE();
                            var subTagLength = reader.ReadUInt16LE();
                            var subTagData = reader.ReadBytes(subTagLength);
                            switch (subTagId)
                            {
                                case _subTag0001Id:
                                    SetDataForSubTag0001(subTagData);
                                    break;
                                default:
                                    // unknown sub tag id
                                    break;
                            }
                        }

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

        /// <inheritdoc/>
        public override TimeSpan DateTimePrecision => TimeSpan.FromTicks(1);

        private ReadOnlyMemory<Byte>? GetDataForSubTag0001()
        {
            // 最終更新日時/最終アクセス日時/作成日時のいずれかが未設定の場合は、この拡張フィールドは無効とする。
            if (LastWriteTimeUtc is null ||
                LastAccessTimeUtc is null ||
                CreationTimeUtc is null)
            {
                return null;
            }

            var builder = new ByteArrayBuilder(sizeof(UInt64) + sizeof(UInt64) + sizeof(UInt64));
            builder.AppendUInt64LE((UInt64)LastWriteTimeUtc.Value.ToFileTimeUtc());
            builder.AppendUInt64LE((UInt64)LastAccessTimeUtc.Value.ToFileTimeUtc());
            builder.AppendUInt64LE((UInt64)CreationTimeUtc.Value.ToFileTimeUtc());
            return builder.ToByteArray();
        }

        private void SetDataForSubTag0001(ReadOnlyMemory<Byte> data)
        {
            var reader = new ByteArrayReader(data);
            LastWriteTimeUtc = DateTime.FromFileTimeUtc((Int64)reader.ReadUInt64LE());
            LastAccessTimeUtc = DateTime.FromFileTimeUtc((Int64)reader.ReadUInt64LE());
            CreationTimeUtc = DateTime.FromFileTimeUtc((Int64)reader.ReadUInt64LE());
        }
    }
}
