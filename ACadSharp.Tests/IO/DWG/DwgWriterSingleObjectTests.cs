using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgWriterSingleObjectTests : IOTestsBase
	{
		public class SingleCaseGenerator : IXunitSerializable
		{
			public string Name { get; private set; }

			public CadDocument Document { get; private set; } = new CadDocument();

			public SingleCaseGenerator() { }

			public SingleCaseGenerator(string name)
			{
				this.Name = name;
			}

			public override string ToString()
			{
				return this.Name;
			}

			public void Empty() { }

			public void SinglePoint()
			{
				this.Document.Entities.Add(new Point(XYZ.Zero));
			}

			public void DefaultLayer()
			{
				this.Document.Layers.Add(new ACadSharp.Tables.Layer("default_layer"));
			}

			public void SingleMText()
			{
				MText mtext = new MText();

				mtext.Value = "HELLO I'm an MTEXT";

				this.Document.Entities.Add(mtext);
			}

			public void Deserialize(IXunitSerializationInfo info)
			{
				this.Name = info.GetValue<string>(nameof(this.Name));
				this.GetType().GetMethod(this.Name).Invoke(this, null);
			}

			public void Serialize(IXunitSerializationInfo info)
			{
				info.AddValue(nameof(this.Name), this.Name);
			}
		}

		public static readonly TheoryData<SingleCaseGenerator> Data;

		public DwgWriterSingleObjectTests(ITestOutputHelper output) : base(output) { }

		static DwgWriterSingleObjectTests()
		{
			Data = new();
			if (!TestVariables.RunDwgWriterSingleCases)
			{
				Data.Add(new(nameof(SingleCaseGenerator.Empty)));
				return;
			}

			Data.Add(new(nameof(SingleCaseGenerator.Empty)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePoint)));
			Data.Add(new(nameof(SingleCaseGenerator.DefaultLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMText)));
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1018(SingleCaseGenerator data)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			var version = ACadVersion.AC1018;

			data.Document.Header.Version = version;
			DwgWriter.Write(this.getPath(data.Name, version), data.Document, this.onNotification);
		}

		[Theory()]
		[MemberData(nameof(Data))]
		public void WriteCasesAC1024(SingleCaseGenerator data)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			var version = ACadVersion.AC1024;

			data.Document.Header.Version = version;
			DwgWriter.Write(this.getPath(data.Name, version), data.Document, this.onNotification);
		}

		private string getPath(string name, ACadVersion version)
		{
			return Path.Combine(_singleCasesOutFolder, $"{name}_{version}.dwg");
		}
	}
}