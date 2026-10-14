using ACadSharp;
using ACadSharp.IO.DWG.Tangent;
using System;
using Xunit;

namespace ACadSharp.Tests.IO.Tangent
{
	/// <summary>
	/// Tests against payloads captured from a drawing that contains native Tianzheng objects
	/// (<c>TZ.dwg</c>, contributed in issue #1131). Each fixture is the class specific data of one
	/// object, taken byte for byte from <c>UnknownEntity.RawData</c>.
	/// </summary>
	public class TchObjectTests
	{
		//TCH_TEXT, handle 828, expected position verified against Tianzheng's own flattened export
		private const string TextPayload =
"ChGQVQWQOQOBBEwC9uR0z13ECPgOLDkLIeNASZ7AEYAOABGADAARgAxADIARgAtADkAMwA4ADkALQA0ADkARgBDAC0AOAAzA" +
"DUARAAtADgAQwA4ADYAQwA2AEUARQBFAEUARABCAH0ASzhAOgA4QDmAOEA5wDhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAO" +
"UA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOkA4QDlAOAA+ADgAOkA4QDlAOE" +
"A5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEG4QDmAOEA5QDhAOQA" +
"RhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBBVQAMgAwAGQAMQBBVQAMgAwAGQAMgB" +
"BVQAMgAwAGQAMwBBVQAMgAwAGQANABFlQARABCAFgARABBAFQAQQBNAEEAUABfAEIATABPAEMASwBfAEUATgBEAF8ASZ7AEI" +
"AMAA3ADEAMwBFADYAQgAtAEEARgBGADYALQA0ADgANwAxAC0AQQAyADcAMwAtADcAOAA5ADYAOAAzADQARAA2ADYANwA2AH0" +
"AQbhAOUA4QDlAOEA5gBFFcAYQBsAGwAIAAmACAAVwBpAG4AZABvAHcAIAAmACAARABvAG8AcgBKOEA6ADhAOQA4QDmAOAA6Q" +
"DhAOoA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOYA4QD" +
"lAOEA5QDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QD" +
"lAEG4QDpAOEA6ADhAOYAQbhAOMA4QDlAOEA4wCKFqkBRSiIoCB/f";

		//TCH_WALL, handle 833
		private const string WallPayload =
"ChGQOQOQUAAAAAAAAWUBBQAAAAAAABZQEIQhCEDkIQMAAEAAEVQMAAEAAAwRCSC4AP/XtxAjkZsiqwNPhQAkguAD/17cQISL" +
"nXOR7X50BJnsARgA4AEYAMABGADEAMgBGAC0AOQAzADgAOQAtADQAOQBGAEMALQA4ADMANQBEAC0AOABDADgANgBDADYARQB" +
"FAEUARQBEAEIAfQBLOEA5gDhAOYA4QDoAOEA5QDhAOUA4ADpAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDl" +
"AOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBKOEA6gDhAOUA4QDnAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhA" +
"OUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUAQbgAPcA4QDkAOEA4wBBuEA6ADhAOoA4QDqAEG4QDjAOEA" +
"5ADgAPgARhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBCAEUARwBJAE4AXwBBVQAMgAwAGIAMgBBVQAMgA" +
"wAGIAMwBBlQAMgAwAGQAMQAyAEFVAAyADAAZAA3AEFVAAyADAAcwAxAECQQCnfkGVAAyADAAcwAxADQAQEWeEFVAAyADAAcw" +
"A0AEDbmYakBZ4QVUADIAMABzADgAQVUADIAMABzADkAQPdTyluQlxBlQAMgAwAHUAMQAxAEGVAAyADAAdQAxADYAQVUADIAM" +
"AB1ADYARZUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAXwBFAE4ARABfAEmewBCADAANwAxADMARQA2AEIALQB" +
"BAEYARgA2AC0ANAA4ADcAMQAtAEEAMgA3ADMALQA3ADgAOQA2ADgAMwA0AEQANgA2ADcANgB9AEG4QDqAOEA5QDhAOQASjhA" +
"OUA4QDlAOEA6ADgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhA" +
"OUA4QDlAEG4AD4AOEA5QDhAOQASzhAOYA4QDmAOAA+ADhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOU" +
"A4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASzhAOkA4QDmAOEA5ADhAOUA4QDlAOAA6QDhAOUA4QDlAOEA" +
"5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUAS7hAOgA4QDkAOEA6gDhAOUA4" +
"QDlAOEA5QDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4" +
"QDlAEG4QDqAOEA5QDhAOcAQbhAOgA4QDjAOEA6QBCOEA6ADhAOkA4AD4AOEA5QBBuEA4wDhAOUA4QDpAEq4QDmAOAA9wDhAO" +
"YA4QDlAOAA6QDhAOUA4AD3AOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5gDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5gDgAP" +
"gASjhAOYA4QDlAOAA9wDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOE" +
"A5QDhAOUA4QDlAEo4QDmAOEA5QDgAPgA4ADpAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA" +
"4QDlAOEA5QDhAOUA4QDlAOEA5QBKOEA5QDhAOUA4QDoAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5" +
"QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUASjhAOoA4QDlAOEA6QDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4Q" +
"DlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEG4QDmAOEA5QDhAOgAQbhAOgA4QDlAOEA5ACSjgAPcA4Q" +
"DlAOEA5gDgAOkA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4Q" +
"DlAEG4QDlAOEA5QDhAOcAyjGpATOpAVkhAZ2gIQHWIQHWICAhAa6hAcuoKCkBRykBNqAgICB0A==";

