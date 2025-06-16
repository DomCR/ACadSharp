using ACadSharp.Extensions;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class LineTypeTests
	{
		[Fact]
		public void CloneTest()
		{
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

			lt.AddSegment(s1);
			lt.AddSegment(s2);

			LineType clone = lt.CloneTyped();

			CadObjectTestUtils.AssertTableEntryClone(lt, clone);

			for (int i = 0; i < lt.Segments.Count(); i++)
			{
				Assert.Equal(lt.Segments.ElementAt(i).Length, clone.Segments.ElementAt(i).Length);
			}
		}
	}
}