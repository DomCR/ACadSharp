using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using System.IO;
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
				writer.OnNotification += this.onNotification;
				writer.Write();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(pathOut));
		}

		[Theory]
		[MemberData(nameof(DxfAsciiFiles))]
		public void DxfToDxf(string test)
		{
			CadDocument doc = DxfReader.Read(test);

			string file = Path.GetFileNameWithoutExtension(test);
			// Viewports are wrong!
			string pathOut = Path.Combine(_samplesOutFolder, $"{file}_rewrite_out.dxf");

			using (DxfWriter writer = new DxfWriter(pathOut, doc, false))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(pathOut));
		}
	}
}
