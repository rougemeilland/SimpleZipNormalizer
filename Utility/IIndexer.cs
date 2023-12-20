namespace Utility
{
    public interface IIndexer<INDEX_T, VALUE_T>
    {
        VALUE_T this[INDEX_T index] { get; set; }
    }
}
