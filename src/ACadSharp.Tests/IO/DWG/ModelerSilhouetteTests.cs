using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using CSMath;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class ModelerSilhouetteTests
{
	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void ReviewerFixtureReadsAndRewritesSilhouette(ACadVersion target)
	{
		// Public reproducer by ilCosmico, already opened successfully in TrueView 2025:
		// https://github.com/DomCR/ACadSharp/pull/1350#issuecomment-6820438181
		string path = Path.Combine(TestVariables.SamplesFolder, "modeler", "silhouette_sab_AC1021.dwg");
		var document = DwgReader.Read(path, new DwgReaderConfiguration { Failsafe = false });
		var original = Assert.Single(document.Entities.OfType<Solid3D>());
		Assert.NotEmpty(original.AcisData);
		Assert.Single(original.Silhouettes);
		var sample = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1021.dwg"));
		Assert.Contains(sample.Entities.OfType<ModelerGeometry>(), e => e.AcisData != null && e.AcisData.SequenceEqual(original.AcisData));
		document.Header.Version = target;
		var actual = Assert.Single(ModelerGeometryWriterTests.Roundtrip(document).Entities.OfType<Solid3D>());
		Assert.Equal(original.AcisData, actual.AcisData);
		AssertSilhouette(Assert.Single(original.Silhouettes), Assert.Single(actual.Silhouettes));
	}

	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1021)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void SilhouetteFieldsAndFollowingEntitiesSurviveRoundtrip(ACadVersion version)
	{
		var document = new CadDocument(); document.Header.Version = version;
		foreach (var geometry in new ModelerGeometry[] { new Solid3D(), new Region(), new CadBody() })
		{
			geometry.AcisData = Encoding.ASCII.GetBytes("400 0 1 0\nEnd-of-ACIS-data\n");
			foreach (int id in new[] { 0, 255, 256, int.MaxValue, int.MinValue })
			{
				var silhouette = new ModelerGeometry.Silhouette
				{
					ViewportId = id, ViewportTarget = new XYZ(1, 2, 3),
					ViewportDirectionFromTarget = new XYZ(4, 5, 6),
					ViewportUpDirection = XYZ.AxisZ, ViewportPerspective = id != 0
				};
				if (id != 0)
				{
					var wire = new ModelerGeometry.Wire { AcisIndex = 17, SelectionMarker = 29, Color = new Color(3) };
					wire.Points.Add(new XYZ(7, 8, 9)); silhouette.Wires.Add(wire);
				}
				geometry.Silhouettes.Add(silhouette);
			}
			document.Entities.Add(geometry);
		}
		var line = new Line(new XYZ(10, 11, 12), new XYZ(13, 14, 15)); document.Entities.Add(line);
		var restored = ModelerGeometryWriterTests.Roundtrip(document);
		Assert.Equal(4, restored.Entities.Count);
		foreach (var original in document.Entities.OfType<ModelerGeometry>())
		{
			var actual = Assert.Single(restored.Entities.OfType<ModelerGeometry>(), e => e.Handle == original.Handle);
			Assert.Equal(original.AcisData, actual.AcisData);
			Assert.Equal(original.Silhouettes.Count, actual.Silhouettes.Count);
			for (int i = 0; i < original.Silhouettes.Count; i++) AssertSilhouette(original.Silhouettes[i], actual.Silhouettes[i]);
		}
		var actualLine = Assert.Single(restored.Entities.OfType<Line>());
		Assert.Equal(line.StartPoint, actualLine.StartPoint); Assert.Equal(line.EndPoint, actualLine.EndPoint);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(255)]
	[InlineData(256)]
	[InlineData(int.MaxValue)]
	[InlineData(int.MinValue)]
	public void ViewportIdUsesBitLongAndLeavesNextFieldAligned(int id)
	{
		// Read the emitted fields directly so matching reader/writer bugs cannot hide BLL encoding.
		using var stream = new MemoryStream();
		var document = new CadDocument(); document.Header.Version = ACadVersion.AC1018;
		var writer = new DwgObjectWriter(stream, document, Encoding.ASCII);
		var bits = DwgStreamWriterBase.GetStreamWriter(ACadVersion.AC1018, stream, Encoding.ASCII);
		typeof(DwgObjectWriter).GetField("_writer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(writer, bits);
		var geometry = new Solid3D();
		geometry.Silhouettes.Add(new ModelerGeometry.Silhouette { ViewportId = id, ViewportTarget = new XYZ(1, 2, 3) });
		typeof(DwgObjectWriter).GetMethod("writeModelerGeometry", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(writer, new object[] { geometry });
		long end = bits.PositionInBits;
		bits.WriteByte(0xa5); bits.WriteSpearShift();
		stream.Position = 0;
		var reader = DwgStreamReaderBase.GetStreamHandler(ACadVersion.AC1018, stream);
		Assert.True(reader.ReadBit()); // Empty ACIS payload.
		Assert.True(reader.ReadBit()); Assert.False(reader.ReadBit()); // Wireframe, no point.
		Assert.Equal(0, reader.ReadBitLong()); Assert.False(reader.ReadBit()); // No isolines.
		Assert.Equal(1, reader.ReadBitLong());
		Assert.Equal(id, reader.ReadBitLong());
		Assert.Equal(new XYZ(1, 2, 3), reader.Read3BitDouble());
		Assert.Equal(XYZ.Zero, reader.Read3BitDouble()); Assert.Equal(XYZ.Zero, reader.Read3BitDouble());
		Assert.False(reader.ReadBit()); Assert.False(reader.ReadBit()); // No perspective or wires.
		Assert.True(reader.ReadBit()); // No second ACIS payload.
		Assert.Equal(end, reader.PositionInBits()); Assert.Equal(0xa5, reader.ReadByte());
	}

	private static void AssertSilhouette(ModelerGeometry.Silhouette expected, ModelerGeometry.Silhouette actual)
	{
		Assert.Equal(expected.ViewportId, actual.ViewportId);
		Assert.Equal(expected.ViewportTarget, actual.ViewportTarget);
		Assert.Equal(expected.ViewportDirectionFromTarget, actual.ViewportDirectionFromTarget);
		Assert.Equal(expected.ViewportUpDirection, actual.ViewportUpDirection);
		Assert.Equal(expected.ViewportPerspective, actual.ViewportPerspective);
		Assert.Equal(expected.Wires.Count, actual.Wires.Count);
		for (int i = 0; i < expected.Wires.Count; i++)
		{
			Assert.Equal(expected.Wires[i].AcisIndex, actual.Wires[i].AcisIndex);
			Assert.Equal(expected.Wires[i].SelectionMarker, actual.Wires[i].SelectionMarker);
			Assert.Equal(expected.Wires[i].Color, actual.Wires[i].Color);
			Assert.Equal(expected.Wires[i].Points, actual.Wires[i].Points);
		}
	}
}
