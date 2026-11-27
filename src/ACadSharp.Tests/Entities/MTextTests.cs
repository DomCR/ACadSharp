using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities;

public class MTextTests : CommonEntityTests<MText>
{
	public override void GetBoundingBoxTest()
	{
		XYZ pt = this._random.NextXYZ();
		MText text = new MText("hello") { InsertPoint = pt };

		var box = text.GetBoundingBox();
		Assert.Equal(BoundingBoxExtent.Point, box.Extent);
		Assert.Equal(pt, box.Center);
	}

	[Theory]
	[InlineData(ACadVersion.AC1015)]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void DwgExtentsRoundTrip(ACadVersion version)
	{
		CadDocument doc = new CadDocument(version);
		MText mtext = new MText("extents") { HorizontalWidth = 123.25, VerticalHeight = 45.5 };
		doc.Entities.Add(mtext);

		MemoryStream ms = new MemoryStream();
		DwgWriter.Write(ms, doc);
		using MemoryStream readStream = new MemoryStream(ms.ToArray());
		MText roundTripped = DwgReader.Read(readStream).Entities.OfType<MText>().Single();

		Assert.Equal(mtext.HorizontalWidth, roundTripped.HorizontalWidth);
		Assert.Equal(mtext.VerticalHeight, roundTripped.VerticalHeight);
	}

	[Fact]
	public void PlainTextTest()
	{
		var s = Text.TextProcessor.Parse("- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}", out List<string> groups);
		Assert.Equal($"- Font: Calibri CDM Consolas{Environment.NewLine}", s);
		//Assert.Equal("- Font: ", groups[0]);
		//Assert.Equal("Calibri", groups[1]);

		MText text = new MText("- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}");
		Assert.Equal($"- Font: Calibri CDM Consolas{Environment.NewLine}", text.PlainText);

		text = new MText("- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor}");
		Assert.Equal("- Color text green, blue, red, ByLayer, ByBlock, TrueColor", text.PlainText);

		text = new MText("- {\\H2x;Double height \\H0.875x;height is: 0.35\\H1.14286x;\\P}");
		Assert.Equal($"- Double height height is: 0.35{Environment.NewLine}", text.PlainText);

		text = new MText("- {\\C4;Text in \\fCalibri|b0|i0|c0|p34;Calibri and cyan\\P}");
		Assert.Equal($"- Text in Calibri and cyan{Environment.NewLine}", text.PlainText);

		text = new MText("\\pxqc;Text in the center\\P\\pq*;Hello this is an mText\\P");
		Assert.Equal($"Text in the center{Environment.NewLine}Hello this is an mText{Environment.NewLine}", text.PlainText);

		text = new MText("{\\fArial|b1|i0|c0|p34;Bold Text \\fArial|b0|i1|c0|p34;Italic Text \\P}");
		Assert.Equal($"Bold Text Italic Text {Environment.NewLine}", text.PlainText);

		text = new MText("Special characters : \\{ \\} * \\\\ ~ % & ( ) ? ¿ ! ¡");
		Assert.Equal(@"Special characters : { } * \ ~ % & ( ) ? ¿ ! ¡", text.PlainText);
	}
}