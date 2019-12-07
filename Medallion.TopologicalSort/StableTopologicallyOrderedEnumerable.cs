using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal sealed class StableTopologicallyOrderedEnumerable<T> : SimpleTopologicallyOrderedEnumerable<T>
    {
        public StableTopologicallyOrderedEnumerable(
            IEnumerable<T> source,
            Func<T, IEnumerable<T>> getDependencies,
            IEqualityComparer<T> comparer)
            : base(source, getDependencies, comparer)
        {
        }

        public override IQueue CreateQueue(IReadOnlyList<T> items) => new PriorityQueue((index1, index2) => index1 < index2, maxCapacity: items.Count);

        // todo revisit: should this ever be called?
        public override Func<int, int, bool> CreateIndexComparison(IReadOnlyList<T> items, Func<int, int, bool>? next)
        {
            return next ?? Less;

            bool Less(int index1, int index2)
            {
                Invariant.Require(index1 != index2);
                return index1 < index2;
            }
        }
    }
}
