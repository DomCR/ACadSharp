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
		public IOTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(DwgFilePaths))]
		public void DwgToDxf(string test)
		{
			CadDocument doc = DwgReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);

			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_out.dxf");

			using (DxfWriter writer = new DxfWriter(pathOut, doc, false))
			{
				writer.Write();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(pathOut));
		}
	}
}
