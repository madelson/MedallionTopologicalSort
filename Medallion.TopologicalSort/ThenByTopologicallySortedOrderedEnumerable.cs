using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Implements a secondary sort on top of a topological sort
    /// </summary>
    internal sealed class ThenByTopologicallySortedOrderedEnumerable<TElement, TKey> : IThenByTopologicalSortProvider<TElement>, IOrderedEnumerable<TElement>
    {
        private readonly IThenByTopologicalSortProvider<TElement> _source;
        private readonly Func<TElement, TKey> _keySelector;
        private readonly IComparer<TKey> _keyComparer;
        private readonly bool _descending;

        public ThenByTopologicallySortedOrderedEnumerable(
            IThenByTopologicalSortProvider<TElement> source,
            Func<TElement, TKey> keySelector,
            IComparer<TKey>? keyComparer,
            bool descending)
        {
            this._source = source;
            this._keySelector = keySelector;
            this._keyComparer = keyComparer ?? Comparer<TKey>.Default;
            this._descending = descending;
        }

        public Func<int, int, bool> CreateIndexComparison(IReadOnlyList<TElement> items, Func<int, int, bool>? next)
        {
            (TKey key, bool initialized)[]? keys = null;

            return Less;

            bool Less(int index1, int index2)
            {
                var key1 = GetKey(index1);
                var key2 = GetKey(index2);
                var comparison = this._descending
                    ? this._keyComparer.Compare(key2, key1)
                    : this._keyComparer.Compare(key1, key2);
                return comparison == 0 && next != null
                    ? next(index1, index2)
                    : comparison < 0;
            }

            TKey GetKey(int index)
            {
                if (keys == null)
                {
                    keys = new (TKey key, bool initialized)[items.Count];
                }
                else
                {
                    var (existingKey, initialized) = keys[index];
                    if (initialized) { return existingKey; }
                }

                var key = this._keySelector(items[index]);
                keys[index] = (key, initialized: true);
                return key;
            }
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey1>(Func<TElement, TKey1> keySelector, IComparer<TKey1> comparer, bool descending) =>
            TopologicalSorter.CreateOrderedEnumerable(this, keySelector, comparer, descending);

        IQueue ITopologicalSortProvider<TElement>.CreateQueue(IReadOnlyList<TElement> items) =>
            new PriorityQueue(this._source.CreateIndexComparison(items, next: this.CreateIndexComparison(items, next: null)), maxCapacity: items.Count);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<TElement> GetEnumerator() => TopologicalSorter.TopologicalSort(this);

        Func<TElement, IEnumerable<TElement>> ITopologicalSortProvider<TElement>.GetGetDependencies() => this._source.GetGetDependencies();

        IEqualityComparer<ValueTuple<TElement>> ITopologicalSortProvider<TElement>.GetItemComparer() => this._source.GetItemComparer();

        IEnumerable<TElement> ITopologicalSortProvider<TElement>.GetSource() => this._source.GetSource();
    }
}
