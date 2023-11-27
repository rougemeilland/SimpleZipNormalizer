using System;
using System.Threading.Tasks;

namespace Utility.Threading
{
    internal interface IAsyncLockable
        : IDisposable
    {
        Task LockAsync();
    }
}
