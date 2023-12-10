using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utility.Text;

namespace Utility
{
    public static class ByteArrayExtensions
    {
        #region private class

        private class Crc32Calculator
            : CrcCalculationMethod<UInt32>
        {
            protected override UInt32 InitialValue => 0xffffffffU;
            protected override UInt32 Update(UInt32 crc, Byte data) => _crcTableOfCommonCrc32[(crc ^ data) & 0xff] ^ (crc >> 8);
            protected override UInt32 Finalize(UInt32 crc) => ~crc;
        }

        private class Crc24ForRadix64Calculator
            : CrcCalculationMethod<UInt32>
        {
            protected override UInt32 InitialValue => 0x00b704ceU;
            protected override UInt32 Update(UInt32 crc, Byte data) => _crcTableOfCrc24ForRadix64[((crc >> 16) ^ data) & 0xff] ^ (crc << 8);
            protected override UInt32 Finalize(UInt32 crc) => crc & 0xffffffU;
        }

        #endregion

        private const Byte _BYTE_ESCAPE_CHAR = 0x1b;
        private const Byte _BYTE_AT_MARK_CHAR = 0x40;
        private const Byte _BYTE_DOLLAR_CHAR = 0x24;
        private const Byte _BYTE_AMPERSAND_CHAR = 0x26;
        private const Byte _BYTE_OPEN_PARENTHESIS_CHAR = 0x28;
        private const Byte _BYTE_B_CHAR = 0x42;
        private const Byte _BYTE_D_CHAR = 0x44;
        private const Byte _BYTE_J_CHAR = 0x4a;
        private const Byte _BYTE_I_CHAR = 0x49;

        private static readonly UInt32[] _crcTableOfCommonCrc32;
        private static readonly UInt32[] _crcTableOfCrc24ForRadix64;
        private static readonly CrcCalculationMethod<UInt32> _commonCrc32;
        private static readonly CrcCalculationMethod<UInt32> _crc24ForRadix64;

        static ByteArrayExtensions()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _crcTableOfCommonCrc32 = new UInt32[]
            {
                0x00000000, 0x77073096, 0xee0e612c, 0x990951ba,
                0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
                0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
                0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
                0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
                0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
                0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec,
                0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
                0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
                0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
                0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940,
                0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
                0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116,
                0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
                0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
                0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a,
                0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
                0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818,
                0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
                0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
                0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c,
                0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
                0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
                0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
                0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
                0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
                0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086,
                0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4,
                0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
                0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
                0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
                0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
                0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
                0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe,
                0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
                0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
                0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
                0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252,
                0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60,
                0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
                0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
                0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04,
                0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
                0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a,
                0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
                0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
                0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
                0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e,
                0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
                0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
                0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
                0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
                0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
                0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0,
                0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6,
                0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
                0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d,
            };

            _crcTableOfCrc24ForRadix64 = new UInt32[]
            {
                0x00000000, 0x00864cfb, 0x008ad50d, 0x000c99f6,
                0x0093e6e1, 0x0015aa1a, 0x001933ec, 0x009f7f17,
                0x00a18139, 0x0027cdc2, 0x002b5434, 0x00ad18cf,
                0x003267d8, 0x00b42b23, 0x00b8b2d5, 0x003efe2e,
                0x00c54e89, 0x00430272, 0x004f9b84, 0x00c9d77f,
                0x0056a868, 0x00d0e493, 0x00dc7d65, 0x005a319e,
                0x0064cfb0, 0x00e2834b, 0x00ee1abd, 0x00685646,
                0x00f72951, 0x007165aa, 0x007dfc5c, 0x00fbb0a7,
                0x000cd1e9, 0x008a9d12, 0x008604e4, 0x0000481f,
                0x009f3708, 0x00197bf3, 0x0015e205, 0x0093aefe,
                0x00ad50d0, 0x002b1c2b, 0x002785dd, 0x00a1c926,
                0x003eb631, 0x00b8faca, 0x00b4633c, 0x00322fc7,
                0x00c99f60, 0x004fd39b, 0x00434a6d, 0x00c50696,
                0x005a7981, 0x00dc357a, 0x00d0ac8c, 0x0056e077,
                0x00681e59, 0x00ee52a2, 0x00e2cb54, 0x006487af,
                0x00fbf8b8, 0x007db443, 0x00712db5, 0x00f7614e,
                0x0019a3d2, 0x009fef29, 0x009376df, 0x00153a24,
                0x008a4533, 0x000c09c8, 0x0000903e, 0x0086dcc5,
                0x00b822eb, 0x003e6e10, 0x0032f7e6, 0x00b4bb1d,
                0x002bc40a, 0x00ad88f1, 0x00a11107, 0x00275dfc,
                0x00dced5b, 0x005aa1a0, 0x00563856, 0x00d074ad,
                0x004f0bba, 0x00c94741, 0x00c5deb7, 0x0043924c,
                0x007d6c62, 0x00fb2099, 0x00f7b96f, 0x0071f594,
                0x00ee8a83, 0x0068c678, 0x00645f8e, 0x00e21375,
                0x0015723b, 0x00933ec0, 0x009fa736, 0x0019ebcd,
                0x008694da, 0x0000d821, 0x000c41d7, 0x008a0d2c,
                0x00b4f302, 0x0032bff9, 0x003e260f, 0x00b86af4,
                0x002715e3, 0x00a15918, 0x00adc0ee, 0x002b8c15,
                0x00d03cb2, 0x00567049, 0x005ae9bf, 0x00dca544,
                0x0043da53, 0x00c596a8, 0x00c90f5e, 0x004f43a5,
                0x0071bd8b, 0x00f7f170, 0x00fb6886, 0x007d247d,
                0x00e25b6a, 0x00641791, 0x00688e67, 0x00eec29c,
                0x003347a4, 0x00b50b5f, 0x00b992a9, 0x003fde52,
                0x00a0a145, 0x0026edbe, 0x002a7448, 0x00ac38b3,
                0x0092c69d, 0x00148a66, 0x00181390, 0x009e5f6b,
                0x0001207c, 0x00876c87, 0x008bf571, 0x000db98a,
                0x00f6092d, 0x007045d6, 0x007cdc20, 0x00fa90db,
                0x0065efcc, 0x00e3a337, 0x00ef3ac1, 0x0069763a,
                0x00578814, 0x00d1c4ef, 0x00dd5d19, 0x005b11e2,
                0x00c46ef5, 0x0042220e, 0x004ebbf8, 0x00c8f703,
                0x003f964d, 0x00b9dab6, 0x00b54340, 0x00330fbb,
                0x00ac70ac, 0x002a3c57, 0x0026a5a1, 0x00a0e95a,
                0x009e1774, 0x00185b8f, 0x0014c279, 0x00928e82,
                0x000df195, 0x008bbd6e, 0x00872498, 0x00016863,
                0x00fad8c4, 0x007c943f, 0x00700dc9, 0x00f64132,
                0x00693e25, 0x00ef72de, 0x00e3eb28, 0x0065a7d3,
                0x005b59fd, 0x00dd1506, 0x00d18cf0, 0x0057c00b,
                0x00c8bf1c, 0x004ef3e7, 0x00426a11, 0x00c426ea,
                0x002ae476, 0x00aca88d, 0x00a0317b, 0x00267d80,
                0x00b90297, 0x003f4e6c, 0x0033d79a, 0x00b59b61,
                0x008b654f, 0x000d29b4, 0x0001b042, 0x0087fcb9,
                0x001883ae, 0x009ecf55, 0x009256a3, 0x00141a58,
                0x00efaaff, 0x0069e604, 0x00657ff2, 0x00e33309,
                0x007c4c1e, 0x00fa00e5, 0x00f69913, 0x0070d5e8,
                0x004e2bc6, 0x00c8673d, 0x00c4fecb, 0x0042b230,
                0x00ddcd27, 0x005b81dc, 0x0057182a, 0x00d154d1,
                0x0026359f, 0x00a07964, 0x00ace092, 0x002aac69,
                0x00b5d37e, 0x00339f85, 0x003f0673, 0x00b94a88,
                0x0087b4a6, 0x0001f85d, 0x000d61ab, 0x008b2d50,
                0x00145247, 0x00921ebc, 0x009e874a, 0x0018cbb1,
                0x00e37b16, 0x006537ed, 0x0069ae1b, 0x00efe2e0,
                0x00709df7, 0x00f6d10c, 0x00fa48fa, 0x007c0401,
                0x0042fa2f, 0x00c4b6d4, 0x00c82f22, 0x004e63d9,
                0x00d11cce, 0x00575035, 0x005bc9c3, 0x00dd8538,
            };

