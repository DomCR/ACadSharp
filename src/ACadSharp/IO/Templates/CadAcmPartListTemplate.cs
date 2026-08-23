using ACadSharp.Entities.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmPartListTemplate : CadMechanicalEntityTemplate<AcmPartList>
{
	public ulong? BomHandle { get; set; }

	public ulong? ItemFilterCustomHandle { get; set; }

	public List<ulong> RowHandles { get; set; } = new List<ulong>();

	public ulong? UnknownHandle1 { get; set; }

	public ulong? UnknownHandle2 { get; set; }

	public CadAcmPartListTemplate(AcmPartList partList) : base(partList)
	{
	}
}
