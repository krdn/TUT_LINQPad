<Query Kind="Statements">
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
  <NuGetReference>Microsoft.EntityFrameworkCore</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore.SqlServer</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
  <Namespace>Sales.Models</Namespace>
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