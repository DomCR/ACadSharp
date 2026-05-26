using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class LineSegmentTests
	{
		[Fact]
		public void LineSegmentFlagsTest()
		{
			LineType.Segment seg1 = new LineType.Segment
			{
				Length = 10
			};

			Assert.False(seg1.IsShape);
			Assert.False(seg1.IsText);
			Assert.True(seg1.Flags == LineTypeShapeFlags.None);

			seg1.Text = "GAS";
			Assert.False(seg1.IsText);
			seg1.IsText = true;
			Assert.True(seg1.IsText);
			Assert.True(seg1.Flags.HasFlag(LineTypeShapeFlags.Text));

			seg1.IsShape = true;
			Assert.True(seg1.IsShape);
			Assert.True(seg1.Flags.HasFlag(LineTypeShapeFlags.Shape));
		}

		[Fact]
		public void LineSegmentTypeTest()
		{
			LineType.Segment seg1 = new LineType.Segment
			{
				Length = 10
			};
			Assert.True(seg1.IsLine);
			Assert.False(seg1.IsPoint);
			Assert.False(seg1.IsSpace);
			LineType.Segment seg2 = new LineType.Segment
			{
				Length = 0
			};
			Assert.False(seg2.IsLine);
			Assert.True(seg2.IsPoint);
			Assert.False(seg2.IsSpace);
			LineType.Segment seg3 = new LineType.Segment
			{
				Length = -5
			};
			Assert.False(seg3.IsLine);
			Assert.False(seg3.IsPoint);
			Assert.True(seg3.IsSpace);
		}
	}
}