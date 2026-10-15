using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests;

public class TransparencyTests
{
	//The alpha that AutoCAD LT 2022 writes for each transparency, measured on 91 lines
	//(CETRANSPARENCY 0 to 90 and DXFOUT): floor(255 * (100 - t) / 100). The alpha math was already right;
	//what was wrong is the method byte.
	private static int autocadAlpha(int t) => (int)(255 * (100 - t) / 100.0);

	[Fact]
	public void FromAlphaValueReadsTheMethodByte()
	{
		//440 = 0 is an explicit ByLayer, it was read as ByBlock
		Assert.True(Transparency.FromAlphaValue(0).IsByLayer);
		Assert.True(Transparency.FromAlphaValue(0x01000000).IsByBlock);
		Assert.Equal(50, Transparency.FromAlphaValue(0x02000000 | 127).Value);
		//alpha 0 is 100% transparent, clamped to 90 like AutoCAD does, not ByBlock
		Assert.Equal(90, Transparency.FromAlphaValue(0x02000000).Value);
	}

	[Fact]
	public void AutoCadAlphaValuesRoundTrip()
	{
		for (short t = 0; t <= 90; t++)
		{
			int value = 0x02000000 | autocadAlpha(t);
			Assert.Equal(t, Transparency.FromAlphaValue(value).Value);
			Assert.Equal(value, Transparency.ToAlphaValue(new Transparency(t)));
		}
	}

	[Fact]
	public void ToAlphaValueByLayerIsZero()
	{
		//(byte)(255 * 101 / 100) saturated into 255, an explicit 0%
		Assert.Equal(0, Transparency.ToAlphaValue(Transparency.ByLayer));
		Assert.Equal(0x01000000, Transparency.ToAlphaValue(Transparency.ByBlock));
	}

	[Theory]
	[InlineData(true, -1)]
	[InlineData(true, 100)]
	[InlineData(true, 50)]
	[InlineData(false, -1)]
	[InlineData(false, 100)]
	[InlineData(false, 70)]
	public void DwgKeepsTheTransparencyMethod(bool byBlockColor, short transparency)
	{
		//A ByBlock color with ByLayer transparency is written as color number 0 with no flags, and the
		//reader returned Opaque: every ByBlock-colored entity became an explicit 0% on the next save.
		CadDocument doc = new CadDocument();
		Line line = new Line(XYZ.Zero, new XYZ(10, 0, 0))
		{
			Color = byBlockColor ? Color.ByBlock : new Color(3),
			Transparency = new Transparency(transparency),
		};
		doc.Entities.Add(line);

		string path = Path.Combine(Path.GetTempPath(), $"acadsharp-{Guid.NewGuid():N}.dwg");
		try
		{
			using (FileStream fs = File.Create(path))
			{
				new DwgWriter(fs, doc).Write();
			}

			Line read = DwgReader.Read(path).Entities.OfType<Line>().Single();
			Assert.Equal(transparency, read.Transparency.Value);
		}
		finally
		{
			File.Delete(path);
		}
	}
}
