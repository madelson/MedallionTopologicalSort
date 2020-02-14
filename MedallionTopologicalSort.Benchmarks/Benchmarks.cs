using BenchmarkDotNet.Running;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Medallion.Collections.Benchmarks
{
    [ShortRunJob]
    public class Benchmarks
    {
        private static readonly int[] Numbers;
        private static readonly Dictionary<int, int[]> Dependencies;

        static Benchmarks()
        {
            var random = new Random(12345);
            Numbers = Enumerable.Range(0, 100).OrderBy(i => random.Next()).ToArray();

            Dependencies = new Dictionary<int, int[]>(Numbers.Length);
            var dependencies = new HashSet<int>();
            foreach (var i in Numbers)
            {
                dependencies.Clear();
                var dependencyCount = Math.Min(i, random.Next(10));
                while (dependencies.Count < dependencyCount)
                {
                    dependencies.Add(random.Next(i));
                }
                Dependencies.Add(i, dependencies.ToArray());
            }
        }

        [Test]
        [Benchmark(Baseline = true)]
        public void MedallionTopologicalSort()
        {
            var sorted = Numbers.OrderTopologicallyBy(i => Dependencies[i]);
            foreach (var _ in sorted) { }
        }

        [Test]
        [Benchmark]
        public void TopologicalSort()
        {
            var sorted = global::TopologicalSort.TopologicalSorter.TopologicalSort(
                Numbers,
                Dependencies.SelectMany(kvp => kvp.Value, (kvp, v) => new global::TopologicalSort.Edge<int>(from: kvp.Key, to: v))
            );
            foreach (var _ in sorted) { }
        }

        [Test]
        [Benchmark]
        public void TopologicalSorting()
        {
            var dependencyGraph = new TopologicalSorting.DependencyGraph();
            var orderedProcesses = Numbers.ToDictionary(i => i, i => new TopologicalSorting.OrderedProcess(dependencyGraph, null));
            foreach (var kvp in Dependencies)
            {
                var from = orderedProcesses[kvp.Key];
                foreach (var dependency in kvp.Value)
                {
                    from.Before(orderedProcesses[dependency]);
                }
            }
            IEnumerable<TopologicalSorting.OrderedProcess> sorted = dependencyGraph.CalculateSort();
            foreach (var _ in sorted) { }
        }

        [Test]
        public void Run()
        {
#if DEBUG
            Assert.Inconclusive("Benchmarks require the release build");
#endif

            var summary = BenchmarkRunner.Run<Benchmarks>();
            TestStatistic(s => s.Mean, "mean");
            TestStatistic(s => s.Median, "median");

            void TestStatistic(Func<BenchmarkDotNet.Mathematics.Statistics, double> statistic, string name)
            {
                Assert.AreSame(
                    summary.Reports.Single(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == nameof(MedallionTopologicalSort)),
                    summary.Reports.OrderBy(r => statistic(r.ResultStatistics)).First(),
                    name
                );
            }
        }
    }
}
