using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Utility.Text
{
    public static class EncodingExtensions
    {
        private class EncodingWithoutPreamble
            : Encoding
        {
            private readonly Encoding _sourceEncoding;

            public EncodingWithoutPreamble(Encoding sourceEncoding)
                : base(sourceEncoding.CodePage)
            {
                _sourceEncoding = sourceEncoding;
            }

            public override Object Clone() => new EncodingWithoutPreamble(_sourceEncoding);
            public override Byte[] GetPreamble() => Array.Empty<Byte>();
            public override ReadOnlySpan<Byte> Preamble => ReadOnlySpan<Byte>.Empty;

            public override String BodyName => _sourceEncoding.BodyName;
            public override Int32 CodePage => _sourceEncoding.CodePage;
            public override String EncodingName => _sourceEncoding.EncodingName;
            public override Int32 GetByteCount(Char[] chars) => _sourceEncoding.GetByteCount(chars);
            public override Int32 GetByteCount(ReadOnlySpan<Char> chars) => _sourceEncoding.GetByteCount(chars);
            public override Int32 GetByteCount(String s) => _sourceEncoding.GetByteCount(s);
            public override unsafe Int32 GetByteCount(Char* chars, Int32 count) => _sourceEncoding.GetByteCount(chars, count);
            public override Int32 GetByteCount(Char[] chars, Int32 index, Int32 count) => _sourceEncoding.GetByteCount(chars, index, count);
            public override Byte[] GetBytes(Char[] chars) => _sourceEncoding.GetBytes(chars);
            public override Byte[] GetBytes(String s) => _sourceEncoding.GetBytes(s);
            public override Int32 GetBytes(ReadOnlySpan<Char> chars, Span<Byte> bytes) => _sourceEncoding.GetBytes(chars, bytes);
            public override Byte[] GetBytes(Char[] chars, Int32 index, Int32 count) => _sourceEncoding.GetBytes(chars, index, count);
            public override unsafe Int32 GetBytes(Char* chars, Int32 charCount, Byte* bytes, Int32 byteCount) => _sourceEncoding.GetBytes(chars, charCount, bytes, byteCount);
            public override Int32 GetBytes(Char[] chars, Int32 charIndex, Int32 charCount, Byte[] bytes, Int32 byteIndex) => _sourceEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            public override Int32 GetBytes(String s, Int32 charIndex, Int32 charCount, Byte[] bytes, Int32 byteIndex) => _sourceEncoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
            public override Int32 GetCharCount(Byte[] bytes) => _sourceEncoding.GetCharCount(bytes);
            public override Int32 GetCharCount(ReadOnlySpan<Byte> bytes) => _sourceEncoding.GetCharCount(bytes);
            public override unsafe Int32 GetCharCount(Byte* bytes, Int32 count) => _sourceEncoding.GetCharCount(bytes, count);
            public override Int32 GetCharCount(Byte[] bytes, Int32 index, Int32 count) => _sourceEncoding.GetCharCount(bytes, index, count);
            public override Char[] GetChars(Byte[] bytes) => _sourceEncoding.GetChars(bytes);
            public override Int32 GetChars(ReadOnlySpan<Byte> bytes, Span<Char> chars) => _sourceEncoding.GetChars(bytes, chars);
            public override Char[] GetChars(Byte[] bytes, Int32 index, Int32 count) => _sourceEncoding.GetChars(bytes, index, count);
            public override unsafe Int32 GetChars(Byte* bytes, Int32 byteCount, Char* chars, Int32 charCount) => _sourceEncoding.GetChars(bytes, byteCount, chars, charCount);
            public override Int32 GetChars(Byte[] bytes, Int32 byteIndex, Int32 byteCount, Char[] chars, Int32 charIndex) => _sourceEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            public override Decoder GetDecoder() => _sourceEncoding.GetDecoder();
            public override Encoder GetEncoder() => _sourceEncoding.GetEncoder();
            public override Int32 GetMaxByteCount(Int32 charCount) => _sourceEncoding.GetMaxByteCount(charCount);
            public override Int32 GetMaxCharCount(Int32 byteCount) => _sourceEncoding.GetMaxCharCount(byteCount);
            public override String GetString(Byte[] bytes) => _sourceEncoding.GetString(bytes);
            public override String GetString(Byte[] bytes, Int32 index, Int32 count) => _sourceEncoding.GetString(bytes, index, count);
            public override String HeaderName => _sourceEncoding.HeaderName;
            public override Boolean IsAlwaysNormalized(NormalizationForm form) => _sourceEncoding.IsAlwaysNormalized(form);
            public override Boolean IsBrowserDisplay => _sourceEncoding.IsBrowserDisplay;
            public override Boolean IsBrowserSave => _sourceEncoding.IsBrowserSave;
            public override Boolean IsMailNewsDisplay => _sourceEncoding.IsMailNewsDisplay;
            public override Boolean IsMailNewsSave => _sourceEncoding.IsMailNewsSave;
            public override Boolean IsSingleByte => _sourceEncoding.IsSingleByte;
            public override String WebName => _sourceEncoding.WebName;
            public override Int32 WindowsCodePage => _sourceEncoding.WindowsCodePage;
            public override Boolean Equals([NotNullWhen(true)] Object? value) => _sourceEncoding.Equals(value);
            public override Int32 GetHashCode() => _sourceEncoding.GetHashCode();
            public override String? ToString() => _sourceEncoding.ToString();
        }

        static EncodingExtensions()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static Encoding WithoutPreamble(this Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            return
                encoding.Preamble.Length > 0
                ? new EncodingWithoutPreamble(encoding)
                : encoding;
        }

        public static Encoding WithFallback(this Encoding encoding, String? encoderReplacement, String? decoderReplacement)
        {
            var newEncoding =
                Encoding.GetEncoding(
                    encoding.CodePage,
                    encoderReplacement is null ? new EncoderExceptionFallback() : new EncoderReplacementFallback(encoderReplacement),
                    decoderReplacement is null ? new DecoderExceptionFallback() : new DecoderReplacementFallback(decoderReplacement));
            return
                encoding.Preamble.Length <= 0 && newEncoding.Preamble.Length > 0
                ? new EncodingWithoutPreamble(newEncoding)
                : newEncoding;
        }
    }
}
