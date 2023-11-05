using ACadSharp.IO;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgWriterSingleObjectTests : IOTestsBase
	{
		public class SingleObjectDocument
		{
			public string Name { get; }

			public CadDocument Document { get; } = new CadDocument();

			public SingleObjectDocument(string name)
			{
				this.Name = name;
			}

			public override string ToString()
			{
				return this.Name;
			}

			public static SingleObjectDocument Empty() { return new SingleObjectDocument("empty_file"); }

			public static SingleObjectDocument SinglePoint() { return new SingleObjectDocument("empty_file"); }
		}

		public static TheoryData<SingleObjectDocument> Data { get; set; }

		public DwgWriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

		static DwgWriterSingleObjectTests()
		{
			Data = new TheoryData<SingleObjectDocument>();
			if (!TestVariables.RunDwgWriterSingleCases)
			{
				Data.Add(SingleObjectDocument.Empty());
				return;
			}

			Data.Add(SingleObjectDocument.Empty());
		}

		[Theory]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1018(SingleObjectDocument data)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			data.Document.Header.Version = ACadVersion.AC1018;
			DwgWriter.Write(this.getPath(data.Name, ACadVersion.AC1018), data.Document, this.onNotification);
		}

		private string getPath(string name, ACadVersion version)
		{
			return Path.Combine(_singleCasesOutFolder, $"{name}_{version}.dwg");
		}
	}
}