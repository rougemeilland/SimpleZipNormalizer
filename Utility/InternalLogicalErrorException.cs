using System;

namespace Utility
{
    public class InternalLogicalErrorException
        : Exception
    {
        public InternalLogicalErrorException()
            : base("Detected internal logical error.")
        {
        }

        public InternalLogicalErrorException(String message)
            : base(message)
        {
        }

        public InternalLogicalErrorException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
