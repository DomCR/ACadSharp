using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class LayerTests
	{
		[Fact]
		public void CloneTest()
		{
			Layer layer = new Layer("my_layer");
			layer.Color = new Color(23, 200, 200);

			Layer clone = (Layer)layer.Clone();

			CadObjectTestUtils.AssertTableEntryClone(layer, clone);

			Assert.Equal(layer.Color, clone.Color);
		}
	}
}