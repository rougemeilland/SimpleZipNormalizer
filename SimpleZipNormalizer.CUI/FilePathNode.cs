using System;
using System.Collections.Generic;
using Palmtree.IO.Compression.Archive.Zip;

namespace SimpleZipNormalizer.CUI
{
    internal class FilePathNode
        : PathNode
    {
        public FilePathNode(string name, string sourceFullName, DirectoryPathNode? parentNode, ZipSourceEntry? sourceEntry)
            : base(name, sourceFullName, parentNode, sourceEntry)
        {
        }

        public override PathNode Clone(DirectoryPathNode? parent, ZipSourceEntry? sourceEntry)
            => new FilePathNode(Name, SourceFullName, parent, sourceEntry);

        public override IEnumerable<PathNode> EnumerateTerminalNodes()
        {
            if (SourceEntry is null)
                throw new Exception($"{nameof(SourceEntry)} is not set on the file node.: {nameof(SourceFullName)}={SourceFullName}");

            yield return this;
        }

        protected override IEnumerable<string> EnumerateDescriptionTextLines(string prefix = "", string childPrefix = "")
        {
            if (SourceEntry is not null)
                yield return $"{prefix}{Name} : {SourceFullName} ({SourceEntry.FullName})";
            else
                yield return $"{prefix}{Name} : {SourceFullName}";
        }
    }
}
