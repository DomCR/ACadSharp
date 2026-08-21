using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A hatch is associative because its boundary points at entities. When those entities are not
/// written - a Region, say, which neither writer implements - the file must not claim otherwise,
/// or AutoCAD reads the boundary as undefined and repairs the drawing.
///
/// DWG only. The same change in the DXF writer was measured on real drawings and made two of them
/// worse (one by 6 audit errors, one by 2), so it is not made there; see T64.
/// </summary>
public class HatchAssociativityTests
{
	public static TheoryData<ACadVersion> Versions => new TheoryData<ACadVersion>
	{
		ACadVersion.AC1018,
		ACadVersion.AC1024,
		ACadVersion.AC1032,
	};

	[Theory]
	[MemberData(nameof(Versions))]
	public void DwgDropsAnAssociativityItCannotWrite(ACadVersion version)
	{
		CadDocument doc = this.document(version);

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		this.assertNotAssociative(DwgReader.Read(new MemoryStream(ms.ToArray())));
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void AnAssociativityItCanWriteIsKept(ACadVersion version)
	{
		CadDocument doc = this.document(version, writable: true);

		MemoryStream ms = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(ms, doc))
		{
			writer.Write();
		}

		CadDocument read = DwgReader.Read(new MemoryStream(ms.ToArray()));
		Hatch hatch = read.Entities.OfType<Hatch>().Single();
		Assert.True(hatch.IsAssociative);
		Assert.Single(hatch.Paths.Single().Entities);
	}

	private CadDocument document(ACadVersion version, bool writable = false)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		//A Region is not written by either writer; a Line is. The hatch keeps its own edges either
		//way, so the only thing that changes is whether the boundary handle can be written.
		Entity boundary = writable
			? (Entity)new Line(new XYZ(0, 0, 0), new XYZ(1, 0, 0))
			: new Region();
		doc.Entities.Add(boundary);

		Hatch hatch = new Hatch { IsSolid = true, IsAssociative = true };
		hatch.SeedPoints.Add(new XY());

		Hatch.BoundaryPath path = new Hatch.BoundaryPath();
		path.Edges.Add(new Hatch.BoundaryPath.Line { Start = new XY(0, 0), End = new XY(1, 0) });
		path.Edges.Add(new Hatch.BoundaryPath.Line { Start = new XY(1, 0), End = new XY(1, 1) });
		path.Edges.Add(new Hatch.BoundaryPath.Line { Start = new XY(1, 1), End = new XY(0, 0) });
		path.Entities.Add(boundary);
		hatch.Paths.Add(path);

		doc.Entities.Add(hatch);

		return doc;
	}

	private void assertNotAssociative(CadDocument doc)
	{
		Hatch hatch = doc.Entities.OfType<Hatch>().Single();
		Assert.False(hatch.IsAssociative);
		Assert.Empty(hatch.Paths.Single().Entities);
		Assert.Equal(3, hatch.Paths.Single().Edges.Count);
	}
}
