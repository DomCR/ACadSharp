using ACadSharp.Entities.ProxyGraphics;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using CSMath;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Entities.ProxyGraphics;

public class ProxyGeometryReaderTests
{
	[Fact]
	public void ReadShellWithVertexNormals()
	{
		ProxyShell shell = new ProxyShell();
		shell.Vertices.AddRange(new[] { new XYZ(0, 0, 0), new XYZ(1, 0, 0), new XYZ(0, 1, 0) });
		shell.Faces.Add(new[] { 0, 1, 2 });
		shell.VertexTraits = new VertexTraits
		{
			Normals = new List<XYZ> { XYZ.AxisZ, XYZ.AxisZ, new XYZ(0, 0, -1) },
			Orientation = 1,
		};

		byte[] data = ProxyGeometryWriter.WriteGeometries(ACadVersion.AC1032, Encoding.UTF8, new[] { shell });

		DwgDocumentBuilder builder = new DwgDocumentBuilder(ACadVersion.AC1032, new CadDocument(), new DwgReaderConfiguration());
		ProxyShell result = Assert.IsType<ProxyShell>(Assert.Single(ProxyGeometryReader.ReadGeometries(builder, data)));

		Assert.Equal(shell.Vertices, result.Vertices);
		Assert.Equal(shell.VertexTraits.Normals, result.VertexTraits.Normals);
		Assert.Equal(shell.VertexTraits.Orientation, result.VertexTraits.Orientation);
	}
}
