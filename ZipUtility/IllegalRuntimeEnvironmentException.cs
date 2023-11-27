using System;

namespace ZipUtility
{
    /// <summary>
    /// 実行環境に由来する例外のクラスです。
    /// </summary>
    public class IllegalRuntimeEnvironmentException
        : Exception
    {
        internal IllegalRuntimeEnvironmentException()
            : this("There is an error in the runtime environment.")
        {
        }

        internal IllegalRuntimeEnvironmentException(String message)
            : base(message)
        {
        }

        internal IllegalRuntimeEnvironmentException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
