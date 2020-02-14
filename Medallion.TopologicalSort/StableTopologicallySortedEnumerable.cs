using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Implements stable topological sorting
    /// </summary>
    internal sealed class StableTopologicallySortedEnumerable<TElement> : TopologicallySortedEnumerableBase<TElement>
    {
        public StableTopologicallySortedEnumerable(
            IEnumerable<TElement> source,
            Func<TElement, IEnumerable<TElement>> getDependencies,
            IEqualityComparer<TElement> comparer)
            : base(source, getDependencies, comparer)
        {
        }

        public override IQueue CreateQueue(IReadOnlyList<TElement> items) => new PriorityQueue((index1, index2) => index1<index2, maxCapacity: items.Count);
    }
}
