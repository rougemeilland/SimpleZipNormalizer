using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Utility;
using Utility.IO;
using Utility.Linq;
using ZipUtility;

namespace SimpleZipNormalizer.CUI
{
    internal class Program
    {
        private enum CommandMode
        {
            Normalize = 0,
            ShowCodePages,
            ListZipEntries,
        }

        private static readonly uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private static readonly uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
        private static readonly uint STD_ERROR_HANDLE = unchecked((uint)-12);
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
        private static readonly string _thisProgramName;
        private static TextWriter? _consoleWriter;
        private static string _currentProgressMessage;

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetStdHandle(uint nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _consoleWriter = null;
            _currentProgressMessage = "";
        }

        private static int Main(string[] args)
        {
            // TODO: stored の性能向上 のテスト
            // TODO: UNICODEにない文字セットを含む名前とコメントを持つエントリをZIPファイルに入れてみる。どういう挙動をするか？
            // TODO: ZIP64対応のテスト 圧縮済みサイズが合計4Gを超えるファイルを含むZIPファイル
            // TODO: データディスクリプタ付きの書き込みに挑戦 (いやがらせ？)
            // TODO: マルチボリューム対応に挑戦
            // TODO: ZIP64対応のテスト ディスク数が65535以上になるように分割してみる。分割サイズが64KBだとして、圧縮済みサイズが合計4GBを超えれば ZIP64 EOCDR が適用されるはず。
            var optionInteractive = false;
            var mode = CommandMode.ListZipEntries;
            var allowedEncodngNames = new List<string>();
            var excludedEncodngNames = new List<string>();
            try
            {
                InitializeConsole();
                _consoleWriter?.Write("\x1b[?12h"); // カーソルの非点滅
                _consoleWriter?.Write("\x1b[?25l"); // カーソルの非表示

                try
                {
                    {
                        for (var index = 0; index < args.Length; ++index)
                        {
                            var arg = args[index];
                            if (arg is "-i" or "--interactive")
                            {
                                optionInteractive = true;
                            }
                            else if (arg is "-n" or "--normalize")
                            {
                                mode = CommandMode.Normalize;
                            }
                            else if (arg is "-l" or "--list_entries")
                            {
                                mode = CommandMode.ListZipEntries;
                            }
                            else if (arg is "-cl" or "--show_code_page_list")
                            {
                                mode = CommandMode.ShowCodePages;
                            }
                            else if ((arg == "-ci" || arg == "--allowed_code_page") && index + 1 < arg.Length)
                            {
                                allowedEncodngNames.AddRange(args[index + 1].Split(','));
                            }
                            else if ((arg == "-ce" || arg == "--excluded_code_page") && index + 1 < arg.Length)
                            {
                                excludedEncodngNames.AddRange(args[index + 1].Split(','));
                            }
                            else if (arg.StartsWith('-'))
                            {
                                throw new Exception($"サポートされていないオプションがコマンド引数に指定されました。: \"{arg}\"");
                            }
                            else
                            {
                                // OK
                            }
                        }
                    }

                    var encodingProvider = ZipEntryNameEncodingProvider.Create(allowedEncodngNames.ToArray(), excludedEncodngNames.ToArray(), "##");

                    if (mode == CommandMode.ShowCodePages)
                    {
                        var encodingList = encodingProvider.SupportedEncodings.ToList();
                        var maximumEncodingNameLength = encodingList.Select(encoding => encoding.WebName.Length).Append(0).Max();
                        foreach (var encoding in encodingList)
                            Console.WriteLine($"Name: {encoding.WebName}\", {new string(' ', maximumEncodingNameLength - encoding.WebName.Length)}Description: {encoding.EncodingName}");
                        if (optionInteractive)
                        {
                            Console.Beep();
                            _ = Console.ReadLine();
                        }

                        return 0;
                    }

                    if (encodingProvider.SupportedEncodings.None())
                        throw new Exception($"エントリ名およびコメントに適用できるコードページがありません。オプションでコードページを指定している場合は指定内容を確認してください。サポートされているコードページのリストは \"--show_code_page_list\" オプションを指定して起動することにより確認できます。");

                    var trashBox = TrashBox.OpenTrashBox();

                    var zipFiles = EnumerateZipFiles(args.Where(arg => !arg.StartsWith('-'))).ToList();

                    if (mode == CommandMode.Normalize)
                    {
                        var totalSize = zipFiles.Sum(zipFile => zipFile.Length);
                        var completedRate = 0.0;
                        ReportProgress(completedRate);

                        foreach (var zipFile in zipFiles)
                        {
                            ReportProgress(completedRate, zipFile);

                            var parentDirectoryPath = zipFile.DirectoryName ?? throw new Exception();
                            var originalZipFileSize = zipFile.Length;
                            var temporaryFileName = $".{zipFile.Name}.0.zip";
                            var temporaryFile = new FileInfo(Path.Combine(parentDirectoryPath, temporaryFileName));
                            if (temporaryFile.Exists)
                            {
                                for (var index = 1; ; ++index)
                                {
                                    temporaryFileName = $".{zipFile.Name}.{index}.zip";
                                    temporaryFile = new FileInfo(Path.Combine(parentDirectoryPath, temporaryFileName));
                                    if (!temporaryFile.Exists)
                                        break;
                                }
                            }

                            try
                            {
                                if (NormalizeZipFile(
                                    zipFile,
                                    temporaryFile,
                                    encodingProvider,
                                    SafetyProgress.CreateIncreasingProgress<double>(
                                        value =>
                                            ReportProgress(
                                                completedRate + value * originalZipFileSize / totalSize,
                                                zipFile))))
                                {
                                    if (!trashBox.DisposeFile(new FileInfo(zipFile.FullName)))
                                        throw new Exception($"ファイルのごみ箱への移動に失敗しました。: \"{zipFile.FullName}\"");

                                    File.Move(temporaryFile.FullName, zipFile.FullName, false);
                                }
                            }
                            catch (Exception ex)
                            {
                                ReportException(ex);
                            }
                            finally
                            {
                                temporaryFile.SafetyDelete();
                                completedRate += (double)originalZipFileSize / totalSize;
                                ReportProgress(completedRate);
                            }
                        }

                        ReportProgress(completedRate);
                    }
                    else
                    {
                        foreach (var zipFile in zipFiles)
                        {
                            Console.WriteLine($"file: {zipFile.FullName}");
                            ListZipFile(zipFile, encodingProvider);
                            Console.WriteLine(new string('-', 20));
                        }
                    }
                }
                finally
                {
                    _consoleWriter?.Write("\x1b[?12h"); // カーソルの点滅
                    _consoleWriter?.Write("\x1b[?25h"); // カーソルの表示
                }

                Console.WriteLine();
                Console.WriteLine("終了しました。");
                if (optionInteractive)
                {
                    Console.Beep();
                    _ = Console.ReadLine();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                ReportException(ex);
                if (optionInteractive)
                {
                    Console.Beep();
                    _ = Console.ReadLine();
                }

                return 1;
            }
        }

        private static IEnumerable<FileInfo> EnumerateZipFiles(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var success = false;
                var directory = TryGetDirectoryInfo(arg);
                if (directory is not null)
                {
                    success = true;
                    foreach (var childFile in EnumerateZipFiles(directory))
                        yield return childFile;
                }
                else
                {
                    var file = TryGetFileInfo(arg);
                    if (file is not null)
                    {
                        success = true;
                        yield return file;
                    }
                }

                if (!success)
                    throw new Exception($"コマンド引数がファイルまたはディレクトリのパス名ではありません。: \"{arg}\"");
            }
        }

