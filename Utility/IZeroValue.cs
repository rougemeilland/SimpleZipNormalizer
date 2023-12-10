namespace Utility
{
    public interface IZeroValue<VALUE_T>
        where VALUE_T : struct, IZeroValue<VALUE_T>
    {
        static abstract VALUE_T ZeroValue { get; }
    }
}
