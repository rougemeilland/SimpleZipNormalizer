using System;
using System.Threading.Tasks;

namespace Utility.IO
{
    public interface ITrashBox
    {
        Boolean DisposeFile(FilePath file);
        Task<Boolean> DisposeFileAsync(FilePath file);
    }
}
