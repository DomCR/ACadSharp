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
		public void WriteCasesAC1015(SingleCaseGenerator data)
		{
			this.writeDxfFile(data, ACadVersion.AC1015);
		}

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

		protected void writeDxfFile(SingleCaseGenerator data, ACadVersion version)
		{
			Assert.True(data.HasExecuted, $"The writer has failed during it's execution.");

			string path = this.getPath(data.Name, "dxf", version);
			data.Document.Header.Version = version;

			if (TestVariables.SaveOutputInStream)
			{
				MemoryStream ms = new MemoryStream();
				DxfWriter.Write(ms, data.Document, false, notification: this.onNotification);
				data.Stream = new MemoryStream(ms.ToArray());
			}
			else
			{
				DxfWriter.Write(path, data.Document, false, notification: this.onNotification);
			}

			if (TestVariables.SelfCheckOutput)
			{
				this._output.WriteLine("--- starting read ---");

				if (TestVariables.SaveOutputInStream)
				{
					DxfReader.Read(data.Stream, this.onNotification);
				}
				else
				{
					DxfReader.Read(path, this.onNotification);
				}
			}
		}
	}
}