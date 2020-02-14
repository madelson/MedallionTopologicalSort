using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Medallion.Collections;

namespace Medallion.TopologicalSort.Tests
{
    public class BasicTest
    {
        [Test]
        public void TestArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => default(int[]).OrderTopologicallyBy(i => Enumerable.Empty<int>()));
            Assert.Throws<ArgumentNullException>(() => new int[0].OrderTopologicallyBy(null));
            Assert.Throws<InvalidOperationException>(() => new[] { 1 }.OrderTopologicallyBy(i => null).First());

            Assert.Throws<ArgumentNullException>(() => default(int[]).StableOrderTopologicallyBy(i => Enumerable.Empty<int>()));
            Assert.Throws<ArgumentNullException>(() => new int[0].StableOrderTopologicallyBy(null));
            Assert.Throws<InvalidOperationException>(() => new[] { 1 }.StableOrderTopologicallyBy(i => null).First());

            Assert.Throws<ArgumentNullException>(() => new[] { 1 }.OrderTopologicallyBy(i => new int[0]).ThenBy(default(Func<int, string>)));
            Assert.Throws<ArgumentNullException>(() => new[] { 1 }.OrderTopologicallyBy(i => new int[0]).ThenByDescending(default(Func<int, string>)));
        }

        [Test]
        public void TestLinearCase()
        {
            var result = Enumerable.Range(0, 10)
                .OrderTopologicallyBy(i => i < 9 ? new[] { i + 1 } : Enumerable.Empty<int>())
                .ToArray();
            CollectionAssert.AreEqual(Enumerable.Range(0, 10).Reverse(), result);
        }

        [Test]
        public void TestNoop()
        {
            var items = new[] { "a", "b", "c" };
            var result = items.OrderTopologicallyBy(s => s switch { "b" => new[] { "a" }, "c" => new[] { "b" }, _ => Enumerable.Empty<string>() })
                .ToArray();
            CollectionAssert.AreEqual(items, result);
        }

        [Test]
        public void TestNoDependencies()
        {
            var result = Enumerable.Range(10, 20)
                .OrderTopologicallyBy(_ => Enumerable.Empty<int>())
                .ToArray();
            CollectionAssert.AreEqual(Enumerable.Range(10, 20), result);
        }

        [Test]
        public void TestStable()
        {
            var items = new[] { 'a', 'b', 'c', 'd', 'e' };
            var stableEnumerable = items.StableOrderTopologicallyBy(ch => ch switch { 'a' => new[] { 'd' }, 'c' => new[] { 'e' }, _ => Enumerable.Empty<char>() });
            var result = stableEnumerable.ToArray();
            CollectionAssert.AreEqual(
                new[] { 'b', 'd', 'a', 'e', 'c' },
                result
            );

            // ordered enumerables support ThenBy(), but a stable enumerable fully describes the ordering and as such it is not desirable/meaningful to support this
            Assert.IsNotInstanceOf<IOrderedEnumerable<char>>(stableEnumerable, "stable enumerable should not be castable to ordered enumerable");
        }

        [Test]
        public void TestThenBy()
        {
            var items = new[] { "house", "brick", "people", "wood", "holes", "mice" };
            var result = items.OrderTopologicallyBy(s => s switch 
                { 
                    "house" => new[] { "brick", "wood" }, 
                    "people" => new[] { "house" },
                    "holes" => new[] { "wood", "mice" },
                    "mice" => new[] { "house" }, 
                    _ => Enumerable.Empty<string>(),
                })
                .ThenBy(s => s)
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { "brick", "wood", "house", "mice", "holes", "people" },
                result
            );
        }

        [Test]
        public void TestThenByDescending()
        {
            var items = Enumerable.Range(0, 9);
            var result = items.OrderTopologicallyBy(i => i == 7 ? new[] { 2 } : Enumerable.Empty<int>())
                .ThenByDescending(i => i)
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { 8, 6, 5, 4, 3, 2, 7, 1, 0 },
                result
            );
        }

        [Test]
        public void TestNestedThenBy()
        {
            var items = new[] { "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
            var result = items.OrderTopologicallyBy(s => s == "dog" ? new[] { "fox", "lazy" } : Enumerable.Empty<string>())
                .ThenByDescending(i => i.Contains("o"))
                .ThenBy(i => i)
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { "brown", "fox", "over", "jumps", "lazy", "dog", "quick", "the", "the" },
                result
            );
        }

        [Test]
        public void TestThenByDoesNotEvaluateKeySelectorMoreThanOncePerItem()
        {
            var items = Enumerable.Range(0, 100).ToArray();
            var keySelectorCounts = new int[items.Length];
            var result = items.OrderTopologicallyBy(i => Array.Empty<int>())
                .ThenBy(i => { ++keySelectorCounts[i]; return -i; })
                .ToArray();
            CollectionAssert.AreEqual(items.Reverse(), result);
            Assert.IsEmpty(keySelectorCounts.Where(c => c > 1));
        }

        [Test]
        public void TestCycleDetection()
        {
            var items = Enumerable.Range(0, 20);
            var result = items.OrderTopologicallyBy(i => new[] { (i + 1) % 20 });
            var ex = Assert.Throws<InvalidOperationException>(() => result.First());
            Assert.That(ex.Message, Does.Contain("dependency cycle detected"));
        }

        [Test]
        public void TestBadDependencyDetection()
        {
            var items = new[] { "a", "aa", "aaa" };
            var result = items.OrderTopologicallyBy(s => new[] { s.Substring(1) });
            var ex = Assert.Throws<InvalidOperationException>(() => result.First());
            Assert.That(ex.Message, Does.Contain("an element has a dependency that is not in source"));
        }

        [Test]
        public void TestDuplicates()
        {
            var items = new[] { 'a', 'a', 'a', 'b', 'b', 'c' };
            var result = items.OrderTopologicallyBy(ch => ch switch { 'a' => new[] { 'b' }, 'b' => new[] { 'c' }, _ => Enumerable.Empty<char>() })
                .ToArray();
            CollectionAssert.AreEqual(items.Reverse(), result);
        }

        [Test]
        public void TestDuplicatesWithStableSort()
        {
            var items = new[] { 1, 2, 3, 4, 5, 2, 3, 4, 3 };
            var result = items.StableOrderTopologicallyBy(i => i == 2 ? new[] { 4 } : Enumerable.Empty<int>())
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { 1, 3, 4, 2, 5, 2, 3, 4, 3 },
                result
            );
        }

        [Test]
        public void TestDependencyListsOfDuplicatesAreNotEnumerated()
        {
            var items = Enumerable.Repeat('a', 10).Append('b');
            var enumerated = false;
            var result = items.OrderTopologicallyBy(
                    ch =>
                    {
                        if (ch == 'a')
                        {
                            Assert.IsFalse(enumerated);
                            enumerated = true;
                            return new[] { 'b' };
                        }
                        return Enumerable.Empty<char>();
                    }
                )
                .ToArray();
            Assert.IsTrue(enumerated);
            CollectionAssert.AreEqual(items.Reverse(), result);
        }

        [Test]
        public void TestDuplicateDependencies()
        {
            var result = Enumerable.Range(0, 10)
                .OrderTopologicallyBy(i => i < 9 ? Enumerable.Repeat(i + 1, 100) : Enumerable.Empty<int>())
                .ToArray();
            CollectionAssert.AreEqual(Enumerable.Range(0, 10).Reverse(), result);
        }

        [Test]
        public void TestCustomComparer()
        {
            var items = new[] { "AAB", "aa", "A", "Abb", "B", "aB", string.Empty, };
            var result = items.StableOrderTopologicallyBy(
                    s => s.Length == 0 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1) }, 
                    StringComparer.OrdinalIgnoreCase
                )
                .ToArray();
            CollectionAssert.AreEqual(
                new[] { string.Empty, "A", "aa", "AAB", "B", "aB", "Abb" },
                result
            );
        }

        [Test]
        public void TestNullItems()
        {
            var items = new[] { "a", "b", "c", null };
            var result = items.OrderTopologicallyBy(
                    s => s != null ? new[] { default(string) } : Enumerable.Empty<string>()
                )
                .ThenByDescending(s => s)
                .ToArray();
            CollectionAssert.AreEqual(new[] { null, "c", "b", "a" }, result);
        }
    }
}
