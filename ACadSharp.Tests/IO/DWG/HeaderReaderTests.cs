using ACadSharp.Header;
using ACadSharp.IO.DWG;
using ACadSharp.Tests.TestCases;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG
{
	public class HeaderReaderTests
	{
		public const string SamplesPath = "../../../../samples/dwg/";

		public static CadHeader HeaderR14;
		public static CadHeader Header2000;
		public static CadHeader Header2004;
		public static CadHeader Header2007;
		public static CadHeader Header2010;
		public static CadHeader Header2013;
		public static CadHeader Header2018;

		public static TheoryData<CadSystemVariablePair> TestData;

		static HeaderReaderTests()
		{
			string path = "../../../Data/header_cases.json";
			TestData = new TheoryData<CadSystemVariablePair>();

			List<JObject> model = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(path));

			foreach (var j in model)
			{
				CadSystemVariablePair test = new CadSystemVariablePair();

				test.Name = (string)j["Name"];
				test.Value = j["Value"];

				TestData.Add(test);
			}

			//Read the files
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_R14.dwg")))
			{
				HeaderR14 = reader.ReadHeader();
			}
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_2000.dwg")))
			{
				Header2000 = reader.ReadHeader();
			}			
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_2007.dwg")))
			{
				Header2007 = reader.ReadHeader();
			}
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_2010.dwg")))
			{
				Header2010 = reader.ReadHeader();
			}
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_2013.dwg")))
			{
				Header2013 = reader.ReadHeader();
			}
			using (DwgReader reader = new DwgReader(Path.Combine(SamplesPath, "drawing_2018.dwg")))
			{
				Header2018 = reader.ReadHeader();
			}
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeaderR14(CadSystemVariablePair test)
		{
			object value = HeaderR14.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeader2000(CadSystemVariablePair test)
		{
			object value = Header2000.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeader2007(CadSystemVariablePair test)
		{
			object value = Header2007.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeader2010(CadSystemVariablePair test)
		{
			object value = Header2010.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeader2013(CadSystemVariablePair test)
		{
			object value = Header2013.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
		[Theory]
		[MemberData(nameof(TestData))]
		public void ReadHeader2018(CadSystemVariablePair test)
		{
			object value = Header2018.GetValue(test.Name);

			if (value == null)
				return;

			object compare = test.Value.ToObject(value.GetType());

			Assert.Equal(compare, value);
		}
	}
}
