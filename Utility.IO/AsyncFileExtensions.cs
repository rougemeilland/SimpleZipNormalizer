using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public static class AsyncFileExtensions
    {
        /// <remarks>
        /// .NET 7 以降では不要な実装だが、.NET 6 では必要なので実装する。
        /// </remarks>
        private class ReadLinesEnumerable
            : IAsyncEnumerable<String>
        {
            private class Enumerator
                : IAsyncEnumerator<String>
            {
                private readonly TextReader _reader;
                private readonly CancellationToken _cancellationToken;

                private Boolean _isDisposed;
                private String? _currentValue;
                private Boolean _endOfStream;

                public Enumerator(TextReader reader, CancellationToken cancellationToken)
                {
                    _reader = reader;
                    _cancellationToken = cancellationToken;
                    _isDisposed = false;
                    _currentValue = null;
                    _endOfStream = false;
                }

                public String Current
                {
                    get
                    {
                        if (_isDisposed)
                            throw new ObjectDisposedException(GetType().FullName);
                        if (_currentValue is null)
                            throw new InvalidOperationException();
                        if (_endOfStream)
                            throw new InvalidOperationException();
                        _cancellationToken.ThrowIfCancellationRequested();

                        return _currentValue;
                    }
                }

                public async ValueTask<Boolean> MoveNextAsync()
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (_endOfStream)
                        return false;
                    _currentValue = await _reader.ReadLineAsync().ConfigureAwait(false);
                    if (_currentValue is null)
                    {
                        _endOfStream = true;
                        return false;
                    }

                    return true;
                }

                public ValueTask DisposeAsync()
                {
                    if (!_isDisposed)
                    {
                        _reader.Dispose();
                        _isDisposed = true;
                    }

                    GC.SuppressFinalize(this);
                    return ValueTask.CompletedTask;
                }
            }

            private readonly TextReader _reader;

            public ReadLinesEnumerable(TextReader reader)
            {
                _reader = reader;
            }

            public IAsyncEnumerator<String> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new Enumerator(_reader, cancellationToken);
        }

        static AsyncFileExtensions()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static Task<Byte[]> ReadAllBytesAsync(this FileInfo file, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadAllBytesAsync(file.FullName, cancellationToken);
        }

        public static Task<Byte[]> ReadAllBytesAsync(this FilePath file, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            return File.ReadAllBytesAsync(file.FullName, cancellationToken);
        }

        public static Task<String[]> ReadAllLinesAsync(this FileInfo file, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadAllLinesAsync(file.FullName, cancellationToken);
        }

        public static Task<String[]> ReadAllLinesAsync(this FilePath file, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return File.ReadAllLinesAsync(file.FullName, cancellationToken);
        }

        public static IAsyncEnumerable<String> ReadLinesAsync(this FileInfo file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return new ReadLinesEnumerable(new StreamReader(file.FullName, Encoding.UTF8));
        }

        public static IAsyncEnumerable<String> ReadLinesAsync(this FilePath file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return new ReadLinesEnumerable(new StreamReader(file.FullName, Encoding.UTF8));
        }

        public static IAsyncEnumerable<String> ReadLinesAsync(this FileInfo file, Encoding encoding)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            return new ReadLinesEnumerable(new StreamReader(file.FullName, encoding));
        }

        public static IAsyncEnumerable<String> ReadLinesAsync(this FilePath file, Encoding encoding)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            return new ReadLinesEnumerable(new StreamReader(file.FullName, encoding));
        }

        public static async Task WriteAllBytesAsync(this FileInfo file, IEnumerable<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteByteSequenceAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllBytesAsync(this FilePath file, IEnumerable<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteByteSequenceAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllBytesAsync(this FileInfo file, IAsyncEnumerable<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteByteSequenceAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllBytesAsync(this FilePath file, IAsyncEnumerable<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteByteSequenceAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllBytesAsync(this FileInfo file, Byte[] data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await File.WriteAllBytesAsync(file.FullName, data, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllBytesAsync(this FilePath file, Byte[] data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await File.WriteAllBytesAsync(file.FullName, data, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllBytesAsync(this FileInfo file, ReadOnlyMemory<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteBytesAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllBytesAsync(this FilePath file, ReadOnlyMemory<Byte> data, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            var stream = file.OpenWrite();
            await using (stream.ConfigureAwait(false))
            {
                await stream.WriteBytesAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async Task WriteAllTextAsync(this FileInfo file, String text, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            await File.WriteAllTextAsync(file.FullName, text, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FilePath file, String text, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            await File.WriteAllTextAsync(file.FullName, text, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FileInfo file, String text, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (text is null)
                throw new ArgumentNullException(nameof(text));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await File.WriteAllTextAsync(file.FullName, text, encoding, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FilePath file, String text, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (text is null)
                throw new ArgumentNullException(nameof(text));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await File.WriteAllTextAsync(file.FullName, text, encoding, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllLinesAsync(this FileInfo file, IEnumerable<String> lines, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            await File.WriteAllLinesAsync(file.FullName, lines, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllLinesAsync(this FilePath file, IEnumerable<String> lines, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            await File.WriteAllLinesAsync(file.FullName, lines, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FileInfo file, IEnumerable<String> lines, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await File.WriteAllLinesAsync(file.FullName, lines, encoding, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FilePath file, IEnumerable<String> lines, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await File.WriteAllLinesAsync(file.FullName, lines, encoding, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllLinesAsync(this FileInfo file, IAsyncEnumerable<String> lines, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            await InternalWriteAllLinesAsync(file.FullName, lines, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllLinesAsync(this FilePath file, IAsyncEnumerable<String> lines, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            await InternalWriteAllLinesAsync(file.FullName, lines, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FileInfo file, IAsyncEnumerable<String> lines, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await InternalWriteAllLinesAsync(file.FullName, lines, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(this FilePath file, IAsyncEnumerable<String> lines, Encoding encoding, CancellationToken cancellationToken = default)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            await InternalWriteAllLinesAsync(file.FullName, lines, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc24Async(this FileInfo sourceFile, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc24Async(cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc24Async(this FilePath sourceFile, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc24Async(cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc24Async(this FileInfo sourceFile, IProgress<UInt64>? progress, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc24Async(progress, cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc24Async(this FilePath sourceFile, IProgress<UInt64>? progress, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc24Async(progress, cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc32Async(this FileInfo sourceFile, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc32Async(cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc32Async(this FilePath sourceFile, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc32Async(cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc32Async(this FileInfo sourceFile, IProgress<UInt64>? progress, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc32Async(progress, cancellationToken);
        }

        public static Task<(UInt32 Crc, UInt64 Length)> CalculateCrc32Async(this FilePath sourceFile, IProgress<UInt64>? progress, CancellationToken cancellationToken = default)
        {
            if (sourceFile is null)
                throw new ArgumentNullException(nameof(sourceFile));

            return sourceFile.OpenRead().CalculateCrc32Async(progress, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2016:'CancellationToken' パラメーターをメソッドに転送する", Justification = "<保留中>")]
        private static async Task InternalWriteAllLinesAsync(String fileFullPath, IAsyncEnumerable<String> lines, Encoding encoding, CancellationToken cancellationToken)
        {
            var writer = new StreamWriter(fileFullPath, false, encoding);
            await using (writer.ConfigureAwait(false))
            {
                var enumerator = lines.GetAsyncEnumerator(cancellationToken);
                await using (enumerator.ConfigureAwait(false))
                {
                    while (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        writer.WriteLine(enumerator.Current);
                    }
                }

                /// TextWriter.FlushAsync(bool cancellationToken) のオーバーロードのサポートは .NET 8.0 以降。
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
