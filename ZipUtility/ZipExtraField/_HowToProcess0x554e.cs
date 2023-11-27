using System;
using System.Text;

namespace ZipUtility.ZipExtraField
{
    // This is a sample program that shows how to handle extra field 0x554e.
    // Note that the layout of extra fields is different between local headers and central directory headers.

    internal class HowToProcess0x554e
    {
        public const UInt16 _extraFieldId = 0x554e;
        private const UInt32 _signature = 0x5843554e;

        private static readonly Encoding _utf16Encoding;

        private String? _fileName;
        private String? _fileComment;

        static HowToProcess0x554e()
        {
            // - Always little endian.
            // - Always encoded/decoded in UTF-16, so no byte order mark is required.
            _utf16Encoding = new UnicodeEncoding(false, false);
        }

        public HowToProcess0x554e()
        {
            _fileName = null;
            _fileComment = null;
        }

        public String? FileName
        {
            get => _fileName;
            set
            {
                if (value is not null && value.Length > UInt16.MaxValue)
                    throw new ArgumentException($"Too long {nameof(value)}.");
                _fileName = value;
            }
        }

        public String? FileComment
        {
            get => _fileComment;
            set
            {
                if (value is not null && value.Length > UInt16.MaxValue)
                    throw new ArgumentException($"Too long {nameof(value)}.");
                _fileComment = value;
            }
        }

