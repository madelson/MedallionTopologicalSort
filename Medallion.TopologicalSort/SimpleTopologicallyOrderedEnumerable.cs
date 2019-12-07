using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal class SimpleTopologicallyOrderedEnumerable<T> : TopologicallyOrderedEnumerable<T>, IEqualityComparer<ValueTuple<T>>
    {
        private readonly IEnumerable<T> _source;
        private readonly Func<T, IEnumerable<T>> _getDependencies;
        private readonly IEqualityComparer<T> _comparer;

        public SimpleTopologicallyOrderedEnumerable(
            IEnumerable<T> source,
            Func<T, IEnumerable<T>> getDependencies,
            IEqualityComparer<T> comparer)
        {
            this._source = source;
            this._comparer = comparer;
            this._getDependencies = getDependencies;
        }

        public override Func<int, int, bool> CreateIndexComparison(IReadOnlyList<T> items, Func<int, int, bool>? next)
        {
            Invariant.Require(next != null);
            return next!;
        }

        public override IQueue CreateQueue(IReadOnlyList<T> items) => new FifoQueue();

        public override Func<T, IEnumerable<T>> GetGetDependencies() => this._getDependencies;

        public override IEqualityComparer<ValueTuple<T>> GetItemComparer() => this;

        public override IEnumerable<T> GetSource() => this._source;

        bool IEqualityComparer<ValueTuple<T>>.Equals(ValueTuple<T> x, ValueTuple<T> y) => this._comparer.Equals(x.Item1, y.Item1);

        int IEqualityComparer<ValueTuple<T>>.GetHashCode(ValueTuple<T> obj) => this._comparer.GetHashCode(obj.Item1);
    }
}
