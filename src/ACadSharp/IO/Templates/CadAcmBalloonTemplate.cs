using ACadSharp.Entities.Mechanical;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates;

internal class CadAcmBalloonTemplate : CadMechanicalEntityTemplate<AcmBalloon>
{
	public ulong? BlockHandle { get; set; }

	public ulong? BomRowHandle { get; set; }

	public CadAcmBalloonTemplate(AcmBalloon balloon) : base(balloon)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (this.getTableReference(builder, this.BlockHandle, null, out BlockRecord record))
		{
			this.CadObject.Block = record;
		}
	}
}