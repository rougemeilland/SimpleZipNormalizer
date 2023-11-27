using System;

namespace ZipUtility
{
    internal static class MiscellaneousExtensions
    {
        internal static ZipStreamPosition Add(this Int64 x, ZipStreamPosition y) => y.Add(x);
        internal static ZipStreamPosition Add(this UInt64 x, ZipStreamPosition y) => y.Add(x);
        internal static ZipStreamPosition Add(this Int32 x, ZipStreamPosition y) => y.Add(x);
    }
}
