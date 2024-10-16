<Query Kind="Program">
  <Connection>
    <ID>67d23f56-2e29-49e2-b6c9-b894cafe52c8</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>172.22.28.13, 1942</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAE0buaQVsAUedbdTc98/yCgAAAAACAAAAAAAQZgAAAAEAACAAAACoac+pmsPn970jdSiiDNXcmEq6ZYIh6dc3d9SyHnV7HQAAAAAOgAAAAAIAACAAAABuDawFrSg9JkJLuUDZTW/Qy1fNNZKlsl3M7jSChIKPwBAAAABi4nBNgwXgYv/UVvvnQ+tVQAAAAEYGPYhOVJIWFB3dxZYX6o6edy80JC/xn4dlM4RAymXvdvgPiP09huzW/0FGERWcW7mZW1AVD1g7LjK9bylazs8=</Password>
    <Database>Modeware3</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore.SqlServer</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Configs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
</Query>

/*
USE [Modeware3]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[WSP_S_고객만족도조사_리스트]    

	@설문지번호	SMALLINT
          
AS    

SET NOCOUNT ON  
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED  
     
	--1. 설문지정보
	SELECT
		설문일련번호,
		설문문항,
		노출여부
	FROM DBO.포상_설문관리
	WHERE 사용용도 = 20
		AND 설문일련번호 = @설문지번호

	--2. 설문조건정보(질문)
	SELECT
		조건일련번호,
		설문명칭,
		중복허용,
		ISNULL(정렬, 0) AS 정렬
	FROM DBO.포상_설문조건
	WHERE 설문일련번호 = @설문지번호
	ORDER BY 정렬 ASC

	--3. 피설문항목정보(답변항목)
	SELECT
		A.조건일련번호,
		A.항목코드,
		B.코드명,
		A.키인필수,
		ISNULL(A.정렬, 0) AS 정렬
	FROM DBO.포상_설문조건_피설문항목 AS A 
		INNER JOIN DBO.코드 AS B ON A.항목코드 = B.코드 AND 종류 = 'JJ'
	WHERE 조건일련번호 IN (	SELECT
								조건일련번호
							FROM DBO.포상_설문조건
							WHERE 설문일련번호 = @설문지번호)
	ORDER BY A.조건일련번호, A.정렬 ASC
	--ORDER BY A.조건일련번호, A.항목코드 ASC


	
SET QUOTED_IDENTIFIER OFF

*/

namespace ModetourBenchmarks
{
	// Custom DbContext
	public class SalesContext : DbContext
	{
		// Connection string to your database
		private const string connectionString = "Data Source=172.22.28.13, 1942;Initial Catalog=ModeWare3;User Id=sa;Password=modetour^^1;Pooling=True;MultipleActiveResultSets=False;Application Name=Modetour;Encrypt=False;";

		// Configuring the DbContext with SQL Server provider
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}
	}

	/// <summary>
	/// 	SP, Dapper, EntityFramework 세가지로 테스트 한다.
	///     각각의 이름은 ModetourSP, ModetureDaper, ModetourEF
	///     기준은 : SP를 기준으로 [Benchmark(Baseline = true)]
	/// </summary>
	[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
	[CategoriesColumn]
	public class ModetourBechmark()
	{
		SalesContext db = new SalesContext();
		private int _custkey = 10954;


		[Benchmark(Baseline = true)]
		public void ModetourSP()
		{
			var connectionString = db.Database.GetConnectionString();
			//var connectionString = "Data Source=172.22.28.13, 1942;Initial Catalog=ModeWare3;User Id=sa;Password=modetour^^1;Pooling=True;MultipleActiveResultSets=False;Application Name=Modetour;Encrypt=False;";

			var dictionary = new Dictionary<string, object>
			{
				{ "@설문지번호", 222 }
			};
			var parameters = new DynamicParameters(dictionary);

			using (var connection = new SqlConnection(connectionString))
			{
				//var sql = @"SELECT top(5) * FROM Orders WHERE CustKey = @CustKey Order By OrderDate";
				var product = connection.Query("WSP_S_고객만족도조사_리스트", parameters);
			}
		}

		[Benchmark]
		public void ModetourDapper()
		{
			var connectionString = db.Database.GetConnectionString();
			//var connectionString = "Data Source=172.22.28.13, 1942;Initial Catalog=ModeWare3;User Id=sa;Password=modetour^^1;Pooling=True;MultipleActiveResultSets=False;Application Name=Modetour;Encrypt=False;";

			var dictionary = new Dictionary<string, object>
			{
				{ "@설문지번호", 222 }
			};
			var parameters = new DynamicParameters(dictionary);
			var sql = @"
	--1. 설문지정보
	SELECT
		설문일련번호,
		설문문항,
		노출여부
	FROM DBO.포상_설문관리
	WHERE 사용용도 = 20
		AND 설문일련번호 = @설문지번호			
		

	--2. 설문조건정보(질문)
	SELECT
		조건일련번호,
		설문명칭,
		중복허용,
		ISNULL(정렬, 0) AS 정렬
	FROM DBO.포상_설문조건
	WHERE 설문일련번호 = @설문지번호
	ORDER BY 정렬 ASC

	--3. 피설문항목정보(답변항목)
	SELECT
		A.조건일련번호,
		A.항목코드,
		B.코드명,
		A.키인필수,
		ISNULL(A.정렬, 0) AS 정렬
	FROM DBO.포상_설문조건_피설문항목 AS A 
		INNER JOIN DBO.코드 AS B ON A.항목코드 = B.코드 AND 종류 = 'JJ'
	WHERE 조건일련번호 IN (	SELECT
								조건일련번호
							FROM DBO.포상_설문조건
							WHERE 설문일련번호 = @설문지번호)
	ORDER BY A.조건일련번호, A.정렬 ASC
	--ORDER BY A.조건일련번호, A.항목코드 ASC		
			";

			using (var connection = new SqlConnection(connectionString))
			{
				//var sql = @"SELECT top(5) * FROM Orders WHERE CustKey = @CustKey Order By OrderDate";
				var product = connection.QueryMultiple(sql, parameters);
			}
		}



		[Benchmark]
		public void ModetourEF()
		{
			db = new SalesContext();
			
			//var temp = db.

			//var Orders = db.Orders
			//	.Where(o => o.CustKey == _custkey)
			//	.OrderByDescending(o => o.OrderDate)
			//	.Take(5)
			//	.ToList();
		}



	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var config = ManualConfig.Create(DefaultConfig.Instance)
							.WithOptions(ConfigOptions.DisableOptimizationsValidator);

			var summary = BenchmarkRunner.Run<ModetourBechmark>(config);

			//var summary = BenchmarkRunner.Run<test01>();
		}
	}
}