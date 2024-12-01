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
  <Namespace>Microsoft.EntityFrameworkCore.Diagnostics</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Sales.Models</Namespace>
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
const string connectionString = "Data Source=krdn-g713rm,1434;Initial Catalog=SalesSimple;User Id=sa;Password=krdn@Passw0rd;Pooling=True;MultipleActiveResultSets=False;Application Name=Sales;Encrypt=False;";

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
			.UseSqlServer(sqlOptions => {
				sqlOptions.CommandTimeout(30);
				//sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				
			})
			.LogTo(log => CapturedQueries.Add(log), LogLevel.Information)
			.EnableSensitiveDataLogging(); // 매개변수 값을 로그에 포함
	}
}

// 벤치마크 클래스
[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 10)]
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

		Console.WriteLine("=== Test_EF0에서 캡처된 SQL 쿼리 ===");
		foreach (var query in _capturedQueries_EF0)
		{
			Console.WriteLine(query);
			Console.WriteLine();
		}
	}

	[Benchmark(Baseline = true)]
	public void Test_EF()
	{
		if (!UseCache)
		{
			ClearCache();
		}

		using (var context = new Sales2Context())
		{
			// 이전에 캡처된 쿼리를 지웁니다.
			context.CapturedQueries.Clear();

			// 쿼리 실행
			var result = context.Orders
				.Include(o => o.LineItems)
				.Where(o => o.CustKey == 1)
				.ToList();

			// 첫 번째 실행에서만 쿼리 저장
			if (_capturedQueries_EF.Count == 0)
			{
				_capturedQueries_EF.AddRange(context.CapturedQueries);
			}
		}
	}

	[Benchmark]
	public void Test_EF0()
	{
		if (!UseCache)
		{
			ClearCache();
		}

		using (var context = new Sales2Context())
		{
			// 이전에 캡처된 쿼리를 지웁니다.
			context.CapturedQueries.Clear();

			// AsSplitQuery 사용하여 쿼리 실행
			var result = context.Orders
				.Include(o => o.LineItems)
				.Where(o => o.CustKey == 1)
				.AsSplitQuery()
				.ToList();

			// 첫 번째 실행에서만 쿼리 저장
			if (_capturedQueries_EF0.Count == 0)
			{
				_capturedQueries_EF0.AddRange(context.CapturedQueries);
			}
		}
	}

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
	public static void Main()
	{
		var config = ManualConfig.Create(DefaultConfig.Instance)
					.WithOptions(ConfigOptions.DisableOptimizationsValidator);

		var summary = BenchmarkRunner.Run<BenchmarkTests>(config);
	}
}
