using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Palmtree.IO.Compression.Archive.Zip;

namespace SimpleZipNormalizer.CUI
{
    internal partial class DirectoryPathNode
        : PathNode
    {
        private readonly IDictionary<string, PathNode> _indexedChildNodes;

        public DirectoryPathNode(string name, string sourceFullName, DirectoryPathNode? parentNode, ZipSourceEntry? sourceEntry)
            : base(name, sourceFullName, parentNode, sourceEntry)
        {
            _indexedChildNodes = new SortedDictionary<string, PathNode>(StringComparer.OrdinalIgnoreCase);
        }

        public DirectoryPathNode(string name, string sourceFullName, DirectoryPathNode? parentNode, ZipSourceEntry? sourceEntry, IEnumerable<PathNode> childNodes)
            : this(name, sourceFullName, parentNode, sourceEntry)
        {
            foreach (var childNode in childNodes)
                _indexedChildNodes.Add(childNode.Name, childNode.Clone(this, childNode.SourceEntry));
        }

        public IEnumerable<PathNode> ChildNodes => _indexedChildNodes.Values;

        public void AddChildNode(string childNodePath, ZipSourceEntry sourceEntry)
        {
            var filePathNodeMatch = GetFilePathPattern().Match(childNodePath);
            if (filePathNodeMatch.Success)
            {
                var name = filePathNodeMatch.Groups["name"].Value;
                if (_indexedChildNodes.TryGetValue(name, out var childNode))
                {
                    if (childNode is DirectoryPathNode)
                        throw new Exception($"ファイル \"{SourceFullName}{name}\" を追加しようとしましたが、ディレクトリ \"{SourceFullName}{childNode.Name}/\" が既に存在しています。");
                    else if (childNode is FilePathNode)
                        throw new Exception($"ファイル \"{SourceFullName}{name}\" が重複しています。");
                    else
                        throw new Exception("未知のノードが登録されています。");
                }

                childNode = new FilePathNode(name, $"{SourceFullName}{name}", this, sourceEntry);
                _indexedChildNodes.Add(childNode.Name, childNode);
                return;
            }

            var directoryPathNodeMatch = GetDirectoryPathPattern().Match(childNodePath);
            if (directoryPathNodeMatch.Success)
            {
                var name = directoryPathNodeMatch.Groups["name"].Value;
                if (name is "." or "..")
                    throw new Exception($"\".\" または \"..\" という名前のディレクトリ名を持つエントリ名は使用できません。: \"{sourceEntry.FullName}\"");
                var delimiter = directoryPathNodeMatch.Groups["delimiter"].Value;
                var remainedPath = directoryPathNodeMatch.Groups["child_path"].Value;
                var isTerminalDirectoryNode = string.IsNullOrEmpty(remainedPath);
                if (_indexedChildNodes.TryGetValue(name, out var childNode))
                {
                    if (childNode is DirectoryPathNode)
                    {
                        if (isTerminalDirectoryNode && childNode.SourceEntry is null)
                        {
                            var newChildNode = childNode.Clone(this, sourceEntry);
                            _ = _indexedChildNodes.Remove(childNode.Name);
                            _indexedChildNodes.Add(newChildNode.Name, newChildNode);
                        }
                    }
                    else if (childNode is FilePathNode)
                    {
                        throw new Exception($"ディレクトリ \"{SourceFullName}{name}{delimiter}\" を追加しようとしましたが、ファイル \"{SourceFullName}{childNode.Name}\" が既に存在しています。");
                    }
                    else
                    {
                        throw new Exception("未知のノードが登録されています。");
                    }
                }
                else
                {
                    childNode =
                        new DirectoryPathNode(
                            name,
                            $"{SourceFullName}{name}{delimiter}",
                            this,
                            isTerminalDirectoryNode ? sourceEntry : null);
                    _indexedChildNodes.Add(childNode.Name, childNode);
                }

                if (!isTerminalDirectoryNode && childNode is DirectoryPathNode childDirectoruNode)
                    childDirectoruNode.AddChildNode(remainedPath, sourceEntry);

                return;
            }

            throw new ArgumentException($"ノードのパス名が未知の形式です。", $"{nameof(childNodePath)}");
        }

        public void Normalize()
        {
            // 子ディレクトリノードを正規化する
            foreach (var childNode in _indexedChildNodes.Values)
            {
                if (childNode is DirectoryPathNode directoryNode)
                    directoryNode.Normalize();
            }

            // 不要なノード (子ノードが存在しないディレクトリノード) を削除する。
            var uselessNodes =
                _indexedChildNodes.Values
                .Where(childNode => childNode is DirectoryPathNode directoryNode && !directoryNode.ChildNodes.Any())
                .ToList();
            foreach (var uselessNode in uselessNodes)
                _ = _indexedChildNodes.Remove(uselessNode.Name);

            // 子ノードがただ一つであり、かつ子ノードがディレクトリノードである場合は、子ノードの子ノードを現在ディレクトリノードの配下にコピーして子ノードを削除する。
            if (_indexedChildNodes.Count == 1)
            {
                var childNode = _indexedChildNodes.Values.First();
                if (childNode is DirectoryPathNode childDirectoryNode)
                {
                    _indexedChildNodes.Clear();
                    foreach (var node in childDirectoryNode.ChildNodes)
                        _indexedChildNodes.Add(node.Name, node.Clone(this, node.SourceEntry));
                }
            }

            // ディレクトリのタイムスタンプの再設定 (子要素のうち最新の値にする)
            if (_indexedChildNodes.Count > 0)
            {
                LastWriteTimeOffset = GetNewestTimeStamp(_indexedChildNodes.Values, node => node.LastWriteTimeOffset);
                LastAccessTimeOffset = GetNewestTimeStamp(_indexedChildNodes.Values, node => node.LastAccessTimeOffset);
                CreationTimeOffset = GetNewestTimeStamp(_indexedChildNodes.Values, node => node.CreationTimeOffset);
            }
        }

        public override PathNode Clone(DirectoryPathNode? parent, ZipSourceEntry? sourceEntry)
            => new DirectoryPathNode(Name, SourceFullName, parent, sourceEntry, _indexedChildNodes.Values)
            {
                LastWriteTimeOffset = LastWriteTimeOffset,
                LastAccessTimeOffset = LastAccessTimeOffset,
                CreationTimeOffset = CreationTimeOffset,
            };

        public override IEnumerable<PathNode> EnumerateTerminalNodes()
        {
            if (ParentNode is not null)
                yield return this;
            foreach (var childNode in _indexedChildNodes.Values)
            {
                foreach (var terminalNode in childNode.EnumerateTerminalNodes())
                    yield return terminalNode;
            }
        }

        protected override IEnumerable<string> EnumerateDescriptionTextLines(string prefix = "", string childPrefix = "")
        {
            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(childPrefix) && string.IsNullOrEmpty(Name))
            {
                foreach (var childNode in _indexedChildNodes.Values)
                {
                    foreach (var line in EnumerateDescriptionTextLines(childNode, "", ""))
                        yield return line;
                }
            }
            else
            {
                if (SourceEntry is not null)
                    yield return $"{prefix}{Name}/ : {SourceFullName} ({SourceEntry.FullName})";
                else
                    yield return $"{prefix}{Name}/ : {SourceFullName}";

                // 子ノードのリストのうち最後のノードとそれ以外を分割する。
                var childNodeListExceptLast = SplitLastPathNode(_indexedChildNodes.Values, out var lastChildNode);

                // 最後以外のノードのテキストを列挙する。
                foreach (var childNode in childNodeListExceptLast)
                {
                    foreach (var line in EnumerateDescriptionTextLines(childNode, $"{childPrefix}  +- ", $"{childPrefix}  |"))
                        yield return line;
                }

                // 最後のノードがもし存在すればそのノードのテキストを列挙する (子ノードのリストが空ではない限り最後のノードは存在する)
                if (lastChildNode is not null)
                {
                    foreach (var line in EnumerateDescriptionTextLines(lastChildNode, $"{childPrefix}  +- ", $"{childPrefix}   "))
                        yield return line;
                }
            }
        }

