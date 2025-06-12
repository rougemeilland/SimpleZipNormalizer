using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Palmtree;
using Palmtree.Application;
using Palmtree.IO;
using Palmtree.IO.Compression.Archive.Zip;
using Palmtree.IO.Compression.Stream.Plugin.SevenZip;
using Palmtree.IO.Console;
using Palmtree.Linq;
using SimpleZipNormalizer.CUI.Models;

namespace SimpleZipNormalizer.CUI
{
    public partial class NormalizerApplication
        : BatchApplication
    {
        private enum CommandMode
        {
            Normalize = 0,
            ShowCodePages,
            ListZipEntries,
        }

        private const string _defaultSettingsJsonUrl = "https://raw.githubusercontent.com/rougemeilland/SimpleZipNormalizer/main/content/zipnorm.settings.json";
        private const string _settingsFileName = "zipnorm.settings.json";

        private static readonly FilePath _settingsFile;

        private readonly string? _title;
        private readonly Encoding? _encoding;

        static NormalizerApplication()
        {
            var homeDirectory =
                DirectoryPath.UserHomeDirectory
                ?? throw new NotSupportedException();
            _settingsFile =
                homeDirectory
                .GetSubDirectory(".palmtree").Create()
                .GetFile(_settingsFileName);
            Bzip2CoderPlugin.EnablePlugin();
            DeflateCoderPlugin.EnablePlugin();
            Deflate64CoderPlugin.EnablePlugin();
            LzmaCoderPlugin.EnablePlugin();
        }

        public NormalizerApplication(string? title, Encoding? encoding)
        {
            _title = title;
            _encoding = encoding;
        }

        protected override string ConsoleWindowTitle => _title ?? base.ConsoleWindowTitle;
        protected override Encoding? InputOutputEncoding => _encoding;

        protected override ResultCode Main(string[] args)
        {
            var settings = ReadSettings();

            var mode = CommandMode.ListZipEntries;
            var allowedEncodngNames = new List<string>();
            var excludedEncodngNames = new List<string>();
            var warnedFilePatterns =
                (settings?.WarnedFilePatterns ?? [])
                .Select(patternText => new Regex(patternText, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToList();
#if DEBUG
            Validation.Assert(warnedFilePatterns[0].IsMatch("000.bmp"));
#endif

            var excludedFilePatterns =
                (settings?.ExcludedFilePatterns ?? [])
                .Select(patternText => new Regex(patternText, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToList();
            var blackList =
                (settings?.BlackList ?? [])
                .Select(element =>
                {
                    var match = GetBlackListParameterPattern().Match(element);
                    if (!match.Success)
                    {
                        ReportWarningMessage($"\"{_settingsFile.FullName}\" ファイルの内容に誤りがあります。\"black_list\" のプロパティの要素 \"{element}\" の形式に誤りがあります。");
                        return ((ulong length, uint crc)?)null;
                    }
                    else
                    {
                        var length = ulong.Parse(match.Groups["length"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                        var crc = uint.Parse(match.Groups["crc"].Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat);
                        return (length, crc);
                    }
                })
                .WhereNotNull()
                .ToHashSet();

            try
            {
                for (var index = 0; index < args.Length; ++index)
                {
                    var arg = args[index];
                    if (arg is "-n" or "--normalize")
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
                    else if ((arg == "-ci" || arg == "--allowed_code_page") && index + 1 < args.Length)
                    {
                        allowedEncodngNames.AddRange(args[index + 1].Split(','));
                        ++index;
                    }
                    else if ((arg == "-ce" || arg == "--excluded_code_page") && index + 1 < args.Length)
                    {
                        excludedEncodngNames.AddRange(args[index + 1].Split(','));
                        ++index;
                    }
                    else if ((arg == "-wf" || arg == "--warned_file") && index + 1 < args.Length)
                    {
                        try
                        {
                            warnedFilePatterns.Add(new Regex(args[index + 1], RegexOptions.Compiled));
                            ++index;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"--warned_file オプションで与えられた正規表現に誤りがあります。", ex);
                        }
                    }
                    else if ((arg == "-ef" || arg == "--excluded_file") && index + 1 < args.Length)
                    {
                        try
                        {
                            excludedFilePatterns.Add(new Regex(args[index + 1], RegexOptions.Compiled));
                            ++index;
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"--excluded_file オプションで与えられた正規表現に誤りがあります。", ex);
                        }
                    }
                    else if ((arg == "-bl" || arg == "--black_list") && index + 1 < args.Length)
                    {
                        foreach (var element in args[index + 1].Split(','))
                        {
                            var match = GetBlackListParameterPattern().Match(element);
                            if (!match.Success)
                                throw new ApplicationException($"--black_list オプションの形式に誤りがあります。");
                            var length = ulong.Parse(match.Groups["length"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                            var crc = uint.Parse(match.Groups["crc"].Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat);
                            _ = blackList.Add((length, crc));
                        }

                        ++index;
                    }
                    else if (arg.StartsWith('-'))
                    {
                        throw new ApplicationException($"サポートされていないオプションがコマンド引数に指定されました。: \"{arg}\"");
                    }
                    else
                    {
                        // OK
                    }
                }

                var encodingProvider = ZipEntryNameEncodingProvider.CreateInstance(allowedEncodngNames.ToArray(), excludedEncodngNames.ToArray(), "##");

                if (mode == CommandMode.ShowCodePages)
                {
                    var encodingList = encodingProvider.SupportedEncodings.ToList();
                    var maximumEncodingNameLength = encodingList.Select(encoding => encoding.WebName.Length).Append(0).Max();
                    foreach (var encoding in encodingList)
                        TinyConsole.WriteLine($"Name: {encoding.WebName}\", {new string(' ', maximumEncodingNameLength - encoding.WebName.Length)}Description: {encoding.EncodingName}");
                    return ResultCode.Success;
                }

                if (encodingProvider.SupportedEncodings.None())
                    throw new ApplicationException($"エントリ名およびコメントに適用できるコードページがありません。オプションでコードページを指定している場合は指定内容を確認してください。サポートされているコードページのリストは \"--show_code_page_list\" オプションを指定して起動することにより確認できます。");

                var trashBox = TrashBox.OpenTrashBox();

                ReportProgress("Searching files...");

                var zipFiles = EnumerateZipFiles(args.Where(arg => !arg.StartsWith('-'))).ToList();

                if (mode == CommandMode.Normalize)
                {
                    var totalSize = zipFiles.Aggregate(0UL, (value, file) => checked(value + file.Length));
                    var completedRate = 0.0;

                    ReportProgress(completedRate);

                    foreach (var zipFile in zipFiles)
                    {
                        if (IsPressedBreak)
                            return ResultCode.Cancelled;

                        ReportProgress(completedRate, zipFile.FullName, (progressRate, content) => $"{progressRate} processing \"{content}\"");

                        var originalZipFileSize = zipFile.Length;
                        var temporaryFile = GetTemporaryFile(zipFile);

                        try
                        {
                            var progressRateValue =
                                new ProgressValueHolder<double>(
                                    value => ReportProgress(value, zipFile.FullName, (progressRate, content) => $"{progressRate} processing \"{content}\""),
                                    completedRate,
                                    TimeSpan.FromMilliseconds(100));
                            if (NormalizeZipFile(
                                zipFile,
                                temporaryFile,
                                encodingProvider,
                                entry =>
                                    !string.Equals(zipFile.Extension, ".epub", StringComparison.OrdinalIgnoreCase)
                                    && (excludedFilePatterns.Any(pattern => pattern.IsMatch(Path.GetFileName(entry.FullName)))
                                        || blackList.Contains((entry.Size, entry.Crc))),
                                entry =>
                                    !string.Equals(zipFile.Extension, ".epub", StringComparison.OrdinalIgnoreCase)
                                    && warnedFilePatterns.Any(pattern => pattern.IsMatch(Path.GetFileName(entry.FullName))),
                                new SimpleProgress<double>(value => progressRateValue.Value = completedRate + value * originalZipFileSize / totalSize)))
                            {
                                CleanUp(zipFile, GetBackupFile(zipFile), temporaryFile, trashBox);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            return ResultCode.Cancelled;
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

                    return ResultCode.Success;
                }
                else
                {
                    foreach (var zipFile in zipFiles)
                    {
                        if (IsPressedBreak)
                            return ResultCode.Cancelled;

                        TinyConsole.WriteLine($"file: {zipFile.FullName}");
                        try
                        {
                            ListZipFile(zipFile, encodingProvider);
                        }
                        catch (OperationCanceledException)
                        {
                            return ResultCode.Cancelled;
                        }

                        TinyConsole.WriteLine(new string('-', 20));
                    }

                    return ResultCode.Success;
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return ResultCode.Failed;
            }

            static void CleanUp(FilePath zipFile, FilePath backupFile, FilePath normalizedFile, ITrashBox trashBox)
            {
                // オリジナルの ".zip" ファイルを ".zip.bak" に変名する。
                try
                {
                    zipFile.MoveTo(backupFile);
                }
                catch (Exception ex)
                {
                    // 変名に失敗した場合はそのまま例外を通知する。
                    throw new ApplicationException($"ファイルの変名に失敗しました。: \"{zipFile.FullName}\" => \"{backupFile.Name}\"", ex);
                }

                // 正規化されたファイルをオリジナルの ".zip" ファイルの名前に変名する。
                try
                {
                    normalizedFile.MoveTo(zipFile);
                }
                catch (Exception ex)
                {
                    // 変名に失敗した場合、復旧のために、 ".zip.bak" ファイルの ".zip" への変名を試みる。
                    try
                    {
                        backupFile.MoveTo(zipFile);
                    }
                    catch (Exception ex2)
                    {
                        // 変名に失敗した場合、もうどうしようもないので、そのまま例外を通知する。
                        // この場合、オリジナルの ".zip" ファイルは残らず、".zip.bak" ファイルが残る。
                        throw new ApplicationException($"ファイルの変名に失敗しました。: \"{backupFile.FullName}\" => \"{zipFile.Name}\"", ex2);
                    }

                    // 復旧に成功した場合、変名に失敗したことを例外で通知する。
                    throw new ApplicationException($"ファイルの変名に失敗しました。: \"{normalizedFile.FullName}\" => \"{zipFile.Name}\"", ex);
                }

                // 不要になった ".zip.bak" ファイルをごみ箱へ移動する。
                if (!trashBox.DisposeFile(backupFile))
                    throw new ApplicationException($"ファイルのごみ箱への移動に失敗しました。: \"{backupFile.FullName}\"");
            }
        }

        protected override void Finish(ResultCode result, bool isLaunchedByConsoleApplicationLauncher)
        {
            if (result == ResultCode.Success)
                TinyConsole.WriteLine("終了しました。");
            else if (result == ResultCode.Cancelled)
                TinyConsole.WriteLine("中断されました。");

            if (isLaunchedByConsoleApplicationLauncher)
            {
                TinyConsole.Beep();
                TinyConsole.WriteLine("ENTER キーを押すとウィンドウが閉じます。");
                _ = TinyConsole.ReadLine();
            }
        }

        private static SettingsModel? ReadSettings()
        {
            if (!_settingsFile.Exists || _settingsFile.Length <= 0)
            {
                using var client = new HttpClient();
                using var inStream = client.GetStreamAsync(_defaultSettingsJsonUrl).Result.AsInputByteStream();
                using var outStream = _settingsFile.Create();
                inStream.CopyTo(outStream);
            }

            if (!_settingsFile.Exists || _settingsFile.Length == 0)
                return null;

            var jsonText = Encoding.UTF8.GetString(_settingsFile.ReadAllBytes());
            return JsonSerializer.Deserialize(jsonText, SettingsModelSourceGenerationContext.Default.SettingsModel);
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

        private void ListZipFile(FilePath sourceZipFile, IZipEntryNameEncodingProvider entryNameEncodingProvider, IProgress<double>? progress = null)
        {
            try
            {
                using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
                foreach (var entry in sourceArchiveReader.EnumerateEntries(progress))
                {
                    if (IsPressedBreak)
                        throw new OperationCanceledException();

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

        private static FilePath GetTemporaryFile(FilePath originalFile)
        {
            var parentDirectory = originalFile.Directory;
            for (var count = 0; ; ++count)
            {
                var temporaryFileName = $".{originalFile.Name}.{count}.zip";
                var temporaryFile = parentDirectory.GetFile(temporaryFileName);
                if (!temporaryFile.Exists)
                    return temporaryFile;
            }
        }

        private static FilePath GetBackupFile(FilePath originalFile)
        {
            var parentDirectory = originalFile.Directory;
            for (var count = 0; ; ++count)
            {
                var temporaryFileName = $"{originalFile.Name}{(count <= 0 ? ".zip.bak" : $".zip ({count + 1}).bak")}";
                var temporaryFile = parentDirectory.GetFile(temporaryFileName);
                if (!temporaryFile.Exists)
                    return temporaryFile;
            }
        }

        private bool NormalizeZipFile(
            FilePath sourceZipFile,
            FilePath destinationZipFile,
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            Func<ZipSourceEntry, bool> excludedFileChecker,
            Func<ZipSourceEntry, bool> warnedFileChecker,
            SimpleProgress<double> progress)
        {
            try
            {
                if (IsPressedBreak)
                    throw new OperationCanceledException();

                ValidateIfWritableFile(sourceZipFile);
                using var sourceArchiveReader = sourceZipFile.OpenAsZipFile(entryNameEncodingProvider);
                var sourceZipFileLength = sourceZipFile.Length;
                var rootNode = PathNode.CreateRootNode();
                var badEntries = sourceArchiveReader.EnumerateEntries().Where(entry => entry.FullName.IsUnknownEncodingText()).ToList();
                if (badEntries.Count > 0)
                    throw new ApplicationException($".NETで認識できない名前のエントリがZIPファイルに含まれています。: ZIP file name:\"{sourceZipFile.FullName}\", Entry name: \"{badEntries.First().FullName}\"");

                var progressValue = new ProgressValueHolder<double>(progress, 0.0, TimeSpan.FromMilliseconds(100));
                progressValue.Report();
                var sourceEntries =
                    sourceArchiveReader.EnumerateEntries(
                        new SimpleProgress<double>(value => progressValue.Value = value * 0.05))
                    .ToList();

                if (IsPressedBreak)
                    throw new OperationCanceledException();

                var trimmedSourceEntries =
                    sourceEntries
                    .Where(entry =>
                    {
                        if (GetArchivePathNamePattern().IsMatch(entry.FullName))
                            ReportWarningMessage($"書庫ファイルが含まれています。: \"{sourceZipFile.FullName}\"/\"{entry.FullName}\"");
                        var excluded = excludedFileChecker(entry);
                        if (excluded)
                            ReportWarningMessage($"削除対象のファイルが見つかったので削除します。: \"{sourceZipFile.FullName}\"/\"{entry.FullName}\"");
                        return !excluded;
                    })
                    .ToList();
                progressValue.Report();
                foreach (var entry in trimmedSourceEntries)
                    rootNode.AddChildNode(entry.FullName, entry);

                // ノードのディレクトリ構成を正規化する (空ディレクトリの削除、無駄なディレクトリ階層の短縮)
                rootNode.Normalize();

                // 正規化されたノードを列挙する
                var normalizedEntries =
                    rootNode.EnumerateTerminalNodes()
                    .Select(node =>
                    {
                        var sourceEntry = node.SourceEntry;
                        if (sourceEntry is not null && warnedFileChecker(sourceEntry))
                            ReportWarningMessage($"\"{sourceZipFile.FullName}/{node.CurrentFullName}\" は適切なファイルではありません。");
                        return new
                        {
                            destinationFullName = node.CurrentFullName,
                            isDirectory = node is DirectoryPathNode,
                            sourceFullName = node.SourceFullName,
                            sourceEntry,
                            lastWriteTimeOffset = node.LastWriteTimeOffset,
                            lastAccessTimeOffset = node.LastAccessTimeOffset,
                            creationTimeOffset = node.CreationTimeOffset,
                        };
                    })
                    .OrderBy(item => item.destinationFullName, StringComparer.OrdinalIgnoreCase)
                    .Select((item, newOrder) => (item.destinationFullName, item.isDirectory, item.sourceFullName, newOrder, item.sourceEntry, item.lastWriteTimeOffset, item.lastAccessTimeOffset, item.creationTimeOffset))
                    .ToList();

                // 正規化前後でエントリが変更する見込みがあるかどうかを調べる
                var needToModify =
                    sourceEntries.Count != trimmedSourceEntries.Count
                    || ExistEntriesToNeedToModify(normalizedEntries);
                if (needToModify)
                {
                    // 正規化の前後でパス名および順序が一致しないエントリが一つでもある場合

                    // ZIPアーカイブの正規化を実行する
                    CreateNormalizedZipArchive(
                        sourceZipFile,
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
                        trimmedSourceEntries,
                        normalizedZipArchiveReader.EnumerateEntries(
                            new SimpleProgress<double>(value => progressValue.Value = 0.50 + value * 0.05)),
                        new Progress<double>(value => progressValue.Value = 0.55 + value * 0.45));
                }

                progress?.Report(1);
                return needToModify;
            }
            catch (CompressionMethodNotSupportedException ex)
            {
                throw new ApplicationException($"ZIPアーカイブがサポートされていない圧縮方式で圧縮されているため正規化できません。: method={ex.CompresssionMethodId}, path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (BadZipFileFormatException ex)
            {
                throw new ApplicationException($"ZIPアーカイブが破損しているため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (EncryptedZipFileNotSupportedException ex)
            {
                throw new ApplicationException($"ZIPアーカイブが暗号化されているため正規化できません。: required=\"{ex.Required}\", path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (MultiVolumeDetectedException ex)
            {
                throw new ApplicationException($"ZIPアーカイブがマルチボリュームであるため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (NotSupportedSpecificationException ex)
            {
                throw new ApplicationException($"ZIPアーカイブを解凍するための機能が不足しているため正規化できません。: path=\"{sourceZipFile.FullName}\"", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"正規化に失敗しました。: path=\"{sourceZipFile.FullName}\"", ex);
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
                throw new ApplicationException($"ZIPアーカイブへの書き込みができません。: \"{sourceZipFile.FullName}\"", ex);
            }
        }

        private static bool ExistEntriesToNeedToModify(IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry, DateTimeOffset? lastWriteTimeOffset, DateTimeOffset? lastAccessTimeOffset, DateTimeOffset? creationTimeOffset)> normalizedEntries)
            => normalizedEntries
                .Any(item =>
                    item.sourceEntry is null
                    || item.destinationFullName != item.sourceEntry.FullName
                    || item.sourceEntry.LastWriteTimeOffsetUtc != item.lastWriteTimeOffset
                    || item.sourceEntry.LastAccessTimeOffsetUtc != item.lastAccessTimeOffset
                    || item.sourceEntry.CreationTimeOffsetUtc != item.creationTimeOffset
                    || normalizedEntries
                        .Any(otherItem =>
                            otherItem.sourceEntry is not null
                            && (
                                otherItem.newOrder > item.newOrder && otherItem.sourceEntry.LocationOrder < item.sourceEntry.LocationOrder
                                || otherItem.newOrder < item.newOrder && otherItem.sourceEntry.LocationOrder > item.sourceEntry.LocationOrder)));

        private void CreateNormalizedZipArchive(
            FilePath sourceZipFile,
            FilePath destinationZipFile,
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            ulong sourceZipFileLength,
            IEnumerable<(string destinationFullName, bool isDirectory, string sourceFullName, int newOrder, ZipSourceEntry? sourceEntry, DateTimeOffset? lastWriteTimeOffset, DateTimeOffset? lastAccessTimeOffset, DateTimeOffset? creationTimeOffset)> normalizedEntries,
            SimpleProgress<double>? progress)
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
                        if (IsPressedBreak)
                            throw new OperationCanceledException();

                        try
                        {
                            var destinationEntry = zipArchiveWriter.CreateEntry(item.destinationFullName, item.sourceEntry?.Comment ?? "");
                            var sourceEntry = item.sourceEntry;
                            if (sourceEntry is not null)
                            {
                                var now = DateTimeOffset.UtcNow;
                                destinationEntry.IsFile = sourceEntry.IsFile;
                                destinationEntry.ExternalAttributes = sourceEntry.ExternalFileAttributes;
                                destinationEntry.LastWriteTimeOffsetUtc = item.lastWriteTimeOffset ?? item.creationTimeOffset ?? now;
                                destinationEntry.LastAccessTimeOffsetUtc = item.lastAccessTimeOffset ?? item.lastWriteTimeOffset ?? item.creationTimeOffset ?? now;
                                destinationEntry.CreationTimeOffsetUtc = item.creationTimeOffset ?? item.lastWriteTimeOffset ?? now;
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
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException($"エントリのコピー中に例外が発生しました。: zipFile=\"{sourceZipFile.FullName}\", entry=\"{item.sourceEntry?.FullName ?? "???"}\"", ex);
                        }

                        progress?.Report(currentProgressValue);
                    }
                }

                // タイムスタンプの設定

                var lastWriteTimeOffsetUtc =
                    normalizedEntries
                    .Select(entry => entry.lastWriteTimeOffset)
                    .Append((DateTimeOffset?)DateTimeOffset.MinValue)
                    .WhereNotNull()
                    .Max();
                try
                {
                    if (lastWriteTimeOffsetUtc.Ticks > 0)
                        File.SetLastWriteTimeUtc(destinationZipFile.FullName, lastWriteTimeOffsetUtc.UtcDateTime);
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

        private void VerifyNormalizedEntries(
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
                throw new Exception($"ApplicationException。 (エントリの個数が異なっています): {sourceZipFile.FullName}");

            var totalSize = sourceEntries.Sum(entry => (double)entry.Size);
            var completedSize = 0UL;
            foreach (var key in indexedNormalizedEntries.Keys)
            {
                if (IsPressedBreak)
                    throw new OperationCanceledException();

                var normalizedEntriesGroup = indexedNormalizedEntries[key];
                if (!indexedSourceEntries.TryGetValue(key, out var sourceEntriesGroup))
                    throw new ApplicationException($"正規化に失敗しました。 (CRCおよび長さが一致するエントリの個数が異なっています): {sourceZipFile.FullName}");
                if (normalizedEntriesGroup.Count != sourceEntriesGroup.Count)
                    throw new ApplicationException($"正規化に失敗しました。 (CRCおよび長さが一致するエントリの個数が異なっています): {sourceZipFile.FullName}");
                Validation.Assert(sourceEntriesGroup.Count > 0);
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
                        throw new ApplicationException($"正規化に失敗しました。 (データの内容が異なっています): {sourceZipFile.FullName}");
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
                        throw new ApplicationException($"正規化に失敗しました。 (データの内容が異なっています): {sourceZipFile.FullName}");
                }

                // 比較が完了したエントリの合計サイズを終了済みサイズを加算する。
                //   ※ Sum() を使用しない理由は、要素の型が ulong の場合の Sum() のオーバーロードがないから。
                completedSize = sourceEntriesGroup.Aggregate(completedSize, (value, entry) => checked(value + entry.Size));

                progress?.Report(completedSize / totalSize);
            }

            progress?.Report(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [GeneratedRegex(@"\.(zip|zipx|rar|cab|7z|tar|z|xz|pdf)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
        private static partial Regex GetArchivePathNamePattern();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [GeneratedRegex(@"^(?<length>\d+):(?<crc>[a-fA-F0-9]{8})$", RegexOptions.Compiled)]
        private static partial Regex GetBlackListParameterPattern();
    }
}
