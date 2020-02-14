using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Implements a standard topological sort, with the option of adding on a secondary sort
    /// </summary>
    internal sealed class TopologicallySortedOrderedEnumerable<TElement> : TopologicallySortedEnumerableBase<TElement>, IThenByTopologicalSortProvider<TElement>, IOrderedEnumerable<TElement>
    {
        public TopologicallySortedOrderedEnumerable(
            IEnumerable<TElement> source, 
            Func<TElement, IEnumerable<TElement>> getDependencies, 
            IEqualityComparer<TElement> comparer) 
            : base(source, getDependencies, comparer)
        {
        }

        public override IQueue CreateQueue(IReadOnlyList<TElement> items) => new FifoQueue();

        Func<int, int, bool> IThenByTopologicalSortProvider<TElement>.CreateIndexComparison(IReadOnlyList<TElement> items, Func<int, int, bool>? next)
        {
            Invariant.Require(next != null);
            return next!;
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) =>
            TopologicalSorter.CreateOrderedEnumerable(this, keySelector, comparer, descending);
    }
}
