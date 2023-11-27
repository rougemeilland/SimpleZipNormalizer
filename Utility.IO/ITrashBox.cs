using System;
using System.IO;
using System.Threading.Tasks;

namespace Utility.IO
{
    public interface ITrashBox
    {
        Boolean DisposeFile(FileInfo file);
        Task<Boolean> DisposeFileAsync(FileInfo file);
    }
}