        private static IEnumerable<FileInfo> EnumerateZipFiles(DirectoryInfo directory)
        {
            foreach (var childFile in directory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (string.Equals(childFile.Extension, ".zip", StringComparison.OrdinalIgnoreCase))
                    yield return childFile;
            }

            foreach (var chilDirectory in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                foreach (var childFile in EnumerateZipFiles(chilDirectory))
                    yield return childFile;
            }
        }

        private static DirectoryInfo? TryGetDirectoryInfo(string path)
        {
            try
            {
                var dir = new DirectoryInfo(path.EndsWith(Path.PathSeparator) ? path[..^1] : path);
                return dir.Exists ? dir : null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static FileInfo? TryGetFileInfo(string path)
        {
            try
            {
                var file = new FileInfo(path);
                return file.Exists && string.Equals(file.Extension, ".zip", StringComparison.OrdinalIgnoreCase) ? file : null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static void ListZipFile(FileInfo sourceZipFile, IZipEntryNameEncodingProvider entryNameEncodingProvider, IProgress<double>? progress = null)
        {
            try
            {
                using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
                foreach (var entry in sourceArchiveReader.GetEntries(progress))
                {
                    var localExtraIdsText = string.Join(",", entry.LocalHeaderExtraFields.EnumerateExtraFieldIds().Select(id => $"0x{id:x4}"));
                    var centralExtraIdsText = string.Join(",", entry.CentralDirectoryHeaderExtraFields.EnumerateExtraFieldIds().Select(id => $"0x{id:x4}"));
                    var exactEncodingText = entry.ExactEntryEncoding?.WebName ?? "???";
                    var possibleEncodings = entry.PossibleEntryEncodings.Select(encoding => encoding.WebName).Take(3).ToList();
                    var possibleEncodingsText = $"{string.Join(",", possibleEncodings)}{(possibleEncodings.Count > 2 ? ", ..." : "")}";
                    Console.WriteLine($"  {entry.FullName}, local:[{localExtraIdsText}], central:[{centralExtraIdsText}], exact={exactEncodingText}, possible=[{possibleEncodingsText}], size={entry.Size:N0}, packedSize={entry.PackedSize:N0}, compress={entry.CompressionMethodId}");
                }
            }
            catch (EncryptedZipFileNotSupportedException ex)
            {
                ReportException(ex);
            }
            catch (CompressionMethodNotSupportedException ex)
            {
                ReportException(ex);
            }
            catch (NotSupportedSpecificationException ex)
            {
                ReportException(ex);
            }
            catch (BadZipFileFormatException ex)
            {
                ReportException(ex);
            }
        }

        private static bool NormalizeZipFile(FileInfo sourceZipFile, FileInfo destinationZipFile, IZipEntryNameEncodingProvider entryNameEncodingProvider, IProgress<double> progress)
        {
            using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
            var sourceZipFileLength = sourceZipFile.Length;
            var rootNode = PathNode.CreateRootNode();
            var badEntries = sourceArchiveReader.GetEntries().Where(entry => entry.FullName.IsUnknownEncodingText()).ToList();
            if (badEntries.Count > 0)
                throw new Exception($".NETで認識できない名前のエントリがZIPファイルに含まれています。: ZIP file name:\"{sourceZipFile.FullName}\", Entry name: \"{badEntries.First().FullName}\"");

            progress?.Report(0);
            var sourceEntries =
                sourceArchiveReader.GetEntries(
                    SafetyProgress.CreateIncreasingProgress(
                        progress,
                        value => value * 0.05,
                        0.0,
                        1.0));
            progress?.Report(0.05);
            foreach (var entry in sourceEntries)
                rootNode.AddChildNode(entry.FullName, entry);

            // ノードのディレクトリ構成を正規化する (空ディレクトリの削除、無駄なディレクトリ階層の短縮)
            rootNode.Normalize();

            // 正規化されたノードを列挙する
            var normalizedEntries =
                rootNode.EnumerateTerminalNodes()
                .Select(node => new
                {
                    destinationFullName = node.CurrentFullName,
                    isDirectory = node is DirectoryPathNode,
                    sourceFullName = node.SourceFullName,
                    sourceEntry = node.SourceEntry,
                })
                .OrderBy(item => item.destinationFullName, StringComparer.OrdinalIgnoreCase)
                .Select((item, newOrder) => (item.destinationFullName, item.isDirectory, item.sourceFullName, newOrder, item.sourceEntry))
                .ToList();

            // 正規化前後でエントリが変更する見込みがあるかどうかを調べる
            var modified = ExistModifiedEntries(normalizedEntries);
            if (modified)
            {
                // 正規化の前後でパス名および順序が一致しないエントリが一つでもある場合

                // ZIPアーカイブの正規化を実行する
                CreateNormalizedZipArchive(
                    destinationZipFile,
                    entryNameEncodingProvider,
                    sourceZipFileLength,
                    sourceEntries,
                    normalizedEntries,
                    SafetyProgress.CreateIncreasingProgress(
                        progress,
                        value => 0.05 + value * 0.45,
                        0.0,
                        1.0));

                progress?.Report(0.5);

                // 元のZIPアーカイブと正規化されたZIPアーカイブの内容が一致しているかどうかを調べる
                using var normalizedZipArchiveReader = destinationZipFile.OpenAsZipFile(entryNameEncodingProvider);
                VerifyNormalizedEntries(
                    sourceZipFile,
                    sourceEntries,
                    normalizedZipArchiveReader.GetEntries(
                        SafetyProgress.CreateIncreasingProgress(
                            progress,
                            value => 0.50 + value * 0.05,
                            0.0,
                            1.0)),
                    SafetyProgress.CreateIncreasingProgress(
                        progress,
                        value => 0.55 + value * 0.45,
                        0.0,
                        1.0));
            }

            progress?.Report(1);
            return modified;
        }

        private static bool ExistModifiedEntries(IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry)> normalizedEntries)
            => normalizedEntries
                .None(item =>
                    item.sourceEntry is not null
                    && item.destinationFullName == item.sourceEntry.FullName
                    && normalizedEntries
                        .None(otherItem =>
                            otherItem.sourceEntry is not null
                            && (
                                otherItem.newOrder > item.newOrder && otherItem.sourceEntry.Order < item.sourceEntry.Order
                                || otherItem.newOrder < item.newOrder && otherItem.sourceEntry.Order > item.sourceEntry.Order)));

        private static void CreateNormalizedZipArchive(
            FileInfo destinationZipFile,
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            long sourceZipFileLength,
            ZipArchiveEntryCollection entries,
            IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry)> normalizedEntries,
            IProgress<double>? progress)
        {
            var success = false;
            try
            {
                using (var zipArchiveWriter = destinationZipFile.CreateAsZipFile(entryNameEncodingProvider))
                {
                    var currentProgressValue = 0.0;
                    foreach (var item in normalizedEntries)
                    {
                        var destinationEntry = zipArchiveWriter.CreateEntry(item.destinationFullName, item.sourceEntry?.Comment ?? "");
                        var sourceEntry = item.sourceEntry;
                        if (sourceEntry is not null)
                        {
                            var now = DateTime.UtcNow;
                            destinationEntry.IsFile = sourceEntry.IsFile;
                            destinationEntry.ExternalAttributes = sourceEntry.ExternalFileAttributes;
                            destinationEntry.LastWriteTimeUtc = sourceEntry.LastWriteTimeUtc ?? sourceEntry.CreationTimeUtc ?? now;
                            destinationEntry.LastAccessTimeUtc = sourceEntry.LastAccessTimeUtc ?? sourceEntry.LastWriteTimeUtc ?? sourceEntry.CreationTimeUtc ?? now;
                            destinationEntry.CreationTimeUtc = sourceEntry.CreationTimeUtc ?? sourceEntry.LastWriteTimeUtc ?? now;

                            if (sourceEntry.IsFile)
                            {
                                if (sourceEntry.Size > 0)
                                {
                                    destinationEntry.CompressionMethodId = ZipEntryCompressionMethodId.Deflate;
                                    destinationEntry.CompressionLevel = ZipEntryCompressionLevel.Maximum;
                                }
                                else
                                {
                                    destinationEntry.CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                                    destinationEntry.CompressionLevel = ZipEntryCompressionLevel.Normal;
                                }

                                using var destinationStream = destinationEntry.GetContentStream();
                                using var sourceStream = sourceEntry.GetContentStream();
                                sourceStream.CopyTo(
                                    destinationStream,
                                    SafetyProgress.CreateIncreasingProgress<ulong, double>(
                                        progress,
                                        value =>
                                            currentProgressValue
                                            + (
                                                sourceEntry.Size <= 0
                                                ? 0.0
                                                : (double)value / sourceEntry.Size * sourceEntry.PackedSize / sourceZipFileLength
                                            )));
                            }

                            currentProgressValue += (double)sourceEntry.PackedSize / sourceZipFileLength;
                        }

                        progress?.Report(currentProgressValue);
                    }
                }

                // タイムスタンプの設定

                var lastWriteTimeUtc =
                    entries
                    .Select(entry => entry.LastWriteTimeUtc)
                    .WhereNotNull()
                    .Append(DateTime.MinValue)
                    .Max();
                try
                {
                    if (lastWriteTimeUtc != DateTime.MinValue)
                        File.SetLastWriteTimeUtc(destinationZipFile.FullName, lastWriteTimeUtc);
                }
                catch (Exception)
                {
                }

                progress?.Report(1);

                success = true;
            }
            finally
            {
                if (!success)
                    destinationZipFile.SafetyDelete();
            }
        }

        private static void VerifyNormalizedEntries(
            FileInfo sourceZipFile,
            IEnumerable<ZipSourceEntry> sourceEntries,
            IEnumerable<ZipSourceEntry> normalizedEntries,
            IProgress<double>? progress)
        {
            var indexedSourceEntries =
                sourceEntries
                .Where(entry => entry.Size > 0)
                .GroupBy(entry => (entry.Size, entry.Crc))
                .ToDictionary(g => g.Key, g => g.ToList().AsReadOnly());

            var indexedNormalizedEntries =
                normalizedEntries
                .Where(entry => entry.Size > 0)
                .GroupBy(entry => (entry.Size, entry.Crc))
                .ToDictionary(g => g.Key, g => g.ToList().AsReadOnly());

            if (indexedNormalizedEntries.Count != indexedSourceEntries.Count)
                throw new Exception($"正規化に失敗しました。 (エントリの個数が異なっています): {sourceZipFile.FullName}");

            var totalSize = sourceEntries.Sum(entry => (double)entry.Size);
            var completedSize = 0UL;
            foreach (var key in indexedNormalizedEntries.Keys)
            {
                var normalizedEntriesGroup = indexedNormalizedEntries[key];
                if (!indexedSourceEntries.TryGetValue(key, out var sourceEntriesGroup))
                    throw new Exception($"正規化に失敗しました。 (CRCおよび長さが一致するエントリの個数が異なっています): {sourceZipFile.FullName}");
                if (normalizedEntriesGroup.Count != sourceEntriesGroup.Count)
                    throw new Exception($"正規化に失敗しました。 (CRCおよび長さが一致するエントリの個数が異なっています): {sourceZipFile.FullName}");
                if (sourceEntriesGroup.Count <= 0)
                    throw new InternalLogicalErrorException();
                if (sourceEntriesGroup.Count == 1)
                {
                    // CRC とサイズが一致するエントリの組み合わせが一組しかない場合

                    using var stream1 = sourceEntriesGroup.First().GetContentStream();
                    using var stream2 = normalizedEntriesGroup.First().GetContentStream();
                    var dataMatch =
                        stream1
                        .StreamBytesEqual(
                            stream2,
                            SafetyProgress.CreateIncreasingProgress<ulong, double>(
                                progress,
                                value => (completedSize + value) / totalSize));
                    if (!dataMatch)
                        throw new Exception($"正規化に失敗しました。 (データの内容が異なっています): {sourceZipFile.FullName}");
                }
                else
                {
                    // CRC とサイズが一致するエントリの組み合わせが二組以上ある場合

                    // 複数の組み合わせで一致するかどうか試行することになるので、データ比較中の進行状況は通知しない
                    var dataMatch =
                        normalizedEntriesGroup
                        .EnumeratePermutations()
                        .Select(entries1 => entries1.Zip(sourceEntriesGroup))
                        .Any(entryPairs =>
                            entryPairs
                            .All(entryPair =>
                            {
#if DEBUG && false
                                System.Diagnostics.Debug.WriteLine($"compare entries: entry1={entryPair.First}, entry2={entryPair.Second}");
#endif
                                using var stream1 = entryPair.First.GetContentStream();
                                using var stream2 = entryPair.Second.GetContentStream();
                                var result = stream1.StreamBytesEqual(stream2);
#if DEBUG && false
                                System.Diagnostics.Debug.WriteLine($"compare entries: result={(result ? "OK" : "NG")}");
#endif
                                return result;
                            }));
                    if (!dataMatch)
                        throw new Exception($"正規化に失敗しました。 (データの内容が異なっています): {sourceZipFile.FullName}");
                }

                // 比較が完了したエントリの合計サイズを終了済みサイズを加算する。
                //   ※ Sum() を使用しない理由は、要素の型が ulong の場合の Sum() のオーバーロードがないから。
                completedSize = sourceEntriesGroup.Aggregate(completedSize, (value, entry) => checked(value + entry.Size));

                progress?.Report(completedSize / totalSize);
            }

            progress?.Report(1);
        }

        private static void InitializeConsole()
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows の場合

                // 現在使用しているコンソールがエスケープコードを解釈しない場合、エスケープコードを解釈するように設定する。

                // コンソールのハンドルを取得する

                IntPtr consoleOutputHandle;
                if (!Console.IsOutputRedirected)
                {
                    // 標準出力がリダイレクトされていない場合は、標準出力がコンソールに紐づけられているとみなす
                    consoleOutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                    _consoleWriter = Console.Out;
                }
                else if (!Console.IsErrorRedirected)
                {
                    // 標準エラー出力がリダイレクトされていない場合は、標準エラー出力がコンソールに紐づけられているとみなす
                    consoleOutputHandle = GetStdHandle(STD_ERROR_HANDLE);
                    _consoleWriter = Console.Error;
                }
                else
                {
                    // その他の場合はコンソールに紐づけられているハンドル/ストリームはない。
                    consoleOutputHandle = INVALID_HANDLE_VALUE;
                    _consoleWriter = null;
                }

                if (consoleOutputHandle != INVALID_HANDLE_VALUE)
                {
                    // 標準出力と標準エラー出力の少なくともどちらかがコンソールに紐づけられている場合

                    // 現在のコンソールモード(フラグ)を取得する
                    if (!GetConsoleMode(consoleOutputHandle, out var mode))
                        throw new Exception("Failed to get console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    // コンソールモードに ENABLE_VIRTUAL_TERMINAL_PROCESSING がセットされていないのならセットする
                    if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
                    {
                        if (!SetConsoleMode(consoleOutputHandle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING))
                            throw new Exception("Failed to set console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                    }
                }
            }
        }

        private static void ReportProgress(double progressRate, FileInfo? zipFile = null)
        {
            if (_consoleWriter is not null)
            {
                var percentage = progressRate * 100.0;
                var progressMessage =
                    zipFile is not null
                    ? $"{percentage:##0.00}%: \"{zipFile.FullName}\""
                    : $"{percentage:##0.00}%";

                if (_currentProgressMessage != progressMessage)
                {
                    var (cursorLeft, cursorTop) = Console.GetCursorPosition();
                    _consoleWriter.Write($"  {progressMessage}\x1b[0J\x1b[{cursorTop + 1};{cursorLeft + 1}H");
                    _currentProgressMessage = progressMessage;
                }
            }
        }

        private static void ReportException(Exception exception, int indent = 0)
        {
            ReportErrorMessage(exception.Message, indent);
            if (exception.InnerException is not null)
                ReportException(exception.InnerException, indent + 2);
            if (exception is AggregateException aggregateException)
            {
                foreach (var ex in aggregateException.InnerExceptions)
                    ReportException(ex, indent + 2);
            }
        }

        private static void ReportErrorMessage(string message, int indent = 0)
        {
            try
            {
#if DEBUG
                var (_, cursorTop1) = Console.GetCursorPosition();
#endif
                _consoleWriter?.Write($"\x1b[0J");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine($"{new string(' ', indent)}{_thisProgramName}:ERROR:{message}");
#if DEBUG
                var (_, cursorTop2) = Console.GetCursorPosition();
                if (cursorTop2 <= cursorTop1)
                    throw new Exception();
#endif
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
