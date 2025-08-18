using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using Xunit;
using ACadSharp.Types.Units;

namespace ACadSharp.Tests.Tables
{
	public class DimensionStyleTests : TableEntryCommonTests<DimensionStyle>
	{
		public static readonly TheoryData<DimensionStyle, DimensionAngular2Line, string> AngularStyleFormat;

		public static readonly TheoryData<DimensionStyle, DimensionAligned, string> LinearStyleFormat;

		static DimensionStyleTests()
		{
			AngularStyleFormat = new()
			{
				{
					new DimensionStyle {
						AngularUnit = AngularUnitFormat.DegreesMinutesSeconds
					},
					new DimensionAngular2Line{
						FirstPoint = XYZ.Zero,
						SecondPoint = new XYZ(10, 0, 0),
						AngleVertex = XYZ.Zero,
						DefinitionPoint = new XYZ(0,10, 0),
					},
					"90°"
				},
				{
					new DimensionStyle {
						AngularUnit = AngularUnitFormat.Radians
					},
					new DimensionAngular2Line{
						FirstPoint = XYZ.Zero,
						SecondPoint = new XYZ(10, 0, 0),
						AngleVertex = XYZ.Zero,
						DefinitionPoint = new XYZ(0,10, 0),
					},
					"1.57r"
				},
				{
					new DimensionStyle {
						AngularUnit = AngularUnitFormat.DecimalDegrees
					},
					new DimensionAngular2Line{
						FirstPoint = XYZ.Zero,
						SecondPoint = new XYZ(10, 0, 0),
						AngleVertex = XYZ.Zero,
						DefinitionPoint = new XYZ(0,10, 0),
					},
					"1.57"
				},
				{
					new DimensionStyle {
						AngularUnit = AngularUnitFormat.DecimalDegrees,
						DecimalSeparator = ','
					},
					new DimensionAngular2Line{
						FirstPoint = XYZ.Zero,
						SecondPoint = new XYZ(10, 0, 0),
						AngleVertex = XYZ.Zero,
						DefinitionPoint = new XYZ(0,10, 0),
					},
					"1,57"
				},
				{
					new DimensionStyle {
						AngularUnit = AngularUnitFormat.Gradians
					},
					new DimensionAngular2Line{
						FirstPoint = XYZ.Zero,
						SecondPoint = new XYZ(10, 0, 0),
						AngleVertex = XYZ.Zero,
						DefinitionPoint = new XYZ(0,10, 0),
					},
					"100.00g"
				},
			};

			LinearStyleFormat = new()
			{
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Decimal
					},
					new DimensionAligned(XYZ.Zero, new XYZ(10, 0, 0)),
					"10"
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Decimal
					},
					new DimensionAligned(XYZ.Zero, new XYZ(10.5, 0, 0)),
					"10.5"
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Decimal,
						DecimalSeparator = ',',
					},
					new DimensionAligned(XYZ.Zero, new XYZ(10.5, 0, 0)),
					"10,5"
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Fractional,
						FractionFormat = FractionFormat.None
					},
					new DimensionAligned(XYZ.Zero, new XYZ(0.5, 0, 0)),
					"1/2"
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Fractional,
						FractionFormat = FractionFormat.None
					},
					new DimensionAligned(XYZ.Zero, new XYZ(1.25, 0, 0)),
					"1 1/4"
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Architectural
					},
					new DimensionAligned(XYZ.Zero, new XYZ(10.25, 0, 0)),
					"\\A1;0'-10{\\H1x;\\S1/4;}\""
				},
				{
					new DimensionStyle {
						LinearUnitFormat = LinearUnitFormat.Scientific
					},
					new DimensionAligned(XYZ.Zero, new XYZ(10.25, 0, 0)),
					"1.03E+01"
				},
			};
		}

		[Fact]
		public void ApplyRoundingTest()
		{
			double value = 42.3645788954;
			DimensionStyle style = this.createEntry();

			style.Rounding = 0.0;
			Assert.Equal(value, style.ApplyRounding(value));
			style.Rounding = 0.0001;
			Assert.Equal(42.3646, style.ApplyRounding(value));
			style.Rounding = 0.0005;
			Assert.Equal(42.3645, style.ApplyRounding(value));
			style.Rounding = 0.25;
			Assert.Equal(42.25, style.ApplyRounding(value));
			style.Rounding = 1;
			Assert.Equal(42.0d, style.ApplyRounding(value));
			style.Rounding = 10;
			Assert.Equal(40.0d, style.ApplyRounding(value));

			style.AlternateUnitRounding = 0.0;
			Assert.Equal(value, style.ApplyRounding(value, true));
		}

		[Theory]
		[MemberData(nameof(LinearStyleFormat))]
		public void GetLinearMeasurementText(DimensionStyle style, DimensionAligned dim, string result)
		{
			Assert.Equal(result, dim.GetMeasurementText(style));
		}

		[Theory]
		[MemberData(nameof(AngularStyleFormat))]
		public void GetAngularMeasurementText(DimensionStyle style, DimensionAngular2Line dim, string result)
		{
			Assert.Equal(result, dim.GetMeasurementText(style));
		}

		[Fact]
		public void RemoveTableEntries()
		{
			string ltypeName = "my_lineType";
			string ext1Name = "ext1_ltype";
			string ext2Name = "ext2_ltype";
			string leaderName = "leader_block";
			string dimArrow1Name = "dimarrow1_block";
			string dimArrow2Name = "dimarrow2_block";
			string arrowBlockName = "arrow_block";

			var lineType = new LineType(ltypeName);
			var ext1 = new LineType(ext1Name);
			var ext2 = new LineType(ext2Name);

			var leader = new BlockRecord(leaderName);
			var dimArrow1 = new BlockRecord(dimArrow1Name);
			var dimArrow2 = new BlockRecord(dimArrow2Name);
			var arrowBlock = new BlockRecord(arrowBlockName);

			DimensionStyle style = new DimensionStyle("my_style");
			style.LineType = lineType;
			style.LineTypeExt1 = ext1;
			style.LineTypeExt2 = ext2;

			style.LeaderArrow = leader;
			style.DimArrow1 = dimArrow1;
			style.DimArrow2 = dimArrow2;
			style.ArrowBlock = arrowBlock;

			CadDocument doc = new CadDocument();
			doc.DimensionStyles.Add(style);

			Assert.Contains(style, doc.DimensionStyles);

			Assert.Contains(lineType, doc.LineTypes);
			Assert.Contains(ext1, doc.LineTypes);
			Assert.Contains(ext2, doc.LineTypes);

			Assert.Contains(leader, doc.BlockRecords);
			Assert.Contains(dimArrow1, doc.BlockRecords);
			Assert.Contains(dimArrow2, doc.BlockRecords);
			Assert.Contains(arrowBlock, doc.BlockRecords);

			//Check line types
			doc.LineTypes.Remove(ltypeName);
			Assert.Null(style.LineType);
			doc.LineTypes.Remove(ext1Name);
			Assert.Null(style.LineTypeExt1);
			doc.LineTypes.Remove(ext2Name);
			Assert.Null(style.LineTypeExt2);

			//Check blocks
			doc.BlockRecords.Remove(leaderName);
			Assert.Null(style.LeaderArrow);
			doc.BlockRecords.Remove(dimArrow1Name);
			Assert.Null(style.DimArrow1);
			doc.BlockRecords.Remove(dimArrow2Name);
			Assert.Null(style.DimArrow2);
			doc.BlockRecords.Remove(arrowBlockName);
			Assert.Null(style.ArrowBlock);
		}

		protected override Table<DimensionStyle> getTable(CadDocument document)
		{
			return document.DimensionStyles;
		}
	}
}