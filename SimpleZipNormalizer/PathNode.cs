using System;
using System.Collections.Generic;
using Palmtree.IO.Compression.Archive.Zip;

namespace SimpleZipNormalizer
{
    internal abstract class PathNode
    {
        protected PathNode(string name, string sourceFullName, DirectoryPathNode? parentNode, ZipSourceEntry? sourceEntry)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(sourceFullName))
                    throw new ArgumentNullException(nameof(sourceFullName), $"{nameof(name)} が null または 空文字である場合は、{nameof(sourceFullName)} も null あるいは空文字でなければなりません。");
                if (parentNode is not null)
                    throw new ArgumentNullException(nameof(name));
            }
            else
            {
                if (string.IsNullOrEmpty(sourceFullName))
                    throw new ArgumentNullException(nameof(sourceFullName), $"{nameof(name)} が null または 空文字の何れでもない場合は、{nameof(sourceFullName)} も null あるいは空文字の何れであってもなりません。");
                if (parentNode is null)
                    throw new ArgumentNullException(nameof(parentNode));
            }

            Name = name;
            SourceFullName = sourceFullName;
            ParentNode = parentNode;
            SourceEntry = sourceEntry;
            LastWriteTime = sourceEntry?.LastWriteTimeUtc;
            LastAccessTime = sourceEntry?.LastAccessTimeUtc;
            CreationTime = sourceEntry?.CreationTimeUtc;
        }

        public string Name { get; }
        public string SourceFullName { get; }
        public DirectoryPathNode? ParentNode { get; }

        public string CurrentFullName
        {
            get
            {
                if (ParentNode is null)
                    throw new InvalidOperationException();

                return $"{(ParentNode.ParentNode is null ? "" : ParentNode.CurrentFullName)}{Name}{(this is DirectoryPathNode ? "/" : "")}";
            }
        }

        public ZipSourceEntry? SourceEntry { get; }
        public DateTime? LastWriteTime { get; protected set; }
        public DateTime? LastAccessTime { get; protected set; }
        public DateTime? CreationTime { get; protected set; }

        public abstract PathNode Clone(DirectoryPathNode? parent, ZipSourceEntry? sourceEntry);
        public abstract IEnumerable<PathNode> EnumerateTerminalNodes();

        public override string ToString()
            => string.Join("\n", EnumerateDescriptionTextLines());

        public static DirectoryPathNode CreateRootNode() => new("", "", null, null);

        protected static IEnumerable<string> EnumerateDescriptionTextLines(PathNode targetNode, string prefix = "", string childPrefix = "")
            => targetNode.EnumerateDescriptionTextLines(prefix, childPrefix);

        protected abstract IEnumerable<string> EnumerateDescriptionTextLines(string prefix = "", string childPrefix = "");
    }
}
