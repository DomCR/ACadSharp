using ACadSharp.Entities.Mechanical;

namespace ACadSharp.IO.Templates;

internal class CadAcmPartRefTemplate : CadMechanicalEntityTemplate<AcmPartRef>
{
	public ulong? DataEntryPartHandle { get; set; }

	public ulong? LineResHandle { get; set; }

	public ulong? UnknownHandle1 { get; set; }

	public CadAcmPartRefTemplate(AcmPartRef partRef) : base(partRef)
	{
	}
}