            _commonCrc32 = new Crc32Calculator();
            _crc24ForRadix64 = new Crc24ForRadix64Calculator();
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc32(this IEnumerable<Byte> source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return _commonCrc32.Calculate(source, progress);
        }

        public static UInt32 CalculateCrc32(this ReadOnlyMemory<Byte> source)
            => _commonCrc32.Calculate(source);

        public static UInt32 CalculateCrc32(this ReadOnlySpan<Byte> source)
            => _commonCrc32.Calculate(source);

        public static async Task<(UInt32 Crc, UInt64 Length)> CalculateCrc32Async(this IAsyncEnumerable<Byte> source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return await _commonCrc32.CalculateAsync(source, progress).ConfigureAwait(false);
        }

        public static IEnumerable<Byte> GetSequenceWithCrc32(this IEnumerable<Byte> source, ValueHolder<(UInt32 Crc, UInt64 Length)> crcValueHolder, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (crcValueHolder is null)
                throw new ArgumentNullException(nameof(crcValueHolder));

            return _commonCrc32.GetSequenceWithCrc(source, crcValueHolder, progress);
        }

        public static IAsyncEnumerable<Byte> GetAsyncSequenceWithCrc32(this IAsyncEnumerable<Byte> source, ValueHolder<(UInt32 Crc, UInt64 Length)> crcValueHolder, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (crcValueHolder is null)
                throw new ArgumentNullException(nameof(crcValueHolder));

            return _commonCrc32.GetAsyncSequenceWithCrc(source, crcValueHolder, progress);
        }

        public static (UInt32 Crc, UInt64 Length) CalculateCrc24(this IEnumerable<Byte> source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return _crc24ForRadix64.Calculate(source, progress);
        }

        public static async Task<(UInt32 Crc, UInt64 Length)> CalculateCrc24Async(this IAsyncEnumerable<Byte> source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return await _crc24ForRadix64.CalculateAsync(source, progress).ConfigureAwait(false);
        }

        public static IEnumerable<Byte> GetSequenceWithCrc24(this IEnumerable<Byte> source, ValueHolder<(UInt32 Crc, UInt64 Length)> crcValueHolder, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (crcValueHolder is null)
                throw new ArgumentNullException(nameof(crcValueHolder));

            return _crc24ForRadix64.GetSequenceWithCrc(source, crcValueHolder, progress);
        }

        public static IAsyncEnumerable<Byte> GetAsyncSequenceWithCrc24(this IAsyncEnumerable<Byte> source, ValueHolder<(UInt32 Crc, UInt64 Length)> crcValueHolder, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (crcValueHolder is null)
                throw new ArgumentNullException(nameof(crcValueHolder));

            return _crc24ForRadix64.GetAsyncSequenceWithCrc(source, crcValueHolder, progress);
        }

