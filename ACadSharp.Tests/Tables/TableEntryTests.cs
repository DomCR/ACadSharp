using Xunit;

namespace ACadSharp.Tables.Tests
{
	public class TableEntryTests
	{
		[Fact()]
		public void SetFlagUsingMapper()
		{
			Layer layer = new Layer("My layer");

			var map = DxfClassMap.Create<Layer>();
			map.DxfProperties[70].SetValue(layer, LayerFlags.Frozen);

			Assert.True(layer.Flags.HasFlag(LayerFlags.Frozen));
		}
	}
}