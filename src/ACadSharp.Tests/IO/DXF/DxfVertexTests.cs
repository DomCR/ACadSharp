using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfVertexTests
{
	private static readonly XYZ[] _locations = { new XYZ(0, 0, 0), new XYZ(10, 0, 0), new XYZ(10, 10, 0), new XYZ(0, 10, 0) };

	[Fact]
	public void ReadVertexWithoutSubtypeOf2dPolyline()
	{
		CadDocument doc = readPolyline(DxfSubclassMarker.Polyline, "70\n1\n");

		Polyline2D polyline = doc.Entities.OfType<Polyline2D>().Single();
		Assert.True(polyline.IsClosed);
		Assert.All(polyline.Vertices, v => Assert.IsType<Vertex2D>(v));
		Assert.Equal(_locations, polyline.Vertices.Select(v => v.Location));
	}

	[Fact]
	public void ReadVertexWithoutSubtypeOf3dPolyline()
	{
		CadDocument doc = readPolyline(DxfSubclassMarker.Polyline3d, "70\n9\n");

		Polyline3D polyline = doc.Entities.OfType<Polyline3D>().Single();
		Assert.True(polyline.IsClosed);
		Assert.All(polyline.Vertices, v => Assert.IsType<Vertex3D>(v));
		Assert.Equal(_locations, polyline.Vertices.Select(v => v.Location));
	}

	[Fact]
	public void ReadVertexWithoutSubtypeOfPolygonMesh()
	{
		CadDocument doc = readPolyline(DxfSubclassMarker.PolygonMesh, "70\n16\n71\n2\n72\n2\n");

		PolygonMesh mesh = doc.Entities.OfType<PolygonMesh>().Single();
		Assert.All(mesh.Vertices, v => Assert.IsType<PolygonMeshVertex>(v));
		Assert.Equal(_locations, mesh.Vertices.Select(v => v.Location));
	}

	private static CadDocument readPolyline(string subclass, string codes)
	{
		//R13+ VERTEX records whose subclass markers end at AcDbVertex, without the subtype marker after it
		StringBuilder dxf = new StringBuilder();
		dxf.Append("0\nSECTION\n2\nHEADER\n9\n$ACADVER\n1\nAC1014\n0\nENDSEC\n");
		dxf.Append("0\nSECTION\n2\nENTITIES\n");
		dxf.Append($"0\nPOLYLINE\n5\n10\n100\nAcDbEntity\n8\n0\n100\n{subclass}\n66\n1\n10\n0\n20\n0\n30\n0\n{codes}");
		for (int i = 0; i < _locations.Length; i++)
		{
			XYZ p = _locations[i];
			dxf.Append(string.Format(CultureInfo.InvariantCulture,
				"0\nVERTEX\n5\n{0:X}\n100\nAcDbEntity\n8\n0\n100\nAcDbVertex\n10\n{1}\n20\n{2}\n30\n{3}\n",
				0x11 + i, p.X, p.Y, p.Z));
		}
		dxf.Append("0\nSEQEND\n5\n20\n100\nAcDbEntity\n8\n0\n");
		dxf.Append("0\nENDSEC\n0\nEOF\n");

		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf.ToString())))
		using (DxfReader reader = new DxfReader(stream))
		{
			return reader.Read();
		}
	}
}
