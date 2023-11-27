using System;
using System.Collections;
using System.Collections.Generic;

namespace ZipUtility
{
    /// <summary>
    /// 読み込んだ ZIP アーカイブのエントリのコレクションのクラスです。
    /// </summary>
    public class ZipArchiveEntryCollection
        : IReadOnlyCollection<ZipSourceEntry>
    {
        private readonly IDictionary<Int32, ZipSourceEntry> _collectionByIndex;
        private readonly IDictionary<String, ZipSourceEntry> _collectionByFullName;

        internal ZipArchiveEntryCollection(IEnumerable<ZipSourceEntry> sourceEntryCollection)
        {
            _collectionByIndex = new SortedList<Int32, ZipSourceEntry>();
            _collectionByFullName = new Dictionary<String, ZipSourceEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var sourceEntry in sourceEntryCollection)
            {
                _collectionByIndex[sourceEntry.Index] = sourceEntry;
                _collectionByFullName[sourceEntry.FullName] = sourceEntry;
            }
        }

        /// <summary>
        /// Gets a <see cref="ZipSourceEntry"/> object with a matching <see cref="ZipSourceEntry.Index"/> property.
        /// </summary>
        /// <param name="index">
        /// The <see cref="UInt64"/> value that indicates the value of the <see cref="ZipSourceEntry.Index"/> property of the <see cref="ZipSourceEntry"/> object to be retrieved.
        /// </param>
        /// <returns>
        /// <para>
        /// If the corresponding <see cref="ZipSourceEntry"/> object exists, that object will be returned.
        /// </para>
        /// <para>
        /// If it does not exist, null is returned.
        /// </para>
        /// </returns>
        public ZipSourceEntry? this[Int32 index]
            => _collectionByIndex.TryGetValue(index, out var entry)
                ? entry
                : null;

        /// <summary>
        /// Gets a <see cref="ZipSourceEntry"/> object with a matching <see cref="ZipSourceEntry.FullName"/> property.
        /// </summary>
        /// <param name="entryName">
        /// The <see cref="UInt64"/> value that indicates the value of the <see cref="ZipSourceEntry.FullName"/> property of the <see cref="ZipSourceEntry"/> object to be retrieved.
        /// </param>
        /// <returns>
        /// <para>
        /// If the corresponding <see cref="ZipSourceEntry"/> object exists, that object will be returned.
        /// </para>
        /// <para>
        /// If it does not exist, null is returned.
        /// </para>
        /// </returns>
        public ZipSourceEntry? this[String entryName]
            => _collectionByFullName.TryGetValue(entryName, out var entry)
                ? entry
                : null;

        /// <summary>
        /// コレクションが保持する ZIP エントリの数を取得します。
        /// </summary>
        public Int32 Count => _collectionByIndex.Count;

        /// <summary>
        /// コレクション上の ZIP エントリ の列挙子を取得します。
        /// </summary>
        /// <returns>
        /// コレクション上の ZIP エントリ の列挙子が返ります。
        /// </returns>
        public IEnumerator<ZipSourceEntry> GetEnumerator() => _collectionByIndex.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