        public static IEnumerable<Char> GetBase64EncodedSequence(this IEnumerable<Byte> source, Char char62 = '+', Char char63 = '/')
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (char62.IsBetween('0', '9') || char62.IsBetween('A', 'Z') || char62.IsBetween('a', 'z') || char62 == '=' || !char62.IsBetween('\u0021', '\u007e'))
                throw new ArgumentException("Invalid character", nameof(char62));
            if (char63.IsBetween('0', '9') || char63.IsBetween('A', 'Z') || char63.IsBetween('a', 'z') || char63 == '=' || !char63.IsBetween('\u0021', '\u007e'))
                throw new ArgumentException("Invalid character", nameof(char63));
            if (char62 == char63)
                throw new ArgumentException($"Invalid character ({nameof(char62)}=={nameof(char63)})");

            return InternalGetBase64EncodedSequence(source, char62, char63);
        }

        public static IEnumerable<Byte> GetBase64DecodedSequence(this IEnumerable<Char> source, Boolean ignoreSpace = false, Boolean ignoreInvalidCharacter = false, Char char62 = '+', Char char63 = '/')
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (char62.IsBetween('0', '9') || char62.IsBetween('A', 'Z') || char62.IsBetween('a', 'z') || char62 == '=' || !char62.IsBetween('\u0021', '\u007e'))
                throw new ArgumentException("Invalid character", nameof(char62));
            if (char63.IsBetween('0', '9') || char63.IsBetween('A', 'Z') || char63.IsBetween('a', 'z') || char63 == '=' || !char63.IsBetween('\u0021', '\u007e'))
                throw new ArgumentException("Invalid character", nameof(char63));
            if (char62 == char63)
                throw new ArgumentException($"Invalid character ({nameof(char62)}=={nameof(char63)})");

