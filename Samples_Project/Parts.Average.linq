<Query Kind="Program">
  <Connection>
    <ID>bbcf5935-9806-400e-a7e9-d7b517411e01</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>localhost, 1434</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAieqJCoaNHE2RMbKFalFqPAAAAAACAAAAAAAQZgAAAAEAACAAAAAKqmk+VTP4YzwbLJaqwh/pfR4iP2ztUIqaDCPZIDSAXAAAAAAOgAAAAAIAACAAAABmgK8osdT3JhfhSwg1FetjqNDSPYhEFDKBGwGhzv35nxAAAAAdH/NpgG73qmgMtGmN4NjaQAAAAPUeID2k5/G2ZFEDOfzJs15B//Bm/5aEB2+6eJINHp6PpP9+WiB0RHpAvPy8t1mZQ2O5Sqkd+NxGj8N9pO1omrc=</Password>
    <Database>SalesSimple</Database>
  </Connection>
  <Reference Relative="..\..\EFCoreDBTuningforSQLServer-Demos\Sales\Sales\bin\Release\net7.0\Sales.dll">D:\30.Modetour\03.Tutorials\EFCoreDBTuningforSQLServer-Demos\Sales\Sales\bin\Release\net7.0\Sales.dll</Reference>
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>Dapper.SqlBuilder</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Sales.Models</Namespace>
  <RuntimeVersion>8.0</RuntimeVersion>
</Query>


// SQL Server 연결 문자열
const string connectionString = "Data Source=localhost,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 10)]
public class BenchmarkTests
{
	//[Params(true, false)]  // 둘다 실행
	[Params(true)]  // 캐시 사용
	public bool UseCache { get; set; } // 캐시 사용 여부 설정

	private readonly SqlConnection _sqlConnection = new(connectionString);
	private SalesContext _dbContext;

	private decimal _averagePrice; // 미리 계산된 Average 값을 저장할 변수

	[GlobalSetup]
	public void Setup()
	{
		_dbContext = new SalesContext();

		_sqlConnection.Open();
		_dbContext.Database.EnsureCreated();

		_averagePrice = _dbContext.Parts.Average(c => c.RetailPrice); // 평균값을 한 번만 계산
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_sqlConnection.Close();
		_dbContext.Dispose();
	}


	[Benchmark(Baseline = true)]
	public void Test_EF()
	{
		if (!UseCache) ClearCache();

		var result = from r in _dbContext.Parts
					 select new
					 {
						 r.Name,
						 r.RetailPrice,
						 Average = _dbContext.Parts.Average(c => c.RetailPrice),
						 Diff = r.RetailPrice - _dbContext.Parts.Average(c => c.RetailPrice)
					 };

	}

	[Benchmark]
	public void Test_EF0()
	{
		if (!UseCache) ClearCache();

		var result = from r in _dbContext.Parts
					 select new
					 {
						 r.Name,
						 r.RetailPrice,
						 Average = _averagePrice,
						 Diff = r.RetailPrice - _averagePrice
					 };

	}


	// SQL Server 캐시를 비우는 메서드
	private void ClearCache()
	{
		using var clearCommand = new SqlCommand("DBCC FREEPROCCACHE; DBCC DROPCLEANBUFFERS;", _sqlConnection);
		clearCommand.ExecuteNonQuery();
	}
}


// 벤치마크 실행 코드
void Main()
{
	var config = ManualConfig.Create(DefaultConfig.Instance)
				.WithOptions(ConfigOptions.DisableOptimizationsValidator);

	var summary = BenchmarkRunner.Run<BenchmarkTests>(config);

	//var summary = BenchmarkRunner.Run<BenchmarkTests>();
	//Console.WriteLine(summary);
}