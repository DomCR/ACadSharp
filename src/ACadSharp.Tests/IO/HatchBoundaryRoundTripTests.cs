using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class HatchBoundaryRoundTripTests
{
	[Theory]
	[InlineData("dwg")]
	[InlineData("dxf")]
	public void RepeatedBoundaryObjectInALoopIsKept(string format)
	{
		//AutoCAD cites the same boundary object more than once in a single loop (found in production drawings:
		//a loop with 11 handles and 7 distinct objects). The reader kept them in a set and dropped the repetition,
		//so the next save lost the link.
		CadDocument doc = new CadDocument();
		LwPolyline boundary = new LwPolyline { IsClosed = true };
		boundary.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		boundary.Vertices.Add(new LwPolyline.Vertex(new XY(10, 0)));
		boundary.Vertices.Add(new LwPolyline.Vertex(new XY(10, 10)));
		boundary.Vertices.Add(new LwPolyline.Vertex(new XY(0, 10)));
		doc.Entities.Add(boundary);

		Hatch.BoundaryPath path = new Hatch.BoundaryPath((IEnumerable<Entity>)new Entity[] { boundary });
		path.Entities.Add(boundary);
		Hatch hatch = new Hatch { IsAssociative = true, IsSolid = true, Pattern = HatchPattern.Solid };
		hatch.Paths.Add(path);
		doc.Entities.Add(hatch);

		CadDocument read = this.writeAndRead(doc, format);

		Hatch.BoundaryPath readPath = read.Entities.OfType<Hatch>().Single().Paths.Single();
		Assert.Equal(2, readPath.Entities.Count);
		Assert.Same(readPath.Entities[0], readPath.Entities[1]);
	}

	private CadDocument writeAndRead(CadDocument doc, string format)
	{
		string path = Path.Combine(Path.GetTempPath(), $"acadsharp-{Guid.NewGuid():N}.{format}");
		try
		{
			if (format == "dwg")
			{
				using (FileStream fs = File.Create(path))
				{
					new DwgWriter(fs, doc).Write();
				}

				return DwgReader.Read(path);
			}

			new DxfWriter(path, doc, false).Write();
			return DxfReader.Read(path);
		}
		finally
		{
			File.Delete(path);
		}
	}
}
