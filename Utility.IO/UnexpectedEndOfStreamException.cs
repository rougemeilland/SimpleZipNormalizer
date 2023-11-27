using System;

namespace Utility.IO
{
    public class UnexpectedEndOfStreamException
        : Exception
    {
        public UnexpectedEndOfStreamException()
            : base("Unexpectedly reached the end of the stream.")
        {
        }

        public UnexpectedEndOfStreamException(String message)
            : base(message)
        {
        }

        public UnexpectedEndOfStreamException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
