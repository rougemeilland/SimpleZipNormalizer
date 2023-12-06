using System;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    static class StreamExtensions
    {
        private class PartialInputStreamForZipInputStream
            : PartialInputStream<UInt64, ZipStreamPosition, UInt64>
        {
            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, Boolean leaveOpen = false)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, UInt64? size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;
            protected override UInt64 FromInt32ToOffset(Int32 offset) => checked((UInt64)offset);
            protected override Int32 FromOffsetToInt32(UInt64 offset) => checked((Int32)offset);
        }

        private class PartialOutputStreamForZipOutputStream
            : PartialOutputStream<UInt64, ZipStreamPosition, UInt64>
        {
            public PartialOutputStreamForZipOutputStream(IOutputByteStream<ZipStreamPosition> baseStream, Boolean leaveOpen = false)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialOutputStreamForZipOutputStream(IOutputByteStream<ZipStreamPosition> baseStream, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialOutputStreamForZipOutputStream(IOutputByteStream<ZipStreamPosition> baseStream, UInt64? size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
            }

            protected override UInt64 ZeroPositionValue => 0;
            protected override UInt64 FromInt32ToOffset(Int32 offset) => checked((UInt64)offset);
            protected override Int32 FromOffsetToInt32(UInt64 offset) => checked((Int32)offset);
        }

        private class PartialRandomInputStreamForZipInputStream
            : PartialRandomInputStream<UInt64, ZipStreamPosition, UInt64>
        {
            public PartialRandomInputStreamForZipInputStream(IZipInputStream baseStream, Boolean leaveOpen = false)
                : base(baseStream, leaveOpen)
            {
            }

            public PartialRandomInputStreamForZipInputStream(IZipInputStream baseStream, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
            }

            public PartialRandomInputStreamForZipInputStream(IZipInputStream baseStream, ZipStreamPosition offset, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, offset, size, leaveOpen)
            {
            }

            public PartialRandomInputStreamForZipInputStream(IZipInputStream baseStream, ZipStreamPosition? offset, UInt64? size, Boolean leaveOpen = false)
                : base(baseStream, offset, size, leaveOpen)
            {
            }

            protected IZipInputStream SourceStream => (BaseStream as IZipInputStream) ?? throw new InternalLogicalErrorException();
            protected override UInt64 ZeroPositionValue => 0;
            protected override ZipStreamPosition EndBasePositionValue => SourceStream.LastDiskStartPosition + SourceStream.LastDiskSize;
            protected override Int32 FromOffsetToInt32(UInt64 offset) => checked((Int32)offset);
        }

        public static IBasicInputByteStream AsPartial(this IZipInputStream baseStream, ZipStreamPosition offset, UInt64 size)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                try
                {
                    baseStream.Seek(offset);
                }
                catch (ArgumentException ex)
                {

                    throw new BadZipFileFormatException($"Unable to read data on ZIP archive.: offset=\"{offset}\"", ex);
                }

                return new PartialInputStreamForZipInputStream(baseStream, size, true);
            }
            catch (Exception)
            {
                baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<UInt64, UInt64> AsRandomPartial(this IZipInputStream baseStream, ZipStreamPosition offset, UInt64 size)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomInputStreamForZipInputStream(baseStream, offset, size, true);
            }
            catch (Exception)
            {
                baseStream?.Dispose();
                throw;
            }
        }
    }
}
