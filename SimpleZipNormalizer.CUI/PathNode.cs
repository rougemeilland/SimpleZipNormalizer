using System;
using System.Collections.Generic;
using Palmtree.IO.Compression.Archive.Zip;

namespace SimpleZipNormalizer.CUI
{
    internal abstract class PathNode
    {
        private DateTime? _lastWriteTime;
        private DateTime? _lastAccessTime;
        private DateTime? _creationTime;

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
            _lastWriteTime = null;
            _lastAccessTime = null;
            _creationTime = null;
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

        public DateTime? LastWriteTime
        {
            get => _lastWriteTime ?? SourceEntry?.LastWriteTimeUtc;
            
            protected set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"The value of the {nameof(DateTime.Kind)} property of the {nameof(DateTime)} object being set must not be {nameof(DateTimeKind.Unspecified)}.", nameof(value));

                _lastWriteTime = value;
            }
        }

        public DateTime? LastAccessTime
        {
            get => _lastAccessTime ?? SourceEntry?.LastAccessTimeUtc;
            
            protected set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"The value of the {nameof(DateTime.Kind)} property of the {nameof(DateTime)} object being set must not be {nameof(DateTimeKind.Unspecified)}.", nameof(value));

                _lastAccessTime = value;
            }
        }

        public DateTime? CreationTime
        {
            get => _creationTime ?? SourceEntry?.CreationTimeUtc;
            
            protected set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"The value of the {nameof(DateTime.Kind)} property of the {nameof(DateTime)} object being set must not be {nameof(DateTimeKind.Unspecified)}.", nameof(value));

                _creationTime = value;
            }
        }

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