        private static IEnumerable<PathNode> SplitLastPathNode(IEnumerable<PathNode> nodes, out PathNode? lastNode)
        {
            var result = new List<PathNode>();
            var cache = (PathNode?)null;
            foreach (var node in nodes)
            {
                if (cache is null)
                {
                    cache = node;
                }
                else
                {
                    result.Add(cache);
                    cache = node;
                }
            }

            lastNode = cache;
            return result;
        }

        private static DateTimeOffset? GetNewestTimeStamp(
            IEnumerable<PathNode> nodes,
            Func<PathNode, DateTimeOffset?> timestampSelector)
        {
            var initialValue = (DateTimeOffset?)DateTimeOffset.MinValue;
            var result =
                nodes
                .Aggregate(
                    initialValue,
                    (dateTime, element) =>
                    {
                        var otherDateTime = timestampSelector(element);
                        return
                            dateTime is null
                            ? otherDateTime
                            : otherDateTime is null
                            ? dateTime
                            : dateTime.Value.CompareTo(otherDateTime.Value) > 0
                            ? dateTime
                            : otherDateTime;
                    });
            return
                result is null || result.Value <= DateTimeOffset.MinValue
                ? null
                : result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [GeneratedRegex("^(?<name>[^\\\\/]+)$", RegexOptions.Compiled)]
        private static partial Regex GetFilePathPattern();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [GeneratedRegex("^(?<name>[^\\\\/]+)(?<delimiter>[\\\\/])(?<child_path>.*)$", RegexOptions.Compiled)]
        private static partial Regex GetDirectoryPathPattern();
    }
}
