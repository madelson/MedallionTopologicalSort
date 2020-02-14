using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Interface representing the inputs to a topological sort routine
    /// </summary>
    internal interface ITopologicalSortProvider<TElement>
    {
        /// <summary>
        /// Gets the sequence to be sorted
        /// </summary>
        IEnumerable<TElement> GetSource();
        
        /// <summary>
        /// Gets a comparer for items in the sequence. <see cref="ValueTuple{T1}"/> is used
        /// to allow for proper handling of null references as dictionary keys
        /// </summary>
        IEqualityComparer<ValueTuple<TElement>> GetItemComparer();
        
        /// <summary>
        /// Creates a data structure which is used to queue up items that are candidates for being the next item yielded
        /// by the sort
        /// </summary>
        IQueue CreateQueue(IReadOnlyList<TElement> items);

        /// <summary>
        /// Gets a "getDependencies" function which, given an element, returns the set of elements which must appear before
        /// it in the topologically sorted sequence
        /// </summary>
        Func<TElement, IEnumerable<TElement>> GetGetDependencies();
    }

    /// <summary>
    /// Extends <see cref="ITopologicalSortProvider{TElement}"/> by providing support for tie-breaker sorting
    /// </summary>
    internal interface IThenByTopologicalSortProvider<TElement> : ITopologicalSortProvider<TElement>
    {
        /// <summary>
        /// Returns a function which, given two indices from elements in <paramref name="items"/>, returns
        /// true if the first should come before the second (a "less" function).
        /// 
        /// If <paramref name="next"/> is provided, it should be used as a tie-breaker.
        /// </summary>
        Func<int, int, bool> CreateIndexComparison(IReadOnlyList<TElement> items, Func<int, int, bool>? next);
    }
}
