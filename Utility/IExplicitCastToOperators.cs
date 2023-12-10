namespace Utility
{
    public interface IExplicitCastToOperators<SOURCE_T, DESTONATION_T>
        where SOURCE_T : IExplicitCastToOperators<SOURCE_T, DESTONATION_T>
    {
        static abstract explicit operator DESTONATION_T(SOURCE_T value);
        static virtual explicit operator checked DESTONATION_T(SOURCE_T value) => checked((DESTONATION_T)value);
    }
}
