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
    <Database>ModeWare3</Database>
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
  <Namespace>BenchmarkDotNet.Jobs</Namespace>
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
		private const string connectionString = "Data Source=krdn-g713rm, 1434;Initial Catalog=ModeWare3;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Modetour;Encrypt=False;";

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
	[Config(typeof(Config))]
	public class ModetourBechmark()
	{
		// BenchmarkDotNet 테스트 수 설정
		public class Config : ManualConfig
		{
			public Config()
			{
				AddJob(Job.Default.WithIterationCount(5).WithLaunchCount(3));
				AddJob(Job.MediumRun.WithIterationCount(3).WithWarmupCount(2));
			}
		}
		
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