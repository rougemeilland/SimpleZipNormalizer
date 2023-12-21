using System;
using Utility;
using Utility.IO;

namespace ZipUtility.ExtraFields
{
    internal abstract class Zip64ExtendedInformationExtraField
        : ExtraField
    {
        private readonly ZipEntryHeaderType _headerType;

        private ReadOnlyMemory<Byte> _buffer;

        protected Zip64ExtendedInformationExtraField(ZipEntryHeaderType headerType)
            : base(ExtraFieldId)
        {
            _headerType = headerType;
            _buffer = ReadOnlyMemory<Byte>.Empty;
        }

        public const UInt16 ExtraFieldId = 0x0001;

        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType, IExtraFieldEncodingParameter parameter)
        {
            if (headerType != _headerType)
                return null;
            if (!RequiredZip64)
                return null;
            return _buffer;
        }

        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, IExtraFieldDecodingParameter parameter)
        {
            if (_headerType != headerType)
                return;
            _buffer = data.ToArray();
        }

        public Boolean RequiredZip64 => !_buffer.IsEmpty;

        protected (UInt32 rawSize, UInt32 rawPackedSize) InternalSetValues(UInt64 size, UInt64 packedSize)
        {
            if (_headerType != ZipEntryHeaderType.LocalHeader)
                throw new InternalLogicalErrorException();

            // 4.5.3 -Zip64 Extended Information Extra Field (0x0001) in APPNOTE:
            //   This entry in the Local header MUST include BOTH original and compressed file size fields.
            //   ローカルヘッダでのこのエントリは、 original および compressed file size を両方とも含まねばならない。

            if (size >= UInt32.MaxValue || packedSize >= UInt32.MaxValue)
            {
                // size または packedSize のどちらかが UInt32.MaxValue 以上であれば、両方とも拡張フィールドに含める。
                var builder = new ByteArrayBuilder(sizeof(UInt64) + sizeof(UInt64));
                builder.AppendUInt64LE(size);
                builder.AppendUInt64LE(packedSize);
                _buffer = builder.ToByteArray();
                return (UInt32.MaxValue, UInt32.MaxValue);
            }
            else
            {
                // size および packedSize の何れも UInt32.MaxValue 未満であれば、拡張フィールドは設定しない。
                _buffer = ReadOnlyMemory<Byte>.Empty; // _buffer は空であり、拡張フィールドは設定されない。
                return (checked((UInt32)size), checked((UInt32)packedSize));
            }
        }

        protected (UInt32 rawSize, UInt32 rawPackedSize, UInt32 rawRelatiiveHeaderOffset, UInt16 rawDiskStartNumber) InternalSetValues(UInt64 size, UInt64 packedSize, UInt64 relatiiveHeaderOffset, UInt32 diskStartNumber)
        {
            if (_headerType != ZipEntryHeaderType.CentralDirectoryHeader)
                throw new InternalLogicalErrorException();

            var builder = new ByteArrayBuilder(sizeof(UInt64) + sizeof(UInt64) + sizeof(UInt64) + sizeof(UInt32));
            var rawSize = SetUInt64Value(size, value => builder.AppendUInt64LE(size));
            var rawPackedSize = SetUInt64Value(packedSize, value => builder.AppendUInt64LE(packedSize));
            var rawRelatiiveHeaderOffset = SetUInt64Value(relatiiveHeaderOffset, value => builder.AppendUInt64LE(relatiiveHeaderOffset));
            var rawDiskStartNumber = SetUInt32Value(diskStartNumber, value => builder.AppendUInt32LE(diskStartNumber));
            _buffer = builder.ToByteArray();
            return (rawSize, rawPackedSize, rawRelatiiveHeaderOffset, rawDiskStartNumber);
        }

        protected (UInt64 size, UInt64 packedSize) InternalGetValues(UInt32 rawSize, UInt32 rawPackedSize)
        {
            if (_headerType != ZipEntryHeaderType.LocalHeader)
                throw new InternalLogicalErrorException();

            try
            {
                var reader = new ByteArrayReader(_buffer);
                var size = GetUInt64Value(rawSize, reader.ReadUInt64LE);
                var packedSize = GetUInt64Value(rawPackedSize, reader.ReadUInt64LE);
                if (!reader.IsEmpty)
                    throw new BadZipFileFormatException($": header={_headerType}, {nameof(rawSize)}=0x{rawSize:x8}, {nameof(rawPackedSize)}=0x{rawPackedSize:x8}, data={_buffer.ToFriendlyString()}");
                return (size, packedSize);
            }
            catch (UnexpectedEndOfBufferException ex)
            {
                throw new BadZipFileFormatException($": header={_headerType}, {nameof(rawSize)}=0x{rawSize:x8}, {nameof(rawPackedSize)}=0x{rawPackedSize:x8}, data={_buffer.ToFriendlyString()}", ex);
            }
        }

        protected (UInt64 size, UInt64 packedSize, UInt64 relatiiveHeaderOffset, UInt32 diskStartNumber) InternalGetValues(UInt32 rawSize, UInt32 rawPackedSize, UInt32 rawRelatiiveHeaderOffset, UInt16 rawDiskStartNumber)
        {
            if (_headerType != ZipEntryHeaderType.CentralDirectoryHeader)
                throw new InternalLogicalErrorException();

            try
            {
                var reader = new ByteArrayReader(_buffer);
                var size = GetUInt64Value(rawSize, reader.ReadUInt64LE);
                var packedSize = GetUInt64Value(rawPackedSize, reader.ReadUInt64LE);
                var relatiiveHeaderOffset = GetUInt64Value(rawRelatiiveHeaderOffset, reader.ReadUInt64LE);
                var diskStartNumber = GetUInt32Value(rawDiskStartNumber, reader.ReadUInt32LE);
                if (!reader.IsEmpty)
                    throw new BadZipFileFormatException($": header={_headerType}, {nameof(rawSize)}=0x{rawSize:x8}, {nameof(rawPackedSize)}=0x{rawPackedSize:x8}, {nameof(rawRelatiiveHeaderOffset)}=0x{rawRelatiiveHeaderOffset:x8}, {nameof(rawDiskStartNumber)}=0x{rawDiskStartNumber:x4}, data={_buffer.ToFriendlyString()}");
                return (size, packedSize, relatiiveHeaderOffset, diskStartNumber);
            }
            catch (UnexpectedEndOfBufferException ex)
            {
                throw new BadZipFileFormatException($": header={_headerType}, {nameof(rawSize)}=0x{rawSize:x8}, {nameof(rawPackedSize)}=0x{rawPackedSize:x8}, {nameof(rawRelatiiveHeaderOffset)}=0x{rawRelatiiveHeaderOffset:x8}, {nameof(rawDiskStartNumber)}=0x{rawDiskStartNumber:x4}, data={_buffer.ToFriendlyString()}", ex);
            }
        }

        private static UInt32 SetUInt64Value(UInt64 value, Action<UInt64> valueWriter)
        {
            if (value >= UInt32.MaxValue)
            {
                valueWriter(value);
                return UInt32.MaxValue;
            }
            else
            {
                return (UInt32)value;
            }
        }

        private static UInt16 SetUInt32Value(UInt32 value, Action<UInt32> addToBuffer)
        {
            if (value >= UInt16.MaxValue)
            {
                addToBuffer(value);
                return UInt16.MaxValue;
            }
            else
            {
                return (UInt16)value;
            }
        }

        private static UInt64 GetUInt64Value(UInt32 rawValue, Func<UInt64> valueReader)
            => rawValue >= UInt32.MaxValue
                ? valueReader()
                : rawValue;

        private static UInt32 GetUInt32Value(UInt16 rawValue, Func<UInt32> valueReader)
            => rawValue >= UInt16.MaxValue
                ? valueReader()
                : rawValue;
    }
}
