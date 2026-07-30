using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class DimensionTests
{
	[Theory]
	[InlineData("dwg")]
	[InlineData("dxf")]
	public void ArcDimensionRoundTrip(string format)
	{
		DimensionArc dim = new DimensionArc
		{
			DefinitionPoint = new XYZ(1, 2, 0),
			TextMiddlePoint = new XYZ(3, 4, 0),
			FirstPoint = new XYZ(10, 0, 0),
			SecondPoint = new XYZ(0, 10, 0),
			Center = new XYZ(5, 6, 0),
			IsPartial = true,
			StartAngle = 0.5,
			EndAngle = 1.5,
			HasLeader = true,
			LeaderPoint1 = new XYZ(7, 8, 0),
			LeaderPoint2 = new XYZ(9, 10, 0),
			AttachmentPoint = AttachmentPointType.MiddleCenter,
		};
		CadDocument doc = createDocument(dim);

		MemoryStream ms = new MemoryStream();
		if (format == "dwg")
		{
			DwgWriter.Write(ms, doc);
		}
		else
		{
			using DxfWriter writer = new DxfWriter(ms, doc, false);
			writer.Write();
		}

		CadDocument read;
		using (MemoryStream readStream = new MemoryStream(ms.ToArray()))
		{
			read = format == "dwg" ? DwgReader.Read(readStream) : DxfReader.Read(readStream);
		}

		DimensionArc result = read.Entities.OfType<DimensionArc>().Single();

		Assert.Equal(dim.DefinitionPoint, result.DefinitionPoint);
		Assert.Equal(dim.TextMiddlePoint, result.TextMiddlePoint);
		Assert.Equal(dim.FirstPoint, result.FirstPoint);
		Assert.Equal(dim.SecondPoint, result.SecondPoint);
		Assert.Equal(dim.Center, result.Center);
		Assert.True(result.IsPartial);
		Assert.Equal(dim.StartAngle, result.StartAngle);
		Assert.Equal(dim.EndAngle, result.EndAngle);
		Assert.True(result.HasLeader);
		Assert.Equal(dim.LeaderPoint1, result.LeaderPoint1);
		Assert.Equal(dim.LeaderPoint2, result.LeaderPoint2);
		Assert.Equal(AttachmentPointType.MiddleCenter, result.AttachmentPoint);
	}

	[Fact]
	public void ReadDimensionsWithoutSubclassMarkers()
	{
		// Pre R13 files do not have the 100 subclass markers, the values must be
		// assigned using the subclass of the entity that is being read.
		string path = Path.Combine(TestVariables.SamplesFolder, "sample_AC1009_ascii.dxf");

		CadDocument doc = DxfReader.Read(path);

		var dimensions = doc.Entities.OfType<Dimension>().ToList();

		Assert.NotEmpty(dimensions);
		Assert.All(dimensions, d => Assert.NotEqual(XYZ.Zero, d.DefinitionPoint));
		Assert.All(dimensions, d => Assert.NotEqual(XYZ.Zero, d.TextMiddlePoint));
	}

	private static CadDocument createDocument(Dimension dim)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = ACadVersion.AC1032;
		doc.Entities.Add(dim);

		return doc;
	}
}
