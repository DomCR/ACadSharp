using Xunit;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Tests.TestCases;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using ACadSharp.Entities;

namespace ACadSharp.Tests.IO.DWG
{
	public class ReadObjectsTests
	{
		public const string SamplesPath = "../../../../samples/dwg/single_entities/";

		public static List<Entity> CadObjectsR14;
		public static List<Entity> CadObjects2000;
		public static List<Entity> CadObjects2004;
		public static List<Entity> CadObjects2007;
		public static List<Entity> CadObjects2010;
		public static List<Entity> CadObjects2013;
		public static List<Entity> CadObjects2018;

		public static TheoryData<CadObjectTestCase> TestData;

		static ReadObjectsTests()
		{
			string path = "../../../Data/entities.json";
			TestData = new TheoryData<CadObjectTestCase>();

			List<CadObjectTestCase> model = JsonConvert.DeserializeObject<List<CadObjectTestCase>>(File.ReadAllText(path));
			foreach (var item in model)
			{
				TestData.Add(item);
			}

			////Read the files
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_R14.dwg")))
			//{
			//	CadObjectsR14 = reader.Read().Entities;
			//}
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_2000.dwg")))
			//{
			//	CadObjects2000 = reader.Read().Entities;
			//}
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_2007.dwg")))
			//{
			//	CadObjects2007 = reader.Read().Entities;
			//}
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_2010.dwg")))
			//{
			//	CadObjects2010 = reader.Read().Entities;
			//}
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_2013.dwg")))
			//{
			//	CadObjects2013 = reader.Read().Entities;
			//}
			//using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "sample_2018.dwg")))
			//{
			//	CadObjects2018 = reader.Read().Entities;
			//}
		}

		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjectsR14(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();

		//	var a = CadObjectsR14.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjects2000(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();

		//	var a = CadObjects2000.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjects2007(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();
			
		//	var a = CadObjects2007.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjects2010(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();

		//	var a = CadObjects2010.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjects2013(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();

		//	var a = CadObjects2013.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
		//[Theory]
		//[MemberData(nameof(TestData))]
		//public void ReadObjects2018(CadObjectTestCase test)
		//{
		//	ulong handle = test.GetHandle();

		//	var a = CadObjects2018.FirstOrDefault(o => o.Handle == handle);
		//	if (a == null)
		//		return;

		//	test.Compare(a);
		//}
	}
}