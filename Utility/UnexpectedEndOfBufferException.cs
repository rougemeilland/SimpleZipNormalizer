using System;

namespace Utility
{
    public class UnexpectedEndOfBufferException
        : Exception
    {
        public UnexpectedEndOfBufferException()
            : base("Unexpectedly reached the end of the buffer.")
        {
        }

        public UnexpectedEndOfBufferException(String message)
            : base(message)
        {
        }

        public UnexpectedEndOfBufferException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
