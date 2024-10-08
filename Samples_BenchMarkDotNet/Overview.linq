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

namespace MyBenchmarks
{
	public class Md5VsSha256
	{
		private const int N = 10000;
		private readonly byte[] data;

		private readonly SHA256 sha256 = SHA256.Create();
		private readonly MD5 md5 = MD5.Create();

		public Md5VsSha256()
		{
			data = new byte[N];
			new Random(42).NextBytes(data);
		}

		[Benchmark]
		public byte[] Sha256() => sha256.ComputeHash(data);

		[Benchmark]
		public byte[] Md5() => md5.ComputeHash(data);
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			//var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
			var summary = BenchmarkRunner.Run<Md5VsSha256>();
			//var summary = BenchmarkRunner.Run(typeof(Md5VsSha256));
			//var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