            return InternalGetBase64DecodedSequence(source, ignoreSpace, ignoreInvalidCharacter, char62, char63);
        }

        public static String EncodeBase64(this IEnumerable<Byte> source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return InternalEncodeBase64(source, Base64EncodingType.Default, progress);
        }

        public static String EncodeBase64(this IEnumerable<Byte> source, Base64EncodingType encodingType, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return InternalEncodeBase64(source, encodingType, progress);
        }

        public static IEnumerable<Byte> DecodeBase64(this String source, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return InternalDecodeBase64(source, Base64EncodingType.Default, progress);
        }

        public static IEnumerable<Byte> DecodeBase64(this String source, Base64EncodingType encodingType, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return InternalDecodeBase64(source, encodingType, progress);
        }

        public static Boolean IsMatchCrc32(this IEnumerable<Byte> source, UInt32 expectedCrc, IProgress<UInt64>? progress = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.CalculateCrc32(progress).Crc == expectedCrc;
        }

        public static Boolean IsMatchCrc32(this ReadOnlyMemory<Byte> source, UInt32 expectedCrc)
            => source.CalculateCrc32() == expectedCrc;

        public static Boolean IsMatchCrc32(this ReadOnlySpan<Byte> source, UInt32 expectedCrc)
            => source.CalculateCrc32() == expectedCrc;

        #region GetBitArraySequence

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this IEnumerable<Byte> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var bitQueue = new BitQueue();
            foreach (var data in source)
            {
                bitQueue.Enqueue(data, bitPackingDirection);
                while (bitQueue.Count >= bitCount)
                    yield return bitQueue.DequeueBitArray(bitCount);
            }

            if (bitQueue.Count > 0)
                yield return bitQueue.DequeueBitArray(bitCount);
        }

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this Memory<Byte> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            var bitQueue = new BitQueue();
            for (var index = 0; index < source.Length; ++index)
            {
                bitQueue.Enqueue(source.Span[index], bitPackingDirection);
                while (bitQueue.Count >= bitCount)
                    yield return bitQueue.DequeueBitArray(bitCount);
            }

            if (bitQueue.Count > 0)
                yield return bitQueue.DequeueBitArray(bitCount);
        }

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this ReadOnlyMemory<Byte> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            var bitQueue = new BitQueue();
            for (var index = 0; index < source.Length; ++index)
            {
                bitQueue.Enqueue(source.Span[index], bitPackingDirection);
                while (bitQueue.Count >= bitCount)
                    yield return bitQueue.DequeueBitArray(bitCount);
            }

            if (bitQueue.Count > 0)
                yield return bitQueue.DequeueBitArray(bitCount);
        }

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this IEnumerable<Byte[]> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.SelectMany(bytes => bytes).GetBitArraySequence(bitCount, bitPackingDirection);
        }

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this IEnumerable<Memory<Byte>> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.SelectMany(bytes => bytes.GetSequence()).GetBitArraySequence(bitCount, bitPackingDirection);
        }

        public static IEnumerable<TinyBitArray> GetBitArraySequence(this IEnumerable<ReadOnlyMemory<Byte>> source, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.SelectMany(bytes => bytes.GetSequence()).GetBitArraySequence(bitCount, bitPackingDirection);
        }

        #endregion

        public static IEnumerable<Byte> GetByteSequence(this IEnumerable<TinyBitArray> source, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var bitQueue = new BitQueue();
            foreach (var bitArray in source)
            {
                bitQueue.Enqueue(bitArray);
                while (bitQueue.Count > 8)
                    yield return bitQueue.DequeueByte(bitPackingDirection);
            }

            if (bitQueue.Count > 0)
                yield return bitQueue.DequeueByte(bitPackingDirection);
        }

        #region ToInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int16)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int16)array.InternalToUInt16LE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16LE(this Memory<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)((ReadOnlySpan<Byte>)array.Span).InternalToUInt16LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)array.Span.InternalToUInt16LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16LE(this Span<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)((ReadOnlySpan<Byte>)array).InternalToUInt16LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)array.InternalToUInt16LE());
        }

        #endregion

        #region ToUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt16)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt16LE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16LE(this Memory<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16LE(this Span<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt16LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToUInt16LE();
        }

        #endregion

        #region ToInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int32)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int32)array.InternalToUInt32LE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32LE(this Memory<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)((ReadOnlySpan<Byte>)array.Span).InternalToUInt32LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)array.Span.InternalToUInt32LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32LE(this Span<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)((ReadOnlySpan<Byte>)array).InternalToUInt32LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)array.InternalToUInt32LE());
        }

        #endregion

        #region ToUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt32)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt32LE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32LE(this Memory<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32LE(this Span<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt32LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToUInt32LE();
        }

        #endregion

        #region ToInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int64)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int64)array.InternalToUInt64LE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64LE(this Memory<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)((ReadOnlySpan<Byte>)array.Span).InternalToUInt64LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)array.Span.InternalToUInt64LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64LE(this Span<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)((ReadOnlySpan<Byte>)array).InternalToUInt64LE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)array.InternalToUInt64LE());
        }

        #endregion

        #region ToUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64LE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt64)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt64LE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64LE(this Memory<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64LE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64LE(this Span<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt64LE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64LE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            return array.InternalToUInt64LE();
        }

        #endregion

        #region ToSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleLE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Single)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToSingleLE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleLE(this Memory<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToSingleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleLE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToSingleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleLE(this Span<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToSingleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleLE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToSingleLE();
        }

        #endregion

        #region ToDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleLE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Double)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToDoubleLE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleLE(this Memory<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToDoubleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleLE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToDoubleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleLE(this Span<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToDoubleLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleLE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToDoubleLE();
        }

        #endregion

        #region ToDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalLE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Decimal)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToDecimalLE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalLE(this Memory<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToDecimalLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalLE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToDecimalLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalLE(this Span<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToDecimalLE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalLE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToDecimalLE();
        }

        #endregion

        #region ToInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int16)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int16)array.InternalToUInt16BE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16BE(this Memory<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)((ReadOnlySpan<Byte>)array.Span).InternalToUInt16BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)array.Span.InternalToUInt16BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16BE(this Span<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)((ReadOnlySpan<Byte>)array).InternalToUInt16BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ToInt16BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int16)array.InternalToUInt16BE());
        }

        #endregion

        #region ToUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt16)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt16BE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16BE(this Memory<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16BE(this Span<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt16BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ToUInt16BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt16) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToUInt16BE();
        }

        #endregion

        #region ToInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int32)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int32)array.InternalToUInt32BE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32BE(this Memory<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)((ReadOnlySpan<Byte>)array.Span).InternalToUInt32BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)array.Span.InternalToUInt32BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32BE(this Span<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)((ReadOnlySpan<Byte>)array).InternalToUInt32BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ToInt32BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int32)array.InternalToUInt32BE());
        }

        #endregion

        #region ToUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt32)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt32BE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32BE(this Memory<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32BE(this Span<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt32BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ToUInt32BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt32) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToUInt32BE();
        }

        #endregion

        #region ToInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Int64)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return unchecked((Int64)array.InternalToUInt64BE(startIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64BE(this Memory<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)((ReadOnlySpan<Byte>)array.Span).InternalToUInt64BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)array.Span.InternalToUInt64BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64BE(this Span<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)((ReadOnlySpan<Byte>)array).InternalToUInt64BE());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ToInt64BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Int64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return unchecked((Int64)array.InternalToUInt64BE());
        }

        #endregion

        #region ToUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64BE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(UInt64)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToUInt64BE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64BE(this Memory<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToUInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64BE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToUInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64BE(this Span<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToUInt64BE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ToUInt64BE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(UInt64) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToUInt64BE();
        }

        #endregion

        #region ToSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleBE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Single)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToSingleBE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleBE(this Memory<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToSingleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleBE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToSingleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleBE(this Span<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToSingleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Single ToSingleBE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Single) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToSingleBE();
        }

        #endregion

        #region ToDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleBE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Double)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToDoubleBE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleBE(this Memory<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToDoubleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleBE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToDoubleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleBE(this Span<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToDoubleBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Double ToDoubleBE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Double) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToDoubleBE();
        }

        #endregion

        #region ToDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalBE(this Byte[] array, Int32 startIndex = 0)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));
            if (checked(startIndex + sizeof(Decimal)) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return array.InternalToDecimalBE(startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalBE(this Memory<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array.Span).InternalToDecimalBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalBE(this ReadOnlyMemory<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.Span.InternalToDecimalBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalBE(this Span<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return ((ReadOnlySpan<Byte>)array).InternalToDecimalBE();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Decimal ToDecimalBE(this ReadOnlySpan<Byte> array)
        {
            if (sizeof(Decimal) > array.Length)
                throw new ArgumentException("Too short array", nameof(array));

            return array.InternalToDecimalBE();
        }

        #endregion

        #region ToFriendlyString

        public static String ToFriendlyString(this Byte[] value, Int32 startIndex = 0)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (startIndex > value.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            return value.AsSpan(startIndex).AsReadOnly().ToFriendlyString();
        }

        public static String ToFriendlyString(this Byte[] value, Int32 startIndex, Int32 length)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (startIndex > value.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (checked(startIndex + length) > value.Length)
                throw new ArgumentException($"The specified range ({nameof(startIndex)} and {nameof(length)}) is not within the {nameof(value)}.");

            return value.AsSpan(startIndex, length).AsReadOnly().ToFriendlyString();
        }

        public static String ToFriendlyString(this Memory<Byte> value)
            => ((ReadOnlySpan<Byte>)value.Span).ToFriendlyString();

        public static String ToFriendlyString(this ReadOnlyMemory<Byte> value)
            => value.Span.ToFriendlyString();

        public static String ToFriendlyString(this Span<Byte> value)
            => ((ReadOnlySpan<Byte>)value).ToFriendlyString();

        public static String ToFriendlyString(this ReadOnlySpan<Byte> array)
        {
            var sb = new StringBuilder();
            var isFirst = true;
            for (var index = 0; index < array.Length; ++index)
            {
                if (!isFirst)
                    _ = sb.Append('-');
                _ = sb.Append(array[index].ToString("x2"));
                isFirst = false;
            }

            return sb.ToString();
        }

        #endregion

        #region GuessWhichEncoding

        public static Encoding? GuessWhichEncoding(this Byte[] bytes)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

            return ((ReadOnlySpan<Byte>)bytes).GuessWhichEncoding();
        }

        public static Encoding? GuessWhichEncoding(this Memory<Byte> bytes)
            => ((ReadOnlySpan<Byte>)bytes.Span).GuessWhichEncoding();

        public static Encoding? GuessWhichEncoding(this ReadOnlyMemory<Byte> bytes)
            => bytes.Span.GuessWhichEncoding();

        public static Encoding? GuessWhichEncoding(this Span<Byte> bytes)
            => ((ReadOnlySpan<Byte>)bytes).GuessWhichEncoding();

        public static Encoding? GuessWhichEncoding(this ReadOnlySpan<Byte> bytes)
        {
            var len = bytes.Length;

            // UTF-16かどうかのチェック
            var isBinary = false;
            for (Int32 i = 0; i < len; i++)
            {
                var b1 = bytes[i];
                if (b1 <= 0x06 || b1.IsAnyOf((Byte)0x7f, (Byte)0xff))
                {
                    isBinary = true;
                    if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7f)
                        return Encoding.Unicode;
                }
            }

            if (isBinary)
                return null;

            // ASCIIかどうかのチェック
            var notJapanese = true;
            for (Int32 i = 0; i < len; i++)
            {
                if (bytes[i] is _BYTE_ESCAPE_CHAR or >= 0x80)
                {
                    notJapanese = false;
                    break;
                }
            }

            if (notJapanese)
                return Encoding.ASCII;

            // JISコードかどうかのチェック
            for (Int32 i = 0; i < len - 2; i++)
            {
                var b1 = bytes[i];
                var b2 = bytes[i + 1];
                var b3 = bytes[i + 2];

                if (b1 == _BYTE_ESCAPE_CHAR)
                {
                    if (b2 == _BYTE_DOLLAR_CHAR && b3 == _BYTE_AT_MARK_CHAR)
                        return Encoding.GetEncoding("iso-2022-jp");//JIS_0208 1978
                    else if (b2 == _BYTE_DOLLAR_CHAR && b3 == _BYTE_B_CHAR)
                        return Encoding.GetEncoding("iso-2022-jp");//JIS_0208 1983
                    else if (b2 == _BYTE_OPEN_PARENTHESIS_CHAR && b3.IsAnyOf(_BYTE_B_CHAR, _BYTE_J_CHAR))
                        return Encoding.GetEncoding("iso-2022-jp");//JIS_ASC
                    else if (b2 == _BYTE_OPEN_PARENTHESIS_CHAR && b3 == _BYTE_I_CHAR)
                        return Encoding.GetEncoding("iso-2022-jp");//JIS_KANA
                    if (i < len - 3)
                    {
                        var b4 = bytes[i + 3];
                        if (b2 == _BYTE_DOLLAR_CHAR &&
                            b3 == _BYTE_OPEN_PARENTHESIS_CHAR &&
                            b4 == _BYTE_D_CHAR)
                        {
                            return Encoding.GetEncoding("iso-2022-jp");//JIS_0212
                        }

                        if (i < len - 5 &&
                            b2 == _BYTE_AMPERSAND_CHAR &&
                            b3 == _BYTE_AT_MARK_CHAR &&
                            b4 == _BYTE_ESCAPE_CHAR &&
                            bytes[i + 4] == _BYTE_DOLLAR_CHAR &&
                            bytes[i + 5] == _BYTE_B_CHAR)
                        {
                            return Encoding.GetEncoding("iso-2022-jp");//JIS_0208 1990
                        }
                    }
                }
            }

            // この時点で euc/shif-jis/utf-8 のいずれかしかない。
            var count_shift_jis = 0;
            var count_euc = 0;
            var count_utf8 = 0;
            for (Int32 i = 0; i < len - 1; i++)
            {
                var b1 = bytes[i];
                var b2 = bytes[i + 1];
                if ((b1.IsBetween((Byte)0x81, (Byte)0x9f) || b1.IsBetween((Byte)0xe0, (Byte)0xfc)) &&
                    (b2.IsBetween((Byte)0x40, (Byte)0x7e) || b2.IsBetween((Byte)0x80, (Byte)0xfc)))
                {
                    //SJIS_C
                    count_shift_jis += 2;
                    ++i;
                }
            }

            for (Int32 i = 0; i < len - 1; i++)
            {
                var b1 = bytes[i];
                var b2 = bytes[i + 1];
                if (b1.IsBetween((Byte)0xa1, (Byte)0xfe) && b2.IsBetween((Byte)0xa1, (Byte)0xfe) ||
                    b1 == 0x8e && b2.IsBetween((Byte)0xa1, (Byte)0xdf))
                {
                    //EUC_C
                    //EUC_KANA
                    count_euc += 2;
                    ++i;
                }
                else if (i < len - 2)
                {
                    if (b1 == 0x8f &&
                        b2.IsBetween((Byte)0xa1, (Byte)0xfe) &&
                        bytes[i + 2].IsBetween((Byte)0xa1, (Byte)0xfe))
                    {
                        //EUC_0212
                        count_euc += 3;
                        i += 2;
                    }
                }
            }

            for (Int32 i = 0; i < len - 1; i++)
            {
                var b1 = bytes[i];
                var b2 = bytes[i + 1];
                if (b1.IsBetween((Byte)0xc0, (Byte)0xdf) &&
                    b2.IsBetween((Byte)0x80, (Byte)0xbf))
                {
                    //UTF8
                    count_utf8 += 2;
                    ++i;
                }
                else if (i < len - 2)
                {
                    if (b1.IsBetween((Byte)0xe0, (Byte)0xef) &&
                        b2.IsBetween((Byte)0x80, (Byte)0xbf) &&
                        bytes[i + 2].IsBetween((Byte)0x80, (Byte)0xbf))
                    {
                        //UTF8
                        count_utf8 += 3;
                        i += 2;
                    }
                }
            }

            if (count_euc > count_shift_jis && count_euc > count_utf8)
                return Encoding.GetEncoding("euc-jp"); // euc
            else if (count_shift_jis > count_euc && count_shift_jis > count_utf8)
                return Encoding.GetEncoding("shift_jis");// shift_jis
            else if (count_utf8 > count_euc && count_utf8 > count_shift_jis)
                return Encoding.UTF8; // utf8
            else
                return null;
        }

        #endregion

        internal static ICrcCalculationState<UInt32> CreateCrc32CalculationState() => _commonCrc32.CreateSession();

        internal static ICrcCalculationState<UInt32> CreateCrc24CalculationState() => _crc24ForRadix64.CreateSession();

        private static String InternalEncodeBase64(IEnumerable<Byte> source, Base64EncodingType encodingType, IProgress<UInt64>? progress)
        {
            switch (encodingType)
            {
                case Base64EncodingType.Rfc4648Encoding: // Default
                case Base64EncodingType.Rfc2045Encoding: // for MIME
                    return
                        String.Join(
                            "\r\n",
                            source.GetBase64EncodedSequence()
                            .ChunkAsString(64));
                case Base64EncodingType.Rfc4880Encoding: // for OpenPGP Radix-64
                    var crc24ValueHolder = new ValueHolder<(UInt32 Crc, UInt64 Length)>();
                    var bodyPart =
                        String.Join(
                            "\r\n",
                            source
                            .GetSequenceWithCrc24(crc24ValueHolder, progress)
                            .GetBase64EncodedSequence()
                            .ChunkAsString(76));
                    var crc24 = crc24ValueHolder.Value.Crc;
                    var crcPart =
                        new String(
                            new[]
                            {
                                (Byte)(crc24 >> 16),
                                (Byte)(crc24 >> 8),
                                (Byte)(crc24 >> 0),
                            }
                            .GetBase64EncodedSequence()
                            .ToArray());
                    return bodyPart + "\r\n=" + crcPart;
                default:
                    throw new ArgumentException($"Unexpected {nameof(Base64EncodingType)} value", nameof(encodingType));
            }
        }

        private static IEnumerable<Byte> InternalDecodeBase64(String source, Base64EncodingType encodingType, IProgress<UInt64>? progress)
        {
            switch (encodingType)
            {
                case Base64EncodingType.Rfc4648Encoding: // Default
                    return source.GetBase64DecodedSequence(false, false);
                case Base64EncodingType.Rfc2045Encoding: // for MIME
                    return source.GetBase64DecodedSequence(true, true);
                case Base64EncodingType.Rfc4880Encoding: // for OpenPGP Radix-64
                    var indexOfLastEqualSign = source.LastIndexOf('=');
                    var bodyPart = indexOfLastEqualSign >= 0 ? source[..indexOfLastEqualSign] : source;
                    var crcPart = indexOfLastEqualSign >= 0 ? source[(indexOfLastEqualSign + 1)..] : null;
                    var data = bodyPart.GetBase64DecodedSequence(true, false).ToArray();
                    if (crcPart is not null)
                    {
                        var crcByteArray = crcPart.GetBase64DecodedSequence(true, false).ToArray();
                        if (crcByteArray.Length != 3)
                            throw new FormatException();
                        var desiredCrc = ((UInt32)crcByteArray[0] << 16) | ((UInt32)crcByteArray[1] << 8) | ((UInt32)crcByteArray[2] << 0);
                        var actualCrc = data.CalculateCrc24(progress).Crc;
                        if (actualCrc != desiredCrc)
                            throw new FormatException();
                    }

                    return data;
                default:
                    throw new ArgumentException($"Unexpected {nameof(Base64EncodingType)} value", nameof(encodingType));
            }
        }

        private static IEnumerable<Char> InternalGetBase64EncodedSequence(IEnumerable<Byte> source, Char char62, Char char63)
        {
            var bytes = new Byte[3];
            var index = 0;
            foreach (var data in source)
            {
                bytes[index++] = data;
                if (index >= bytes.Length)
                {
                    yield return ToBase64Character(bytes[0] >> 2, char62, char63);
                    yield return ToBase64Character(((bytes[0] << 4) | (bytes[1] >> 4)) & 0x3f, char62, char63);
                    yield return ToBase64Character(((bytes[1] << 2) | (bytes[2] >> 6)) & 0x3f, char62, char63);
                    yield return ToBase64Character(bytes[2] & 0x3f, char62, char63);
                    index = 0;
                }
            }

            switch (index)
            {
                case 0:
                    break;
                case 1:
                    yield return ToBase64Character(bytes[0] >> 2, char62, char63);
                    yield return ToBase64Character((bytes[0] << 4) & 0x3f, char62, char63);
                    yield return '=';
                    yield return '=';
                    break;
                case 2:
                    yield return ToBase64Character(bytes[0] >> 2, char62, char63);
                    yield return ToBase64Character(((bytes[0] << 4) | (bytes[1] >> 4)) & 0x3f, char62, char63);
                    yield return ToBase64Character((bytes[1] << 2) & 0x3f, char62, char63);
                    yield return '=';
                    break;
                default:
                    throw new InternalLogicalErrorException();
            }
        }

        private static IEnumerable<Byte> InternalGetBase64DecodedSequence(IEnumerable<Char> source, Boolean ignoreSpace, Boolean ignoreInvalidCharacter, Char char62, Char char63)
        {
            var charSource =
                source
                .Where(c => !c.IsAnyOf('\r', '\n') && (!Char.IsWhiteSpace(c) || (ignoreSpace ? false : throw new FormatException())))
                .TakeWhile(c => c != '=')
                .Select(c => FromBase64Character(c, char62, char63))
                .Where(n => n >= 0 || (ignoreInvalidCharacter ? false : throw new FormatException()));
            var buffer = new Int32[4];
            var index = 0;
            foreach (var data in charSource)
            {
                buffer[index++] = data;
                if (index >= buffer.Length)
                {
                    yield return (Byte)((buffer[0] << 2) | (buffer[1] >> 4));
                    yield return (Byte)((buffer[1] << 4) | (buffer[2] >> 2));
                    yield return (Byte)((buffer[2] << 6) | (buffer[3] >> 0));
                }
            }

            switch (index)
            {
                case 1:
                    break;
                case 2:
                    yield return (Byte)((buffer[0] << 2) | (buffer[1] >> 4));
                    break;
                case 3:
                    yield return (Byte)((buffer[0] << 2) | (buffer[1] >> 4));
                    yield return (Byte)((buffer[1] << 4) | (buffer[2] >> 2));
                    break;
                default:
                    throw new FormatException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Char ToBase64Character(Int32 n, Char char62, Char char63)
        {
            if (n < 0)
                throw new InternalLogicalErrorException();
            if (n < 26)
                return (Char)('A' + n);
            if (n < 52)
                return (Char)('a' + n - 26);
            if (n < 62)
                return (Char)('0' + n - 52);
            if (n == 62)
                return char62;
            if (n == 63)
                return char63;
            throw new InternalLogicalErrorException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 FromBase64Character(Char c, Char char62, Char char63)
            => c.IsBetween('A', 'Z')
                ? c - 'A'
                : c.IsBetween('a', 'z')
                ? c - 'a' + 26
                : c.IsBetween('0', '9')
                ? c - '0' + 52
                : c == char62
                ? 62
                : c == char63 ? 63 : -1;

        #region InternalToUInt16LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt16 InternalToUInt16LE(this Byte[] array, Int32 startIndex)
            => (UInt16)
                (
                    (array[startIndex + 0] << 0)
                    | (array[startIndex + 1] << 8)
                );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt16 InternalToUInt16LE(this ReadOnlySpan<Byte> array)
            => (UInt16)
                (
                    (array[0] << 0)
                    | (array[1] << 8)
                );

        #endregion

        #region InternalToUInt32LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 InternalToUInt32LE(this Byte[] array, Int32 startIndex)
            => ((UInt32)array[startIndex + 0] << 00)
                | ((UInt32)array[startIndex + 1] << 08)
                | ((UInt32)array[startIndex + 2] << 16)
                | ((UInt32)array[startIndex + 3] << 24);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 InternalToUInt32LE(this ReadOnlySpan<Byte> array)
            => ((UInt32)array[0] << 00)
                | ((UInt32)array[1] << 08)
                | ((UInt32)array[2] << 16)
                | ((UInt32)array[3] << 24);

        #endregion

        #region InternalToUInt64LE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 InternalToUInt64LE(this Byte[] array, Int32 startIndex)
            => ((UInt64)array[startIndex + 0] << 0)
                | ((UInt64)array[startIndex + 1] << 8)
                | ((UInt64)array[startIndex + 2] << 16)
                | ((UInt64)array[startIndex + 3] << 24)
                | ((UInt64)array[startIndex + 4] << 32)
                | ((UInt64)array[startIndex + 5] << 40)
                | ((UInt64)array[startIndex + 6] << 48)
                | ((UInt64)array[startIndex + 7] << 56);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 InternalToUInt64LE(this ReadOnlySpan<Byte> array)
            => ((UInt64)array[0] << 0)
                | ((UInt64)array[1] << 8)
                | ((UInt64)array[2] << 16)
                | ((UInt64)array[3] << 24)
                | ((UInt64)array[4] << 32)
                | ((UInt64)array[5] << 40)
                | ((UInt64)array[6] << 48)
                | ((UInt64)array[7] << 56);

        #endregion

        #region InternalToSingleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Single InternalToSingleLE(this Byte[] array, Int32 startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(array, startIndex);
            }
            else
            {
                Span<Byte> value = stackalloc[]
                {
                    array[startIndex + 3],
                    array[startIndex + 2],
                    array[startIndex + 1],
                    array[startIndex + 0],
                };
                return BitConverter.ToSingle(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Single InternalToSingleLE(this ReadOnlySpan<Byte> array)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(array);
            }
            else
            {
                Span<Byte> value = stackalloc[]
                {
                    array[3],
                    array[2],
                    array[1],
                    array[0],
                };
                return BitConverter.ToSingle(value);
            }
        }

        #endregion

        #region InternalToDoubleLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Double InternalToDoubleLE(this Byte[] array, Int32 startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToDouble(array, startIndex);
            }
            else
            {
                ReadOnlySpan<Byte> value = stackalloc[]
                {
                    array[startIndex + 7],
                    array[startIndex + 6],
                    array[startIndex + 5],
                    array[startIndex + 4],
                    array[startIndex + 3],
                    array[startIndex + 2],
                    array[startIndex + 1],
                    array[startIndex + 0],
                };
                return BitConverter.ToDouble(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Double InternalToDoubleLE(this ReadOnlySpan<Byte> array)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToDouble(array);
            }
            else
            {
                ReadOnlySpan<Byte> value = stackalloc[]
                {
                    array[7],
                    array[6],
                    array[5],
                    array[4],
                    array[3],
                    array[2],
                    array[1],
                    array[0],
                };
                return BitConverter.ToDouble(value);
            }
        }

        #endregion

        #region InternalToDecimalLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal InternalToDecimalLE(this Byte[] array, Int32 startIndex)
        {
            ReadOnlySpan<Int32> buffer = stackalloc Int32[]
            {
                array.ToInt32LE(startIndex + sizeof(Int32) * 0),
                array.ToInt32LE(startIndex + sizeof(Int32) * 1),
                array.ToInt32LE(startIndex + sizeof(Int32) * 2),
                array.ToInt32LE(startIndex + sizeof(Int32) * 3),
            };
            return new Decimal(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal InternalToDecimalLE(this ReadOnlySpan<Byte> array)
        {
            Span<Int32> buffer = stackalloc Int32[]
            {
                array[(sizeof(Int32) * 0)..(sizeof(Int32) * 1)].ToInt32LE(),
                array[(sizeof(Int32) * 1)..(sizeof(Int32) * 2)].ToInt32LE(),
                array[(sizeof(Int32) * 2)..(sizeof(Int32) * 3)].ToInt32LE(),
                array[(sizeof(Int32) * 3)..(sizeof(Int32) * 4)].ToInt32LE(),
            };
            return new Decimal(buffer);
        }

        #endregion

        #region InternalToUInt16BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt16 InternalToUInt16BE(this Byte[] array, Int32 startIndex)
            => (UInt16)
                (
                    (array[startIndex + 0] << 8)
                    | (array[startIndex + 1] << 0)
                );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt16 InternalToUInt16BE(this ReadOnlySpan<Byte> array)
            => (UInt16)
                (
                    (array[0] << 8)
                    | (array[1] << 0)
                );

        #endregion

        #region InternalToUInt32BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 InternalToUInt32BE(this Byte[] array, Int32 startIndex)
            => ((UInt32)array[startIndex + 0] << 24)
                | ((UInt32)array[startIndex + 1] << 16)
                | ((UInt32)array[startIndex + 2] << 08)
                | ((UInt32)array[startIndex + 3] << 00);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 InternalToUInt32BE(this ReadOnlySpan<Byte> array)
            => ((UInt32)array[0] << 24)
                | ((UInt32)array[1] << 16)
                | ((UInt32)array[2] << 08)
                | ((UInt32)array[3] << 00);

        #endregion

        #region InternalToUInt64BE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 InternalToUInt64BE(this Byte[] array, Int32 startIndex)
            => ((UInt64)array[startIndex + 0] << 56)
                | ((UInt64)array[startIndex + 1] << 48)
                | ((UInt64)array[startIndex + 2] << 40)
                | ((UInt64)array[startIndex + 3] << 32)
                | ((UInt64)array[startIndex + 4] << 24)
                | ((UInt64)array[startIndex + 5] << 16)
                | ((UInt64)array[startIndex + 6] << 08)
                | ((UInt64)array[startIndex + 7] << 00);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 InternalToUInt64BE(this ReadOnlySpan<Byte> array)
            => ((UInt64)array[0] << 56)
                | ((UInt64)array[1] << 48)
                | ((UInt64)array[2] << 40)
                | ((UInt64)array[3] << 32)
                | ((UInt64)array[4] << 24)
                | ((UInt64)array[5] << 16)
                | ((UInt64)array[6] << 08)
                | ((UInt64)array[7] << 00);

        #endregion

        #region InternalToSingleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Single InternalToSingleBE(this Byte[] array, Int32 startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                ReadOnlySpan<Byte> value =
                    stackalloc[]
                    {
                    array[startIndex + 3],
                    array[startIndex + 2],
                    array[startIndex + 1],
                    array[startIndex + 0],
                };
                return BitConverter.ToSingle(value);
            }
            else
            {
                return BitConverter.ToSingle(array, startIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Single InternalToSingleBE(this ReadOnlySpan<Byte> array)
        {
            if (BitConverter.IsLittleEndian)
            {
                ReadOnlySpan<Byte> value =
                    stackalloc[]
                    {
                    array[3],
                    array[2],
                    array[1],
                    array[0],
                };
                return BitConverter.ToSingle(value);
            }
            else
            {
                return BitConverter.ToSingle(array);
            }
        }

        #endregion

        #region InternalToDoubleBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Double InternalToDoubleBE(this Byte[] array, Int32 startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                ReadOnlySpan<Byte> value =
                    new[]
                    {
                        array[startIndex + 7],
                        array[startIndex + 6],
                        array[startIndex + 5],
                        array[startIndex + 4],
                        array[startIndex + 3],
                        array[startIndex + 2],
                        array[startIndex + 1],
                        array[startIndex + 0],
                    };
                return BitConverter.ToDouble(value);
            }
            else
            {
                return BitConverter.ToDouble(array, startIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Double InternalToDoubleBE(this ReadOnlySpan<Byte> array)
        {
            if (BitConverter.IsLittleEndian)
            {
                ReadOnlySpan<Byte> value =
                    new[]
                    {
                        array[7],
                        array[6],
                        array[5],
                        array[4],
                        array[3],
                        array[2],
                        array[1],
                        array[0],
                    };
                return BitConverter.ToDouble(value);
            }
            else
            {
                return BitConverter.ToDouble(array);
            }
        }

        #endregion

        #region InternalToDecimalBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal InternalToDecimalBE(this Byte[] array, Int32 startIndex)
        {
            ReadOnlySpan<Int32> buffer = stackalloc Int32[]
            {
                array.ToInt32BE(startIndex + sizeof(Int32) * 3),
                array.ToInt32BE(startIndex + sizeof(Int32) * 2),
                array.ToInt32BE(startIndex + sizeof(Int32) * 1),
                array.ToInt32BE(startIndex + sizeof(Int32) * 0),
            };
            return new Decimal(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Decimal InternalToDecimalBE(this ReadOnlySpan<Byte> array)
        {
            Span<Int32> buffer = stackalloc Int32[]
            {
                array[(sizeof(Int32) * 3)..(sizeof(Int32) * 4)].ToInt32BE(),
                array[(sizeof(Int32) * 2)..(sizeof(Int32) * 3)].ToInt32BE(),
                array[(sizeof(Int32) * 1)..(sizeof(Int32) * 2)].ToInt32BE(),
                array[(sizeof(Int32) * 0)..(sizeof(Int32) * 1)].ToInt32BE(),
            };
            return new Decimal(buffer);
        }

        #endregion
    }
}
