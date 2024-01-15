using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Compression.Archive.Zip;
using Palmtree.IO.Compression.Stream.Plugin.SevenZip;
using Palmtree.IO.Console;
using Palmtree.Linq;

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

        private static readonly string _thisProgramName;
        private static string _currentProgressMessage;

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _currentProgressMessage = "";
            Bzip2CoderPlugin.EnablePlugin();
            DeflateCoderPlugin.EnablePlugin();
            Deflate64CoderPlugin.EnablePlugin();
            LzmaCoderPlugin.EnablePlugin();
        }

        private static int Main(string[] args)
        {
            // TODO: 指定した拡張子のファイルの除去をする機能を追加。
            // TODO: 特定の長さとCRCのファイルを除去する機能を追加。
            var optionInteractive = false;
            var mode = CommandMode.ListZipEntries;
            var allowedEncodngNames = new List<string>();
            var excludedEncodngNames = new List<string>();
            try
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
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

                    var encodingProvider = ZipEntryNameEncodingProvider.CreateInstance(allowedEncodngNames.ToArray(), excludedEncodngNames.ToArray(), "##");

                    if (mode == CommandMode.ShowCodePages)
                    {
                        var encodingList = encodingProvider.SupportedEncodings.ToList();
                        var maximumEncodingNameLength = encodingList.Select(encoding => encoding.WebName.Length).Append(0).Max();
                        foreach (var encoding in encodingList)
                            TinyConsole.WriteLine($"Name: {encoding.WebName}\", {new string(' ', maximumEncodingNameLength - encoding.WebName.Length)}Description: {encoding.EncodingName}");
                        if (optionInteractive)
                        {
                            TinyConsole.Beep();
                            _ = TinyConsole.ReadLine();
                        }

                        return 0;
                    }

                    if (encodingProvider.SupportedEncodings.None())
                        throw new Exception($"エントリ名およびコメントに適用できるコードページがありません。オプションでコードページを指定している場合は指定内容を確認してください。サポートされているコードページのリストは \"--show_code_page_list\" オプションを指定して起動することにより確認できます。");

                    var trashBox = TrashBox.OpenTrashBox();

                    var zipFiles = EnumerateZipFiles(args.Where(arg => !arg.StartsWith('-'))).ToList();

                    if (mode == CommandMode.Normalize)
                    {
                        var totalSize = zipFiles.Aggregate(0UL, (value, file) => checked(value + file.Length));
                        var completedRate = 0.0;

                        ReportProgress(completedRate);

                        foreach (var zipFile in zipFiles)
                        {
                            ReportProgress(completedRate, zipFile);

                            var parentDirectory = zipFile.Directory ?? throw new Exception();
                            var originalZipFileSize = zipFile.Length;
                            var temporaryFileName = $".{zipFile.Name}.0.zip";
                            var temporaryFile = parentDirectory.GetFile(temporaryFileName);
                            if (temporaryFile.Exists)
                            {
                                for (var index = 1; ; ++index)
                                {
                                    temporaryFileName = $".{zipFile.Name}.{index}.zip";
                                    temporaryFile = parentDirectory.GetFile(temporaryFileName);
                                    if (!temporaryFile.Exists)
                                        break;
                                }
                            }

                            try
                            {
                                var progressRateValue =
                                    new ProgressValueHolder<double>(
                                        value => ReportProgress(value, zipFile),
                                        completedRate,
                                        TimeSpan.FromMilliseconds(100));
                                if (NormalizeZipFile(
                                    zipFile,
                                    temporaryFile,
                                    encodingProvider,
                                    new SimpleProgress<double>(value => progressRateValue.Value = completedRate + value * originalZipFileSize / totalSize)))
                                {
                                    if (!trashBox.DisposeFile(zipFile))
                                        throw new Exception($"ファイルのごみ箱への移動に失敗しました。: \"{zipFile.FullName}\"");

                                    temporaryFile.MoveTo(zipFile, false);
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
                            TinyConsole.WriteLine($"file: {zipFile.FullName}");
                            ListZipFile(zipFile, encodingProvider);
                            TinyConsole.WriteLine(new string('-', 20));
                        }
                    }
                }
                finally
                {
                    TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                }

                TinyConsole.WriteLine();
                TinyConsole.WriteLine("終了しました。");
                if (optionInteractive)
                {
                    TinyConsole.Beep();
                    _ = TinyConsole.ReadLine();
                }

                return 0;
            }
            catch (Exception ex)
            {
                TinyConsole.WriteLine();
                ReportException(ex);
                if (optionInteractive)
                {
                    TinyConsole.Beep();
                    _ = TinyConsole.ReadLine();
                }

                return 1;
            }
        }

        private static IEnumerable<FilePath> EnumerateZipFiles(IEnumerable<string> args)
        {
            return
                args.EnumerateFilesFromArgument(true)
                    .Where(file => string.Equals(file.Extension, ".zip", StringComparison.OrdinalIgnoreCase) && CheckIfValidFilePath(file));

            static bool CheckIfValidFilePath(FilePath file)
            {
                return
                    !file.Name.StartsWith('.')
                    && CheckIfValidDirectoryPath(file.Directory);
            }

            static bool CheckIfValidDirectoryPath(DirectoryPath directory)
            {
                for (var dir = directory; dir is not null; dir = dir.Parent)
                {
                    if (dir.Name.StartsWith('.'))
                        return false;
                }

                return true;
            }
        }

        private static void ListZipFile(FilePath sourceZipFile, IZipEntryNameEncodingProvider entryNameEncodingProvider, IProgress<double>? progress = null)
        {
            try
            {
                using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
                foreach (var entry in sourceArchiveReader.EnumerateEntries(progress))
                {
                    var localExtraIdsText = string.Join(",", entry.LocalHeaderExtraFields.EnumerateExtraFieldIds().Select(id => $"0x{id:x4}"));
                    var centralExtraIdsText = string.Join(",", entry.CentralDirectoryHeaderExtraFields.EnumerateExtraFieldIds().Select(id => $"0x{id:x4}"));
                    var exactEncodingText = entry.ExactEntryEncoding?.WebName ?? "???";
                    var possibleEncodings = entry.PossibleEntryEncodings.Select(encoding => encoding.WebName).Take(3).ToList();
                    var possibleEncodingsText = $"{string.Join(",", possibleEncodings)}{(possibleEncodings.Count > 2 ? ", ..." : "")}";
                    TinyConsole.WriteLine($"  {entry.FullName}, local:[{localExtraIdsText}], central:[{centralExtraIdsText}], exact={exactEncodingText}, possible=[{possibleEncodingsText}], size={entry.Size:N0}, packedSize={entry.PackedSize:N0}, compress={entry.CompressionMethodId}");
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

        private static bool NormalizeZipFile(FilePath sourceZipFile, FilePath destinationZipFile, IZipEntryNameEncodingProvider entryNameEncodingProvider, IProgress<double> progress)
        {
            try
            {
                ValidateIfWritableFile(sourceZipFile);
                using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
                var sourceZipFileLength = sourceZipFile.Length;
                var rootNode = PathNode.CreateRootNode();
                var badEntries = sourceArchiveReader.EnumerateEntries().Where(entry => entry.FullName.IsUnknownEncodingText()).ToList();
                if (badEntries.Count > 0)
                    throw new Exception($".NETで認識できない名前のエントリがZIPファイルに含まれています。: ZIP file name:\"{sourceZipFile.FullName}\", Entry name: \"{badEntries.First().FullName}\"");

                var progressValue = new ProgressValueHolder<double>(progress, 0.0, TimeSpan.FromMilliseconds(100));
                progressValue.Report();
                var sourceEntries =
                    sourceArchiveReader.EnumerateEntries(
                        new SimpleProgress<double>(value => progressValue.Value = value * 0.05));
                progressValue.Report();
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
                        lastWriteTime = node.LastWriteTime,
                        lastAccessTime = node.LastAccessTime,
                        creationTime = node.CreationTime,
                    })
                    .OrderBy(item => item.destinationFullName, StringComparer.OrdinalIgnoreCase)
                    .Select((item, newOrder) => (item.destinationFullName, item.isDirectory, item.sourceFullName, newOrder, item.sourceEntry, item.lastWriteTime, item.lastAccessTime, item.creationTime))
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
                        normalizedEntries,
                        new SimpleProgress<double>(value => progressValue.Value = 0.05 + value * 0.45));

                    progress?.Report(0.5);

                    // 元のZIPアーカイブと正規化されたZIPアーカイブの内容が一致しているかどうかを調べる
                    using var normalizedZipArchiveReader = destinationZipFile.OpenAsZipFile(entryNameEncodingProvider);
                    VerifyNormalizedEntries(
                        sourceZipFile,
                        sourceEntries,
                        normalizedZipArchiveReader.EnumerateEntries(
                            new SimpleProgress<double>(value => progressValue.Value = 0.50 + value * 0.05)),
                        new Progress<double>(value => progressValue.Value = 0.55 + value * 0.45));
                }

                progress?.Report(1);
                return modified;
            }
            catch (CompressionMethodNotSupportedException ex)
            {
                throw new Exception($"ZIPアーカイブがサポートされていない圧縮方式で圧縮されているため正規化できません。: method={ex.CompresssionMethodId}, path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (BadZipFileFormatException ex)
            {
                throw new Exception($"ZIPアーカイブが破損しているため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (EncryptedZipFileNotSupportedException ex)
            {
                throw new Exception($"ZIPアーカイブが暗号化されているため正規化できません。: required=\"{ex.Required}\", path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (MultiVolumeDetectedException ex)
            {
                throw new Exception($"ZIPアーカイブがマルチボリュームであるため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (NotSupportedSpecificationException ex)
            {
                throw new Exception($"ZIPアーカイブを解凍するための機能が不足しているため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
        }

        private static void ValidateIfWritableFile(FilePath sourceZipFile)
        {
            try
            {
                using var outStream = sourceZipFile.OpenWrite();
            }
            catch (Exception ex)
            {
                throw new Exception($"ZIPアーカイブへの書き込みができません。: \"{sourceZipFile.FullName}\"", ex);
            }
        }

        private static bool ExistModifiedEntries(IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry, DateTime? lastWriteTime, DateTime? lastAccessTime, DateTime? creationTime)> normalizedEntries)
            => normalizedEntries
                .Any(item =>
                    item.sourceEntry is null
                    || item.destinationFullName != item.sourceEntry.FullName
                    || item.sourceEntry.LastWriteTimeUtc != item.lastWriteTime
                    || item.sourceEntry.LastAccessTimeUtc != item.lastAccessTime
                    || item.sourceEntry.CreationTimeUtc != item.creationTime
                    || normalizedEntries
                        .Any(otherItem =>
                            otherItem.sourceEntry is not null
                            && (
                                otherItem.newOrder > item.newOrder && otherItem.sourceEntry.LocationOrder < item.sourceEntry.LocationOrder
                                || otherItem.newOrder < item.newOrder && otherItem.sourceEntry.LocationOrder > item.sourceEntry.LocationOrder)));

        private static void CreateNormalizedZipArchive(
            FilePath destinationZipFile,
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            ulong sourceZipFileLength,
            IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry, DateTime? lastWriteTime, DateTime? lastAccessTime, DateTime? creationTime)> normalizedEntries,
            IProgress<double>? progress)
        {
            var success = false;
            try
            {
                using (var zipArchiveWriter = destinationZipFile.CreateAsZipFile(entryNameEncodingProvider))
                {
                    var currentProgressValue = 0.0;
                    var progressValue = new ProgressValueHolder<double>(progress, currentProgressValue, TimeSpan.FromMilliseconds(100));
                    progressValue.Report();
                    foreach (var item in normalizedEntries)
                    {
                        var destinationEntry = zipArchiveWriter.CreateEntry(item.destinationFullName, item.sourceEntry?.Comment ?? "");
                        var sourceEntry = item.sourceEntry;
                        if (sourceEntry is not null)
                        {
                            var now = DateTime.UtcNow;
                            destinationEntry.IsFile = sourceEntry.IsFile;
                            destinationEntry.ExternalAttributes = sourceEntry.ExternalFileAttributes;
                            destinationEntry.LastWriteTimeUtc = item.lastWriteTime ?? item.creationTime ?? now;
                            destinationEntry.LastAccessTimeUtc = item.lastAccessTime ?? item.lastWriteTime ?? item.creationTime ?? now;
                            destinationEntry.CreationTimeUtc = item.creationTime ?? item.lastWriteTime ?? now;
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

                                using var destinationStream = destinationEntry.CreateContentStream();
                                using var sourceStream = sourceEntry.OpenContentStream();
                                sourceStream.CopyTo(
                                    destinationStream,
                                    new SimpleProgress<ulong>(
                                        value =>
                                            progressValue.Value = currentProgressValue
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
                    normalizedEntries
                    .Select(entry => entry.lastWriteTime)
                    .Append((DateTime?)new DateTime(0, DateTimeKind.Utc))
                    .WhereNotNull()
                    .Max();
                try
                {
                    if (lastWriteTimeUtc.Ticks > 0)
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
            FilePath sourceZipFile,
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
                Validation.Assert(sourceEntriesGroup.Count > 0, "sourceEntriesGroup.Count > 0");
                if (sourceEntriesGroup.Count == 1)
                {
                    // CRC とサイズが一致するエントリの組み合わせが一組しかない場合

                    var progressValue = new ProgressValueHolder<double>(progress, 0, TimeSpan.FromMilliseconds(100));
                    using var stream1 = sourceEntriesGroup.First().OpenContentStream();
                    using var stream2 = normalizedEntriesGroup.First().OpenContentStream();
                    var dataMatch =
                        stream1
                        .StreamBytesEqual(
                            stream2,
                            new SimpleProgress<ulong>(value => progressValue.Value = (completedSize + value) / totalSize));
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
                                using var stream1 = entryPair.First.OpenContentStream();
                                using var stream2 = entryPair.Second.OpenContentStream();
                                var result = stream1.StreamBytesEqual(stream2);
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

        private static void ReportProgress(double progressRate, FilePath? zipFile = null)
        {
            var percentage = progressRate * 100.0;
            var progressMessage =
                zipFile is not null
                ? $"{percentage:##0.00}%: \"{zipFile.FullName}\""
                : $"{percentage:##0.00}%";

            if (_currentProgressMessage != progressMessage)
            {
                // progressMessageを表示するために予想し得る最大の表示桁数を計算する。
                var maximumToralColumns = progressMessage.Length * 2;

                // progressMessageを表示するために予想し得る最大の行数を計算する。
                var rows = (maximumToralColumns + TinyConsole.WindowWidth - 1) / TinyConsole.WindowWidth;

                // rows 行だけ改行して、rows 行だけカーソルを上に移動する。
                TinyConsole.Write($"{new string('\n', rows)}");
                TinyConsole.CursorUp(rows);

                // これ以降、progressMessage を表示してもスクロールは発生しないはず。

                // 現在のカーソル位置を取得する。
                var (cursorLeft, cursorTop) = TinyConsole.GetCursorPosition();

                // メッセージを表示して、その後の文字列を消去し、カーソル位置を元に戻す。
                TinyConsole.Write($"  {progressMessage}");
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfScreen);
                TinyConsole.SetCursorPosition(cursorLeft, cursorTop);

                _currentProgressMessage = progressMessage;
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
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfScreen);
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.BackgroundColor = ConsoleColor.Black;
                TinyConsole.Error.WriteLine($"{new string(' ', indent)}{_thisProgramName}:ERROR:{message}");
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }
    }
}
