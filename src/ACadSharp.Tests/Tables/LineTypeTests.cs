using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Tests.Common;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class LineTypeTests : TableEntryCommonTests<LineType>
	{
		[Fact]
		public void CloneTest()
		{
			var textStyle = new TextStyle("my_style");

			LineType lt = new LineType("segmented");
			lt.Description = "line type description";

			LineType.Segment s1 = new LineType.Segment
			{
				Length = 12,
				//Style = this.Document.TextStyles[TextStyle.DefaultName]
			};

			LineType.Segment s2 = new LineType.Segment
			{
				Length = -3,
				//Style = this.Document.TextStyles[TextStyle.DefaultName]
			};

			LineType.Segment s3 = new LineType.Segment
			{
				Length = 1,
				Style = textStyle
			};

			lt.AddSegment(s1);
			lt.AddSegment(s2);
			lt.AddSegment(s3);

			LineType clone = lt.CloneTyped();

			CadObjectTestUtils.AssertTableEntryClone(lt, clone);

			for (int i = 0; i < lt.Segments.Count(); i++)
			{
				Assert.Equal(lt.Segments.ElementAt(i).Length, clone.Segments.ElementAt(i).Length);
			}

			var last = clone.Segments.Last();

			Assert.NotNull(last.Style);
			Assert.NotEqual(textStyle, last.Style);
			Assert.Equal(textStyle.Name, last.Style.Name);
		}

		protected override Table<LineType> getTable(CadDocument document)
		{
			return document.LineTypes;
		}
	}
}