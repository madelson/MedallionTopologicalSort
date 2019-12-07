using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medallion.Collections;
using System.Runtime.CompilerServices;

namespace Medallion.TopologicalSort.Tests
{
    public class FuzzTest
    {
        [Test, Combinatorial]
        public void TestFuzz(
            [Values(1, 2, 3, 4, 5)] int randomSeed, 
            [Values] SecondarySortType secondarySortType)
        {
            var items = Generate((randomSeed, secondarySortType).GetHashCode(), out var dependencies);
            IEnumerable<string> GetDependencies(string s) => dependencies.TryGetValue(s, out var itemDependencies) ? itemDependencies : Enumerable.Empty<string>();

            switch (secondarySortType)
            {
                case SecondarySortType.None:
                    Validate(
                        items: items,
                        dependencies: dependencies,
                        sorted: items.OrderTopologicallyBy(GetDependencies)
                    );
                    break;
                case SecondarySortType.Stable:
                    Validate(
                        items: items,
                        dependencies: dependencies,
                        sorted: items.StableOrderTopologicallyBy(GetDependencies),
                        stable: true
                    );
                    break;
                case SecondarySortType.Reverse:
                    Validate(
                        items: items,
                        dependencies: dependencies,
                        sorted: items.OrderTopologicallyBy(GetDependencies).ThenByDescending(i => i),
                        tieBreaker: (a, b) => b.CompareTo(a)
                    );
                    break;
                case SecondarySortType.Mod3ThenReverse:
                    Validate(
                        items: items,
                        dependencies: dependencies,
                        sorted: items.OrderTopologicallyBy(GetDependencies).ThenBy(i => int.Parse(i) % 3).ThenByDescending(i => i),
                        tieBreaker: (a, b) =>
                        {
                            var mod3Comparison = (int.Parse(a) % 3).CompareTo(int.Parse(b) % 3);
                            return mod3Comparison != 0 ? mod3Comparison : b.CompareTo(a);
                        }
                    );
                    break;
            }
        }

        public enum SecondarySortType
        {
            None,
            Stable,
            Reverse,
            Mod3ThenReverse,
        }

        private static void Validate(
            IReadOnlyList<string> items,
            Dictionary<string, List<string>> dependencies,
            IEnumerable<string> sorted,
            bool stable = false,
            Comparison<string> tieBreaker = null)
        {
            var sortedItems = sorted.ToList();

            // sequence check
            CollectionAssert.AreEquivalent(items, sortedItems);
            var itemsByStringValue = items.ToLookup(i => i);
            Assert.IsEmpty(items.Where(i => !itemsByStringValue[i].Contains(i, ByReferenceComparer.Instance)), "object identity of duplicates not preserved");

            var flatDependencies = sortedItems.Distinct()
                .ToDictionary(i => i, i => GetAllDependencies(i, dependencies));

            // topological check
            {
                var seen = new HashSet<string>();
                foreach (var item in sortedItems)
                {
                    if (flatDependencies[item].Except(seen).Any())
                    {
                        Assert.Fail($"{item} depends on {string.Join(", ", flatDependencies[item].Except(seen))} which have not appeared yet");
                    }
                    seen.Add(item);
                }
            }

            // stability check
            if (stable)
            {
                var originalIndicesByItem = items.Select((item, index) => (item, index))
                    .ToDictionary(t => t.item, t => t.index, ByReferenceComparer.Instance);
                ValidateSecondaryOrdering((a, b) => originalIndicesByItem[a].CompareTo(originalIndicesByItem[b]));
            }

            if (tieBreaker != null)
            {
                ValidateSecondaryOrdering(tieBreaker);
            }

            void ValidateSecondaryOrdering(Comparison<string> comparison)
            {
                var seen = new HashSet<int>();
                for (var i = 1; i < sortedItems.Count; ++i)
                {
                    var previous = sortedItems[i - 1];
                    var current = sortedItems[i];

                    if (comparison(previous, current) > 0
                        && !flatDependencies[current].Contains(previous))
                    {
                        Assert.Fail($"{previous} appears before {current}, but {current} does not depend on {previous}");
                    }
                }
            }
        }

        private static IReadOnlyList<string> Generate(int randomSeed, out Dictionary<string, List<string>> dependencies)
        {
            var random = new Random(randomSeed);

            var items = new string[random.Next(50, 150)];
            for (var i = 0; i < items.Length; ++i)
            {
                // copy to ensure unique object identity. This is helpful for stability testing
                items[i] = string.Copy(random.Next(items.Length).ToString());
            }

            var edgeCount = random.Next(0, (items.Length * items.Length) / 2);
            dependencies = new Dictionary<string, List<string>>();
            for (var i = 0; i < edgeCount; ++i)
            {
                var from = items[random.Next(items.Length)];
                var to = items[random.Next(items.Length)];
                if (from != to
                    && !GetAllDependencies(to, dependencies).Contains(from))
                {
                    if (dependencies.TryGetValue(from, out var existing))
                    {
                        existing.Add(to);
                    }
                    else
                    {
                        dependencies.Add(from, new List<string> { to });
                    }
                }
            }

            return items;
        }

        private static HashSet<string> GetAllDependencies(string value, Dictionary<string, List<string>> dependencies)
        {
            var results = new HashSet<string>();
            Visit(value);
            return results;

            void Visit(string value)
            {
                if (dependencies.TryGetValue(value, out var valueDependencies))
                {
                    foreach (var valueDependency in valueDependencies)
                    {
                        if (results.Add(valueDependency))
                        {
                            Visit(valueDependency);
                        }
                    }
                }
            }
        }
    }

    internal class ByReferenceComparer : IEqualityComparer<string>
    {
        public static readonly ByReferenceComparer Instance = new ByReferenceComparer();

        public bool Equals(string x, string y) => ReferenceEquals(x, y);

        public int GetHashCode(string obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
