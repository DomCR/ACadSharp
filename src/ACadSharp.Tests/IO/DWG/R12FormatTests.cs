using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class R12FormatTests
{
	private static CadDocument read(byte[] bytes) => DwgReader.Read(new MemoryStream(bytes), new DwgReaderConfiguration { Failsafe = false, CrcCheck = true });

	[Fact]
	public void ReadsNativeFixtureWithAttributesAndSignedMeshes()
	{
		var doc = read(File.ReadAllBytes(Path.Combine(TestVariables.SamplesFolder, "legacy", "r12-native.dwg")));
		Assert.Equal(ACadVersion.AC1009, doc.Header.Version); Assert.Equal(8, doc.Entities.Count);
		Assert.Equal(new XYZ(1, 2, 3), Assert.Single(doc.Entities.OfType<Line>()).StartPoint);
		var insert = Assert.Single(doc.Entities.OfType<Insert>());
		Assert.Equal("LEGACY", insert.Block.Name); Assert.Equal("ABC-123", Assert.Single(insert.Attributes).Value);
		Assert.Equal(4, Assert.Single(doc.Entities.OfType<PolygonMesh>()).Vertices.Count);
		Assert.Equal(-3, Assert.Single(Assert.Single(doc.Entities.OfType<PolyfaceMesh>()).Faces).Index3);
	}

	[Fact]
	public void WritesGeometryAttributesAndMeshSequences()
	{
		var doc = LegacyFormatTests.fixture(ACadVersion.AC1009);
		LegacyFormatTests.verify(roundtrip(doc), true);
		LegacyFormatTests.verifyMeshes(roundtrip(LegacyFormatTests.meshFixture()));
	}

	private static CadDocument roundtrip(CadDocument document)
	{
		using var stream = new MemoryStream();
		DwgWriter.Write(stream, document, new DwgWriterConfiguration { CloseStream = false });
		byte[] bytes = stream.ToArray();
		using var reader = new DwgReader(new MemoryStream(bytes));
		Assert.Equal(ACadVersion.AC1009, reader.ReadHeader().Version);
		return read(bytes);
	}

	[Fact]
	public void RetainsLegacyTextAndRadians()
	{
		var doc = new CadDocument(); doc.Header.Version = ACadVersion.AC1009;
		doc.Entities.Add(new TextEntity { Value = "Grüße €", Rotation = System.Math.PI / 3, InsertPoint = new XYZ(2, 3, 4) });
		var text = Assert.Single(roundtrip(doc).Entities.OfType<TextEntity>());
		Assert.Equal("Grüße €", text.Value); Assert.Equal(System.Math.PI / 3, text.Rotation, 12); Assert.Equal(new XYZ(2, 3, 4), text.InsertPoint);
	}

	[Fact]
	public void RejectsDamagedCrcAndTruncatedSections()
	{
		byte[] bytes = File.ReadAllBytes(Path.Combine(TestVariables.SamplesFolder, "legacy", "r12-native.dwg"));
		bytes[0x100] ^= 1; Assert.Throws<InvalidDataException>(() => read(bytes));
		Assert.Throws<InvalidDataException>(() => read(bytes.Take(100).ToArray()));
	}
}
