<Query Kind="Statements">
  <Connection>
    <ID>3da83162-ac14-4a65-83e9-c583bb0b3b68</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>192.168.0.18,1434</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <SqlSecurity>true</SqlSecurity>
    <UserName>sa</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAALqYKck3+v0akEg0HeHio4AAAAAACAAAAAAAQZgAAAAEAACAAAACE51B/BY++Os8WipGj1haBE+X/gF+YTYgkxNfWnE/SNAAAAAAOgAAAAAIAACAAAAD8pJ00axnAaEJi7foDePf1bttDQ/5K5aUgLDtNy1/SSBAAAACidrBahnDpOnGC4VhsXF4yQAAAAOqmPK6nINvNPWFKxNdUJ25afxSVFZFn0QPVbDgrFz53Olbam2X+D6d//vromwOebyr5aSuvNITDFYTDkmLpf8Q=</Password>
    <Database>SalesSimple</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
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