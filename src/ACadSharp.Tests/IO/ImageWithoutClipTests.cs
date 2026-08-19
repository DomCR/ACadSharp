using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class ImageWithoutClipTests
{
	public static TheoryData<ACadVersion> Versions => new TheoryData<ACadVersion>
	{
		ACadVersion.AC1015,
		ACadVersion.AC1018,
		ACadVersion.AC1024,
		ACadVersion.AC1032,
	};

	private static CadDocument document(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;
		ImageDefinition definition = new ImageDefinition
		{
			Name = "photo",
			FileName = "photo.png",
			Size = new XY(550, 550),
			DefaultSize = new XY(1, 1),
		};
		RasterImage image = new RasterImage(definition)
		{
			InsertPoint = new XYZ(0, 0, 0),
			UVector = new XYZ(2, 0, 0),
			VVector = new XYZ(0, 2, 0),
			Size = new XY(550, 550),
			ClippingState = false,
		};
		//No clip boundary at all: the case AutoCAD refuses to read when it is written as such.
		Assert.Empty(image.ClipBoundaryVertices);
		doc.ModelSpace.Entities.Add(image);
		return doc;
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void DxfWritesTheDefaultRectangleForAnImageWithoutAClip(ACadVersion version)
	{
		//AutoCAD writes an unclipped image with a rectangular boundary of two vertices, (-0.5,-0.5)
		//and (Size - 0.5): checked on six such images of a production drawing it saved as DXF. An
		//image written with no boundary at all makes it discard the whole file.
		CadDocument doc = document(version);

		MemoryStream ms = new MemoryStream();
		using (DxfWriter writer = new DxfWriter(ms, doc, false))
		{
			writer.Write();
		}

		CadDocument rt = DxfReader.Read(new MemoryStream(ms.ToArray()));
		RasterImage got = Assert.Single(rt.ModelSpace.Entities.OfType<RasterImage>());
		Assert.Equal(ClipType.Rectangular, got.ClipType);
		Assert.Equal(2, got.ClipBoundaryVertices.Count);
		Assert.Equal(new XY(-0.5, -0.5), got.ClipBoundaryVertices[0]);
		Assert.Equal(new XY(549.5, 549.5), got.ClipBoundaryVertices[1]);
		Assert.False(got.ClippingState);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void DwgWritesTheDefaultRectangleForAnImageWithoutAClip(ACadVersion version)
	{
		//The DWG writer used to index [0] and [1] on the empty list and throw.
		CadDocument doc = document(version);

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		CadDocument rt = DwgReader.Read(new MemoryStream(ms.ToArray()));
		RasterImage got = Assert.Single(rt.ModelSpace.Entities.OfType<RasterImage>());
		Assert.Equal(ClipType.Rectangular, got.ClipType);
		Assert.Equal(2, got.ClipBoundaryVertices.Count);
		Assert.Equal(new XY(-0.5, -0.5), got.ClipBoundaryVertices[0]);
		Assert.Equal(new XY(549.5, 549.5), got.ClipBoundaryVertices[1]);
	}
}
