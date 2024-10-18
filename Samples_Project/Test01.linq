<Query Kind="Statements">
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
  <Reference>D:\30.Modetour\03.Tutorials\EFCoreDBTuningforSQLServer-Demos\Sales\Sales\bin\Debug\net7.0\Sales.dll</Reference>
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Sales.Models</Namespace>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
</Query>


//using SalesContext db = new SalesContext();

namespace MyBenchmarks
{
	
	public class test01()
	
	{
		SalesContext db; // = new SalesContext();
		
		private int _custkey = 10954;
		
		//public test01() 
		//{
		//}

		[Benchmark]
		public void test()
		{
			db = new SalesContext();

			var Orders =  db.Orders
				.Where(o => o.CustKey == _custkey)
				.OrderByDescending(o => o.OrderDate)
				.Take(5)
				.ToList();


			foreach (Order o in Orders)
			{
				Console.WriteLine($"OrderKey:    {o.OrderKey}");
				Console.WriteLine($"OrderDate:   {o.OrderDate}");
				Console.WriteLine($"TotalPrice:  {o.TotalPrice}");
				Console.WriteLine(new string('-', 20));
			}
		}
		

	}

	public class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<test01>();
		}
	}
}