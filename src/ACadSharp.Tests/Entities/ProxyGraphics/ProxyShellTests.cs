using ACadSharp.Entities.ProxyGraphics;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities.ProxyGraphics;

public class ProxyShellTests
{
	[Fact]
	public void CloneHoles()
	{
		ProxyShell shell = new ProxyShell();
		shell.Holes.Add(1);

		ProxyShell clone = (ProxyShell)shell.Clone();

		Assert.Equal(shell.Holes, clone.Holes);
		Assert.NotSame(shell.Holes, clone.Holes);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ReadShellWithHole(bool withTraits)
	{
		MemoryStream data = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(data);

		writer.Write(8);
		for (int i = 0; i < 8; i++)
		{
			writer.Write((double)i);
			writer.Write(0.0);
			writer.Write(0.0);
		}

		int[] faceList = { 4, 0, 1, 2, 3, -4, 4, 5, 6, 7 };
		writer.Write(faceList.Length);
		foreach (int value in faceList)
		{
			writer.Write(value);
		}

		PrimitiveFlags flags = withTraits ? PrimitiveFlags.HasColors : PrimitiveFlags.None;

		writer.Write((int)flags);
		if (withTraits)
		{
			for (int i = 1; i <= 8; i++)
			{
				writer.Write(i);
			}
		}

		writer.Write((int)flags);
		if (withTraits)
		{
			writer.Write(11);
			writer.Write(12);
		}

		writer.Write((int)PrimitiveFlags.None);

		byte[] shellData = data.ToArray();
		MemoryStream record = new MemoryStream();
		writer = new BinaryWriter(record);
		writer.Write(16 + shellData.Length);
		writer.Write(1);
		writer.Write(8 + shellData.Length);
		writer.Write((int)GraphicsType.Shell);
		writer.Write(shellData);

		DwgDocumentBuilder builder = new DwgDocumentBuilder(ACadVersion.AC1032, new CadDocument(), new DwgReaderConfiguration());
		ProxyShell shell = Assert.IsType<ProxyShell>(Assert.Single(ProxyGeometryReader.ReadGeometries(builder, record.ToArray())));

		Assert.Equal(8, shell.Vertices.Count);
		Assert.Equal(2, shell.Faces.Count);
		Assert.Equal(new[] { 0, 1, 2, 3 }, shell.Faces[0]);
		Assert.Equal(new[] { 4, 5, 6, 7 }, shell.Faces[1]);
		Assert.Equal(1, Assert.Single(shell.Holes));

		if (withTraits)
		{
			Assert.Equal(Enumerable.Range(1, 8), shell.EdgeTraits.Colors);
			Assert.Equal(new[] { 11, 12 }, shell.FaceTraits.Colors);
		}
		else
		{
			Assert.Null(shell.EdgeTraits);
			Assert.Null(shell.FaceTraits);
		}
	}
}
