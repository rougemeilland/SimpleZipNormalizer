using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using Utility.Linq;
using ZipUtility.IO;
using ZipUtility.IO.Compression;

namespace ZipUtility
{
    internal class ZipEntryCompressionMethod
    {
        private enum CoderType
        {
            Decoder,
            Encoder,
        }

        private static readonly Regex _pluginFileNamePattern;
        private static readonly IDictionary<(CompressionMethodId CompressionMethodId, CoderType CoderType), ICompressionCoder> _compresssionMethods;
        private static readonly ZipEntryCompressionMethod? _stored;
        private static readonly ZipEntryCompressionMethod? _deflateWithNormal;
        private static readonly ZipEntryCompressionMethod? _deflateWithMaximum;
        private static readonly ZipEntryCompressionMethod? _deflateWithFast;
        private static readonly ZipEntryCompressionMethod? _deflateWithSuperFast;
        private static readonly ZipEntryCompressionMethod? _deflate64WithNormal;
        private static readonly ZipEntryCompressionMethod? _deflate64WithMaximum;
        private static readonly ZipEntryCompressionMethod? _deflate64WithFast;
        private static readonly ZipEntryCompressionMethod? _deflate64WithSuperFast;
        private static readonly ZipEntryCompressionMethod? _bzip2;
        private static readonly ZipEntryCompressionMethod? _lzmaWithEOS;
        private static readonly ZipEntryCompressionMethod? _lzmaWithoutEOS;
        private static readonly ZipEntryCompressionMethod? _ppmd;

        private readonly ICompressionCoder? _decoderPlugin;
        private readonly ICoderOption? _decoderOption;
        private readonly ICompressionCoder? _encoderPlugin;
        private readonly ICoderOption? _encoderOption;

