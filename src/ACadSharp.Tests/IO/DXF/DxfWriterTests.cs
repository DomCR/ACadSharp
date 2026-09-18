using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF;

public class DxfWriterTests : IOTestsBase
{
	public DxfWriterTests(ITestOutputHelper output) : base(output) { }

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteEmptyAsciiTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_empty_sample_{version}_ascii.dxf");

		using (var wr = new DxfWriter(path, doc, false))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}

		this._output.WriteLine("Writer successful");

		using (var re = new DxfReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteEmptyBinaryTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_empty_sample_{version}_binary.dxf");

		using (var wr = new DxfWriter(path, doc, true))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}

		this._output.WriteLine("Writer successful");

		using (var re = new DxfReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteDocumentWithEntitiesTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		List<Entity> entities = new List<Entity>
		{
			EntityFactory.Create<Point>(),
			EntityFactory.Create<Line>(),
			EntityFactory.Create<Polyline2D>(),
			EntityFactory.Create<Polyline3D>(),
			EntityFactory.Create<Line>(),
			EntityFactory.Create<Arc>(),
			EntityFactory.Create<LwPolyline>(),
		};


		doc.Entities.AddRange(entities);

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_sample_{version}_ascii.dxf");

		using (var wr = new DxfWriter(path, doc, false))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}
	}

	[Fact]
	public void WriteTextAlignmentPointOnlyWhenAlignedTest()
	{
		CadDocument doc = new CadDocument();
		doc.Entities.Add(new TextEntity
		{
			Value = "default",
			InsertPoint = new XYZ(10, 20, 0),
		});
		doc.Entities.Add(new TextEntity
		{
			Value = "centered",
			InsertPoint = new XYZ(10, 20, 0),
			AlignmentPoint = new XYZ(30, 40, 0),
			HorizontalAlignment = TextHorizontalAlignment.Center,
		});

		List<List<(string Code, string Value)>> texts = this.writeAndGetEntities(doc, "TEXT");

		Assert.Equal(2, texts.Count);

		//Left/baseline aligned: group 10 positions the text, group 11 must be omitted
		List<(string Code, string Value)> defaultText = texts.Single(t => t.Contains(("1", "default")));
		Assert.DoesNotContain(defaultText, p => p.Code == "11");

		List<(string Code, string Value)> centeredText = texts.Single(t => t.Contains(("1", "centered")));
		Assert.Contains(("11", "30"), centeredText);
		Assert.Contains(("21", "40"), centeredText);

		//Roundtrip keeps the position of both texts
		CadDocument readed = DxfReader.Read(new MemoryStream(this._lastWritten), this.onNotification);
		TextEntity defaultRead = readed.Entities.OfType<TextEntity>().Single(t => t.Value == "default");
		Assert.Equal(new XYZ(10, 20, 0), defaultRead.InsertPoint);
		Assert.Equal(TextHorizontalAlignment.Left, defaultRead.HorizontalAlignment);

		TextEntity centeredRead = readed.Entities.OfType<TextEntity>().Single(t => t.Value == "centered");
		Assert.Equal(new XYZ(30, 40, 0), centeredRead.AlignmentPoint);
	}

	private byte[] _lastWritten;

	private List<List<(string Code, string Value)>> writeAndGetEntities(CadDocument doc, string entityName)
	{
		using (MemoryStream ms = new MemoryStream())
		{
			DxfWriter.Write(ms, doc, false, notification: this.onNotification);
			this._lastWritten = ms.ToArray();
		}

		string[] lines = System.Text.Encoding.ASCII.GetString(this._lastWritten)
			.Split('\n')
			.Select(l => l.Trim())
			.ToArray();

		List<List<(string Code, string Value)>> entities = new();
		List<(string Code, string Value)> current = null;
		for (int i = 0; i + 1 < lines.Length; i += 2)
		{
			(string Code, string Value) pair = (lines[i], lines[i + 1]);
			if (pair.Code == "0")
			{
				current = pair.Value == entityName ? new() : null;
				if (current != null)
				{
					entities.Add(current);
				}
				continue;
			}

			current?.Add(pair);
		}

		return entities;
	}
}
