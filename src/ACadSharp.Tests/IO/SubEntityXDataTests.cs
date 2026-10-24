using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.XData;
using CSMath;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// An entity followed by its own sub-entities in a DXF (a heavy POLYLINE's VERTEX + SEQEND, an INSERT's ATTRIB + SEQEND)
/// must write its extended data with its own header. A reader attaches 1001 groups to the last entity it opened, so
/// extended data written after the SEQEND is read back as the SEQEND's, and the owner loses it.
/// </summary>
public class SubEntityXDataTests
{
	private const string AppName = "SUBENTITY_XDATA_TEST";

	private static ExtendedData xdata(string value)
	{
		ExtendedData data = new ExtendedData();
		data.Records.Add(new ExtendedDataString(value));
		data.Records.Add(new ExtendedDataReal(2.5));
		return data;
	}

	private static string writeDxf(CadDocument doc, ACadVersion version)
	{
		doc.Header.Version = version;
		using MemoryStream stream = new MemoryStream();
		DxfWriter.Write(stream, doc, false, new DxfWriterConfiguration { CloseStream = false });
		return Encoding.ASCII.GetString(stream.ToArray());
	}

	private static void assertXData(Entity entity, string value)
	{
		Assert.True(entity.ExtendedData.TryGet(AppName, out ExtendedData data), $"{entity.ObjectName} lost its {AppName} xdata");
		Assert.Equal(value, Assert.IsType<ExtendedDataString>(data.Records[0]).Value);
		Assert.Equal(2.5, Assert.IsType<ExtendedDataReal>(data.Records[1]).Value);
	}

	private static void assertNoXData(Entity entity)
	{
		Assert.False(entity.ExtendedData.TryGet(AppName, out _), $"{entity.ObjectName} was given its owner's xdata");
	}

	/// <summary>The 1001 group of the app must come before the first marker that opens a sub-entity.</summary>
	private static void assertWrittenBefore(string dxf, string subEntity)
	{
		string[] lines = dxf.Split('\n').Select(l => l.Trim()).ToArray();
		int app = -1;
		int first = -1;
		for (int i = 0; i + 1 < lines.Length; i += 2)
		{
			if (app < 0 && lines[i] == "1001" && lines[i + 1] == AppName)
			{
				app = i;
			}
			if (first < 0 && lines[i] == "0" && lines[i + 1] == subEntity)
			{
				first = i;
			}
		}

		Assert.True(app >= 0, $"no 1001 {AppName} in the DXF");
		Assert.True(first >= 0, $"no {subEntity} in the DXF");
		Assert.True(app < first, $"the owner's xdata (line {app}) is written after its first {subEntity} (line {first})");
	}

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1032)]
	public void PolylineXDataIsWrittenBeforeItsVertices(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.AppIds.Add(new AppId(AppName));

		Polyline2D polyline = new Polyline2D(new[] { new Vertex2D(new XY(0, 0)), new Vertex2D(new XY(10, 5)), new Vertex2D(new XY(20, 0)) }, false);
		polyline.ExtendedData.Add(AppName, xdata("polyline"));
		doc.Entities.Add(polyline);

		string dxf = writeDxf(doc, version);
		assertWrittenBefore(dxf, DxfFileToken.EntityVertex);

		CadDocument read = DxfReader.Read(new MemoryStream(Encoding.ASCII.GetBytes(dxf)));
		Polyline2D readBack = read.Entities.OfType<Polyline2D>().Single();
		Assert.Equal(3, readBack.Vertices.Count);
		assertXData(readBack, "polyline");
		assertNoXData(readBack.Vertices.Seqend);
		foreach (Vertex2D vertex in readBack.Vertices)
		{
			assertNoXData(vertex);
		}
	}

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1032)]
	public void InsertXDataIsWrittenBeforeItsAttributes(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.AppIds.Add(new AppId(AppName));

		BlockRecord block = new BlockRecord("SUBENTITY_BLOCK");
		doc.BlockRecords.Add(block);

		Insert insert = new Insert(block) { InsertPoint = new XYZ(10, 10, 0) };
		insert.Attributes.Add(new AttributeEntity { Tag = "TAG", Value = "value", InsertPoint = new XYZ(5, 5, 0) });
		insert.ExtendedData.Add(AppName, xdata("insert"));
		doc.Entities.Add(insert);

		string dxf = writeDxf(doc, version);
		assertWrittenBefore(dxf, DxfFileToken.EntityAttribute);

		CadDocument read = DxfReader.Read(new MemoryStream(Encoding.ASCII.GetBytes(dxf)));
		Insert readBack = read.Entities.OfType<Insert>().Single();
		AttributeEntity attribute = Assert.Single(readBack.Attributes);
		assertXData(readBack, "insert");
		assertNoXData(attribute);
		assertNoXData(readBack.Attributes.Seqend);
	}

	[Fact]
	public void InsertWithoutAttributesKeepsItsXData()
	{
		CadDocument doc = new CadDocument();
		doc.AppIds.Add(new AppId(AppName));

		BlockRecord block = new BlockRecord("SUBENTITY_BLOCK");
		doc.BlockRecords.Add(block);

		Insert insert = new Insert(block);
		insert.ExtendedData.Add(AppName, xdata("plain insert"));
		doc.Entities.Add(insert);

		string dxf = writeDxf(doc, ACadVersion.AC1032);

		CadDocument read = DxfReader.Read(new MemoryStream(Encoding.ASCII.GetBytes(dxf)));
		assertXData(read.Entities.OfType<Insert>().Single(), "plain insert");
	}
}
