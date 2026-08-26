using ACadSharp.Entities;
using ACadSharp.Tables;
using Xunit;

namespace ACadSharp.Tests.Tables.Collections;

public class LayersTableTests
{
	[Fact()]
	public void LayerReferenceTest()
	{
		string name = "existing_layer";
		Layer layer = new Layer(name);

		CadDocument doc = new CadDocument();
		doc.Layers.Add(layer);

		Line line = new Line();
		doc.Entities.Add(line);

		Assert.Equal(doc, line.Layer.Document);
		Assert.Empty(doc.Layers.GetReferences(name));
		Assert.NotEmpty(doc.Layers.GetReferences(Layer.DefaultName));
		Assert.NotEmpty(doc.Layers.GetReferences(line.Layer.Name));

		line.Layer = layer;
		Assert.Equal(layer, line.Layer);

		doc.Layers.Remove(name);

		Assert.Equal(doc.Layers[Layer.DefaultName], line.Layer);
	}
}