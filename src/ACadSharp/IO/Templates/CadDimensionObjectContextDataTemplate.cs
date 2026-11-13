using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates;

internal class CadDimensionObjectContextDataTemplate : CadAnnotScaleObjectContextDataTemplate
{
	public ulong? BlockHandle { get; set; }

	public string BlockName { get; set; }

	public CadDimensionObjectContextDataTemplate(DimensionObjectContextData cadObject) : base(cadObject) { }

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (this.getTableReference(builder, this.BlockHandle, this.BlockName, out BlockRecord block))
		{
			((DimensionObjectContextData)this.CadObject).Block = block;
		}
	}
}
