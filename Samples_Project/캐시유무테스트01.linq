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
  <NuGetReference>Microsoft.EntityFrameworkCore.Proxies</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore.SqlServer</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Sales.Models</Namespace>
</Query>


// SQL Server 연결 문자열
const string connectionString = "Data Source=krdn-g713rm,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;";

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