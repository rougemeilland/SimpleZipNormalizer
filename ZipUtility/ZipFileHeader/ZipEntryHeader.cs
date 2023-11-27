using System;
using Utility;

namespace ZipUtility.ZipFileHeader
{
    class ZipEntryHeader
    {
        public ZipEntryHeader(ZipEntryCentralDirectoryHeader centralDirectoryHeader, ZipEntryLocalHeader localFileHeader)
        {
            if (centralDirectoryHeader.LocalHeaderPosition != localFileHeader.LocalHeaderPosition)
                throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.LocalHeaderPosition)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            if (!centralDirectoryHeader.FullNameBytes.Span.SequenceEqual(localFileHeader.FullNameBytes.Span))
                throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.FullNameBytes)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            if (centralDirectoryHeader.VersionNeededToExtract != localFileHeader.VersionNeededToExtract)
                throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.VersionNeededToExtract)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            if (centralDirectoryHeader.CompressionMethodId != localFileHeader.CompressionMethodId)
                throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.CompressionMethodId)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            if (centralDirectoryHeader.DosDateTime != localFileHeader.DosDateTime)
                throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.DosDateTime)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");

            if (centralDirectoryHeader.GeneralPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.HasDataDescriptor)
                != localFileHeader.GeneralPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.HasDataDescriptor))
            {
                throw new BadZipFileFormatException($"The value of general purpose flag 3bit does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            }

            if (!centralDirectoryHeader.GeneralPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.HasDataDescriptor))
            {
                if (centralDirectoryHeader.Crc != localFileHeader.Crc)
                    throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.Crc)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
                if (centralDirectoryHeader.PackedSize != localFileHeader.PackedSize)
                    throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.PackedSize)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
                if (centralDirectoryHeader.Size != localFileHeader.Size)
                    throw new BadZipFileFormatException($"The value of {nameof(centralDirectoryHeader.PackedSize)} does not match between the central directory header and local directory header.: centralDirectoryIndex={centralDirectoryHeader.Index}");
            }

            CentralDirectoryHeader = centralDirectoryHeader;
            LocalFileHeader = localFileHeader;
        }

        public ZipEntryCentralDirectoryHeader CentralDirectoryHeader { get; }
        public ZipEntryLocalHeader LocalFileHeader { get; }
    }
}
