using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class MTextTests : CommonEntityTests<MText>
	{
		/// <summary>
		/// The column height count (group code 72) is followed by one height per count. Writing a
		/// different number of heights than the count shifts every following bit of the file, and
		/// AutoCAD refuses to open the result, so the two have to stay in sync through a round trip.
		/// </summary>
		[Fact]
		public void DwgRoundTripKeepsDynamicColumns()
		{
			//The column block is only written from R2018 on, see writeMText.
			CadDocument doc = new CadDocument(ACadVersion.AC1032);

			MText mtext = new MText
			{
				Value = @"first column\Psecond column",
				InsertPoint = new XYZ(1, 2, 0),
				Height = 1,
			};
			mtext.ColumnData.ColumnType = ColumnType.DynamicColumns;
			mtext.ColumnData.AutoHeight = false;
			mtext.ColumnData.Width = 15.5;
			mtext.ColumnData.Gutter = 0.5;
			mtext.ColumnData.Heights.Add(-2.5);
			mtext.ColumnData.Heights.Add(3.25);
			doc.Entities.Add(mtext);

			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			ACadSharp.IO.DwgWriter.Write(ms, doc);
			using System.IO.MemoryStream readStream = new System.IO.MemoryStream(ms.ToArray());
			CadDocument rt = ACadSharp.IO.DwgReader.Read(readStream);

			MText result = null;
			foreach (Entity e in rt.Entities)
			{
				if (e is MText m)
				{
					result = m;
					break;
				}
			}

			Assert.NotNull(result);
			Assert.Equal(ColumnType.DynamicColumns, result.ColumnData.ColumnType);
			Assert.Equal(mtext.ColumnData.Heights.Count, result.ColumnData.Heights.Count);
			Assert.Equal(mtext.ColumnData.Heights.Count, result.ColumnData.ColumnCount);
			for (int i = 0; i < mtext.ColumnData.Heights.Count; i++)
			{
				Assert.Equal(mtext.ColumnData.Heights[i], result.ColumnData.Heights[i], 9);
			}

			Assert.Equal(mtext.ColumnData.Width, result.ColumnData.Width, 9);
			Assert.Equal(mtext.ColumnData.Gutter, result.ColumnData.Gutter, 9);
		}

		public override void GetBoundingBoxTest()
		{
			XYZ pt = this._random.NextXYZ();
			MText text = new MText("hello") { InsertPoint = pt };

			var box = text.GetBoundingBox();
			Assert.Equal(BoundingBoxExtent.Point, box.Extent);
			Assert.Equal(pt, box.Center);
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
}