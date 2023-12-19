using Utility.IO;

namespace ZipUtility
{
    internal interface IZipFileWriterOutputStreamAccesser
    {
        IZipOutputStream MainStream { get; }
        ISequentialOutputByteStream StreamForCentralDirectoryHeaders { get; }
        void BeginToWriteContent();
        void EndToWritingContent();
        void SetErrorMark();
        void LockZipStream();
        void UnlockZipStream();
    }
}
