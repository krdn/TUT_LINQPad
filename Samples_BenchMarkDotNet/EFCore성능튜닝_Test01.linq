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
</Query>


namespace SalesSimpleBenchmarks
{
	// Custom DbContext
	public class SalesContext : DbContext
	{
		// Connection string to your database
		private const string connectionString = "Data Source=krdn-g713rm,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

		public DbSet<Order> Orders { get; set; }

		// Configuring the DbContext with SQL Server provider
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}
	}

	[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
	[CategoriesColumn]
	public class SaleBenchmark()
	{
		SalesContext dbContext = new SalesContext();
		private int _custkey = 10954;

		[Benchmark(Baseline = true)]
		public void SaleSP_Dapper()
		{
			dbContext = new SalesContext();
			
			var connectionString2 = dbContext.Database.GetConnectionString();
			
			var dictionary = new Dictionary<string, object>
			{
				{ "@CustKey", _custkey }
			};
			var parameters = new DynamicParameters(dictionary);

			using (var connection = new SqlConnection(connectionString2))
			{
				//var sql = @"SELECT top(5) * FROM Orders WHERE CustKey = @CustKey Order By OrderDate";
				var product = connection.Query("OrderByCustKey", parameters);
			}

		}



		[Benchmark]
		public void SaleSP_EF_ExcuteSQL()
		{
			dbContext = new SalesContext();
			
			var employees = dbContext.Orders
				.FromSqlRaw("SELECT * FROM Orders WHERE CustKey = {0}", _custkey)
				.ToList();				

		}

		[Benchmark]
		public void SaleSP_EF()
		{
			dbContext = new SalesContext();

			var employees = dbContext.Orders
				.FromSqlRaw("EXEC dbo.OrderByCustKey @CustKey = {0}", _custkey)
				.ToList();

		}

		[Benchmark]
		public void SaleDapper()
		{
			var connectionString = dbContext.Database.GetConnectionString();

			var dictionary = new Dictionary<string, object>
			{
				{ "@CustKey", _custkey }
			};
			var parameters = new DynamicParameters(dictionary);

			using (var connection = new SqlConnection(connectionString))
			{
				var sql = @"SELECT top(5) * FROM Orders WHERE CustKey = @CustKey Order By OrderDate";
				var product = connection.Query (sql, parameters);

				//var builder = new SqlBuilder();
				//var template = builder.AddTemplate(sql);
				//builder.Where("Age > @age", new { age = 18 });
				//string sql3 = builder.AddTemplate(template.ToString);
				//Console.WriteLine(sql);
			}
		}

		[Benchmark]
		public void SaleEF()
		{
			dbContext = new SalesContext();

			var Orders1 = dbContext.Orders
				.Where(o => o.CustKey == _custkey)
				.OrderByDescending(o => o.OrderDate)
				.Take(5)
				.ToList();

			//var temp = db.Orders
			//	.Where(o => o.CustKey == _custkey)
			//	.OrderByDescending(o => o.OrderDate)
			//	.Take(5).ToQueryString();
			//	
			//Debug.Print(temp);

		}

	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var config = ManualConfig.Create(DefaultConfig.Instance)
							.WithOptions(ConfigOptions.DisableOptimizationsValidator);

			var summary = BenchmarkRunner.Run<SaleBenchmark>(config);
			
			//var summary = BenchmarkRunner.Run<test01>();
		}
	}
}