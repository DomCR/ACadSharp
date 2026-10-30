using ACadSharp.Entities.Mechanical;
using ACadSharp.Objects.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmBomRowTemplate : CadTemplate<AcmBomRow>
{
	public ulong? DataEntryHandle { get; set; }

	public List<ulong> PartReferenceHandles { get; } = new();

	public List<ulong> BalloonHandles { get; } = new();

	public List<ulong> UnknownHandles { get; } = new();

	public CadAcmBomRowTemplate(AcmBomRow row) : base(row)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetCadObject(this.DataEntryHandle, out AcmDataEntryPart dataEntry))
		{
			this.CadObject.DataEntry = dataEntry;
			if (!dataEntry.BomRows.Contains(this.CadObject))
			{
				dataEntry.BomRows.Add(this.CadObject);
			}
		}

		foreach (ulong handle in this.PartReferenceHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmPartRef partReference) &&
				!this.CadObject.PartReferences.Contains(partReference))
			{
				this.CadObject.PartReferences.Add(partReference);
			}
		}

		foreach (ulong handle in this.BalloonHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmBalloon balloon) &&
				!this.CadObject.Balloons.Contains(balloon))
			{
				this.CadObject.Balloons.Add(balloon);
			}
		}
	}
}
