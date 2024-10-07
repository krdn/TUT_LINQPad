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
	// Don't remove outliers
	[Outliers(OutlierMode.DontRemove)]
	// Skip jitting, pilot, warmup; measure 10 iterations
	[SimpleJob(RunStrategy.Monitoring, iterationCount: 10, invocationCount: 1)]
	public class IntroRatioSD
	{
		private int counter;

		[GlobalSetup]
		public void Setup() => counter = 0;

		[Benchmark(Baseline = true)]
		public void Base()
		{
			Thread.Sleep(100);
			if (++counter % 7 == 0)
				Thread.Sleep(5000); // Emulate outlier
		}

		[Benchmark]
		public void Slow() => Thread.Sleep(200);

		[Benchmark]
		public void Fast() => Thread.Sleep(50);
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<IntroRatioSD>();
		}
	}
}
