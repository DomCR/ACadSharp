using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.Tests.Common;
using Xunit;

namespace ACadSharp.Tests.Tables
{
	public class LayerTests : TableEntryCommonTests<Layer>
	{
		[Fact]
		public void DefpointsPlotTest()
		{
			Layer defpoints = Layer.Defpoints;

			Assert.False(defpoints.PlotFlag);
			defpoints.PlotFlag = true;
			Assert.False(defpoints.PlotFlag);
		}

		[Fact]
		public void CloneTest()
		{
			Layer layer = new Layer("my_layer");
			layer.Color = new Color(23, 200, 200);

			Layer clone = (Layer)layer.Clone();

			CadObjectTestUtils.AssertTableEntryClone(layer, clone);

			Assert.Equal(layer.Color, clone.Color);
		}

		protected override Table<Layer> getTable(CadDocument document)
		{
			return document.Layers;
		}
	}
}