using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Utility.Collections
{
    public class RandomAccessQueue<ELEMENT_T>
        : IEnumerable<ELEMENT_T>, ICloneable<RandomAccessQueue<ELEMENT_T>>, IEquatable<RandomAccessQueue<ELEMENT_T>>
        where ELEMENT_T : IEquatable<ELEMENT_T>
    {
        private readonly SortedDictionary<UInt64, ELEMENT_T> _queue;

        private UInt64 _indexOfStart;
        private UInt64 _indexOfEnd;

        public RandomAccessQueue()
            : this(Array.Empty<ELEMENT_T>())
        {
        }

        public RandomAccessQueue(IEnumerable<ELEMENT_T> dataSource)
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));

            _queue = new SortedDictionary<UInt64, ELEMENT_T>();
            _indexOfStart = 0;
            _indexOfEnd = 0;
            foreach (var value in dataSource)
            {
                _queue.Add(_indexOfEnd, value);
                ++_indexOfEnd;
            }

            Normalize();
        }

        public void Clear()
        {
            _queue.Clear();
            _indexOfStart = 0;
            _indexOfEnd = 0;
        }

        public void Enqueue(ELEMENT_T value)
        {
            _queue.Add(_indexOfEnd, value);
            ++_indexOfEnd;
            Normalize();
#if DEBUG
            Check();
#endif
        }

        public ELEMENT_T Dequeue()
        {
            if (_queue.Count <= 0)
                throw new InvalidOperationException();
            var value = _queue[_indexOfStart];
            _queue.Remove(_indexOfStart);
            ++_indexOfStart;
            Normalize();
#if DEBUG
            Check();
#endif
            return value;
        }

        public ELEMENT_T this[Int32 index]
        {
            get
            {
                if (!index.InRange(0, _queue.Count))
                    throw new ArgumentOutOfRangeException(nameof(index));
                try
                {
                    return _queue[checked(_indexOfStart + (UInt32)index)];
                }
                catch (OverflowException ex)
                {
                    throw new ArgumentOutOfRangeException($"Invalid {nameof(index)} value", ex);
                }
                catch (KeyNotFoundException ex)
                {
                    throw new ArgumentOutOfRangeException($"Invalid {nameof(index)} value", ex);
                }
            }
        }
        public Int32 Length => _queue.Count;
        public Int32 Count => _queue.Count;
        public IEnumerator<ELEMENT_T> GetEnumerator() => _queue.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public RandomAccessQueue<ELEMENT_T> Clone() => new(_queue.Values);
        public Boolean Equals(RandomAccessQueue<ELEMENT_T>? other) => other is not null && _queue.Count == other._queue.Count && _queue.Values.SequenceEqual(other._queue.Values);
        public override Boolean Equals(Object? obj) => obj is not null && GetType() == obj.GetType() && Equals((RandomAccessQueue<ELEMENT_T>)obj);
        public override Int32 GetHashCode() => _queue.Values.Aggregate(0, (hashCode, value) => hashCode ^ value.GetHashCode());

        private void Normalize()
        {
            if (_queue.Count <= 0)
            {
                _indexOfStart = 0;
                _indexOfEnd = 0;
            }

            // _indexOfStart のオーバーフロー対策
            // ある程度 _indexOfStart が大きくなったら _queue を再構築する
            if (_indexOfStart > UInt32.MaxValue || _indexOfEnd > UInt32.MaxValue)
            {
                if (_queue.Count > 0)
                {
                    var firstKey = _queue.Keys.First();
                    foreach (var item in _queue)
                    {
                        if (item.Key < firstKey)
                            throw new InternalLogicalErrorException();

                        _ = _queue.Remove(item.Key);
                        _queue[item.Key - firstKey] = item.Value;
                    }
                }

                _indexOfStart = 0;
                _indexOfEnd = (UInt32)_queue.Count;
            }
        }

#if DEBUG
        private void Check()
        {
            if (_queue.Count > 0)
            {
                if (_queue.Keys.First() != _indexOfStart)
                    throw new Exception();
                if (_queue.Keys.Last() + 1 != _indexOfEnd)
                    throw new Exception();
                if ((UInt32)_queue.Count != checked(_indexOfEnd - _indexOfStart))
                    throw new Exception();
            }
            else
            {
                if (_indexOfStart != 0)
                    throw new Exception();
                if (_indexOfEnd != 0)
                    throw new Exception();
            }
        }
#endif
    }
}
