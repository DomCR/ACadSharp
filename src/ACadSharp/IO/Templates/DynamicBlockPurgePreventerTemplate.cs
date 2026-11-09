using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates;

internal class DynamicBlockPurgePreventerTemplate : CadTemplate<DynamicBlockPurgePreventer>
{
	public ulong? BlockHandle { get; set; }

	public DynamicBlockPurgePreventerTemplate(DynamicBlockPurgePreventer obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (this.getTableReference(builder, this.BlockHandle, null, out BlockRecord blockRecord))
		{
			this.CadObject.Block = blockRecord;
		}
	}
}