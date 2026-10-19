using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.XData;
using CSMath;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO;

public class LegacyFormatTests
{
	private static CadDocument fixture(ACadVersion version)
	{
		var doc = new CadDocument();
		doc.Header.Version = version;
		var line = new Line(new XYZ(1, 2, 3), new XYZ(4, 5, 6)) { Color = new Color(255, 0, 0) };
		doc.Entities.Add(line);
		var app = new AppId("LEGACY_DATA"); doc.AppIds.Add(app);
		line.ExtendedData.Add(app, new ExtendedData(new ExtendedDataRecord[] {
			new ExtendedDataString("legacy data"), new ExtendedDataInteger16(42),
		}));
		doc.Entities.Add(new Circle { Center = new XYZ(7, 8, 0), Radius = 3 });
		doc.Entities.Add(new TextEntity { Value = "Legacy sample" });
		var block = new BlockRecord("LEGACY");
		block.Entities.Add(new Line(new XYZ(7, 8, 0), new XYZ(9, 10, 0)));
		if (version == ACadVersion.AC1009)
			block.Entities.Add(new AttributeDefinition { Tag = "PART", Prompt = "Part number", Value = "default" });
		doc.BlockRecords.Add(block);
		var insert = new Insert(block);
		if (insert.Attributes.Any()) insert.Attributes.First().Value = "ABC-123";
		doc.Entities.Add(insert);
		var polyline = new LwPolyline { Elevation = 6, IsClosed = true };
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(2, 3)) { Bulge = 0.5, StartWidth = 0.2, EndWidth = 0.4 });
		polyline.Vertices.Add(new LwPolyline.Vertex(new XY(4, 5)));
		doc.Entities.Add(polyline);
		return doc;
	}

	private static byte[] writeDxf(CadDocument doc, bool binary)
	{
		using var stream = new MemoryStream();
		DxfWriter.Write(stream, doc, binary, new DxfWriterConfiguration { CloseStream = false, WriteShapes = true });
		return stream.ToArray();
	}

	private static CadDocument readDxf(byte[] data)
	{
		using var reader = new DxfReader(new MemoryStream(data));
		reader.Configuration.Failsafe = false;
		return reader.Read();
	}

	private static void verify(CadDocument doc, bool attributes)
	{
		Assert.True(doc.Entities.Count == 5, string.Join(", ", doc.Entities.Select(e => e.ObjectName)));
		var line = Assert.Single(doc.Entities.OfType<Line>());
		Assert.Equal(new XYZ(1, 2, 3), line.StartPoint);
		Assert.Equal(new XYZ(4, 5, 6), line.EndPoint);
		Assert.Equal(3, Assert.Single(doc.Entities.OfType<Circle>()).Radius);
		Assert.Equal("Legacy sample", Assert.Single(doc.Entities.OfType<TextEntity>()).Value);
		var insert = Assert.Single(doc.Entities.OfType<Insert>());
		Assert.Equal("LEGACY", insert.Block.Name);
		Assert.Equal(attributes ? 2 : 1, insert.Block.Entities.Count);
		if (attributes)
		{
			var attribute = Assert.Single(insert.Attributes);
			Assert.Equal("PART", attribute.Tag);
			Assert.Equal("ABC-123", attribute.Value);
			Assert.Same(insert, attribute.Owner);
			Assert.Equal("Part number", Assert.Single(insert.Block.Entities.OfType<AttributeDefinition>()).Prompt);
		}
		var polyline = Assert.Single(doc.Entities.OfType<IPolyline>());
		Assert.Equal(6, polyline.Elevation);
		Assert.True(polyline.IsClosed);
		Assert.Equal(0.5, polyline.Vertices.First().Bulge);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void R12AsciiAndBinaryPreserveLegacyContent(bool binary)
	{
		var doc = fixture(ACadVersion.AC1009);
		byte[] data = writeDxf(doc, binary);
		var result = readDxf(data);
		Assert.Equal(ACadVersion.AC1009, result.Header.Version);
		verify(result, true);
		Assert.Single(result.PaperSpace.Entities.OfType<Viewport>());
		Assert.Same(result, result.PaperSpace.Layout.Document);
		var line = Assert.Single(result.Entities.OfType<Line>());
		Assert.Equal(1, line.Color.Index);
		Assert.True(line.ExtendedData.TryGet("LEGACY_DATA", out var extended));
		Assert.Equal("legacy data", extended.Records[0].RawValue);
		Assert.Equal((short)42, extended.Records[1].RawValue);
		Assert.IsType<Polyline2D>(Assert.Single(result.Entities.OfType<IPolyline>()));
		Assert.IsType<LwPolyline>(Assert.Single(doc.Entities.OfType<IPolyline>()));
		if (binary) Assert.Equal(new byte[] { 0, 83, 69, 67, 84, 73, 79, 78 }, data.Skip(22).Take(8));
		else
		{
			string text = Encoding.ASCII.GetString(data);
			Assert.DoesNotContain("\nCLASSES", text); Assert.DoesNotContain("\nOBJECTS", text);
			Assert.DoesNotContain("AcDb", text); Assert.DoesNotContain("$CELWEIGHT", text);
			Assert.DoesNotContain("BLOCK_RECORD", text);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void R12Preserves3dPolylinesAndMeshTopology(bool binary)
	{
		verifyMeshes(readDxf(writeDxf(meshFixture(), binary)));
	}

	private static CadDocument meshFixture()
	{
		var doc = new CadDocument(); doc.Header.Version = ACadVersion.AC1009;
		var line = new Polyline3D();
		line.Vertices.Add(new Vertex3D(new XYZ(1, 2, 3))); line.Vertices.Add(new Vertex3D(new XYZ(4, 5, 6)));
		doc.Entities.Add(line);
		var mesh = new PolygonMesh { MVertexCount = 2, NVertexCount = 3 };
		foreach (var point in new[] { new XYZ(0, 0, 0), new XYZ(1, 0, 0), new XYZ(0, 1, 0),
			new XYZ(1, 1, 0), new XYZ(0, 2, 0), new XYZ(1, 2, 0) }) mesh.Vertices.Add(new PolygonMeshVertex(point));
		doc.Entities.Add(mesh);
		var polyface = new PolyfaceMesh();
		foreach (var point in new[] { new XYZ(0, 0, 0), new XYZ(1, 0, 0), new XYZ(0, 1, 0) })
			polyface.Vertices.Add(new VertexFaceMesh(point));
		polyface.Faces.Add(new VertexFaceRecord { Index1 = 1, Index2 = 2, Index3 = -3 });
		doc.Entities.Add(polyface);
		return doc;
	}

	private static void verifyMeshes(CadDocument result)
	{
		Assert.Equal(3, result.Entities.Count);
		Assert.Equal(new XYZ(4, 5, 6), Assert.Single(result.Entities.OfType<Polyline3D>()).Vertices.Last().Location);
		var actualMesh = Assert.Single(result.Entities.OfType<PolygonMesh>());
		Assert.Equal(2, actualMesh.MVertexCount); Assert.Equal(3, actualMesh.NVertexCount);
		Assert.Equal(6, actualMesh.Vertices.Count);
		var actualFace = Assert.Single(result.Entities.OfType<PolyfaceMesh>());
		Assert.Equal(3, actualFace.Vertices.Count); Assert.Equal(-3, Assert.Single(actualFace.Faces).Index3);
	}

	[Fact]
	public void R12ReportsUnsupportedModernEntities()
	{
		var doc = fixture(ACadVersion.AC1009); doc.Entities.Add(new MText());
		using var stream = new MemoryStream();
		using var writer = new DxfWriter(stream, doc);
		bool reported = false;
		writer.OnNotification += (_, e) => reported |= e.NotificationType == NotificationType.NotImplemented && e.Message.Contains("MTEXT");
		writer.Write(); Assert.True(reported);
	}

	[Fact]
	public void R12DimensionSubtypeIgnoresTextPositionFlag()
	{
		string text = "0\nSECTION\n2\nHEADER\n9\n$ACADVER\n1\nAC1009\n0\nENDSEC\n0\nSECTION\n2\nENTITIES\n"
			+ "0\nDIMENSION\n5\n10\n8\n0\n10\n1\n20\n2\n30\n0\n70\n131\n0\nENDSEC\n0\nEOF\n";
		var dimension = Assert.IsType<DimensionDiameter>(Assert.Single(readDxf(Encoding.ASCII.GetBytes(text)).Entities));
		Assert.True(dimension.IsTextUserDefinedLocation);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void R12ShapeNameSurvivesWithoutExternalShapeFont(bool binary)
	{
		string text = "0\nSECTION\n2\nHEADER\n9\n$ACADVER\n1\nAC1009\n0\nENDSEC\n0\nSECTION\n2\nENTITIES\n"
			+ "0\nSHAPE\n5\n10\n8\n0\n2\nMY-SHAPE\n10\n1\n20\n2\n30\n0\n40\n1\n0\nENDSEC\n0\nEOF\n";
		var doc = readDxf(Encoding.ASCII.GetBytes(text));
		Assert.Equal("MY-SHAPE", Assert.Single(doc.Entities.OfType<Shape>()).Clone() is Shape clone ? clone.ShapeName : null);
		var result = readDxf(writeDxf(doc, binary));
		Assert.Equal("MY-SHAPE", Assert.Single(result.Entities.OfType<Shape>()).ShapeName);
	}

	[Theory]
	[InlineData(ACadVersion.AC1009, false)]
	[InlineData(ACadVersion.AC1009, true)]
	[InlineData(ACadVersion.AC1012, false)]
	public void LegacyOutputPassesIndependentOdaAudit(ACadVersion version, bool binary)
	{
		string converter = Environment.GetEnvironmentVariable("ODA_FILE_CONVERTER");
		if (string.IsNullOrEmpty(converter)) return;
		string run = Path.Combine(TestVariables.OutputSamplesFolder, "legacy-oda-" + Guid.NewGuid().ToString("N"));
		string input = Path.GetFullPath(Path.Combine(run, "input")), output = Path.GetFullPath(Path.Combine(run, "output"));
		Directory.CreateDirectory(input); Directory.CreateDirectory(output);
		bool dwg = version == ACadVersion.AC1012;
		var doc = fixture(version);
		if (dwg) DwgWriter.Write(Path.Combine(input, "drawing.dwg"), doc);
		else
		{
			File.WriteAllBytes(Path.Combine(input, "drawing.dxf"), writeDxf(doc, binary));
			File.WriteAllBytes(Path.Combine(input, "mesh.dxf"), writeDxf(meshFixture(), binary));
		}
		using var process = Process.Start(new ProcessStartInfo(converter,
			$"\"{input}\" \"{output}\" ACAD2018 DXF 0 1 *.{(dwg ? "DWG" : "DXF")}") {
			UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
		});
		bool finished = process.WaitForExit(60000);
		if (!finished) process.Kill();
		Assert.True(finished, "ODA conversion timed out");
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(Directory.GetFiles(output, "*.err"));
		string converted = Path.Combine(output, "drawing.dxf");
		Assert.True(File.Exists(converted), "ODA produced no DXF");
		verify(readDxf(File.ReadAllBytes(converted)), !dwg);
		if (!dwg) verifyMeshes(readDxf(File.ReadAllBytes(Path.Combine(output, "mesh.dxf"))));
	}
}
