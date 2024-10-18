<Query Kind="Program">
  <Connection>
    <ID>55cb31a9-6a89-48ae-9629-dc335eb370eb</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>localhost, 1434</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAlRQcAFV9W0WYo/HEzz39WwAAAAACAAAAAAAQZgAAAAEAACAAAAAbTYsPaZLOOUDj62W+mTLzE4UAkkK9M6uXAH1CdmwLhgAAAAAOgAAAAAIAACAAAAAi5CPDv1jYxJEhkK2Eo47Nv1fcqcSgNJ3BvG1ERdxRYBAAAAC9fmX/EinZ4Ei9ma+cmnbBQAAAAMqvjO4IbbFGQ4RiAPeK5HufJoqHnr0kxtkQJ2Uu3yDg9xxQe4TDBmzsS7v+OGKYTuY1lCdiH+4Oy8HLoLRtfUI=</Password>
    <Database>SalesSimple</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <Reference Relative="..\..\TUT_EntityFrameworkSales\Sales\bin\Release\net7.0\Sales.dll">C:\03.Tutorials\TUT_EntityFrameworkSales\Sales\bin\Release\net7.0\Sales.dll</Reference>
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

			var Orders1 = db.Orders
				.Where(o => o.CustKey == _custkey)
				.OrderByDescending(o => o.OrderDate)
				.Take(5)
				.ToList();

			var temp = db.Orders
				.Where(o => o.CustKey == _custkey)
				.OrderByDescending(o => o.OrderDate)
				.Take(5).ToQueryString();
				
			Debug.Print(temp);
			
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

				//var builder = new SqlBuilder();
				//var template = builder.AddTemplate(sql);
				//builder.Where("Age > @age", new { age = 18 });
				//string sql3 = builder.AddTemplate(template.ToString);
				//Console.WriteLine(sql);
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