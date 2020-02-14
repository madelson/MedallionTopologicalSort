using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Contains the actual topological sort algorithm
    /// </summary>
    internal static class TopologicalSorter
    {
        public static IEnumerator<TElement> TopologicalSort<TElement>(ITopologicalSortProvider<TElement> provider)
        {
            // setup

            var items = provider.GetSource().ToList();

            var itemsToIndices = new Dictionary<ValueTuple<TElement>, int>(provider.GetItemComparer());
            Dictionary<int, List<int>>? duplicates = null;
            for (var i = 0; i < items.Count; ++i)
            {
                var itemKey = new ValueTuple<TElement>(items[i]);
                if (itemsToIndices.TryGetValue(itemKey, out var existingIndex))
                {
                    duplicates ??= new Dictionary<int, List<int>>();
                    if (!duplicates.TryGetValue(existingIndex, out var list))
                    {
                        duplicates.Add(existingIndex, list = new List<int>());
                    }
                    list.Add(i);
                }
                else
                {
                    itemsToIndices.Add(itemKey, i);
                }
            }

            var incomingDependencyIndexLinkedLists = new List<(int dependencyIndex, int next)>();
            var dependencyInfo = new (int dependencyCount, int incomingDependencyIndexLinkedListHead)[items.Count];
            for (var i = 0; i < dependencyInfo.Length; ++i) { dependencyInfo[i].incomingDependencyIndexLinkedListHead = -1; };
            var yieldableIndices = provider.CreateQueue(items);
            var getDependencies = provider.GetGetDependencies();
            foreach (var i in itemsToIndices.Values)
            {
                foreach (var dependency in getDependencies(items[i]) ?? throw SortError(nameof(getDependencies) + " may not return null"))
                {
                    if (itemsToIndices.TryGetValue(new ValueTuple<TElement>(dependency), out var dependencyIndex))
                    {
                        // Note that right now we add a new entry for each dependency, even when there are duplicates. While this takes some extra storage
                        // space in the linked lists, it has no correctness consequences because when the dependency gets yielded it will enumerate each entry
                        // and end up decrementing the count for i the right number of times. This isn't even costly in terms of runtime since we only push each
                        // dependency entry once and pop it once. Given that I expect the duplicate dependency case to be rare in practice, adding additional checks
                        // here to avoid the extra space doesn't feel like a good tradeoff
                        incomingDependencyIndexLinkedLists.Add((dependencyIndex: i, next: dependencyInfo[dependencyIndex].incomingDependencyIndexLinkedListHead));
                        dependencyInfo[dependencyIndex].incomingDependencyIndexLinkedListHead = incomingDependencyIndexLinkedLists.Count - 1;
                        checked { ++dependencyInfo[i].dependencyCount; }
                    }
                    else
                    {
                        throw SortError("an element has a dependency that is not in source.");
                    }
                }

                if (dependencyInfo[i].dependencyCount == 0)
                {
                    EnqueueYieldable(i);
                }
            }

            // sort

            for (var i = 0; i < items.Count; ++i)
            {
                if (yieldableIndices.Count == 0)
                {
                    throw SortError("dependency cycle detected");
                }

                var nextYieldableIndex = yieldableIndices.Dequeue();
                yield return items[nextYieldableIndex];

                var current = dependencyInfo[nextYieldableIndex].incomingDependencyIndexLinkedListHead;
                while (current != -1)
                {
                    var (dependencyIndex, next) = incomingDependencyIndexLinkedLists[current];
                    if (--dependencyInfo[dependencyIndex].dependencyCount == 0)
                    {
                        EnqueueYieldable(dependencyIndex);
                    }
                    current = next;
                }
            }

            void EnqueueYieldable(int index)
            {
                yieldableIndices.Enqueue(index);
                if (duplicates != null && duplicates.TryGetValue(index, out var duplicateList))
                {
                    foreach (var duplicateIndex in duplicateList) { yieldableIndices.Enqueue(duplicateIndex); }
                }
            }
        }

        private static Exception SortError(string message) => new InvalidOperationException("Unable to complete topological sort: " + message);

        public static IOrderedEnumerable<TElement> CreateOrderedEnumerable<TElement, TKey>(
            IThenByTopologicalSortProvider<TElement> provider, 
            Func<TElement, TKey> keySelector, 
            IComparer<TKey>? comparer, 
            bool descending)
        {
            if (keySelector == null) { throw new ArgumentNullException(nameof(keySelector)); }

            return new ThenByTopologicallySortedOrderedEnumerable<TElement, TKey>(
                provider,
                keySelector,
                comparer,
                descending
            );
        }
    }
}