        public Boolean Read(Byte[] extraFieldData, Int32 offset, Int32 size, Boolean forCentralDirectoryHeader)
        {
            if (extraFieldData is null)
                throw new ArgumentNullException(nameof(extraFieldData));
            if (offset < 0 || offset > extraFieldData.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            checked
            {
                if (size < 0 || checked(offset + size > extraFieldData.Length))
                    throw new ArgumentOutOfRangeException(nameof(size));

                if (forCentralDirectoryHeader)
                {
                    // for central directory header

                    var fixedSizePartLength = (UInt16)8;
                    if (size < fixedSizePartLength)
                    {
                        // Extra field length is less than the minimum header length
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    var signature =
                            ((UInt32)extraFieldData[offset + 0] << (8 * 0))
                            | ((UInt32)extraFieldData[offset + 1] << (8 * 1))
                            | ((UInt32)extraFieldData[offset + 2] << (8 * 2))
                            | ((UInt32)extraFieldData[offset + 3] << (8 * 3));
                    offset += 4;
                    if (signature != _signature)
                    {
                        // Signatures do not match.
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    var fileNameCharacterLength =
                        (UInt16)(
                            (extraFieldData[offset + 0] << (8 * 0))
                            | (extraFieldData[offset + 1] << (8 * 1)));
                    offset += 2;
                    var fileCommentCharacterLength =
                        (UInt16)(
                            (extraFieldData[offset + 0] << (8 * 0))
                            | (extraFieldData[offset + 1] << (8 * 1)));
                    offset += 2;

                    if (fileNameCharacterLength * 2 + fileCommentCharacterLength * 2 > (size - fixedSizePartLength))
                    {
                        // The remaining length of the extra field is shorter than the calculated string length
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    _fileComment = "";
                    if (fileNameCharacterLength > 0)
                    {
                        try
                        {
                            _fileName = _utf16Encoding.GetString(extraFieldData, offset, fileNameCharacterLength * 2);
                        }
                        catch (Exception)
                        {
                            // The byte array cannot be decoded by UTF-16.
                            // => Extra field is broken

                            _fileName = null;
                            _fileComment = null;
                            return false;
                        }

                        offset += fileNameCharacterLength * 2;
                    }

                    _fileComment = "";
                    if (fileCommentCharacterLength > 0)
                    {
                        try
                        {
                            _fileComment = _utf16Encoding.GetString(extraFieldData, offset, fileCommentCharacterLength * 2);
                        }
                        catch (Exception)
                        {
                            // The byte array cannot be decoded by UTF-16.
                            // => Extra field is broken

                            _fileName = null;
                            _fileComment = null;
                            return false;
                        }
                    }
                }
                else
                {
                    // for local header

                    var fixedSizePartLength = (UInt16)6;
                    if (size < fixedSizePartLength)
                    {
                        // Extra field length is less than the minimum header length
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    var signature =
                            ((UInt32)extraFieldData[offset + 0] << (8 * 0))
                            | ((UInt32)extraFieldData[offset + 1] << (8 * 1))
                            | ((UInt32)extraFieldData[offset + 2] << (8 * 2))
                            | ((UInt32)extraFieldData[offset + 3] << (8 * 3));
                    offset += 4;
                    if (signature != _signature)
                    {
                        // Signatures do not match.
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    var fileNameCharacterLength =
                        (UInt16)(
                            (extraFieldData[offset + 0] << (8 * 0))
                            | (extraFieldData[offset + 1] << (8 * 1)));
                    offset += 2;

                    if ((fileNameCharacterLength * 2) > (size - fixedSizePartLength))
                    {
                        // The remaining length of the extra field is shorter than the calculated string length
                        // => Extra field is broken

                        _fileName = null;
                        _fileComment = null;
                        return false;
                    }

                    _fileName = "";
                    if (fileNameCharacterLength > 0)
                    {
                        try
                        {
                            _fileName = _utf16Encoding.GetString(extraFieldData, offset, fileNameCharacterLength * 2);
                        }
                        catch (Exception)
                        {
                            // The byte array cannot be decoded by UTF-16.
                            // => Extra field is broken

                            _fileName = null;
                            _fileComment = null;
                            return false;
                        }
                    }

                    _fileComment = null;
                }
            }

            // OK
            return true;
        }

        public Int32 Write(Span<Byte> buffer, Boolean forCentralDirectoryHeader)
        {
            checked
            {
                if (forCentralDirectoryHeader)
                {
                    if (String.IsNullOrEmpty(_fileName) && String.IsNullOrEmpty(_fileComment))
                    {
                        // _fileName and _fileComment are both empty or not set
                        // => This extra field does not need to be written
                        return -1;
                    }

                    var totalLength = 8;

                    // Since the internal representation of String is UTF-16, _utf16Envoding.GetBytes() is always successful.
                    // Therefore, there is no need to check for exceptions.
                    var fileNameBytes = _utf16Encoding.GetBytes(_fileName ?? "");
                    System.Diagnostics.Debug.Assert(fileNameBytes.Length % 2 == 0, $"The length of {nameof(fileNameBytes)} should be an even number.");
                    totalLength += fileNameBytes.Length;

                    // Since the internal representation of String is UTF-16, _utf16Envoding.GetBytes() is always successful.
                    // Therefore, there is no need to check for exceptions.
                    var fileCommentBytes = Encoding.Unicode.GetBytes(_fileComment ?? "");
                    System.Diagnostics.Debug.Assert(fileCommentBytes.Length % 2 == 0, $"The length of {nameof(fileCommentBytes)} should be an even number.");
                    totalLength += fileCommentBytes.Length;

                    if (totalLength > UInt16.MaxValue)
                    {
                        // Extra header length is too long.
                        // => This extra field does not need to be written
                        return -1;
                    }

                    if (totalLength > buffer.Length)
                    {
                        // Extra header buffer length is insufficient.
                        // => This extra field does not need to be written
                        return -1;
                    }

                    // Stores signature.
                    buffer[0] = unchecked((Byte)(_signature >> (8 * 0)));
                    buffer[1] = unchecked((Byte)(_signature >> (8 * 1)));
                    buffer[2] = unchecked((Byte)(_signature >> (8 * 2)));
                    buffer[3] = unchecked((Byte)(_signature >> (8 * 3)));
                    buffer = buffer[4..];

                    // Stores a value that is half the length of fileNameBytes.
                    buffer[0] = unchecked((Byte)(fileNameBytes.Length >> 1 >> (8 * 0)));
                    buffer[1] = unchecked((Byte)(fileNameBytes.Length >> 1 >> (8 * 1)));
                    buffer = buffer[2..];

                    // Stores a value that is half the length of fileCommentBytes.
                    buffer[0] = unchecked((Byte)(fileCommentBytes.Length >> 1 >> (8 * 0)));
                    buffer[1] = unchecked((Byte)(fileCommentBytes.Length >> 1 >> (8 * 1)));
                    buffer = buffer[2..];

                    // Stores fileNameBytes.
                    fileNameBytes.CopyTo(buffer);
                    buffer = buffer[fileNameBytes.Length..];

                    // Stores fileCommentBytes.
                    fileCommentBytes.CopyTo(buffer);

                    // OK
                    return totalLength;
                }
                else
                {
                    if (String.IsNullOrEmpty(_fileName))
                    {
                        // _fileName is empty or not set.
                        // => This extra field does not need to be written
                        return -1;
                    }

                    var totalLength = 6;

                    // Since the internal representation of String is UTF-16, _utf16Envoding.GetBytes() is always successful.
                    // Therefore, there is no need to check for exceptions.
                    var fileNameBytes = _utf16Encoding.GetBytes(_fileName ?? "");
                    System.Diagnostics.Debug.Assert(fileNameBytes.Length % 2 == 0, $"The length of {nameof(fileNameBytes)} should be an even number.");
                    totalLength += fileNameBytes.Length;

                    if (totalLength > UInt16.MaxValue)
                    {
                        // Extra header length is too long.
                        // => This extra field does not need to be written
                        return -1;
                    }

                    if (totalLength > buffer.Length)
                    {
                        // Extra header buffer length is insufficient.
                        // => This extra field does not need to be written
                        return -1;
                    }

                    // Stores signature.
                    buffer[0] = unchecked((Byte)(_signature >> (8 * 0)));
                    buffer[1] = unchecked((Byte)(_signature >> (8 * 1)));
                    buffer[2] = unchecked((Byte)(_signature >> (8 * 2)));
                    buffer[3] = unchecked((Byte)(_signature >> (8 * 3)));
                    buffer = buffer[4..];

                    // Stores a value that is half the length of fileNameBytes.
                    buffer[0] = unchecked((Byte)(fileNameBytes.Length >> 1 >> (8 * 0)));
                    buffer[1] = unchecked((Byte)(fileNameBytes.Length >> 1 >> (8 * 1)));
                    buffer = buffer[2..];

                    // Stores fileNameBytes.
                    fileNameBytes.CopyTo(buffer);

                    // OK
                    return totalLength;
                }
            }
        }
    }
}
