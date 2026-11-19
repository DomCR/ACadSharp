namespace ACadSharp.Tests.IO;

using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

/// <summary>
/// An XCLIP boundary is a SPATIAL_FILTER, and it carries two matrices that say where the boundary
/// sits. Both formats record them as three rows of four with the translation in the fourth column,
/// which is the transpose of the layout Matrix4 itself uses - so every reader and writer has to
/// turn them, and for a long time not all four did.
///
/// The existing single-object round trip could not see any of this: its fixture built the filter
/// with identity matrices, and an identity survives being transposed, dropped or doubled. These
/// tests use a matrix with a scale AND a translation, which nothing but a correct pairing returns
/// unchanged.
/// </summary>
public class SpatialFilterRoundTripTests
{
	[Fact]
	public void TheClipTransformSurvivesDxf()
	{
		CadDocument doc = this.docWithFilter(out Matrix4 inverse, out Matrix4 forward);

		SpatialFilter back = this.filterOf(this.roundTripDxf(doc));

		AssertMatrix(inverse, back.InverseInsertTransform);
		AssertMatrix(forward, back.InsertTransform);
	}

	[Fact]
	public void TheClipTransformSurvivesDwg()
	{
		CadDocument doc = this.docWithFilter(out Matrix4 inverse, out Matrix4 forward);

		SpatialFilter back = this.filterOf(this.roundTripDwg(doc));

		AssertMatrix(inverse, back.InverseInsertTransform);
		AssertMatrix(forward, back.InsertTransform);
	}

	[Fact]
	public void TheTwoFormatsAgreeWithEachOther()
	{
		//The one a per-format round trip cannot catch: each format can be self-consistent and still
		//disagree with the other, and then a drawing read from a DWG and the same drawing read back
		//out of our own DXF put the clip in different places.
		CadDocument doc = this.docWithFilter(out _, out _);

		SpatialFilter viaDxf = this.filterOf(this.roundTripDxf(doc));
		SpatialFilter viaDwg = this.filterOf(this.roundTripDwg(doc));

		AssertMatrix(viaDwg.InverseInsertTransform, viaDxf.InverseInsertTransform);
		AssertMatrix(viaDwg.InsertTransform, viaDxf.InsertTransform);
	}

	private static void AssertMatrix(Matrix4 expected, Matrix4 actual)
	{
		for (int row = 0; row < 4; row++)
		{
			for (int col = 0; col < 4; col++)
			{
				Assert.Equal(expected[row, col], actual[row, col], 6);
			}
		}
	}

	private CadDocument docWithFilter(out Matrix4 inverse, out Matrix4 forward)
	{
		//A scale and a translation together: a pure scale survives a transpose, and so does an
		//identity, so neither would prove anything.
		inverse = Matrix4.CreateTranslation(new XYZ(-1087.9487, 942.61675, 0))
			* Matrix4.CreateScale(new XYZ(25.4, 25.4, 25.4));
		forward = Matrix4.CreateTranslation(new XYZ(3, -5, 7));

		CadDocument doc = new CadDocument();
		BlockRecord block = new BlockRecord("clipped_block");
		block.Entities.Add(new Circle { Radius = 20 });
		doc.BlockRecords.Add(block);

		Insert insert = new Insert(block);
		SpatialFilter filter = new SpatialFilter(SpatialFilter.SpatialFilterEntryName);
		filter.BoundaryPoints.Add(new XY(5, 5));
		filter.BoundaryPoints.Add(new XY(30, 30));
		filter.InverseInsertTransform = inverse;
		filter.InsertTransform = forward;
		insert.SpatialFilter = filter;

		doc.Entities.Add(insert);
		return doc;
	}

	private SpatialFilter filterOf(CadDocument doc)
	{
		Insert insert = doc.Entities.OfType<Insert>().Single(i => i.SpatialFilter != null);
		return insert.SpatialFilter;
	}

	private CadDocument roundTripDxf(CadDocument doc)
	{
		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		return DxfReader.Read(new MemoryStream(stream.ToArray()));
	}

	private CadDocument roundTripDwg(CadDocument doc)
	{
		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		return DwgReader.Read(new MemoryStream(stream.ToArray()));
	}
}
