using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Utility
{
    public class FragmentSet<POSITION_T, SIZE_T>
        where POSITION_T : IComparable<POSITION_T>, IAdditionOperators<POSITION_T, SIZE_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, SIZE_T>
        where SIZE_T : IComparable<SIZE_T>, IAdditionOperators<SIZE_T, SIZE_T, SIZE_T>
    {
        private readonly LinkedList<FragmentSetElement<POSITION_T, SIZE_T>> _fragments;

        public FragmentSet()
        {
            _fragments = new LinkedList<FragmentSetElement<POSITION_T, SIZE_T>>();
        }

        public FragmentSet(POSITION_T initialStartPosition, SIZE_T initialSize)
            : this()
        {
            if (initialStartPosition is null)
                throw new ArgumentNullException(nameof(initialStartPosition));
            if (initialSize is null)
                throw new ArgumentNullException(nameof(initialSize));

            _ = _fragments.AddFirst(new FragmentSetElement<POSITION_T, SIZE_T>(initialStartPosition, initialSize));
        }

        public bool IsEmpty => _fragments.First is null;

        public void AddFragment(FragmentSetElement<POSITION_T, SIZE_T> fragment)
        {
            if (fragment is null)
                throw new ArgumentNullException(nameof(fragment));

            var firstNode = _fragments.First;
            if (firstNode is not null)
            {
                var c = fragment.EndPosition.CompareTo(firstNode.Value.StartPosition);
                if (c < 0)
                {
                    _ = _fragments.AddFirst(fragment);
                    return;
                }
                else if (c == 0)
                {
                    _ = _fragments.AddBefore(firstNode, new FragmentSetElement<POSITION_T, SIZE_T>(fragment.StartPosition, fragment.Size + firstNode.Value.Size));
                    _fragments.Remove(firstNode);
                    return;
                }
            }
            else
            {
                _ = _fragments.AddFirst(fragment);
                return;
            }

            var node = _fragments.First;
            if (node is not null)
            {
                while (true)
                {
                    var nextNode = node.Next;
                    if (nextNode is null)
                        break;

                    Int32 c1;
                    Int32 c2;
                    if ((c1 = node.Value.EndPosition.CompareTo(fragment.StartPosition)) <= 0 && (c2 = fragment.EndPosition.CompareTo(nextNode.Value.StartPosition)) <= 0)
                    {
                        if (c1 == 0)
                        {
                            if (c2 == 0)
                            {
                                _ = _fragments.AddAfter(node, new FragmentSetElement<POSITION_T, SIZE_T>(node.Value.StartPosition, node.Value.Size + fragment.Size + nextNode.Value.Size));
                                _fragments.Remove(node);
                                _fragments.Remove(nextNode);
                                return;
                            }
                            else
                            {
                                _ = _fragments.AddAfter(node, new FragmentSetElement<POSITION_T, SIZE_T>(node.Value.StartPosition, node.Value.Size + fragment.Size));
                                _fragments.Remove(node);
                                return;
                            }
                        }
                        else
                        {
                            if (c2 == 0)
                            {
                                _ = _fragments.AddAfter(node, new FragmentSetElement<POSITION_T, SIZE_T>(fragment.StartPosition, fragment.Size + nextNode.Value.Size));
                                _fragments.Remove(nextNode);
                                return;
                            }
                            else
                            {
                                _ = _fragments.AddAfter(node, fragment);
                                return;
                            }
                        }
                    }

                    node = nextNode;
                }
            }

            var lastNode = _fragments.Last;
            if (lastNode is not null)
            {
                var c = fragment.StartPosition.CompareTo(lastNode.Value.EndPosition);
                if (c == 0)
                {
                    _ = _fragments.AddAfter(lastNode, new FragmentSetElement<POSITION_T, SIZE_T>(lastNode.Value.StartPosition, lastNode.Value.Size + fragment.Size));
                    _fragments.Remove(lastNode);
                    return;
                }
                else if (c > 0)
                {
                    _ = _fragments.AddLast(fragment);
                    return;
                }
            }

            throw new ArgumentException($"Cannot add fragment element to fragments: fragments={this}, fragment-element={fragment}");
        }

        public void RemoveFragment(FragmentSetElement<POSITION_T, SIZE_T> fragment)
        {
            if (fragment is null)
                throw new ArgumentNullException(nameof(fragment));

            for (var node = _fragments.First; node is not null; node = node.Next)
            {
                Int32 c1;
                Int32 c2;
                if ((c1 = node.Value.StartPosition.CompareTo(fragment.StartPosition)) <= 0 && (c2 = fragment.EndPosition.CompareTo(node.Value.EndPosition)) <= 0)
                {
                    if (c1 < 0)
                        _ = _fragments.AddBefore(node, new FragmentSetElement<POSITION_T, SIZE_T>(node.Value.StartPosition, fragment.StartPosition - node.Value.StartPosition));
                    if (c2 < 0)
                        _ = _fragments.AddBefore(node, new FragmentSetElement<POSITION_T, SIZE_T>(fragment.EndPosition, node.Value.EndPosition - fragment.EndPosition));
                    _fragments.Remove(node);
                    return;
                }
            }

            throw new ArgumentException($"Cannot remove fragment element from fragments.: fragments={this}, fragment-element={fragment}");
        }

        public IEnumerable<FragmentSetElement<POSITION_T, SIZE_T>> EnumerateFragments()
        {
            for (var node = _fragments.First; node is not null; node = node.Next)
                yield return node.Value;
        }

        public override String ToString() => $"[{String.Join(" + ", EnumerateFragments().Select(fragment => fragment.ToString()))} ]";
    }
}