		//TCH_COLUMN, handle 827
		private const string ColumnPayload =
"ChGQOQhCEDAABAABBQAAAAAAABZQEICBEJILgA/9e3ECORmyKpAruFAAUE5GbIqsCI4UAkguAD/xPcQLkZsiqwNPhQCSC4AP" +
"/E9xAuRmyKrA0+FAJILgA/+p3EC5GbIqsCI4UAkguAD/6ncQJJnsAMgBFADAAMABDADIARgA4AC0ARgA2ADUANAAtADQAOQA" +
"yADgALQBBAEYAMwBEAC0AMABFAEUANgA0ADUANQA1ADgANAAyADMAfQBLOAA+ADhAOYA4QDlAOEA5QDhAOUA4ADpAOEA5QDh" +
"AOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBKOEA6QDhAOUA4AD3A" +
"OAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUAQbhAO" +
"gA4QDkAOEA5ABBuEA5wDhAOoA4QDkAEG4QDjAOEA5ADhAOgARhUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwBCAEwATwBDAEsAX" +
"wBCAEUARwBJAE4AXwBBVQAMgAwAGIAMABBVQAMgAwAHMAMQBBaKUS3v3bd1RH1dBVQAMgAwAHMAMgBCVAAVQBCAF8ASABBAF" +
"QAQwBIAEFVAAyADAAcwAzAEFVAAyADAAcwA0AEFVAAyADAAcwA1AEDopRLezx4RZUAEQAQgBYAEQAQQBUAEEATQBBAFAAXwB" +
"CAEwATwBDAEsAXwBFAE4ARABfAEmewBCADAANwAxADMARQA2AEIALQBBAEYARgA2AC0ANAA4ADcAMQAtAEEAMgA3ADMALQA3" +
"ADgAOQA2ADgAMwA0AEQANgA2ADcANgB9AEG4QDoAOEA5QDhAOQAS7hAOYA4QDkAOAA9wDhAOUA4QDlAOEA5QDgAOkA4QDlAO" +
"EA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAEG4QDoAOEA5gDhAOQ" +
"AQbgAPcA4QDjAOEA4wBCOEA4wDhAOcA4AD4AOEA5QBBuEA5wDhAOUA4QDnAEG4AD4AOEA5QDhAOMASrhAOcA4AD3AOEA5QDh" +
"AOUA4ADpAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QBK" +
"OEA5QDhAOUA4QDlAOAA6QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlAOEA5QDhAOUA4QDlA" +
"OEA5QDhAOUApiHKkBKqEBXiEB1ikBRykBNvm";

		private static byte[] decode(string value) => Convert.FromBase64String(value);

		[Fact]
		public void TchTextPositionMatchesTheFlattenedExport()
		{
			TchText text = TchText.Parse(decode(TextPayload), ACadVersion.AC1021);

			Assert.True(text.HasPosition);
			Assert.Equal(39176.3507, text.InsertionPoint.X, 4);
			Assert.Equal(29653.1919, text.InsertionPoint.Y, 4);
		}

		[Fact]
		public void TchTextRejectsAShortPayload()
		{
			TchText text = TchText.Parse(new byte[4], ACadVersion.AC1021);

			Assert.False(text.HasPosition);
			Assert.Equal(CSMath.XYZ.Zero, text.InsertionPoint);
		}

