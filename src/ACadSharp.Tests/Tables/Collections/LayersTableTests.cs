using ACadSharp.Tables;
using ACadSharp.Tables.Collections;

namespace ACadSharp.Tests.Tables.Collections;

public class LayersTableTests : TableEntryCommonTests<Layer>
{
	protected override Layer createInstance(string name)
	{
		return new Layer(name);
	}

	protected override Table<Layer> getTable(CadDocument document)
	{
		return document.Layers;
	}
}