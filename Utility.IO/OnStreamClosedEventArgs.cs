using System;

namespace Utility.IO
{
    public class OnStreamClosedEventArgs<SIZE_T>
        : EventArgs
    {
        public SIZE_T Length { get; }

        public OnStreamClosedEventArgs(SIZE_T length)
        {
            Length = length;
        }
    }
}
