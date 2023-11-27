using System;

namespace ZipUtility.IO
{
    public class DataErrorException
        : Exception
    {
        public DataErrorException()
            : base("Data Error")
        {
        }

        public DataErrorException(String message)
            : base(message)
        {
        }

        public DataErrorException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
