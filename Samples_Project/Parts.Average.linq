<Query Kind="Program">
  <Connection>
    <ID>5ab80da3-0340-4803-8b21-038154e63b5f</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>krdn-g713rm,1434</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAALqYKck3+v0akEg0HeHio4AAAAAACAAAAAAAQZgAAAAEAACAAAAD1qbEDLPYNe2sLtNBFxVmqDbAddj6v9Uxb8h8rwEGQdQAAAAAOgAAAAAIAACAAAAAM0/qQ/HhIJlDtfHaRBPpTraX/zACF/AfojL7GVzv02xAAAABwHB9TnO0LeXbdrVpWt5VoQAAAAAED7RB9X6sWrq3RfVim8VlfsURFqyhceD6ixBuChu91/mtPFoA2kjha3DgIResIAgnzYKm3g5ummZoD9LpIp3Q=</Password>
    <Database>SalesSimple</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <Reference Relative="..\Sales.dll">C:\01.Modeware\TUT_LINQPad\Sales.dll</Reference>
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>Dapper.SqlBuilder</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore.SqlServer</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Sales.Models</Namespace>
  <RuntimeVersion>8.0</RuntimeVersion>
</Query>


// SQL Server 연결 문자열
const string connectionString = "Data Source=krdn-g713rm,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

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


		//var sql = _query.ToQueryString();

		//Console.WriteLine("=========================================================");
		//Console.WriteLine(sql);
		//Console.WriteLine("=========================================================");
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