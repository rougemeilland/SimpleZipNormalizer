using System;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    static class StreamExtensions
    {
        private class PartialInputStreamForZipInputStream
            : PartialInputStream<UInt64, ZipStreamPosition>
        {
#if DEBUG && false
            private readonly IInputByteStream<ZipStreamPosition> _baseStream;
#endif
            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, Boolean leaveOpen = false)
                : base(baseStream, leaveOpen)
            {
#if DEBUG && false
                _baseStream = baseStream;
#endif
            }

            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, UInt64 size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
#if DEBUG && false
                _baseStream = baseStream;
#endif
            }

            public PartialInputStreamForZipInputStream(IInputByteStream<ZipStreamPosition> baseStream, UInt64? size, Boolean leaveOpen = false)
                : base(baseStream, size, leaveOpen)
            {
#if DEBUG && false
                _baseStream = baseStream;
#endif
            }

#if DEBUG && false
            public override Int32 Read(Span<Byte> buffer)
            {
                System.Diagnostics.Debug.WriteLine($"Partial stream: read stream={_baseStream}, position={_baseStream.Position}");
                return base.Read(buffer);
            }
#endif

            protected override UInt64 ZeroPositionValue => 0;

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class PartialOutputStreamForZipOutputStream
            : PartialOutputStream<UInt64, ZipStreamPosition>
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

            protected override UInt64 AddPosition(UInt64 x, UInt64 y)
                => checked(x + y);
        }

        private class PartialRandomInputStreamForZipInputStream
            : PartialRandomInputStream<UInt64, ZipStreamPosition>
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

            protected override (Boolean Success, ZipStreamPosition Position) AddBasePosition(ZipStreamPosition x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, EndBasePositionValue);
                }
            }

            protected override (Boolean Success, UInt64 Position) AddPosition(UInt64 x, UInt64 y)
            {
                try
                {
                    return (true, checked(x + y));
                }
                catch (OverflowException)
                {
                    return (false, 0);
                }
            }

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenBasePositions(ZipStreamPosition x, ZipStreamPosition y)
                => x >= y ? (true, x - y) : (false, 0);

            protected override (Boolean Success, UInt64 Distance) GetDistanceBetweenPositions(UInt64 x, UInt64 y)
                => x >= y ? (true, x - y) : (false, 0);
        }

        public static IBasicInputByteStream AsPartial(this IZipInputStream baseStream, ZipStreamPosition offset, UInt64 size)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

#if DEBUG && false
                System.Diagnostics.Debug.WriteLine($"Partial stream: open({baseStream}, {offset}, 0x{size:x16})");
#endif
                baseStream.Seek(offset);
#if DEBUG && false
                System.Diagnostics.Debug.WriteLine($"Partial stream: stream={baseStream}, position={baseStream.Position}");
#endif
                return new PartialInputStreamForZipInputStream(baseStream, size, true);
            }
            catch (Exception)
            {
                baseStream?.Dispose();
                throw;
            }
        }

        public static IBasicOutputByteStream AsPartial(this IZipOutputStream baseStream, ZipStreamPosition offset, UInt64? size)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                baseStream.Seek(offset);
                return new PartialOutputStreamForZipOutputStream(baseStream, size, true);
            }
            catch (Exception)
            {
                baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<UInt64> AsRandomPartial(this IZipInputStream baseStream, ZipStreamPosition offset, UInt64 size)
        {
            try
            {
                return
                    baseStream is null
                    ? throw new ArgumentNullException(nameof(baseStream))
                    : (IRandomInputByteStream<UInt64>)new PartialRandomInputStreamForZipInputStream(baseStream, offset, size, true);
            }
            catch (Exception)
            {
                baseStream?.Dispose();
                throw;
            }
        }
    }
}
