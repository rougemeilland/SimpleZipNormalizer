using System;
using System.IO;
using System.IO.Compression;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// Windows NT Security Descriptor Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class WindowsSecurityDescriptorExtraField
        : ExtraField
    {
        private const Byte _supportedVersion = 0;
        private ZipEntryCompressionMethodId _compressionType;
        private UInt32? _securityDescriptorBinaryFormLength;
        private ReadOnlyMemory<Byte>? _securityDescriptorBinaryForm;

        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public WindowsSecurityDescriptorExtraField()
            : base(ExtraFieldId)
        {
            _compressionType = ZipEntryCompressionMethodId.Unknown;
            _securityDescriptorBinaryFormLength = null;
            _securityDescriptorBinaryForm = null;
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x4453;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType)
        {
            // ローカルヘッダとセントラルディレクトリヘッダの場合で内容が異なるため、場合分けして処理する。
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalFileHeader:
                {
                    // ローカルヘッダとして必須なプロパティのチェックをする
                    if (_securityDescriptorBinaryForm is null || _compressionType == ZipEntryCompressionMethodId.Unknown)
                    {
                        // 設定が必須であるプロパティが設定されていないため、「この拡張フィールドは設定不要」とする。
                        return null;
                    }

                    // ここから先は拡張ヘッダのバイト列を構築している。

                    var crc = _securityDescriptorBinaryForm.Value.CalculateCrc32();
                    var compressionResult = Compress(_compressionType, _securityDescriptorBinaryForm.Value);
                    if (compressionResult is null)
                    {
                        // このルートには到達しない
                        return null;
                    }

                    var (compressedSD, compressionMethodId) = compressionResult.Value;
                    _compressionType = compressionMethodId;

                    var bufferLength = checked(sizeof(UInt32) + sizeof(Byte) + sizeof(UInt16) + sizeof(UInt32) + compressedSD.Length);
                    if (bufferLength > UInt16.MaxValue)
                        return null;

                    var builder = new ByteArrayBuilder(bufferLength);
                    builder.AppendUInt32LE((UInt32)_securityDescriptorBinaryForm.Value.Length);
                    builder.AppendByte(_supportedVersion);
                    builder.AppendUInt16LE((UInt16)compressionMethodId);
                    builder.AppendUInt32LE(crc);
                    builder.AppendBytes(compressedSD.Span);
                    return builder.ToByteArray();
                }
                case ZipEntryHeaderType.CentralDirectoryHeader:
                {
                    // セントラルディレクトリヘッダとして必須なプロパティのチェックをする
                    if (_securityDescriptorBinaryForm is null)
                    {
                        // 設定が必須であるプロパティが設定されていないため、「この拡張フィールドは設定不要」とする。
                        return null;
                    }

                    // ここから先は拡張ヘッダのバイト列を構築している。

                    var builder = new ByteArrayBuilder(sizeof(UInt32));
                    builder.AppendUInt32LE((UInt32)_securityDescriptorBinaryForm.Value.Length);
                    return builder.ToByteArray();
                }
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
        {
            _securityDescriptorBinaryForm = null;
            var reader = new ByteArrayReader(data);
            var success = false;
            try
            {
                switch (headerType)
                {
                    case ZipEntryHeaderType.LocalFileHeader:
                    {
                        // ローカルヘッダとして、与えられたバイト列を解析する。
                        var uncompressedSDSize = reader.ReadUInt32LE();
                        var version = reader.ReadByte();
                        if (version != _supportedVersion)
                        {
                            // 現時点ではバージョン番号は 0 しか規定されていないため、バージョン番号として 0 以外が与えられた場合はすべての内容を無視する。
                            return;
                        }

                        var compressionMethodId = (ZipEntryCompressionMethodId)reader.ReadUInt16LE();
                        var crc = reader.ReadUInt32LE();
                        var compressedSD = reader.ReadAllBytes();
                        var uncompressedSD =
                            Decompress(compressionMethodId, compressedSD)
                            ?? throw new NotSupportedSpecificationException($"The NTFS security descriptor is compressed using an unsupported compression method.: {compressionMethodId}");

                        if (uncompressedSD.Length != uncompressedSDSize)
                        {
                            // 伸長したデータの長さが一致しない
                            // => 拡張ヘッダが壊れている
                            throw GetBadFormatException(headerType, data);
                        }

                        var actualCrc = uncompressedSD.CalculateCrc32();
                        if (actualCrc != crc)
                        {
                            // CRCが一致しない
                            // => 拡張ヘッダが壊れている
                            throw GetBadFormatException(headerType, data);
                        }

                        _compressionType = compressionMethodId;
                        _securityDescriptorBinaryFormLength = uncompressedSDSize;
                        _securityDescriptorBinaryForm = uncompressedSD;
                        break;
                    }
                    case ZipEntryHeaderType.CentralDirectoryHeader:
                    {
                        // セントラルディレクトリヘッダとして、与えられたバイト列を解析する。
                        _securityDescriptorBinaryFormLength = reader.ReadUInt32LE();
                        _compressionType = ZipEntryCompressionMethodId.Unknown;
                        _securityDescriptorBinaryForm = null;
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
                // 復帰時に success フラグが立っていない場合は、すべての設定値を初期値に戻す。
                if (!success)
                {
                    _securityDescriptorBinaryFormLength = null;
                    _compressionType = ZipEntryCompressionMethodId.Unknown;
                    _securityDescriptorBinaryForm = null;
                }
            }
        }

        /// <summary>
        /// セキュリティディスクリプタの圧縮前の長さを取得または設定します。
        /// </summary>
        public UInt32 SecurityDescriptorBinaryFormLength
            => _securityDescriptorBinaryFormLength ?? throw new InvalidOperationException();

        /// <summary>
        /// セキュリティディスクリプタの圧縮方式を取得または設定します。
        /// </summary>
        public ZipEntryCompressionMethodId CompressionType
        {
            get => _compressionType;
            set
            {
                if (value == ZipEntryCompressionMethodId.Unknown)
                    throw new ArgumentException($"{value} compression method cannot be set.", nameof(value));
                if (!value.IsAnyOf(ZipEntryCompressionMethodId.Deflate, ZipEntryCompressionMethodId.Stored))
                    throw new ArgumentException($"{value} compression method is not supported.", nameof(value));

                _compressionType = value;
            }
        }

        /// <summary>
        /// バイナリ形式の NTFS セキュリティディスクリプタ です。
        /// </summary>
        public ReadOnlyMemory<Byte> SecurityDescriptorBinaryForm
        {
            get
            {
                if (_securityDescriptorBinaryForm is null)
                    throw new InvalidOperationException($"{nameof(SecurityDescriptorBinaryForm)} is not set.");

                return _securityDescriptorBinaryForm.Value;
            }

            set
            {
                _securityDescriptorBinaryForm = value;
                _securityDescriptorBinaryFormLength = checked((UInt32)value.Length);
            }
        }

        private static (ReadOnlyMemory<Byte> compressedData, ZipEntryCompressionMethodId compressionMethodId)? Compress(ZipEntryCompressionMethodId compressionMethodId, ReadOnlyMemory<Byte> uncompressedBytes)
        {
            switch (compressionMethodId)
            {
                case ZipEntryCompressionMethodId.Stored:
                    return (uncompressedBytes, ZipEntryCompressionMethodId.Stored);
                case ZipEntryCompressionMethodId.Deflate:
                {
                    using var compressedStream = new MemoryStream();
                    using (var compressor = new DeflateStream(compressedStream, CompressionLevel.SmallestSize, true))
                    {
                        compressor.WriteBytes(uncompressedBytes.Span);
                    }

                    compressedStream.Position = 0;
                    var compressedData = compressedStream.ReadAllBytes();
                    return
                        compressedData.Length < uncompressedBytes.Length
                        ? (compressedData, ZipEntryCompressionMethodId.Deflate)
                        : (uncompressedBytes, ZipEntryCompressionMethodId.Stored);
                }
                default:
                    return null;
            }
        }

        private static ReadOnlyMemory<Byte>? Decompress(ZipEntryCompressionMethodId compressionMethodId, ReadOnlyMemory<Byte> compressedBytes)
        {
            switch (compressionMethodId)
            {
                case ZipEntryCompressionMethodId.Stored:
                    return compressedBytes;
                case ZipEntryCompressionMethodId.Deflate:
                {
                    using var compressedStream = new MemoryStream();
                    compressedStream.WriteBytes(compressedBytes.Span);
                    compressedStream.Position = 0;
                    using var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress);
                    return decompressor.ReadAllBytes();
                }
                default:
                    return null;
            }
        }
    }
}
