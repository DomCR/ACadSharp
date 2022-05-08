using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class IOTests : IOTestsBase
	{
		private const string _samplesFolder = "../../../../samples/out";

		static IOTests()
		{
			if (!Directory.Exists(_samplesFolder))
			{
				Directory.CreateDirectory(_samplesFolder);
			}
		}

		public IOTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDxf(string test)
		{
			CadDocument doc = DwgReader.Read(test, this.onNotification);

			string dir = Path.GetDirectoryName(test);
			string file = Path.GetFileNameWithoutExtension(test);

			string pathOut = Path.Combine(dir, "out", $"{file}_out.dxf");

			using (DxfWriter writer = new DxfWriter(pathOut, doc, false))
			{
				writer.Write();
			}
		}
	}
}
