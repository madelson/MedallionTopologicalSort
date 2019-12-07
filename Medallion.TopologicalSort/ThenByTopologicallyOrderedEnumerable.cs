using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal sealed class ThenByTopologicallyOrderedEnumerable<TElement, TKey> : TopologicallyOrderedEnumerable<TElement>
    {
        private readonly TopologicallyOrderedEnumerable<TElement> _source;
        private readonly Func<TElement, TKey> _keySelector;
        private readonly IComparer<TKey> _keyComparer;
        private readonly bool _descending;

        public ThenByTopologicallyOrderedEnumerable(
            TopologicallyOrderedEnumerable<TElement> source,
            Func<TElement, TKey> keySelector,
            IComparer<TKey>? keyComparer,
            bool descending)
        {
            this._source = source;
            this._keySelector = keySelector;
            this._keyComparer = keyComparer ?? Comparer<TKey>.Default;
            this._descending = descending;
        }

        public override Func<int, int, bool> CreateIndexComparison(IReadOnlyList<TElement> items, Func<int, int, bool>? next)
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

        public override IQueue CreateQueue(IReadOnlyList<TElement> items) => 
            new PriorityQueue(this._source.CreateIndexComparison(items, next: this.CreateIndexComparison(items, next: null)), maxCapacity: items.Count);

        public override Func<TElement, IEnumerable<TElement>> GetGetDependencies() => this._source.GetGetDependencies();

        public override IEqualityComparer<ValueTuple<TElement>> GetItemComparer() => this._source.GetItemComparer();

        public override IEnumerable<TElement> GetSource() => this._source.GetSource();
    }
}
