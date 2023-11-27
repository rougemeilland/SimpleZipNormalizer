using System;

namespace Utility.Threading
{
    internal interface ILockable
        : IDisposable
    {
        void Lock();
    }
}
