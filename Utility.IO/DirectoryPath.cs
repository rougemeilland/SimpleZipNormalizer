using System;
using System.Collections.Generic;
using System.IO;

namespace Utility.IO
{
    public class DirectoryPath
        : FileSystemPath
    {
        private readonly DirectoryInfo _directory;

        public DirectoryPath(String path)
            : this(GetDirectoryInfo(path))
        {
        }

        private DirectoryPath(DirectoryInfo directory)
            : base(directory)
        {
            _directory = directory;
        }

        public DirectoryPath? Parent
        {
            get
            {
                _directory.Refresh();
                var parent = _directory.Parent;
                return
                    parent is null
                    ? null
                    : new DirectoryPath(parent);
            }
        }

        public DirectoryPath Root
        {
            get
            {
                _directory.Refresh();
                return new DirectoryPath(_directory.Root);
            }
        }

        public void Create()
        {
            _directory.Refresh();
            try
            {
                _directory.Create();
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public DirectoryPath CreateSubdirectory(String subDirectoryName)
        {
            if (subDirectoryName is null)
                throw new ArgumentNullException(nameof(subDirectoryName));

            _directory.Refresh();
            try
            {
                return new DirectoryPath(_directory.CreateSubdirectory(subDirectoryName));
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public void Delete(Boolean recursive = false)
        {
            _directory.Refresh();
            try
            {
                _directory.Delete(recursive);
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public IEnumerable<DirectoryPath> EnumerateDirectories(Boolean recursive = false)
        {
            _directory.Refresh();
            try
            {
                var subDirectories =
                    _directory.EnumerateDirectories(
                        "*",
                        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var directory in subDirectories)
                    yield return new DirectoryPath(directory);
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public IEnumerable<FilePath> EnumerateFiles(Boolean recursive = false)
        {
            _directory.Refresh();
            try
            {
                var subFiles =
                    _directory.EnumerateFiles(
                        "*",
                        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var file in subFiles)
                    yield return FilePath.CreateInstance(file);
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public FilePath GetFile(String fileName)
        {
            if (fileName is null)
                throw new ArgumentNullException(nameof(fileName));

            _directory.Refresh();
            try
            {
                return new FilePath(Path.Combine(_directory.FullName, fileName));
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public DirectoryPath GetSubDirectory(String subDirectoryName)
        {
            if (subDirectoryName is null)
                throw new ArgumentNullException(nameof(subDirectoryName));

            _directory.Refresh();
            try
            {
                return new DirectoryPath(Path.Combine(_directory.FullName, subDirectoryName));
            }
            finally
            {
                _directory.Refresh();
            }
        }

        public void MoveTo(DirectoryPath destinationDirectory)
        {
            if (destinationDirectory is null)
                throw new ArgumentNullException(nameof(destinationDirectory));

            _directory.Refresh();
            destinationDirectory.Refresh();
            try
            {
                Directory.Move(_directory.FullName, destinationDirectory.FullName);
            }
            finally
            {
                _directory.Refresh();
                destinationDirectory.Refresh();
            }
        }

        public static implicit operator DirectoryInfo(DirectoryPath path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            return new(path._directory.FullName);
        }

        public static implicit operator DirectoryPath(DirectoryInfo directory)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            return new(new DirectoryInfo(directory.FullName));
        }

        /// <remarks>
        /// The same instance as the object indicated by parameter <paramref name="directory"/> must not be used elsewhere.
        /// </remarks>
        internal static DirectoryPath CreateInstance(DirectoryInfo directory) => new(directory);

        private static DirectoryInfo GetDirectoryInfo(String path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            try
            {
                if (path.EndsWith(Path.AltDirectorySeparatorChar) || path.EndsWith(Path.DirectorySeparatorChar))
                    path = path[..^1];
                return new DirectoryInfo(path);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"A character string that cannot be used as a directory path name was specified. : \"{path}\"", nameof(path), ex);
            }
        }
    }
}
