using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class TextStyleTests : TableEntryCommonTests<TextStyle>
	{
		[Fact]
		public void DefaultEntryTest()
		{
			var def = TextStyle.Default;

			Assert.True(def.Flags.HasFlag(StyleFlags.XrefDependent));
		}

		protected override Table<TextStyle> getTable(CadDocument document)
		{
			return document.TextStyles;
		}
	}
}