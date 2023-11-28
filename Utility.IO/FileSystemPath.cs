using System;
using System.IO;

namespace Utility.IO
{
    public abstract class FileSystemPath
    {
        private readonly FileSystemInfo _path;

        protected FileSystemPath(FileSystemInfo path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                _path.Refresh();
                return _path.CreationTimeUtc;
            }

            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException("Do not set a DateTime value whose Kind property value is DateTimeKind.Unspecified.", nameof(value));

                try
                {
                    _path.CreationTimeUtc = value.ToUniversalTime();
                }
                finally
                {
                    _path.Refresh();
                }
            }
        }

        public Boolean Exists
        {
            get
            {
                _path.Refresh();
                return _path.Exists;
            }
        }

        public String Extension
        {
            get
            {
                _path.Refresh();
                return _path.Extension;
            }
        }

        public String FullName
        {
            get
            {
                _path.Refresh();
                return _path.FullName;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                _path.Refresh();
                return _path.LastWriteTimeUtc;
            }

            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException("Do not set a DateTime value whose Kind property value is DateTimeKind.Unspecified.", nameof(value));

                try
                {
                    _path.LastWriteTimeUtc = value.ToUniversalTime();
                }
                finally
                {
                    _path.Refresh();
                }
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                _path.Refresh();
                return _path.LastAccessTimeUtc;
            }

            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException("Do not set a DateTime value whose Kind property value is DateTimeKind.Unspecified.", nameof(value));

                try
                {
                    _path.LastAccessTimeUtc = value.ToUniversalTime();
                }
                finally
                {
                    _path.Refresh();
                }
            }
        }

        public String Name
        {
            get
            {
                _path.Refresh();
                return _path.Name;
            }
        }

        public void Delete()
        {
            _path.Refresh();
            try
            {
                _path.Delete();

            }
            finally
            {
                _path.Refresh();
            }
        }

        public override String ToString()
        {
            _path.Refresh();
            return _path.FullName;
        }

        internal void Refresh() => _path.Refresh();
    }
}
