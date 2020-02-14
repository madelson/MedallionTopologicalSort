using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Contains extension methods related to topological sorting
    /// </summary>
    public static class TopologicalSortExtensions
    {
        /// <summary>
        /// Given an <see cref="IEnumerable{T}"/> <paramref name="source"/>, returns an <see cref="IOrderedEnumerable{TElement}"/>
        /// that is sorted topologically. In other words, any element X in the returned <see cref="IOrderedEnumerable{TElement}"/>
        /// will occur after the elements X depends on as determined by the specified <paramref name="getDependencies"/> function.
        /// 
        /// Because the result of this function is <see cref="IOrderedQueryable{T}"/>, the sort may be further refined using
        /// <see cref="Enumerable.ThenBy{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey})"/>. In this case, as the sort
        /// proceeds, the secondary comparison is used to determine which element among a group of elements whose dependencies have
        /// already appeared in output will appear first.
        /// </summary>
        /// <typeparam name="TSource">the <see cref="IEnumerable{T}"/> element type</typeparam>
        /// <param name="source">the starting <see cref="IEnumerable{T}"/></param>
        /// <param name="getDependencies">given an element, returns a set of elements which must appear before it in topological order</param>
        /// <param name="comparer">
        /// Optionally specifies an equality relationship for elements. This is used, for example, when comparing the output of
        /// <paramref name="getDependencies"/> to the elements of <paramref name="source"/>
        /// </param>
        /// <returns><paramref name="source"/>, in topological order.</returns>
        public static IOrderedEnumerable<TSource> OrderTopologicallyBy<TSource>(
            this IEnumerable<TSource> source, 
            Func<TSource, IEnumerable<TSource>> getDependencies, 
            IEqualityComparer<TSource>? comparer = null) =>
            new TopologicallySortedOrderedEnumerable<TSource>(
                source ?? throw new ArgumentNullException(nameof(source)),
                getDependencies ?? throw new ArgumentNullException(nameof(getDependencies)),
                comparer ?? EqualityComparer<TSource>.Default
            );

        /// <summary>
        /// Given an <see cref="IEnumerable{T}"/> <paramref name="source"/>, returns an <see cref="IOrderedEnumerable{TElement}"/>
        /// that is sorted topologically. In other words, any element X in the returned <see cref="IOrderedEnumerable{TElement}"/>
        /// will occur after the elements X depends on as determined by the specified <paramref name="getDependencies"/> function.
        /// 
        /// Unlike <see cref="OrderTopologicallyBy{TSource}(IEnumerable{TSource}, Func{TSource, IEnumerable{TSource}}, IEqualityComparer{TSource})"/>,
        /// this function performs a "stable" sort. This means that, given a choice of which element to place next in the resulting sequence, the
        /// algorithm will always prefer the element with the lowest index in the original sequence. As a result, if <paramref name="source"/> is already
        /// in topological order then the output sequence will be the same as <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">the <see cref="IEnumerable{T}"/> element type</typeparam>
        /// <param name="source">the starting <see cref="IEnumerable{T}"/></param>
        /// <param name="getDependencies">given an element, returns a set of elements which must appear before it in topological order</param>
        /// <param name="comparer">
        /// Optionally specifies an equality relationship for elements. This is used, for example, when comparing the output of
        /// <paramref name="getDependencies"/> to the elements of <paramref name="source"/>
        /// </param>
        /// <returns><paramref name="source"/>, in topological order.</returns>
        public static IEnumerable<TSource> StableOrderTopologicallyBy<TSource>(
            this IEnumerable<TSource> source, 
            Func<TSource, IEnumerable<TSource>> getDependencies, 
            IEqualityComparer<TSource>? comparer = null) =>
            new StableTopologicallySortedEnumerable<TSource>(
                source ?? throw new ArgumentNullException(nameof(source)),
                getDependencies ?? throw new ArgumentNullException(nameof(getDependencies)),
                comparer ?? EqualityComparer<TSource>.Default
            );
    }
}
