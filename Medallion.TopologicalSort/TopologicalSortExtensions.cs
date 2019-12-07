using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    public static class TopologicalSortExtensions
    {
        public static IOrderedEnumerable<TSource> OrderTopologicallyBy<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> getDependencies, IEqualityComparer<TSource>? comparer = null) =>
            new SimpleTopologicallyOrderedEnumerable<TSource>(
                source ?? throw new ArgumentNullException(nameof(source)),
                getDependencies ?? throw new ArgumentNullException(nameof(getDependencies)),
                comparer ?? EqualityComparer<TSource>.Default
            );

        public static IEnumerable<TSource> StableOrderTopologicallyBy<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> getDependencies, IEqualityComparer<TSource>? comparer = null) =>
            new StableTopologicallyOrderedEnumerable<TSource>(
                source ?? throw new ArgumentNullException(nameof(source)),
                getDependencies ?? throw new ArgumentNullException(nameof(getDependencies)),
                comparer ?? EqualityComparer<TSource>.Default
            );
    }
}
