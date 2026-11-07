using ACadSharp.Entities.ProxyGraphics;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.Entities.ProxyGraphics;

public class ProxySubentTrueColorTests
{
	[Theory]
	[InlineData(255, 0, 0, new byte[] { 0x00, 0x00, 0xFF, 0xC2 })]
	[InlineData(10, 20, 30, new byte[] { 0x1E, 0x14, 0x0A, 0xC2 })]
	public void ReadByColor(byte r, byte g, byte b, byte[] color)
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(stream);
		writer.Write(20);
		writer.Write(1);
		//Size of the record, includes size and type
		writer.Write(12);
		writer.Write((int)GraphicsType.SubentTrueColor);
		writer.Write(color);

		DwgDocumentBuilder builder = new DwgDocumentBuilder(ACadVersion.AC1032, new CadDocument(), new DwgReaderConfiguration());
		IProxyGeometry geometry = Assert.Single(ProxyGeometryReader.ReadGeometries(builder, stream.ToArray()));

		ProxySubentTrueColor trueColor = Assert.IsType<ProxySubentTrueColor>(geometry);
		Assert.Equal(ProxyColorMethod.ByColor, trueColor.ColorMethod);
		Assert.Equal(new Color(r, g, b), trueColor.Color);
	}

	[Theory]
	[InlineData(255, 0, 0, new byte[] { 0x00, 0x00, 0xFF, 0xC2 })]
	[InlineData(10, 20, 30, new byte[] { 0x1E, 0x14, 0x0A, 0xC2 })]
	public void WriteByColor(byte r, byte g, byte b, byte[] color)
	{
		ProxySubentTrueColor trueColor = new ProxySubentTrueColor
		{
			Color = new Color(r, g, b),
			ColorMethod = ProxyColorMethod.ByColor
		};

		byte[] data = ProxyGeometryWriter.WriteGeometries(ACadVersion.AC1032, Encoding.UTF8, new[] { trueColor });

		Assert.Equal(color, data.Skip(data.Length - 4).ToArray());
	}
}
