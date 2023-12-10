using System;
using System.IO;
using System.Text;

namespace Utility.IO
{
    public class FilePath
        : FileSystemPath
    {
        private readonly FileInfo _file;

        public FilePath(String path)
            : this(GetFineInfo(path))
        {
        }

        private FilePath(FileInfo file)
            : base(file)
        {
            _file = file;
        }

        public DirectoryPath? Directory
        {
            get
            {
                _file.Refresh();
                var directory = _file.Directory;
                return
                    directory is null
                    ? null
                    : DirectoryPath.CreateInstance(directory);
            }
        }

        public Int64 Length
        {
            get
            {
                _file.Refresh();
                return _file.Length;
            }
        }

        public TextWriter AppendText()
        {
            try
            {
                _file.Refresh();
                return _file.AppendText();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public TextWriter AppendText(Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            try
            {
                _file.Refresh();
                var outStream = _file.OpenWrite();
                _ = outStream.Seek(0, SeekOrigin.End);
                return new StreamWriter(outStream, encoding);
            }
            finally
            {
                _file.Refresh();
            }
        }

        public void CopyTo(FilePath destinationFile, Boolean overwrite = false)
        {
            if (destinationFile is null)
                throw new ArgumentNullException(nameof(destinationFile));

            _file.Refresh();
            destinationFile.Refresh();
            try
            {
                _ = _file.CopyTo(destinationFile.FullName, overwrite);
            }
            finally
            {
                _file.Refresh();
                destinationFile.Refresh();
            }
        }

        public ISequentialOutputByteStream Create()
        {
            _file.Refresh();
            try
            {
                return _file.Create().AsOutputByteStream();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public TextWriter CreateText()
        {
            _file.Refresh();
            try
            {
                return _file.CreateText();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public TextWriter CreateText(Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            _file.Refresh();
            try
            {
                return new StreamWriter(_file.Create(), encoding);
            }
            finally
            {
                _file.Refresh();
            }
        }

        public void MoveTo(FilePath destinationFile, Boolean overwrite = false)
        {
            if (destinationFile is null)
                throw new ArgumentNullException(nameof(destinationFile));

            _file.Refresh();
            destinationFile.Refresh();
            try
            {
                File.Move(_file.FullName, destinationFile._file.FullName, overwrite);
            }
            finally
            {
                _file.Refresh();
                destinationFile.Refresh();
            }
        }

        public ISequentialInputByteStream OpenRead()
        {
            _file.Refresh();
            try
            {
                return _file.OpenRead().AsInputByteStream();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public TextReader OpenText()
        {
            _file.Refresh();
            try
            {
                return _file.OpenText();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public TextReader OpenText(Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            _file.Refresh();
            try
            {
                return new StreamReader(_file.OpenRead(), encoding, true);
            }
            finally
            {
                _file.Refresh();
            }
        }

        public ISequentialOutputByteStream OpenWrite()
        {
            _file.Refresh();
            try
            {
                return _file.OpenWrite().AsOutputByteStream();
            }
            finally
            {
                _file.Refresh();
            }
        }

        public void Replace(FilePath destination, FilePath destinatonBackupFile)
        {
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));
            if (destinatonBackupFile is null)
                throw new ArgumentNullException(nameof(destinatonBackupFile));

            _file.Refresh();
            destination.Refresh();
            destinatonBackupFile.Refresh();
            try
            {
                _ = _file.Replace(destination._file.FullName, destinatonBackupFile._file.FullName);
            }
            finally
            {
                _file.Refresh();
                destination.Refresh();
                destinatonBackupFile.Refresh();
            }
        }

        public static implicit operator FileInfo(FilePath path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            return new(path._file.FullName);
        }

        public static implicit operator FilePath(FileInfo directory)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            return new(new FileInfo(directory.FullName));
        }

        /// <remarks>
        /// The same instance as the object indicated by parameter <paramref name="file"/> must not be used elsewhere.
        /// </remarks>
        internal static FilePath CreateInstance(FileInfo file) => new(file);

        private static FileInfo GetFineInfo(String path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            try
            {
                return new FileInfo(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"A character string that cannot be used as a file path name was specified. : \"{path}\"", nameof(path), ex);
            }
        }
    }
}
