using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class DimensionStyleTests
	{
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
	}
}