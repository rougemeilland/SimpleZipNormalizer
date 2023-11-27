namespace Utility
{
    public class ValueHolder<VALUE_T>
        where VALUE_T : struct
    {
        public ValueHolder()
        {
            Value = default;
        }

        public VALUE_T Value { get; set; }
    }
}
