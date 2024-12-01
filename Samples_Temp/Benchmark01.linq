<Query Kind="Statements">
  <Connection>
    <ID>3da83162-ac14-4a65-83e9-c583bb0b3b68</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>192.168.0.18,1434</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAALqYKck3+v0akEg0HeHio4AAAAAACAAAAAAAQZgAAAAEAACAAAACE51B/BY++Os8WipGj1haBE+X/gF+YTYgkxNfWnE/SNAAAAAAOgAAAAAIAACAAAAD8pJ00axnAaEJi7foDePf1bttDQ/5K5aUgLDtNy1/SSBAAAACidrBahnDpOnGC4VhsXF4yQAAAAOqmPK6nINvNPWFKxNdUJ25afxSVFZFn0QPVbDgrFz53Olbam2X+D6d//vromwOebyr5aSuvNITDFYTDkmLpf8Q=</Password>
    <Database>SalesSimple</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
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
			var summary = BenchmarkRunner.Run<Md5VsSha256>();
		}
	}
}