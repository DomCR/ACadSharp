using ACadSharp.IO.DWG;
using ACadSharp.Tests.TestCases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgReaderTests
	{
		//private const string samplesFolder = "../../../../samples/dwg/single_entities/";
		private const string samplesFolder = "../../../../samples/dwg/";
		public static readonly TheoryData<string> _filePaths;

		static DwgReaderTests()
		{
			_filePaths = new TheoryData<string>();
			foreach (string file in Directory.GetFiles(samplesFolder, "*.dwg"))
			{
				_filePaths.Add(file);
			}
		}

		[Theory]
		[MemberData(nameof(_filePaths))]
		public void ReadTest(string test)
		{
			using (DwgReader reader = new DwgReader(test))
			{
				CadDocument doc = reader.Read();
			}
		}
	}
}
