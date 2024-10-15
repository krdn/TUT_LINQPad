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
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Sales.Models</Namespace>
</Query>


namespace MyBenchmarks
{
	// Custom DbContext
	public class SalesContext : DbContext
	{
		// Connection string to your database
		private const string connectionString = "Data Source=localhost,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

		public DbSet<Order> Orders { get; set; }

		// Configuring the DbContext with SQL Server provider
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}
	}

	[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
	[CategoriesColumn]
	public class test01()
	{
		SalesContext db = new SalesContext();
		private int _custkey = 10954;

		[Benchmark(Baseline = true)]
		public void test()
		{
			db = new SalesContext();

			var Orders = db.Orders
				.Where(o => o.CustKey == _custkey)
				.OrderByDescending(o => o.OrderDate)
				.Take(5)
				.ToList();
		}

		[Benchmark]
		public void testDapper()
		{
			var connectionString = db.Database.GetConnectionString();

			var dictionary = new Dictionary<string, object>
			{
				{ "@CustKey", _custkey }
			};
			var parameters = new DynamicParameters(dictionary);

			using (var connection = new SqlConnection(connectionString))
			{
				var sql = @"SELECT top(5) * FROM Orders WHERE CustKey = @CustKey Order By OrderDate";
				var product = connection.Query (sql, parameters);
			}
		}
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var config = ManualConfig.Create(DefaultConfig.Instance)
							.WithOptions(ConfigOptions.DisableOptimizationsValidator);

			var summary = BenchmarkRunner.Run<test01>(config);
			
			//var summary = BenchmarkRunner.Run<test01>();
		}
	}
}