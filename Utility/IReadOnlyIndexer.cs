namespace Utility
{
    public interface IReadOnlyIndexer<INDEX_T, VALUE_T>
    {
        VALUE_T this[INDEX_T index] { get; }
    }
}
