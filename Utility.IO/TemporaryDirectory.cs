using System;
using System.IO;

namespace Utility.IO
{
    public class TemporaryDirectory
        : IDisposable
    {
        private readonly String _lockFilePath;
        private readonly String _directoryPath;

        private Boolean _isDisposed;

        private TemporaryDirectory(String lockFilePath, String directoryPath)
        {
            if (String.IsNullOrEmpty(lockFilePath))
                throw new ArgumentException($"'{nameof(lockFilePath)}' を NULL または空にすることはできません。", nameof(lockFilePath));
            if (String.IsNullOrEmpty(directoryPath))
                throw new ArgumentException($"'{nameof(directoryPath)}' を NULL または空にすることはできません。", nameof(directoryPath));

            _lockFilePath = lockFilePath;
            _directoryPath = directoryPath;
        }

        ~TemporaryDirectory()
        {
            Dispose(disposing: false);
        }

        public String FullName
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _directoryPath ?? throw new InvalidOperationException();
            }
        }

        public static TemporaryDirectory Create()
        {
            while (true)
            {
                var success = false;
                var lockFilePath = (String?)null;
                var directoryPath = (String?)null;
                try
                {
                    try
                    {
                        lockFilePath = Path.GetTempFileName();
                        directoryPath = lockFilePath + ".dir";
                        if (!Directory.Exists(directoryPath))
                        {
                            _ = Directory.CreateDirectory(directoryPath);
                            success = true;
                            return new TemporaryDirectory(lockFilePath, directoryPath);
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
                finally
                {
                    if (!success)
                    {
                        if (directoryPath is not null)
                            Directory.Delete(directoryPath, true);
                        if (lockFilePath is not null)
                            File.Delete(lockFilePath);
                    }
                }
            }
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                // ファイルはアンマネージリソース扱い
                Directory.Delete(_directoryPath, true);
                File.Delete(_lockFilePath);
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
