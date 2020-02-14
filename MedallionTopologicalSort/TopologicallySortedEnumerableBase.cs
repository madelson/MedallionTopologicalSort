using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Base implementation of <see cref="ITopologicalSortProvider{TElement}"/> with <see cref="IEnumerable{T}"/>
    /// </summary>
    internal abstract class TopologicallySortedEnumerableBase<TElement> : ITopologicalSortProvider<TElement>, IEnumerable<TElement>, IEqualityComparer<ValueTuple<TElement>>
    {
        private readonly IEnumerable<TElement> _source;
        private readonly Func<TElement, IEnumerable<TElement>> _getDependencies;
        private readonly IEqualityComparer<TElement> _comparer;

        protected TopologicallySortedEnumerableBase(
            IEnumerable<TElement> source,
            Func<TElement, IEnumerable<TElement>> getDependencies,
            IEqualityComparer<TElement> comparer)
        {
            this._source = source;
            this._getDependencies = getDependencies;
            this._comparer = comparer;
        }

        IEnumerable<TElement> ITopologicalSortProvider<TElement>.GetSource() => this._source;
        IEqualityComparer<ValueTuple<TElement>> ITopologicalSortProvider<TElement>.GetItemComparer() => this;
        public abstract IQueue CreateQueue(IReadOnlyList<TElement> items);
        Func<TElement, IEnumerable<TElement>> ITopologicalSortProvider<TElement>.GetGetDependencies() => this._getDependencies;

        bool IEqualityComparer<ValueTuple<TElement>>.Equals(ValueTuple<TElement> x, ValueTuple<TElement> y) => this._comparer.Equals(x.Item1, y.Item1);

        int IEqualityComparer<ValueTuple<TElement>>.GetHashCode(ValueTuple<TElement> obj) => this._comparer.GetHashCode(obj.Item1);

        public IEnumerator<TElement> GetEnumerator() => TopologicalSorter.TopologicalSort(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

}
