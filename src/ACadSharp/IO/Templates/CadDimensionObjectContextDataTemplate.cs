using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates;

internal class CadDimensionObjectContextDataTemplate : CadAnnotScaleObjectContextDataTemplate
{
	public ulong? BlockHandle { get; set; }

	public CadDimensionObjectContextDataTemplate(DimensionObjectContextData cadObject) : base(cadObject) { }

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetCadObject(this.BlockHandle, out BlockRecord block))
		{
			((DimensionObjectContextData)this.CadObject).Block = block;
		}
	}
}