		[Fact]
		public void TchTextThrowsOnANullPayload()
		{
			Assert.Throws<ArgumentNullException>(() => TchText.Parse(null, ACadVersion.AC1021));
		}

		[Fact]
		public void TchPolygonRecoversTheColumnSection()
		{
			TchPolygon polygon = TchPolygon.FindInPayload(decode(ColumnPayload), ACadVersion.AC1021, out long offset);

			Assert.NotNull(polygon);
			Assert.True(polygon.IsComplete);
			Assert.Equal(332, offset);
			Assert.Equal(4, polygon.Vertices.Count);

			//The four corners of the column section, in order.
			Assert.Equal(35910.0208, polygon.Vertices[0].Position.X, 4);
			Assert.Equal(28751.9846, polygon.Vertices[0].Position.Y, 4);
			Assert.Equal(36510.0208, polygon.Vertices[1].Position.X, 4);
			Assert.Equal(28751.9846, polygon.Vertices[1].Position.Y, 4);
			Assert.Equal(36510.0208, polygon.Vertices[2].Position.X, 4);
			Assert.Equal(29351.9846, polygon.Vertices[2].Position.Y, 4);
			Assert.Equal(35910.0208, polygon.Vertices[3].Position.X, 4);
			Assert.Equal(29351.9846, polygon.Vertices[3].Position.Y, 4);

			//A straight edged outline, so every bulge is zero.
			foreach (TchVertex vertex in polygon.Vertices)
			{
				Assert.Equal(0.0, vertex.Bulge);
			}
		}

		[Fact]
		public void TchPolygonRecoversTheSectionSize()
		{
			TchPolygon polygon = TchPolygon.FindInPayload(decode(ColumnPayload), ACadVersion.AC1021, out _);

			double width = polygon.Vertices[1].Position.X - polygon.Vertices[0].Position.X;
			double height = polygon.Vertices[2].Position.Y - polygon.Vertices[1].Position.Y;

			//The column is square, and its edge lands on the same grid as the walls in the flattened
			//export of the same drawing.
			Assert.Equal(600.0, width, 4);
			Assert.Equal(600.0, height, 4);
		}

		[Fact]
		public void TchPolygonReturnsNullWhenThereIsNoOutline()
		{
			//A text object carries no outline.
			TchPolygon polygon = TchPolygon.FindInPayload(decode(TextPayload), ACadVersion.AC1021, out long offset);

			Assert.Null(polygon);
			Assert.Equal(-1, offset);
		}

		[Fact]
		public void TchPolygonThrowsOnANullPayload()
		{
			Assert.Throws<ArgumentNullException>(() => TchPolygon.FindInPayload(null, ACadVersion.AC1021, out _));
		}

		[Fact]
		public void TchDataMapFindsTheBlockInsideANativePayload()
		{
			TchDataMap map = TchDataMap.ReadFromPayload(decode(WallPayload), ACadVersion.AC1021);

			Assert.True(map.IsComplete);
			Assert.Contains(TchDataMap.BlockEndMarker, map.Entries);
			Assert.Contains("T20s1", map.Entries);
		}

		[Fact]
		public void TchDataMapPairsDescriptorsWithTheirValues()
		{
			TchDataMap map = TchDataMap.ReadFromPayload(decode(WallPayload), ACadVersion.AC1021);

			Assert.False(map.IsEmpty);
			Assert.Equal("A级", map.Properties["T20s1"]);
			Assert.Equal("普通砖", map.Properties["T20s4"]);
			Assert.Equal("保温层", map.Properties["T20s9"]);
		}

		[Fact]
		public void TchDataMapTreatsANameLookingValueAsAValue()
		{
			//PUB_HATCH is the value of T20s2, it only looks like a property name
			TchDataMap map = TchDataMap.ReadFromPayload(decode(ColumnPayload), ACadVersion.AC1021);

			Assert.Equal("PUB_HATCH", map.Properties["T20s2"]);
			Assert.Equal("钢筋混凝土", map.Properties["T20s1"]);
			Assert.Equal("钢筋砼", map.Properties["T20s5"]);
		}

		[Fact]
		public void TchDataMapReturnsAnEmptyMapWhenThereIsNoBlock()
		{
			TchDataMap map = TchDataMap.ReadFromPayload(new byte[64], ACadVersion.AC1021);

			Assert.False(map.IsComplete);
			Assert.True(map.IsEmpty);
		}
	}
}
