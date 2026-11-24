using ACadSharp.Entities.ProxyGraphics;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Internal;

public class ProxyGeometryReaderTests
{
	[Fact]
	public void ReadGeometriesContinuesAtDeclaredRecordEnd()
	{
		XYZ point1 = new XYZ(1, 2, 3);
		XYZ point2 = new XYZ(4, 5, 6);

		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);
		writer.Write(8 + 8 + 56);
		writer.Write(2);

		//Circle without data, its reader reads past the end of the record
		writer.Write(8);
		writer.Write((int)GraphicsType.Circle);

		writer.Write(56);
		writer.Write((int)GraphicsType.XLine);
		writer.Write(point1.X);
		writer.Write(point1.Y);
		writer.Write(point1.Z);
		writer.Write(point2.X);
		writer.Write(point2.Y);
		writer.Write(point2.Z);

		DwgDocumentBuilder builder = new DwgDocumentBuilder(ACadVersion.AC1032, new CadDocument(), new DwgReaderConfiguration());
		var geometries = ProxyGeometryReader.ReadGeometries(builder, stream.ToArray()).ToList();

		Assert.Equal(2, geometries.Count);
		Assert.IsType<ProxyCircle>(geometries[0]);
		ProxyXLine xline = Assert.IsType<ProxyXLine>(geometries[1]);
		Assert.Equal(point1, xline.Point1);
		Assert.Equal(point2, xline.Point2);
	}
}
