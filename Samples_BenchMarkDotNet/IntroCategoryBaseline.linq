<Query Kind="Program">
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Engines</Namespace>
  <Namespace>BenchmarkDotNet.Environments</Namespace>
  <Namespace>BenchmarkDotNet.Jobs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Perfolizer.Mathematics.OutlierDetection</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

namespace BenchmarkDotNet.Samples
{
	[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
	[CategoriesColumn]
	public class IntroCategoryBaseline
	{
		[BenchmarkCategory("Fast"), Benchmark(Baseline = true)]
		public void Time50() => Thread.Sleep(50);

		[BenchmarkCategory("Fast"), Benchmark]
		public void Time100() => Thread.Sleep(100);

		[BenchmarkCategory("Slow"), Benchmark(Baseline = true)]
		public void Time550() => Thread.Sleep(550);

		[BenchmarkCategory("Slow"), Benchmark]
		public void Time600() => Thread.Sleep(600);
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<IntroCategoryBaseline>();
		}
	}
}
