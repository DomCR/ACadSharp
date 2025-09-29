using ACadSharp.Tables;
using CSUtilities.Extensions;
using Xunit;

namespace ACadSharp.Tests.Tables.Collections
{
	public class TextStylesTableTests
	{
		[Fact()]
		public void AddIsShapeTextStyleTest()
		{
			CadDocument doc = new CadDocument();

			TextStyle style = new TextStyle("custom_text_00");
			style.Flags |= StyleFlags.IsShape;

			doc.TextStyles.Add(style);
		}
	}
}