        static ZipEntryCompressionMethod()
        {
            _pluginFileNamePattern = new Regex(@"^ZipUtility\.IO\.Compression\.[a-zA-Z0-9_]+\.dll$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            _compresssionMethods = EnumeratePlugin();

            _stored =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Stored,
                    plugin => plugin.DefaultOption);
            _deflateWithNormal =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Normal));
            _deflateWithMaximum =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Maximum));
            _deflateWithFast =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Fast));
            _deflateWithSuperFast =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.SuperFast));
            _deflate64WithNormal =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate64,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Normal));
            _deflate64WithMaximum =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate64,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Maximum));
            _deflate64WithFast =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate64,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.Fast));
            _deflate64WithSuperFast =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.Deflate64,
                    _ => CompressionOption.GetDeflateCompressionOption(DeflateCompressionLevel.SuperFast));
            _bzip2 =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.BZIP2,
                    plugin => plugin.DefaultOption);
            _lzmaWithEOS =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.LZMA,
                    _ => CompressionOption.GetLzmaCompressionOption(true));
            _lzmaWithoutEOS =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.LZMA,
                    _ => CompressionOption.GetLzmaCompressionOption(false));
            _ppmd =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    IO.Compression.CompressionMethodId.PPMd,
                    plugin => plugin.DefaultOption);
        }

        internal ZipEntryCompressionMethod(ZipEntryCompressionMethodId compressMethodId, ICompressionCoder? decoderPlugin, IO.ICoderOption? decoderOption, ICompressionCoder? encoderPlugin, IO.ICoderOption? encoderOption)
        {
            if (decoderPlugin is null && encoderPlugin is null)
                throw new ArgumentException($"Both {nameof(decoderPlugin)} and {nameof(encoderPlugin)} are null.");
            if (decoderPlugin is not null and not ICompressionDecoder and not ICompressionHierarchicalDecoder)
                throw new ArgumentException($"{nameof(decoderPlugin)} does not implement the required interface.");
            if (encoderPlugin is not null and not ICompressionEncoder and not ICompressionHierarchicalEncoder)
                throw new ArgumentException($"{nameof(encoderPlugin)} does not implement the required interface.");
            if (decoderPlugin is not null && decoderOption is null)
                throw new ArgumentNullException(nameof(decoderOption));
            if (encoderPlugin is not null && encoderOption is null)
                throw new ArgumentNullException(nameof(encoderOption));

            CompressionMethodId = compressMethodId;
            IsSupportedGetDecodingStream = decoderPlugin is ICompressionHierarchicalDecoder;
            IsSupportedDecode = decoderPlugin is ICompressionDecoder;
            IsSupportedGetEncodingStream = encoderPlugin is ICompressionHierarchicalEncoder;
            IsSupportedEncode = encoderPlugin is ICompressionEncoder;
            _decoderPlugin = decoderPlugin;
            _decoderOption = decoderOption;
            _encoderPlugin = encoderPlugin;
            _encoderOption = encoderOption;
        }

        public static IEnumerable<ZipEntryCompressionMethodId> SupportedCompresssionMethodIds => _compresssionMethods.Keys.Select(key => GetCompressionMethodId(key.CompressionMethodId));
        public static ZipEntryCompressionMethod Stored => _stored ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Stored);
        public static ZipEntryCompressionMethod DeflateWithNormal => _deflateWithNormal ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate);
        public static ZipEntryCompressionMethod DeflateWithMaximum => _deflateWithMaximum ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate);
        public static ZipEntryCompressionMethod DeflateWithFast => _deflateWithFast ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate);
        public static ZipEntryCompressionMethod DeflateWithSuperFast => _deflateWithSuperFast ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate);
        public static ZipEntryCompressionMethod Deflate64WithNormal => _deflate64WithNormal ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate64);
        public static ZipEntryCompressionMethod Deflate64WithMaximum => _deflate64WithMaximum ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate64);
        public static ZipEntryCompressionMethod Deflate64WithFast => _deflate64WithFast ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate64);
        public static ZipEntryCompressionMethod Deflate64WithSuperFast => _deflate64WithSuperFast ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.Deflate64);
        public static ZipEntryCompressionMethod BZIP2 => _bzip2 ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.BZIP2);
        public static ZipEntryCompressionMethod LZMAWithEOS => _lzmaWithEOS ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.LZMA);
        public static ZipEntryCompressionMethod LZMAWithoutEOS => _lzmaWithoutEOS ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.LZMA);
        public static ZipEntryCompressionMethod PPMd => _ppmd ?? throw new CompressionMethodNotSupportedException(ZipEntryCompressionMethodId.PPMd);

        public ZipEntryCompressionMethodId CompressionMethodId { get; }
        public Boolean IsSupportedGetDecodingStream { get; }
        public Boolean IsSupportedDecode { get; }
        public Boolean IsSupportedGetEncodingStream { get; }
        public Boolean IsSupportedEncode { get; }

        public IBasicInputByteStream GetDecodingStream(IBasicInputByteStream baseStream, UInt64 unpackedSize, UInt64 packedSize, IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress = null)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            return InternalGetDecodingStream(baseStream, unpackedSize, packedSize, progress);
        }

        public IBasicOutputByteStream GetEncodingStream(IBasicOutputByteStream baseStream, UInt64? size, IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress = null)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            return InternalGetEncodingStream(baseStream, size, progress);
        }

        internal static ZipEntryCompressionMethod GetCompressionMethod(ZipEntryCompressionMethodId compressionMethodId, ZipEntryGeneralPurposeBitFlag flag)
        {
            var instance =
                CreateCompressionMethodDefaultInstance(
                    _compresssionMethods,
                    GetPluginId(compressionMethodId),
                    plugin => plugin.GetOptionFromGeneralPurposeFlag(

                    flag.HasFlag(ZipEntryGeneralPurposeBitFlag.CompresssionOption0),
                    flag.HasFlag(ZipEntryGeneralPurposeBitFlag.CompresssionOption1)));
            return
                instance ?? throw new CompressionMethodNotSupportedException(compressionMethodId);
        }

        internal (UInt32 Crc, UInt64 Length) CalculateCrc32(
            IZipInputStream zipInputStream,
            ZipStreamPosition offset,
            UInt64 size,
            UInt64 packedSize,
            IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress)
        {
            if (_decoderPlugin is ICompressionHierarchicalDecoder hierarchicalDecoder)
            {
                if (_decoderOption is null)
                    throw new InternalLogicalErrorException();

                progress?.Report((0UL, 0UL));
                var (baseStream, unpackedCountProgress) = zipInputStream.AsPartial(offset, packedSize).CreateProgressFilter(progress);
                return
                    hierarchicalDecoder
                        .GetDecodingStream(
                            baseStream,
                            _decoderOption,
                            size,
                            packedSize,
                            unpackedCountProgress)
                        .CalculateCrc32();
            }
            else if (_decoderPlugin is ICompressionDecoder decoder)
            {
                if (_decoderOption is null)
                    throw new InternalLogicalErrorException();

                progress?.Report((0UL, 0UL));
                var (baseStream, unpackedCountProgress) = zipInputStream.AsPartial(offset, packedSize).CreateProgressFilter(progress);
                var valueHolder = new ValueHolder<(UInt32 Crc, UInt64 Length)>();
                using var outputStream = new NullOutputStream();
                decoder.Decode(
                    baseStream,
                    outputStream.WithCrc32Calculation(valueHolder),
                    _decoderOption,
                    size,
                    packedSize,
                    unpackedCountProgress);

                return valueHolder.Value;
            }
            else
            {
                throw new IllegalRuntimeEnvironmentException($"Can not uncompress content: method={CompressionMethodId}.");
            }
        }

        private static ZipEntryCompressionMethod? CreateCompressionMethodDefaultInstance(IDictionary<(CompressionMethodId CompressionMethodId, CoderType CoderType), ICompressionCoder> compresssionMethodSource, CompressionMethodId compressionMethodId, Func<ICompressionCoder, IO.ICoderOption> optionGetter)
        {
            if (!compresssionMethodSource.TryGetValue((compressionMethodId, CoderType.Decoder), out ICompressionCoder? deoderPlugin))
                deoderPlugin = null;
            if (!compresssionMethodSource.TryGetValue((compressionMethodId, CoderType.Encoder), out ICompressionCoder? enoderPlugin))
                enoderPlugin = null;
            return
                deoderPlugin is null && enoderPlugin is null
                ? null
                : new ZipEntryCompressionMethod(
                    GetCompressionMethodId(compressionMethodId),
                    deoderPlugin,
                    deoderPlugin is null ? null : optionGetter(deoderPlugin),
                    enoderPlugin,
                    enoderPlugin is null ? null : optionGetter(enoderPlugin));
        }

        private static IDictionary<(CompressionMethodId CompressionMethodId, CoderType CoderType), ICompressionCoder> EnumeratePlugin()
        {
            var interfaceType = typeof(ICompressionCoder);
            var interfaceTypeName = interfaceType.FullName ?? throw new InvalidOperationException();
            var thisAssembly = typeof(ZipEntryCompressionMethodIdExtensions).Assembly;
            var pluginLocation = Path.GetDirectoryName(thisAssembly.Location) ?? throw new InvalidOperationException();
            var pluginSequence =
                Directory.EnumerateFiles(pluginLocation, "*.dll", SearchOption.AllDirectories)
                    .Where(filePath =>
                        _pluginFileNamePattern.IsMatch(Path.GetFileName(filePath)) &&
                        !String.Equals(filePath, thisAssembly.Location, StringComparison.OrdinalIgnoreCase))
                    .Select(filePath =>
                    {
                        try
                        {
                            return Assembly.LoadFile(filePath);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    })
                    .WhereNotNull()
                    .Select(assembly =>
                    {
#if DEBUG && false
                        System.Diagnostics.Debug.WriteLine($"loaded {assembly.Location}");
#endif
                        return new { assembly, externalAssembly = true };
                    })
                    .Concat(new[]
                    {
                        new { assembly = thisAssembly, externalAssembly = false },
                        new { assembly = interfaceType.Assembly, externalAssembly = true },
                    })
                    .SelectMany(item =>
                        item.assembly.GetTypes()
                        .Where(type =>
                            type.IsClass &&
                            (type.IsPublic || !item.externalAssembly) &&
                            !type.IsAbstract &&
                            type.GetInterface(interfaceTypeName) is not null)
                        .Select(type =>
                        {
                            try
                            {
#if DEBUG && false
                                System.Diagnostics.Debug.WriteLine($"create plugin {type.FullName}");
#endif
                                return
                                    type.FullName is null
                                    ? null
                                    : item.assembly.CreateInstance(type.FullName) as ICompressionCoder;
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        })
                        .WhereNotNull()
                        .Where(plugin => plugin is not null && plugin.CompressionMethodId != IO.Compression.CompressionMethodId.Unknown));
            var plugins = new Dictionary<(CompressionMethodId CompressionMethodId, CoderType CoderType), ICompressionCoder>();
            foreach (var plugin in pluginSequence)
            {
                if (plugin is ICompressionDecoder or ICompressionHierarchicalDecoder)
                {
                    if (!plugins.TryAdd((plugin.CompressionMethodId, CoderType.Decoder), plugin))
                        throw new IllegalRuntimeEnvironmentException($"Duplicate Compress plug-in. : method = {plugin.CompressionMethodId}, type = {CoderType.Decoder}");
                }

                if (plugin is ICompressionEncoder or ICompressionHierarchicalEncoder)
                {
                    if (!plugins.TryAdd((plugin.CompressionMethodId, CoderType.Encoder), plugin))
                        throw new IllegalRuntimeEnvironmentException($"Duplicate Compress plug-in. : method = {plugin.CompressionMethodId}, type = {CoderType.Decoder}");
                }
            }

            return plugins;
        }

        private static CompressionMethodId GetPluginId(ZipEntryCompressionMethodId compressionMethodId)
            => compressionMethodId switch
            {
                ZipEntryCompressionMethodId.Stored => IO.Compression.CompressionMethodId.Stored,
                ZipEntryCompressionMethodId.Deflate => IO.Compression.CompressionMethodId.Deflate,
                ZipEntryCompressionMethodId.Deflate64 => IO.Compression.CompressionMethodId.Deflate64,
                ZipEntryCompressionMethodId.BZIP2 => IO.Compression.CompressionMethodId.BZIP2,
                ZipEntryCompressionMethodId.LZMA => IO.Compression.CompressionMethodId.LZMA,
                ZipEntryCompressionMethodId.PPMd => IO.Compression.CompressionMethodId.PPMd,
                _ => IO.Compression.CompressionMethodId.Unknown,
            };

        private static ZipEntryCompressionMethodId GetCompressionMethodId(CompressionMethodId pluginId)
            => pluginId switch
            {
                IO.Compression.CompressionMethodId.Stored => ZipEntryCompressionMethodId.Stored,
                IO.Compression.CompressionMethodId.Deflate => ZipEntryCompressionMethodId.Deflate,
                IO.Compression.CompressionMethodId.Deflate64 => ZipEntryCompressionMethodId.Deflate64,
                IO.Compression.CompressionMethodId.BZIP2 => ZipEntryCompressionMethodId.BZIP2,
                IO.Compression.CompressionMethodId.LZMA => ZipEntryCompressionMethodId.LZMA,
                IO.Compression.CompressionMethodId.PPMd => ZipEntryCompressionMethodId.PPMd,
                _ => ZipEntryCompressionMethodId.Unknown,
            };

        private IBasicInputByteStream InternalGetDecodingStream(
            IBasicInputByteStream baseStream,
            UInt64 unpackedSize,
            UInt64 packedSize,
            IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress)
        {
            switch (_decoderPlugin)
            {
                case ICompressionHierarchicalDecoder hierarchicalDecoder:
                    if (_decoderOption is null)
                        throw new InternalLogicalErrorException();

                    progress?.Report((0UL, 0UL));
                    var filter = baseStream.CreateProgressFilter(progress);
                    return
                        hierarchicalDecoder.GetDecodingStream(
                            filter.baseStream,
                            _decoderOption,
                            unpackedSize,
                            packedSize,
                            filter.unpackedCountProgress);

                case ICompressionDecoder decoder:
                {
                    if (_decoderOption is null)
                        throw new InternalLogicalErrorException();

                    var queue = new AsyncByteIOQueue();
                    var decoderOption = _decoderOption;
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            progress?.Report((0UL, 0UL));
                            var filter = baseStream.CreateProgressFilter(progress);
                            using var queueWriter = queue.GetWriter();
                            decoder.Decode(
                                filter.baseStream,
                                queueWriter,
                                decoderOption,
                                unpackedSize,
                                packedSize,
                                filter.unpackedCountProgress);
                        }
                        catch (Exception)
                        {
                        }
                    });
                    return queue.GetReader();
                }

                default:
                    throw new NotSupportedException($"Compression is not suppoted. : method = {CompressionMethodId}");
            }
        }

        private IBasicOutputByteStream InternalGetEncodingStream(
            IBasicOutputByteStream baseStream,
            UInt64? unpackedSize,
            IProgress<(UInt64 unpackedCount, UInt64 packedCount)>? progress)
        {
            progress?.Report((0UL, 0UL));
            switch (_encoderPlugin)
            {
                case ICompressionHierarchicalEncoder hierarchicalEncoder:
                    if (_encoderOption is null)
                        throw new InternalLogicalErrorException();

                    var filter = baseStream.CreateProgressFilter(progress);
                    return
                        hierarchicalEncoder.GetEncodingStream(
                            filter.baseStream,
                            _encoderOption,
                            unpackedSize,
                            filter.unpackedCountProgress);

                case ICompressionEncoder encoder:
                {
                    if (_encoderOption is null)
                        throw new InternalLogicalErrorException();

                    var queue = new AsyncByteIOQueue();
                    var encoderOption = _encoderOption;
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            using var queueReader = queue.GetReader();
                            var filter = baseStream.CreateProgressFilter(progress);
                            encoder.Encode(
                                queueReader,
                                filter.baseStream,
                                encoderOption,
                                unpackedSize,
                                filter.unpackedCountProgress);
                        }
                        catch (Exception)
                        {
                        }
                    });
                    return queue.GetWriter();
                }

                default:
                    throw new NotSupportedException($"Compression is not suppoted. : method = {CompressionMethodId}");
            }
        }
    }
}
