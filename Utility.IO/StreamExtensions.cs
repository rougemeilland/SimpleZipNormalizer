using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Utility;
using Utility.IO.StreamFilters;

namespace Utility.IO
{
    public static class StreamExtensions
    {
        private const Int32 _COPY_TO_DEFAULT_BUFFER_SIZE = 81920;
        private const Int32 _WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE = 81920;

        #region AsInputByteStream

        public static ISequentialInputByteStream AsInputByteStream(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanRead)
                    throw new ArgumentException($"The stream specified by parameter {nameof(baseStream)} is not readable.", nameof(baseStream));

                return
                    baseStream.CanSeek
                    ? new RandomInputByteStreamByDotNetStream(baseStream, leaveOpen)
                    : new SequentialInputByteStreamByDotNetStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsOutputByteStream

        public static ISequentialOutputByteStream AsOutputByteStream(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanWrite)
                    throw new ArgumentException($"The stream specified by parameter {nameof(baseStream)} is not writable.", nameof(baseStream));

                return baseStream.CanSeek
                    ? new RandomOutputByteStreamByDotNetStream(baseStream, leaveOpen)
                    : new SequentialOutputByteStreamByDotNetStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsDotNetStream

        public static Stream AsDotNetStream(this ISequentialInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new DotNetStreamBySequentialInputByteStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static Stream AsDotNetStream(this ISequentialOutputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new DotNetStreamBySequentialOutputByteStream(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsByteStream

        public static ISequentialInputByteStream AsByteStream(this IEnumerable<Byte> baseSequence)
        {
            if (baseSequence is null)
                throw new ArgumentNullException(nameof(baseSequence));

            return new SequentialInputByteStreamBySequence(baseSequence);
        }

        public static ISequentialInputByteStream AsByteStream(this IInputBitStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialInputByteStreamByBitStream(baseStream, BitPackingDirection.MsbToLsb, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialInputByteStream AsByteStream(this IInputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialInputByteStreamByBitStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream AsByteStream(this IOutputBitStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialOutputByteStreamByBitStream(baseStream, BitPackingDirection.MsbToLsb, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream AsByteStream(this IOutputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialOutputByteStreamByBitStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsBitStream

        public static IInputBitStream AsBitStream(this ISequentialInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialInputBitStreamByByteStream(baseStream, BitPackingDirection.Default, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputBitStream AsBitStream(this ISequentialInputByteStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialInputBitStreamByByteStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IInputBitStream AsBitStream(this IEnumerable<Byte> baseSequence, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (baseSequence is null)
                throw new ArgumentNullException(nameof(baseSequence));

            return new SequentialInputBitStreamBySequence(baseSequence, bitPackingDirection);
        }

        public static IOutputBitStream AsBitStream(this ISequentialOutputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialOutputBitStreamByByteStream(baseStream, BitPackingDirection.Default, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IOutputBitStream AsBitStream(this ISequentialOutputByteStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new SequentialOutputBitStreamByByteStream(baseStream, bitPackingDirection, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region AsSequentialAccess

        public static ISequentialInputByteStream AsSequentialAccess<POSITION_T>(this IRandomInputByteStream<POSITION_T> baseStream)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            return baseStream;
        }

        public static ISequentialOutputByteStream AsSequentialAccess<POSITION_T>(this IRandomOutputByteStream<POSITION_T> baseStream)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            return baseStream;
        }

        #endregion

        #region AsRandomAccess

        public static IRandomInputByteStream<POSITION_T> AsRandomAccess<POSITION_T>(this ISequentialInputByteStream baseStream)
            where POSITION_T : struct
        {
            if (baseStream is not IRandomInputByteStream<POSITION_T>)
                throw new ArgumentException($"Stream object {nameof(baseStream)} does not support interface {nameof(IRandomInputByteStream<POSITION_T>)}.", nameof(baseStream));

            return (IRandomInputByteStream<POSITION_T>)baseStream;
        }

        public static IRandomOutputByteStream<POSITION_T> AsRandomAccess<POSITION_T>(this ISequentialOutputByteStream baseStream)
            where POSITION_T : struct
        {
            if (baseStream is not IRandomOutputByteStream<POSITION_T>)
                throw new ArgumentException($"Stream object {nameof(baseStream)} does not support interface {nameof(IRandomOutputByteStream<POSITION_T>)}.", nameof(baseStream));

            return (IRandomOutputByteStream<POSITION_T>)baseStream;
        }

        #endregion

        #region WithPartial

        public static ISequentialInputByteStream WithPartial(this ISequentialInputByteStream baseStream, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> randomAccessStream
                            => new PartialRandomInputStream<UInt64, UInt64>(randomAccessStream, size, 0UL, leaveOpen),
                        _
                            => new PartialSequentialInputStream(baseStream, size, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialInputByteStream WithPartial(this ISequentialInputByteStream baseStream, UInt64 offset, UInt64? size, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> randomAccessStream
                            => new PartialRandomInputStream<UInt64, UInt64>(randomAccessStream, offset, size, 0UL, leaveOpen),
                        _
                            => throw new ArgumentException($"Stream object {nameof(baseStream)} does not support interface {nameof(IRandomInputByteStream<UInt64>)}.", nameof(baseStream))
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<UInt64> WithPartial<BASE_POSITION_T>(this IRandomInputByteStream<BASE_POSITION_T> baseStream, UInt64? size, Boolean leaveOpen = false)
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomInputStream<UInt64, BASE_POSITION_T>(baseStream, size, 0UL, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<UInt64> WithPartial<BASE_POSITION_T>(this IRandomInputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, Boolean leaveOpen = false)
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomInputStream<UInt64, BASE_POSITION_T>(baseStream, offset, size, 0UL, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<POSITION_T> WithPartial<POSITION_T, BASE_POSITION_T>(this IRandomInputByteStream<BASE_POSITION_T> baseStream, UInt64? size, POSITION_T zeroPositionValue, Boolean leaveOpen = false)
            where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomInputStream<POSITION_T, BASE_POSITION_T>(baseStream, size, zeroPositionValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<POSITION_T> WithPartial<POSITION_T, BASE_POSITION_T>(this IRandomInputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, POSITION_T zeroPositionValue, Boolean leaveOpen = false)
            where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomInputStream<POSITION_T, BASE_POSITION_T>(baseStream, offset, size, zeroPositionValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithPartial(this ISequentialOutputByteStream baseStream, UInt64 size, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> randomAccessStream
                            => new PartialRandomOutputStream<UInt64, UInt64>(randomAccessStream, size, 0UL, leaveOpen),
                        _
                            => new PartialSequentialOutputStream(baseStream, size, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithPartial(this ISequentialOutputByteStream baseStream, UInt64 offset, UInt64? size, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> randomAccessStream
                            => new PartialRandomOutputStream<UInt64, UInt64>(randomAccessStream, offset, size, 0UL, leaveOpen),
                        _
                            => throw new ArgumentException($"Stream object {nameof(baseStream)} does not support interface {nameof(IRandomOutputByteStream<UInt64>)}.", nameof(baseStream))
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<UInt64> WithPartial<BASE_POSITION_T>(this IRandomOutputByteStream<BASE_POSITION_T> baseStream, UInt64 size, Boolean leaveOpen = false)
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomOutputStream<UInt64, BASE_POSITION_T>(baseStream, size, 0UL, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<UInt64> WithPartial<BASE_POSITION_T>(this IRandomOutputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, Boolean leaveOpen = false)
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomOutputStream<UInt64, BASE_POSITION_T>(baseStream, offset, size, 0UL, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<POSITION_T> WithPartial<POSITION_T, BASE_POSITION_T>(this IRandomOutputByteStream<BASE_POSITION_T> baseStream, UInt64 size, POSITION_T zeroPositionValue, Boolean leaveOpen = false)
            where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomOutputStream<POSITION_T, BASE_POSITION_T>(baseStream, size, zeroPositionValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<POSITION_T> WithPartial<POSITION_T, BASE_POSITION_T>(this IRandomOutputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, POSITION_T zeroPositionValue, Boolean leaveOpen = false)
            where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
            where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new PartialRandomOutputStream<POSITION_T, BASE_POSITION_T>(baseStream, offset, size, zeroPositionValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region WithCache

        public static ISequentialInputByteStream WithCache(this ISequentialInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStream<UInt64>(baseRandomAccessStream, leaveOpen),
                        _
                            => new BufferedSequentialInputStream(baseStream, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialInputByteStream WithCache(this ISequentialInputByteStream baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomInputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomInputStream<UInt64>(baseRandomAccessStream, cacheSize, leaveOpen),
                        _
                            => new BufferedSequentialInputStream(baseStream, cacheSize, leaveOpen),
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<POSITION_T> WithCache<POSITION_T>(this IRandomInputByteStream<POSITION_T> baseStream, Boolean leaveOpen = false)
            where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new BufferedRandomInputStream<POSITION_T>(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomInputByteStream<POSITION_T> WithCache<POSITION_T>(this IRandomInputByteStream<POSITION_T> baseStream, Int32 cacheSize, Boolean leaveOpen = false)
            where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return new BufferedRandomInputStream<POSITION_T>(baseStream, cacheSize, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithCache(this ISequentialOutputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStream<UInt64>(baseRandomAccessStream, leaveOpen),
                        _
                            => new BufferedSequentialOutputStream(baseStream, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithCache(this ISequentialOutputByteStream baseStream, Int32 cacheSize, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return
                    baseStream switch
                    {
                        IRandomOutputByteStream<UInt64> baseRandomAccessStream
                            => new BufferedRandomOutputStream<UInt64>(baseRandomAccessStream, cacheSize, leaveOpen),
                        _
                            => new BufferedSequentialOutputStream(baseStream, cacheSize, leaveOpen)
                    };
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<POSITION_T> WithCache<POSITION_T>(this IRandomOutputByteStream<POSITION_T> baseStream, Boolean leaveOpen = false)
            where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return new BufferedRandomOutputStream<POSITION_T>(baseStream, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IRandomOutputByteStream<POSITION_T> WithCache<POSITION_T>(this IRandomOutputByteStream<POSITION_T> baseStream, Int32 cacheSize, Boolean leaveOpen = false)
            where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (cacheSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(cacheSize));

                return new BufferedRandomOutputStream<POSITION_T>(baseStream, cacheSize, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region WithProgression

        public static ISequentialInputByteStream WithProgression(this ISequentialInputByteStream baseStream, IProgress<UInt64> progress, Boolean leaveOpen = false)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));

            return new SequentialInputByteStreamWithProgression(baseStream, progress, leaveOpen);
        }

        public static ISequentialOutputByteStream WithProgression(this ISequentialOutputByteStream baseStream, IProgress<UInt64> progress, Boolean leaveOpen = false)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));

            return new SequentialOutputByteStreamWithProgression(baseStream, progress, leaveOpen);
        }

        #endregion

        #region WithEndAction

        public static ISequentialInputByteStream WithEndAction(this ISequentialInputByteStream baseStream, Action<UInt64> endAction, Boolean leaveOpen = false)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (endAction is null)
                throw new ArgumentNullException(nameof(endAction));

            return new SequentialInputByteStreamWithEndAction(baseStream, endAction, leaveOpen);
        }

        public static ISequentialOutputByteStream WithEndAction(this ISequentialOutputByteStream baseStream, Action<UInt64> endAction, Boolean leaveOpen = false)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));
            if (endAction is null)
                throw new ArgumentNullException(nameof(endAction));

            return new SequentialOutputByteStreamWithEndAction(baseStream, endAction, leaveOpen);
        }

        #endregion

        #region WithCrc32Calculation

        public static ISequentialInputByteStream WithCrc32Calculation(this ISequentialInputByteStream baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> resultValueHolder, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (resultValueHolder is null)
                    throw new ArgumentNullException(nameof(resultValueHolder));

                return new SequentialInputByteStreamWithCrc32Calculation(baseStream, Crc32.CreateCalculationState(), resultValue => resultValueHolder.Value = resultValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialInputByteStream WithCrc32Calculation(this ISequentialInputByteStream baseStream, Action<UInt32, UInt64> onCompleted, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (onCompleted is null)
                    throw new ArgumentNullException(nameof(onCompleted));

                return
                    new SequentialInputByteStreamWithCrc32Calculation(
                        baseStream,
                        Crc32.CreateCalculationState(),
                        resultValue =>
                        {
                            try
                            {
                                onCompleted(resultValue.Crc, resultValue.Length);
                            }
                            catch (Exception)
                            {
                            }
                        },
                        leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithCrc32Calculation(this ISequentialOutputByteStream baseStream, ValueHolder<(UInt32 Crc, UInt64 Length)> resultValueHolder, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (resultValueHolder is null)
                    throw new ArgumentNullException(nameof(resultValueHolder));

                return new SequentialOutputByteStreamWithCrc32Calculation(baseStream, Crc32.CreateCalculationState(), resultValue => resultValueHolder.Value = resultValue, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static ISequentialOutputByteStream WithCrc32Calculation(this ISequentialOutputByteStream baseStream, Action<UInt32, UInt64> onCompleted, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (onCompleted is null)
                    throw new ArgumentNullException(nameof(onCompleted));

                return
                    new SequentialOutputByteStreamWithCrc32Calculation(
                        baseStream,
                        Crc32.CreateCalculationState(),
                        resultValue =>
                        {
                            try
                            {
                                onCompleted(resultValue.Crc, resultValue.Length);
                            }
                            catch (Exception)
                            {
                            }
                        },
                        leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region Branch

        public static ISequentialOutputByteStream Branch(this ISequentialOutputByteStream baseStream1, ISequentialOutputByteStream baseStream2, Boolean leaveOpen = false)
        {
            if (baseStream1 is null)
                throw new ArgumentNullException(nameof(baseStream1));
            if (baseStream2 is null)
                throw new ArgumentNullException(nameof(baseStream2));

            return new BranchOutputStream(baseStream1, baseStream2, leaveOpen);
        }

        #endregion

        #region GetByteSequence

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence(progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, count);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetByteSequence(offset, count, progress: progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return InternalGetByteSequence<UInt64>(baseStream, null, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return InternalGetByteSequence<UInt64>(baseStream, null, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> byteStream)
                    throw new NotSupportedException();
                if (offset > byteStream.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return InternalGetByteSequence(byteStream, offset, byteStream.Length - offset, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> byteStream)
                    throw new NotSupportedException();
                if (offset > byteStream.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return InternalGetByteSequence(byteStream, offset, byteStream.Length - offset, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return InternalGetByteSequence(baseRamdomAccessStream, offset, count, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return InternalGetByteSequence(baseRamdomAccessStream, offset, count, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region GetReverseByteSequence

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, count);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this Stream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                return baseStream.AsInputByteStream(leaveOpen).GetReverseByteSequence(offset, count, progress);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new ArgumentException($"The stream specified by parameter {nameof(baseStream)} must be a random access stream.", nameof(baseStream));

                return baseRamdomAccessStream.InternalGetReverseByteSequence<UInt64>(0, baseRamdomAccessStream.Length, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();

                return baseRamdomAccessStream.InternalGetReverseByteSequence<UInt64>(0, baseRamdomAccessStream.Length, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (!offset.IsBetween(0UL, (UInt64)baseRamdomAccessStream.Length))
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return baseRamdomAccessStream.InternalGetReverseByteSequence(offset, baseRamdomAccessStream.Length - offset, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (!offset.IsBetween(0UL, (UInt64)baseRamdomAccessStream.Length))
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return baseRamdomAccessStream.InternalGetReverseByteSequence(offset, baseRamdomAccessStream.Length - offset, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, UInt64 count, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return baseRamdomAccessStream.InternalGetReverseByteSequence(offset, count, null, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public static IEnumerable<Byte> GetReverseByteSequence(this ISequentialInputByteStream baseStream, UInt64 offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (baseStream is not IRandomInputByteStream<UInt64> baseRamdomAccessStream)
                    throw new NotSupportedException();
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                if (checked(offset + count) > baseRamdomAccessStream.Length)
                    throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(baseStream)}.");

                return baseRamdomAccessStream.InternalGetReverseByteSequence(offset, count, progress, leaveOpen);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        #endregion

        #region StreamBytesEqual

        public static Boolean StreamBytesEqual(this Stream stream1, Stream stream2, Boolean leaveOpen = false)
        {
            if (stream1 is null)
                throw new ArgumentNullException(nameof(stream1));
            if (stream2 is null)
                throw new ArgumentNullException(nameof(stream2));

            try
            {
                using var byteStream1 = stream1.AsInputByteStream(true);
                using var byteStream2 = stream2.AsInputByteStream(true);
                return byteStream1.StreamBytesEqual(byteStream2, null, leaveOpen);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this Stream stream1, Stream stream2, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            if (stream1 is null)
                throw new ArgumentNullException(nameof(stream1));
            if (stream2 is null)
                throw new ArgumentNullException(nameof(stream2));

            try
            {
                using var byteStream1 = stream1.AsInputByteStream(true);
                using var byteStream2 = stream2.AsInputByteStream(true);
                return byteStream1.StreamBytesEqual(byteStream2, progress, leaveOpen);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this ISequentialInputByteStream stream1, ISequentialInputByteStream stream2, Boolean leaveOpen = false)
        {
            try
            {
                if (stream1 is null)
                    throw new ArgumentNullException(nameof(stream1));
                if (stream2 is null)
                    throw new ArgumentNullException(nameof(stream2));

                return InternalStreamBytesEqual(stream1, stream2, null);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        public static Boolean StreamBytesEqual(this ISequentialInputByteStream stream1, ISequentialInputByteStream stream2, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (stream1 is null)
                    throw new ArgumentNullException(nameof(stream1));
                if (stream2 is null)
                    throw new ArgumentNullException(nameof(stream2));

                return InternalStreamBytesEqual(stream1, stream2, progress);
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream1?.Dispose();
                    stream2?.Dispose();
                }
            }
        }

        #endregion

        #region CopyTo

        public static void CopyTo(this Stream source, Stream destination, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            using var sourceByteStream = source.AsInputByteStream(true);
            using var destinationByteStream = destination.AsOutputByteStream(true);
            sourceByteStream.InternalCopyTo(destinationByteStream, _COPY_TO_DEFAULT_BUFFER_SIZE, progress);
        }

        public static void CopyTo(this ISequentialInputByteStream source, ISequentialOutputByteStream destination, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            source.InternalCopyTo(destination, _COPY_TO_DEFAULT_BUFFER_SIZE, progress);
        }

        public static void CopyTo(this Stream source, Stream destination, Int32 bufferSize, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            using var sourceByteStream = source.AsInputByteStream(true);
            using var destinationByteStream = destination.AsOutputByteStream(true);
            sourceByteStream.InternalCopyTo(destinationByteStream, bufferSize, progress);
        }

        public static void CopyTo(this ISequentialInputByteStream source, ISequentialOutputByteStream destination, Int32 bufferSize, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            source.InternalCopyTo(destination, bufferSize, progress);
        }

        #endregion

        #region Read

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            return stream.Read(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer, Int32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return
                stream.Read(
                    buffer,
                    offset,
                    buffer.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this Stream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif
            return
                (UInt32)stream.Read(
                    buffer,
                    (Int32)offset,
                    buffer.Length - (Int32)offset)
                .Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Byte[] buffer, Range range)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            return stream.Read(buffer, offset, count);
        }

#if false
        public static Int32 Read(this Stream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.Read(Byte[], Int32, Int32)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this Stream stream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return
                (UInt32)stream.Read(
                    buffer,
                    (Int32)offset,
                    (Int32)count).Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this Stream stream, Memory<Byte> buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            return stream.Read(buffer.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this ISequentialInputByteStream stream, Byte[] buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            return stream.Read(buffer.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this ISequentialInputByteStream stream, Byte[] buffer, Int32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return stream.Read(buffer.AsSpan(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this ISequentialInputByteStream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return (UInt32)stream.Read(buffer.AsSpan(offset)).Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this ISequentialInputByteStream stream, Byte[] buffer, Range range)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            return stream.Read(buffer.AsSpan(offset, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this ISequentialInputByteStream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return stream.Read(buffer.AsSpan(offset, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Read(this ISequentialInputByteStream stream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return (UInt32)stream.Read(buffer.AsSpan(offset, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Read(this ISequentialInputByteStream stream, Memory<Byte> buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            return stream.Read(buffer.Span);
        }

        #endregion

        #region ReadByteOrNull

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte? ReadByteOrNull(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[1];
            return
                sourceStream.Read(buffer) > 0
                ? buffer[0]
                : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte? ReadByteOrNull(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[1];
            return
                sourceStream.Read(buffer) > 0
                ? buffer[0]
                : null;
        }

        #endregion

        #region ReadByte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte ReadByte(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            Span<Byte> buffer = stackalloc Byte[1];
            if (sourceStream.Read(buffer) <= 0)
                throw new UnexpectedEndOfStreamException();

            return buffer[0];
        }

        #endregion

        #region ReadBytes

        public static ReadOnlyMemory<Byte> ReadBytes(this Stream sourceStream, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this Stream sourceStream, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            return
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Int32 offset)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return
                InternalReadBytes(
                    offset,
                    buffer.Length - offset,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
        }

        public static UInt32 ReadBytes(this Stream sourceStream, Byte[] buffer, UInt32 offset)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return
                (UInt32)InternalReadBytes(
                    (Int32)offset,
                    buffer.Length - (Int32)offset,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count)).Maximum(0);
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Range range)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            return
                InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
        }

        public static Int32 ReadBytes(this Stream sourceStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return
                InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
        }

        public static UInt32 ReadBytes(this Stream sourceStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset) + count > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return
                (UInt32)InternalReadBytes(
                    (Int32)offset,
                    (Int32)count,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count)).Maximum(0);
        }

        public static Int32 ReadBytes(this Stream sourceStream, Memory<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            return
                InternalReadBytes(
                    buffer,
                    _buffer => sourceStream.Read(_buffer.Span));
        }

        public static Int32 ReadBytes(this Stream sourceStream, Span<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var processedCount = 0;
            while (!buffer.IsEmpty)
            {
                var length = sourceStream.Read(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                processedCount += length;
            }

            return processedCount;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this ISequentialInputByteStream sourceStream, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer, _offset, _count));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static ReadOnlyMemory<Byte> ReadBytes(this ISequentialInputByteStream sourceStream, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var buffer = new Byte[count];
            var length =
                InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
            if (length < buffer.Length)
                Array.Resize(ref buffer, length);
            return buffer;
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            return InternalReadBytes(
                    0,
                    buffer.Length,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer, Int32 offset)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return InternalReadBytes(
                offset,
                buffer.Length - offset,
                (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
        }

        public static UInt32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer, UInt32 offset)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return
                (UInt32)InternalReadBytes(
                (Int32)offset,
                buffer.Length - (Int32)offset,
                (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count))).Maximum(0);
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer, Range range)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            return InternalReadBytes(
                offset,
                count,
                (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return
                InternalReadBytes(
                    offset,
                    count,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count)));
        }

        public static UInt32 ReadBytes(this ISequentialInputByteStream sourceStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return
                (UInt32)InternalReadBytes(
                    (Int32)offset,
                    (Int32)count,
                    (_offset, _count) => sourceStream.Read(buffer.AsSpan(_offset, _count))).Maximum(0);
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Memory<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            return InternalReadBytes(buffer, _buffer => sourceStream.Read(_buffer.Span));
        }

        public static Int32 ReadBytes(this ISequentialInputByteStream sourceStream, Span<Byte> buffer)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            var processedCount = 0;
            while (!buffer.IsEmpty)
            {
                var length = sourceStream.Read(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                processedCount += length;
            }

            return processedCount;
        }

        #endregion

        #region ReadByteSequence

        public static IEnumerable<Byte> ReadByteSequence(this Stream sourceStream, Int64 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return
                InternalReadByteSequence(
                    (UInt64)count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });
        }

        public static IEnumerable<Byte> ReadByteSequence(this Stream sourceStream, UInt64 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            return
                InternalReadByteSequence(
                    count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });
        }

        public static IEnumerable<Byte> ReadByteSequence(this ISequentialInputByteStream sourceStream, Int64 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return
                InternalReadByteSequence(
                    (UInt64)count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });
        }

        public static IEnumerable<Byte> ReadByteSequence(this ISequentialInputByteStream sourceStream, UInt64 count)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            return
                InternalReadByteSequence(
                    count,
                    _count =>
                    {
                        var buffer = new Byte[_count];
                        var length = sourceStream.Read(buffer);
                        if (length <= 0)
                            return null;
                        if (length < buffer.Length)
                            Array.Resize(ref buffer, length);
                        return buffer;
                    });
        }

        #endregion

        #region ReadAllBytes

        public static ReadOnlyMemory<Byte> ReadAllBytes(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            return InternalReadAllBytes(sourceStream.Read);
        }

        public static ReadOnlyMemory<Byte> ReadAllBytes(this ISequentialInputByteStream sourceByteStream)
        {
            if (sourceByteStream is null)
                throw new ArgumentNullException(nameof(sourceByteStream));

            return InternalReadAllBytes((_buffer, _offset, _count) => sourceByteStream.Read(_buffer.AsSpan(_offset, _count)));
        }

        #endregion

        #region ReadInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt16LE();
        }

        #endregion

        #region ReadUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt16LE();
        }

        #endregion

        #region ReadInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt32LE();
        }

        #endregion

        #region ReadUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return
                buffer.ToUInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt32LE();
        }

        #endregion

        #region ReadInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt64LE();
        }

        #endregion

        #region ReadUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64LE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64LE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt64LE();
        }

        #endregion

        #region ReadSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToSingleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleLE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToSingleLE();
        }

        #endregion

        #region ReadDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDoubleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleLE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDoubleLE();
        }

        #endregion

        #region ReadDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalLE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDecimalLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalLE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDecimalLE();
        }

        #endregion

        #region ReadInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt16BE();
        }

        #endregion

        #region ReadUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt16BE();
        }

        #endregion

        #region ReadInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt32BE();
        }

        #endregion

        #region ReadUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt32BE();
        }

        #endregion

        #region ReadInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToInt64BE();
        }

        #endregion

        #region ReadUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64BE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64BE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToUInt64BE();
        }

        #endregion

        #region ReadSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToSingleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ReadSingleBE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToSingleBE();
        }

        #endregion

        #region ReadDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDoubleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ReadDoubleBE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDoubleBE();
        }

        #endregion

        #region ReadDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalBE(this Stream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDecimalBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ReadDecimalBE(this ISequentialInputByteStream sourceStream)
        {
            if (sourceStream is null)
                throw new ArgumentNullException(nameof(sourceStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            if (sourceStream.ReadBytes(buffer) != buffer.Length)
                throw new UnexpectedEndOfStreamException();

            return buffer.ToDecimalBE();
        }

        #endregion

        #region Write

#if false
        public static Int32 Write(this Stream stream, Byte[] buffer)
        {
            throw new NotImplementedException(); // equivalent to System.IO.Stream.Write(ReadOnlyMemory<Byte>)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this Stream stream, Byte[] buffer, Int32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            var count = buffer.Length - offset;
            stream.Write(buffer, offset, count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this Stream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            var count = buffer.Length - (Int32)offset;
            stream.Write(buffer, (Int32)offset, count);
            return (UInt32)count;
        }

#if false

        public static Int32 Write(this Stream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.Write(Byte[], Int32, Int32)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this Stream stream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            stream.Write(buffer, (Int32)offset, (Int32)count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this Stream stream, ReadOnlyMemory<Byte> buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            stream.Write(buffer.Span);
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this ISequentialOutputByteStream stream, Byte[] buffer, Int32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return stream.Write(buffer.AsSpan(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this ISequentialOutputByteStream stream, Byte[] buffer, UInt32 offset)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return (UInt32)stream.Write(buffer.AsSpan((Int32)offset)).Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this ISequentialOutputByteStream stream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return stream.Write(buffer.AsSpan(offset, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Write(this ISequentialOutputByteStream stream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            return (UInt32)stream.Write(buffer.AsSpan(offset, count)).Maximum(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Write(this ISequentialOutputByteStream stream, ReadOnlyMemory<Byte> buffer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            return stream.Write(buffer.Span);
        }

        #endregion

        #region WriteByte

#if false
        public static void WriteByte(this Stream stream, Byte value)
        {
            throw new NotImplementedException(); // defined in System.IO.Stream.WriteByte(Byte)
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(this ISequentialOutputByteStream stream, Byte value)
        {
            Span<Byte> buffer = stackalloc Byte[1];
            buffer[0] = value;
            var length = stream.Write(buffer);
            if (length <= 0)
                throw new InternalLogicalErrorException();
        }

        #endregion

        #region WriteBytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            destinationStream.Write(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Int32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            destinationStream.Write(buffer, offset, buffer.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, UInt32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            destinationStream.Write(buffer, (Int32)offset, buffer.Length - (Int32)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Range range)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            destinationStream.Write(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            destinationStream.Write(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            destinationStream.Write(buffer, (Int32)offset, (Int32)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, ReadOnlyMemory<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            destinationStream.Write(buffer.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, ReadOnlySpan<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            destinationStream.Write(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this Stream destinationStream, IEnumerable<ReadOnlyMemory<Byte>> buffers)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffers is null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                destinationStream.Write(buffer.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            InternalWriteBytes(0, buffer.Length, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer, Int32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            InternalWriteBytes(0, buffer.Length, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer, UInt32 offset)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
#endif

            InternalWriteBytes((Int32)offset, buffer.Length - (Int32)offset, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer, Range range)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalWriteBytes(offset, count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer, Int32 offset, Int32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            InternalWriteBytes(offset, count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, Byte[] buffer, UInt32 offset, UInt32 count)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");
#if DEBUG
            if (offset > Int32.MaxValue)
                throw new Exception();
            if (count > Int32.MaxValue)
                throw new Exception();
#endif

            InternalWriteBytes((Int32)offset, (Int32)count, (_offset, _count) => destinationStream.Write(buffer.AsSpan(_offset, _count)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, ReadOnlyMemory<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            var spanOfBuffer = buffer.Span;
            while (!spanOfBuffer.IsEmpty)
            {
                var length = destinationStream.Write(spanOfBuffer);
                if (length <= 0)
                    throw new IOException("Can not write any more");
                spanOfBuffer = spanOfBuffer[length..];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, ReadOnlySpan<Byte> buffer)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            while (!buffer.IsEmpty)
            {
                var length = destinationStream.Write(buffer);
                if (length <= 0)
                    throw new IOException("Can not write any more");
                buffer = buffer[length..];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytes(this ISequentialOutputByteStream destinationStream, IEnumerable<ReadOnlyMemory<Byte>> buffers)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (buffers is null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteByteSequence

        public static void WriteByteSequence(this Stream destinationStream, IEnumerable<Byte> sequence)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            using var enumerator = sequence.GetEnumerator();
            var buffer = new Byte[_WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE];
            var isEndOfSequence = false;
            while (!isEndOfSequence)
            {
                var index = 0;
                while (index < buffer.Length)
                {
                    if (!enumerator.MoveNext())
                    {
                        isEndOfSequence = true;
                        break;
                    }

                    buffer[index++] = enumerator.Current;
                }

                if (index > 0)
                    destinationStream.Write(buffer, 0, index);
            }
        }

        public static void WriteByteSequence(this ISequentialOutputByteStream destinationStream, IEnumerable<Byte> sequence)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));
            if (sequence is null)
                throw new ArgumentNullException(nameof(sequence));

            using var enumerator = sequence.GetEnumerator();
            var buffer = new Byte[_WRITE_BYTE_SEQUENCE_DEFAULT_BUFFER_SIZE];
            var isEndOfSequence = false;
            while (!isEndOfSequence)
            {
                var index = 0;
                while (index < buffer.Length)
                {
                    if (!enumerator.MoveNext())
                    {
                        isEndOfSequence = true;
                        break;
                    }

                    buffer[index++] = enumerator.Current;
                }

                if (index > 0)
                    destinationStream.WriteBytes(buffer.AsSpan(0, index));
            }
        }

        #endregion

        #region WriteInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LE(this Stream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LE(this ISequentialOutputByteStream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LE(this Stream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LE(this ISequentialOutputByteStream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LE(this Stream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LE(this ISequentialOutputByteStream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LE(this Stream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LE(this ISequentialOutputByteStream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LE(this Stream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LE(this ISequentialOutputByteStream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LE(this Stream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LE(this ISequentialOutputByteStream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLE(this Stream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLE(this ISequentialOutputByteStream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLE(this Stream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLE(this ISequentialOutputByteStream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalLE(this Stream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalLE(this ISequentialOutputByteStream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueLE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BE(this Stream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BE(this ISequentialOutputByteStream destinationStream, Int16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BE(this Stream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BE(this ISequentialOutputByteStream destinationStream, UInt16 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt16)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BE(this Stream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BE(this ISequentialOutputByteStream destinationStream, Int32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BE(this Stream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BE(this ISequentialOutputByteStream destinationStream, UInt32 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt32)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BE(this Stream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BE(this ISequentialOutputByteStream destinationStream, Int64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Int64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BE(this Stream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BE(this ISequentialOutputByteStream destinationStream, UInt64 value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(UInt64)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleBE(this Stream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleBE(this ISequentialOutputByteStream destinationStream, Single value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Single)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleBE(this Stream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleBE(this ISequentialOutputByteStream destinationStream, Double value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Double)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region WriteDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalBE(this Stream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimalBE(this ISequentialOutputByteStream destinationStream, Decimal value)
        {
            if (destinationStream is null)
                throw new ArgumentNullException(nameof(destinationStream));

            Span<Byte> buffer = stackalloc Byte[sizeof(Decimal)];
            buffer.SetValueBE(value);
            destinationStream.WriteBytes(buffer);
        }

        #endregion

        #region CalculateCrc24

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc24(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc24(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc24(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this Stream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc24(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this ISequentialInputByteStream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc24(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this ISequentialInputByteStream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc24(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this ISequentialInputByteStream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc24(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this ISequentialInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc24(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        #endregion

        #region CalculateCrc32

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc32(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc32(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc32(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this Stream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.AsInputByteStream(true).InternalCalculateCrc32(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this ISequentialInputByteStream inputStream, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc32(MAX_BUFFER_SIZE, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this ISequentialInputByteStream inputStream, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            const Int32 MAX_BUFFER_SIZE = 80 * 1024;

            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc32(MAX_BUFFER_SIZE, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this ISequentialInputByteStream inputStream, Int32 bufferSize, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc32(bufferSize, null);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this ISequentialInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, Boolean leaveOpen = false)
        {
            try
            {
                if (inputStream is null)
                    throw new ArgumentNullException(nameof(inputStream));

                return inputStream.InternalCalculateCrc32(bufferSize, progress);
            }
            finally
            {
                if (!leaveOpen)
                    inputStream?.Dispose();
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalGetByteSequence<POSITION_T>(ISequentialInputByteStream baseStream, UInt64? count, IProgress<UInt64>? progress, Boolean leaveOpen)
            where POSITION_T : struct
        {
            const Int32 BUFFER_SIZE = 80 * 1024;

            var processedCounter = new ProgressCounterUInt64(progress);
            try
            {
                processedCounter.Report();
                var buffer = new Byte[BUFFER_SIZE];
                while (true)
                {
                    var readCount = buffer.Length;
                    if (count is not null)
                        readCount = (Int32)((UInt64)readCount).Minimum(count.Value - processedCounter.Value);
                    if (readCount <= 0)
                        break;
                    var length = baseStream.Read(buffer, 0, readCount);
                    if (length <= 0)
                        break;
                    for (var index = 0; index < length; ++index)
                    {
                        yield return buffer[index];
                        processedCounter.Increment();
                    }
                }
            }
            finally
            {
                if (!leaveOpen)
                    baseStream.Dispose();
                processedCounter.Report();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalGetByteSequence<POSITION_T>(ISequentialInputByteStream baseStream, POSITION_T offset, UInt64? count, IProgress<UInt64>? progress, Boolean leaveOpen)
            where POSITION_T : struct
        {
            const Int32 BUFFER_SIZE = 80 * 1024;

            var processedCounter = new ProgressCounterUInt64(progress);
            try
            {
                if (baseStream is not IRandomInputByteStream<POSITION_T> randomAccessStream)
                    throw new ArgumentException($"If stream {nameof(baseStream)} is sequential, parameter {nameof(offset)} must not be specified.", nameof(offset));

                randomAccessStream.Seek(offset);
                processedCounter.Report();
                var buffer = new Byte[BUFFER_SIZE];
                while (true)
                {
                    var readCount = buffer.Length;
                    if (count is not null)
                        readCount = (Int32)((UInt64)readCount).Minimum(count.Value - processedCounter.Value);
                    if (readCount <= 0)
                        break;
                    var length = baseStream.Read(buffer, 0, readCount);
                    if (length <= 0)
                        break;
                    for (var index = 0; index < length; ++index)
                    {
                        yield return buffer[index];
                        processedCounter.Increment();
                    }
                }
            }
            finally
            {
                if (!leaveOpen)
                    baseStream.Dispose();
                processedCounter.Report();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalGetReverseByteSequence<POSITION_T>(this IRandomInputByteStream<POSITION_T> baseStream, POSITION_T offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen = false)
            where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        {
            const Int32 BUFFER_SIZE = 80 * 1024;

            var progressCounter = new ProgressCounterUInt64(progress);
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                progressCounter.Report();
                var buffer = new Byte[BUFFER_SIZE];
                var pos = checked(offset + count);
                while (pos.CompareTo(offset + BUFFER_SIZE) > 0)
                {
                    pos -= BUFFER_SIZE;
                    baseStream.Seek(pos);
                    var length = baseStream.ReadBytes(buffer);
                    if (length != buffer.Length)
                        throw new InternalLogicalErrorException();
                    for (var index = BUFFER_SIZE - 1; index >= 0; --index)
                    {
                        yield return buffer[index];
                        progressCounter.Increment();
                    }
                }

                if (pos.CompareTo(offset) > 0)
                {
                    var remain = checked((Int32)(pos - offset));
                    var length = baseStream.ReadBytes(buffer.AsMemory(0, remain));
                    if (length != remain)
                        throw new InternalLogicalErrorException();
                    for (var index = remain - 1; index >= 0; --index)
                    {
                        yield return buffer[index];
                        progressCounter.Increment();
                    }
                }
            }
            finally
            {
                if (!leaveOpen)
                    baseStream.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalStreamBytesEqual(this ISequentialInputByteStream stream1, ISequentialInputByteStream stream2, IProgress<UInt64>? progress)
        {
            const Int32 bufferSize = 81920;

            if (bufferSize % sizeof(UInt64) != 0)
                throw new InternalLogicalErrorException();

            var processedCounter = new ProgressCounterUInt64(progress);
            processedCounter.Report();
            var buffer1 = new Byte[bufferSize];
            var buffer2 = new Byte[bufferSize];
            try
            {
                while (true)
                {
                    // まず両方のストリームから bufferSize バイトだけ読み込みを試みる
                    var bufferCount1 = stream1.ReadBytes(buffer1);
                    var bufferCount2 = stream2.ReadBytes(buffer2);
                    processedCounter.AddValue((UInt32)bufferCount1);

                    if (bufferCount1 != bufferCount2)
                    {
                        // 実際に読み込めたサイズが異なっている場合はどちらかだけがEOFに達したということなので、ストリームの内容が異なると判断しfalseを返す。
                        return false;
                    }

                    // この時点で bufferCount1 == bufferCount2 (どちらのストリームも読み込めたサイズは同じ)

                    if (!buffer1.SequenceEqual(0, buffer2, 0, bufferCount1))
                    {
                        // バッファの内容が一致しなかった場合は false を返す。
                        return false;
                    }

                    if (bufferCount1 < buffer1.Length)
                    {
                        // どちらのストリームも同時にEOFに達したがそれまでに読み込めたデータはすべて一致していた場合
                        // 全てのデータが一致したと判断して true を返す。
                        return true;
                    }
                }
            }
            finally
            {
                processedCounter.Report();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyTo(this ISequentialInputByteStream source, ISequentialOutputByteStream destination, Int32 bufferSize, IProgress<UInt64>? progress)
        {
            var processedCounter = new ProgressCounterUInt64(progress);
            processedCounter.Report();
            var buffer = new Byte[bufferSize];
            try
            {
                while (true)
                {
                    var length = source.ReadBytes(buffer);
                    if (length <= 0)
                        break;
                    destination.WriteBytes(buffer.AsSpan(0, length).AsReadOnly());
                    processedCounter.AddValue((UInt32)length);
                }

                destination.Flush();
            }
            finally
            {
                processedCounter.Report();
            }
        }

        #region InternalReadBytes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalReadBytes(Int32 offset, Int32 count, Func<Int32, Int32, Int32> reader)
        {
            var index = 0;
            while (count > 0)
            {
                var length = reader(offset + index, count);
                if (length <= 0)
                    break;
                index += length;
                count -= length;
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalReadBytes(Memory<Byte> buffer, Func<Memory<Byte>, Int32> reader)
        {
            var index = 0;
            while (!buffer.IsEmpty)
            {
                var length = reader(buffer);
                if (length <= 0)
                    break;
                buffer = buffer[length..];
                index += length;
            }

            return index;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Byte> InternalReadByteSequence(UInt64 count, Func<Int32, IEnumerable<Byte>?> reader)
        {
            var byteArrayChain = Array.Empty<Byte>().AsEnumerable();
            while (count > 0)
            {
                var length = (Int32)count.Minimum((UInt32)Int32.MaxValue);
                var data = reader(length);
                if (data is null)
                    break;
                byteArrayChain = byteArrayChain.Concat(data);
                count -= (UInt32)length;
            }

            return byteArrayChain;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlyMemory<Byte> InternalReadAllBytes(Func<Byte[], Int32, Int32, Int32> reader)
        {
            const Int32 BUFFER_SIZE = 80 * 2024;
            var buffers = new Queue<Byte[]>();
            var dataLength = 0;
            while (true)
            {
                var partialBuffer = new Byte[BUFFER_SIZE];
                var length = reader(partialBuffer, 0, partialBuffer.Length);
                if (length <= 0)
                    break;
                if (length < partialBuffer.Length)
                    Array.Resize(ref partialBuffer, length);
                buffers.Enqueue(partialBuffer);
                dataLength += length;
            }

            if (buffers.Count <= 0)
            {
                return ReadOnlyMemory<Byte>.Empty;
            }
            else if (buffers.Count == 1)
            {
                return buffers.Dequeue();
            }
            else
            {
                var buffer = new Byte[dataLength];
                var destinationWindow = buffer.AsMemory();
                while (buffers.Count > 0)
                {
                    var partialBuffer = buffers.Dequeue();
                    partialBuffer.CopyTo(destinationWindow);
                    destinationWindow = destinationWindow[partialBuffer.Length..];
                }
#if DEBUG
                if (!destinationWindow.IsEmpty)
                    throw new Exception();
#endif
                return buffer;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalWriteBytes(Int32 offset, Int32 count, Func<Int32, Int32, Int32> writer)
        {
            while (count > 0)
            {
                var length = writer(offset, count);
                if (length <= 0)
                    break;
                checked
                {
                    offset += length;
                    count -= length;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc24(this ISequentialInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress)
            => InternalCalculateCrc(inputStream, bufferSize, progress, Crc24.CreateCalculationState());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc32(this ISequentialInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress)
            => InternalCalculateCrc(inputStream, bufferSize, progress, Crc32.CreateCalculationState());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (UInt32 Crc, UInt64 Length) InternalCalculateCrc(ISequentialInputByteStream inputStream, Int32 bufferSize, IProgress<UInt64>? progress, ICrcCalculationState<UInt32> session)
        {
            var processedCounter = new ProgressCounterUInt64(progress);
            processedCounter.Report();
            var buffer = new Byte[bufferSize];
            try
            {
                while (true)
                {
                    var length = inputStream.Read(buffer);
                    if (length <= 0)
                        break;
                    session.Put(buffer, 0, length);
                    processedCounter.AddValue((UInt32)length);
                }

                return session.GetResultValue();
            }
            finally
            {
                processedCounter.Report();
            }
        }
    }
}
