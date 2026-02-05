using ACadSharp.IO;
using ACadSharp.IO.DWG;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgWriterSingleObjectTests : WriterSingleObjectTests
	{
		public DwgWriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1015(SingleCaseGenerator data)
		{
			this.writeDwgFile(data, ACadVersion.AC1015);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1018(SingleCaseGenerator data)
		{
			this.writeDwgFile(data, ACadVersion.AC1018);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1024(SingleCaseGenerator data)
		{
			this.writeDwgFile(data, ACadVersion.AC1024);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1027(SingleCaseGenerator data)
		{
			this.writeDwgFile(data, ACadVersion.AC1027);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1032(SingleCaseGenerator data)
		{
			this.writeDwgFile(data, ACadVersion.AC1032);
		}

		protected virtual void writeDwgFile(SingleCaseGenerator data, ACadVersion version)
		{
			Assert.True(data.HasExecuted, $"The writer has failed during it's execution.");

			string path = this.getPath(data.Name, "dwg", version);
			data.Document.Header.Version = version;

			DwgWriterConfiguration configuration = new DwgWriterConfiguration()
			{
				WriteXRecords = true,
			};

			if (TestVariables.SaveOutputInStream)
			{
				MemoryStream ms = new MemoryStream();
				DwgWriter.Write(ms, data.Document, configuration, notification: this.onNotification);
				data.Stream = new MemoryStream(ms.ToArray());
			}
			else
			{
				DwgWriter.Write(path, data.Document, configuration, this.onNotification);
			}

			if (TestVariables.SelfCheckOutput)
			{
				this._output.WriteLine("--- starting read ---");

				if (TestVariables.SaveOutputInStream)
				{
					DwgReader.Read(data.Stream, this.onNotification);
				}
				else
				{
					DwgReader.Read(path, this.onNotification);
				}
			}
		}
	}
}