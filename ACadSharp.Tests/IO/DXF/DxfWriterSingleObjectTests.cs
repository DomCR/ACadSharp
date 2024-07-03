using ACadSharp.IO;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfWriterSingleObjectTests : WriterSingleObjectTests
	{
		public DxfWriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1018(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1018);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1021(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1021);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1024(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1024);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1027(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1027);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1032(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1032);
		}

		protected virtual void writeDxfFile(SingleCaseGenerator data, ACadVersion version)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			string path = this.getPath(data.Name, "dxf", version);

			data.Document.Header.Version = version;
			DxfWriter.Write(path, data.Document, false, this.onNotification);
		}
	}
}