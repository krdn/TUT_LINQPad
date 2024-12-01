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
</Query>


// SQL Server 연결 문자열
const string connectionString = "Data Source=localhost,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

public class BenchmarkTests
{
	//[Params(true, false)]
	[Params(true)]
	public bool UseCache { get; set; } // 캐시 사용 여부 설정

	private readonly SqlConnection _sqlConnection = new(connectionString);
	private readonly SalesContext _dbContext = new SalesContext();
	private int _custkey = 10954;

	[GlobalSetup]
	public void Setup()
	{
		_sqlConnection.Open();
		_dbContext.Database.EnsureCreated();
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_sqlConnection.Close();
		_dbContext.Dispose();
	}
	
	

	[Benchmark(Baseline = true)]
	public void Test_SP_Dapper()
	{
		if (!UseCache) ClearCache();
		
		var dictionary = new Dictionary<string, object>
			{
				{ "@CustKey", _custkey }
			};
		var parameters = new DynamicParameters(dictionary);

		using (var connection = new SqlConnection(connectionString))
		{
			var product = connection.Query("OrderByCustKey", parameters);
		}

	}

	[Benchmark]
	public void Test_SP_EF()
	{
		if (!UseCache) ClearCache();
		
		var employees = _dbContext.Orders
			.FromSqlRaw("EXEC dbo.OrderByCustKey @CustKey = {0}", _custkey)
			.ToList();

	}

	[Benchmark]
	public void Test_EF_ExcuteSQL()
	{
		if (!UseCache) ClearCache();
		
		var employees = _dbContext.Orders
			.FromSqlRaw("SELECT * FROM Orders WHERE CustKey = {0}", _custkey)
			.ToList();
	}
	


	[Benchmark]
	public void Test_EF()
	{		
		if (!UseCache) ClearCache();
		
		var Orders1 = _dbContext.Orders
			.Where(o => o.CustKey == _custkey)
			.OrderByDescending(o => o.OrderDate)
			.ToList();
	}

	// SQL Server 캐시를 비우는 메서드
	private void ClearCache()
	{
		using var clearCommand = new SqlCommand("DBCC FREEPROCCACHE; DBCC DROPCLEANBUFFERS;", _sqlConnection);
		clearCommand.ExecuteNonQuery();
	}
}

// DB Context 설정
public class SampleDbContext : DbContext
{
	public DbSet<YourEntity> YourTable { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlServer(connectionString);
	}
}

// 엔티티 모델 정의 (예시)
public class YourEntity
{
	public int Id { get; set; }
	public string Name { get; set; }
}

// 벤치마크 실행 코드
void Main()
{
	var config = ManualConfig.Create(DefaultConfig.Instance)
				.WithOptions(ConfigOptions.DisableOptimizationsValidator);

	var summary = BenchmarkRunner.Run<BenchmarkTests>(config);

	//var summary = BenchmarkRunner.Run<BenchmarkTests>();
	Console.WriteLine(summary);
}