using ACadSharp.Entities.ProxyGraphics;
using CSMath;
using System;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Entities.ProxyGraphics;

public class ProxyGeometryWriterTests
{
	[Fact]
	public void WriteGeometriesSize()
	{
		IProxyGeometry[] geometries =
		{
			new ProxyCircle { Center = XYZ.Zero, Radius = 1, Normal = XYZ.AxisZ },
			new ProxySubentColor { ColorIndex = 1 },
		};

		byte[] data = ProxyGeometryWriter.WriteGeometries(ACadVersion.AC1032, Encoding.UTF8, geometries);

		Assert.Equal(data.Length, BitConverter.ToInt32(data, 0));
		Assert.Equal(geometries.Length, BitConverter.ToInt32(data, 4));

		int offset = 8;
		for (int i = 0; i < geometries.Length; i++)
		{
			int recordSize = BitConverter.ToInt32(data, offset);
			Assert.Equal((int)geometries[i].GraphicsType, BitConverter.ToInt32(data, offset + 4));
			offset += recordSize;
		}

		Assert.Equal(data.Length, offset);
	}
}
