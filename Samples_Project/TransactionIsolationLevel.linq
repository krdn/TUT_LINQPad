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
  <Namespace>Microsoft.EntityFrameworkCore.Diagnostics</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Sales.Models</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore.Storage</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <RuntimeVersion>8.0</RuntimeVersion>
</Query>

// 필요한 네임스페이스를 포함합니다.
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// SQL Server 연결 문자열
const string connectionString = "Data Source=localhost,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

// DbContext 정의
public class Sales2Context : SalesContext
{
	//public DbSet<Order> Orders { get; set; }
	//public DbSet<LineItem> LineItems { get; set; }

	// 캡처된 쿼리를 저장할 리스트
	public List<string> CapturedQueries { get; } = new List<string>();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlServer(connectionString);

		// 로깅 설정: 쿼리를 CapturedQueries 리스트에 저장
		optionsBuilder
			.UseSqlServer(sqlOptions =>
			{
				sqlOptions.CommandTimeout(30);
				//sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

			})
			.LogTo(log => CapturedQueries.Add(log), LogLevel.Information)
			.EnableSensitiveDataLogging(); // 매개변수 값을 로그에 포함
	}
}

// 벤치마크 클래스
//[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 10)]
public class BenchmarkTests
{
	// 캐시 사용 여부 설정 (현재는 true로 고정)
	[Params(true)]
	public bool UseCache { get; set; }

	private readonly SqlConnection _sqlConnection = new(connectionString);

	// 각 벤치마크 메서드에서 캡처된 쿼리를 저장할 리스트
	private List<string> _capturedQueries_EF = new();
	private List<string> _capturedQueries_EF0 = new();

	[GlobalSetup]
	public void Setup()
	{
		_sqlConnection.Open();

		using (var context = new Sales2Context())
		{
			// 데이터베이스가 없으면 생성
			context.Database.EnsureCreated();
		}
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_sqlConnection.Close();

		// 캡처된 쿼리 출력
		Console.WriteLine("=== Test_EF에서 캡처된 SQL 쿼리 ===");
		foreach (var query in _capturedQueries_EF)
		{
			Console.WriteLine(query);
			Console.WriteLine();
		}

		//Console.WriteLine("=== Test_EF0에서 캡처된 SQL 쿼리 ===");
		//foreach (var query in _capturedQueries_EF0)
		//{
		//	Console.WriteLine(query);
		//	Console.WriteLine();
		//}
	}

	//[Benchmark(Baseline = true)]
	public async Task Test_EF()
	{
		//주문번호 - 먼저 존재하는지 확인
		var orderkey = 100;

		using (Sales2Context db = new Sales2Context())
		{
			var order = await db
				.Orders
				.FirstOrDefaultAsync(o => o.OrderKey == orderkey);

			Console.WriteLine($"TotalPrice: {order.TotalPrice} (Before)");

			//데이터 변경
			order.TotalPrice -= 10000;

			using (var tx = await db.Database.BeginTransactionAsync())
			{
				//여기서 중단점 후에 디버깅
				// 1. 격리수준 확인
				Console.WriteLine(tx.GetDbTransaction().IsolationLevel.ToString());

				// 사용자 정의 Savepoint 정의
				tx.CreateSavepoint("BeforeUpdate");

				//Update
				// 2. MARS = OFF/ON에 따른 동작 비교(콘솔 경고 메시지도 확인)
				var rowsAffected = await db.SaveChangesAsync();

				Console.WriteLine($"{rowsAffected} rows affected, TotalPrice: {order.TotalPrice} (After)");

				// 3. Savepoint 동작 확인
				await tx.RollbackToSavepointAsync("BeforeUpdate");
				Console.WriteLine($"Rollback to SavePoint, [BeforeUpdate]");

				order = await db
					.Orders
					.AsNoTracking() //DBContext가 아니라 DB에서 새로 검색하기 위해
					.FirstOrDefaultAsync(o => o.OrderKey == orderkey);

				Console.WriteLine($"TotalPrice: {order.TotalPrice} (After Rollback)");

				//다루지는 않음
				//await tx.RollbackToSavepointAsync("__EFSavePoint");
				//Console.WriteLine($"Rollback to SavePoint, __EFSavePoint");

				// 4. Commit or Rollback
				//await tx.CommitAsync();
				await tx.RollbackAsync();
			}
		}
	}

//	[Benchmark]
//	public void Test_EF0()
//	{
//	}

	// SQL Server 캐시를 비우는 메서드
	private void ClearCache()
	{
		using var clearCommand = new SqlCommand("DBCC FREEPROCCACHE; DBCC DROPCLEANBUFFERS;", _sqlConnection);
		clearCommand.ExecuteNonQuery();
	}
}

// 프로그램 시작점
public class Program
{
	public static async void Main()
	{
		var config = ManualConfig.Create(DefaultConfig.Instance)
					.WithOptions(ConfigOptions.DisableOptimizationsValidator);

		var summary = BenchmarkRunner.Run<BenchmarkTests>(config);
		
		
		var aaa = new BenchmarkTests();
		
		await aaa.Test_EF();
		
	}
}
