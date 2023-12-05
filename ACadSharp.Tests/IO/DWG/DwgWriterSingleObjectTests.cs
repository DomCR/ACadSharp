using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DWG
{
	public class DwgWriterSingleObjectTests : IOTestsBase
	{
		public class SingleCaseGenerator : IXunitSerializable
		{
			private const string _dxfExt = "dxf";
			private const string _dwgExt = "dwg";

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

			public void DefaultLayer()
			{
				this.Document.Layers.Add(new Layer("default_layer"));
			}

			public void EntityColorByLayer()
			{
				Layer layer = new Layer("Test");
				layer.Color = new Color(25);
				this.Document.Layers.Add(layer);

				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Layer = layer;
				c.Color = Color.ByLayer;

				this.Document.Entities.Add(c);
			}

			public void EntityColorTrueColor()
			{
				Circle c = new Circle();
				c.Center = new XYZ(0, 0, 0);
				c.Radius = 10;
				c.Color = Color.FromTrueColor(1151726);

				this.Document.Entities.Add(c);
			}


			public void SingleLine()
			{
				Line line = new Line(XYZ.Zero, new XYZ(100, 100, 0));

				this.Document.Entities.Add(line);
			}

			public void SingleMText()
			{
				MText mtext = new MText();

				mtext.Value = "HELLO I'm an MTEXT";

				this.Document.Entities.Add(mtext);
			}

			public void SinglePoint()
			{
				this.Document.Entities.Add(new Point(XYZ.Zero));
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
			Data.Add(new(nameof(SingleCaseGenerator.SingleLine)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorByLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.EntityColorTrueColor)));
			Data.Add(new(nameof(SingleCaseGenerator.DefaultLayer)));
			Data.Add(new(nameof(SingleCaseGenerator.SingleMText)));
			Data.Add(new(nameof(SingleCaseGenerator.SinglePoint)));
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

		private void writeDxfFile(SingleCaseGenerator data, ACadVersion version)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			string path = this.getPath(data.Name, "dxf", version);

			data.Document.Header.Version = version;
			DxfWriter.Write(path, data.Document, false, this.onNotification);

			this.checkDxfDocumentInAutocad(path);
		}

		private void writeDwgFile(SingleCaseGenerator data, ACadVersion version)
		{
			if (!TestVariables.RunDwgWriterSingleCases)
				return;

			string path = this.getPath(data.Name, "dwg", version);

			data.Document.Header.Version = version;
			DwgWriter.Write(path, data.Document, this.onNotification);

			this.checkDwgDocumentInAutocad(path);
		}

		private string getPath(string name, string ext, ACadVersion version)
		{
			return Path.Combine(_singleCasesOutFolder, $"{name}_{version}.{ext}");
		}
	}
}