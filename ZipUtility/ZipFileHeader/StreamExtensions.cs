using System;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    static class StreamExtensions
    {
        public static Int64 FindFirstSigunature(this IRandomInputByteStream<UInt64> inputStream, UInt32 signature, UInt64 offset, UInt64 count)
            => inputStream.FindFirstSigunature(offset, count, (buffer, index) => buffer.Slice(index, 4).ToUInt32LE() == signature);

        public static Int64 FindFirstSigunature(this IRandomInputByteStream<UInt64> inputStream, UInt64 offset, UInt64 count, Func<ReadOnlyMemory<Byte>, Int32, Boolean> predicate)
        {
            var buffer = new Byte[(sizeof(UInt32))];
            var readOnlyBuffer = buffer.AsReadOnly();
            var index = -(Int64)sizeof(UInt32);
            foreach (var data in inputStream.GetByteSequence(offset, count, true))
            {
                if (index >= 0)
                {
                    if (predicate(readOnlyBuffer, 0))
                        return index;
                }

                Array.Copy(buffer, 1, buffer, 0, buffer.Length - 1);
                buffer[^1] = data;
                ++index;
            }

            return
                predicate(readOnlyBuffer, 0)
                ? index
                : -1;
        }

        public static UInt64? FindLastSigunature(this IRandomInputByteStream<UInt64> inputStream, UInt32 signature, UInt64 offset, UInt64 count)
            => inputStream.FindLastSigunature(offset, count, (buffer, index) => buffer.Slice(index, 4).ToUInt32LE() == signature);

        public static UInt64? FindLastSigunature(this IRandomInputByteStream<UInt64> inputStream, UInt64 offset, UInt64 count, Func<ReadOnlyMemory<Byte>, Int32, Boolean> predicate)
        {
            var buffer = new Byte[(sizeof(UInt32))];
            var readOnlyBuffer = buffer.AsReadOnly();
            var index = offset + count;
            foreach (var data in inputStream.GetReverseByteSequence(offset, count, true))
            {
                if (index <= offset + count - sizeof(UInt32))
                {
                    if (predicate(readOnlyBuffer, 0))
                        return index;
                }

                Array.Copy(buffer, 0, buffer, 1, buffer.Length - 1);
                buffer[0] = data;
                --index;
            }

            return
                predicate(readOnlyBuffer, 0)
                ? index
                : null;
        }
    }
}
