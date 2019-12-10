# MedallionTopologicalSort

MedallionTopologicalSort is a fast implementation of [topological sorting](https://en.wikipedia.org/wiki/Topological_sorting) for .NET that supports stable sorting as well as breaking ties via `ThenBy[Descending]`.

MedallionTopologicalSort is available for download as a [NuGet package](https://www.nuget.org/packages/Medallion.TopologicalSort). [![NuGet Status](http://img.shields.io/nuget/v/Medallion.TopologicalSort.svg?style=flat)](https://www.nuget.org/packages/Medallion.TopologicalSort/)

[Release notes](#release-notes)

## Documentation

### Basic Usage

The `OrderByTopologically` operation is available as an extension method on `IEnumerable<T>`. The "graph" underlying the sort is defined on-the-fly by providing a function which can be used to map each element to the set of elements it depends on (must go before).

As a toy example, we could use topological sort to perform a normal sort of a range of integers by specifying that each integer has a dependency on the next integer in the sequence.

```C#
using Medallion.Collections;

...

var shuffledInts = Enumerable.Range(0, 10).OrderBy(_ => Guid.NewGuid());

var sortedInts = shuffledInts.OrderByTopologically(getDependencies: i => i > 0 ? new[] { i - 1 } : Enumerable.Empty<int>());

Console.WriteLine(string.Join(", ", sortedInts)); // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
```

### Stable sort

In many cases, topological sorting does not fully specify the order of a sequence of elements. For example, if we have [a, b, c] as elements and b depends on a, then all of the following would be valid topological sorts:
* [a, b, c]
* [a, c, b]
* [c, a, b]

By calling `StableOrderTopologicallyBy` instead of `OrderTopologicallyBy`, we further restrict the sort in a way that prefers maintaining the original order. In particular, *starting from the beginning, whenever the algorithm has a choice of elements to place as the ith element, it will prefer the element which has the lowest index in the original sequence*. 

In our example above, this would result in [a, b, c]. For the first element, the sort has a choice of a or c which both have no dependencies. Since a came first it choses that. For the second element, it has a choice of b or c, so it chooses b.

This form of sorting is useful when you want results that are predictable, or want to generally respect the provided order and only make changes where necessary.

### Secondary sort

Another way to "break ties" is to use a secondary sorting metric. This is accomplished by having `OrderTopologicallyBy` return an `IOrderedEnumerable<T>`, which allows for using `ThenBy` and `ThenByDescending` to further specify order.

As with stable sorting, the algorithm used starts from the beginning of the sequence and employs the tiebreaker sort whenever it has a choice of elements to place next.

Here's an example:
```C#
var sorted = new[] { "cart", "horse", "island", "apple" }
	// don't put the cart before the horse
	.OrderByTopologically(s => s == "cart" ? new[] { "horse" } : Enumerable.Empty<string>())
	// otherwise use alphabetical order
	.ThenBy(s => s);

Console.WriteLine(string.Join(", ", sorted)); // apple, horse, cart, island
```

Note that `StableOrderTopologicallyBy` does not return `IOrderedEnumerable<T>`, since a stable sort fully defines the output order and thus there is no need for a tie-breaker.

### Edge-case handling

The library makes an effort to handle edge-cases well and logically, for example:

* Dependency cycles (e. g. a depends on b depends on a) are detected and will result in an `InvalidOperationException` as the output sequence is enumerated.
* Invalid dependencies (e. g. a depends on x but x is not in the source enumerable) are detected and will result in an `InvalidOperationException` as the output sequence is enumerated.
* An optional `comparer` argument of type `IEqualityComparer<T>` can be provided to specify how items in the sequence are matched against dependency items.
* Duplicates (as defined by `comparer`) are allowed in the source sequence. In the case of duplicates, `getDependencies` will only be evaluated once for the item, and the resulting sequence will contain the same exact items as the source sequence.
* Duplicates can be returned by `getDependencies`; they have no effect on the algorithm (e. g. if a depends on b twice, it won't affect the sort).
* `null`s are allowed both in the source sequence and as dependencies.

### Performance

Performance is a focus of the library. The core topological sort algorithm is O(V + E), where V is the number of vertices (items in the source enumerable) and E is the number of edges (dependencies across all items). For stable or `ThenBy` sorts, the worst case is O(Vlog(V) + E), since at worst we have no edges and end up applying an O(Nlog(N)) sort to the entire sequence. In practice, though, it will often be closer to V + E. In addition to employing efficient algorithms, the library makes an effort to minimize allocations and other "overhead".

Here is the results of a [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) comparison of MedallionTopologicalSort against two other topological sorting libraries available on NuGet. As always with micro benchmarks, your particular usage pattern may yield different results:

| Library | Mean (smaller is better) | StdDev | Ratio (smaller is better) |
| ----------- | ----------- | ----------- | ----------- |
| MedallionTopologicalSort | 37.07 us | 0.139 us | 1.0.0 |
| [TopologicalSort](https://www.nuget.org/packages/TopologicalSort/) | 94.94 us | 0.318 us | 2.56 |
| [TopologicalSorting](https://www.nuget.org/packages/TopologicalSorting/) | 267.58 us | 16.923 us | 7.22 |

While these other libraries don't appear to be as fast as MedallionTopologicalSort, note that they each contain some other algorithms in addition to basic topological sort which may be useful. Don't discount them.

## Release notes
- 1.0.0 Initial release
