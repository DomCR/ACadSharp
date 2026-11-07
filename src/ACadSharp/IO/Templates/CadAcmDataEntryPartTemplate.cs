using ACadSharp.Entities.Mechanical;
using ACadSharp.Objects.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmDataEntryPartTemplate : CadTemplate<AcmDataEntryPart>
{
	public List<ulong> PartReferenceHandles { get; } = new();

	public List<ulong> BomRowHandles { get; } = new();

	public List<ulong> UnknownHandles { get; } = new();

	public CadAcmDataEntryPartTemplate(AcmDataEntryPart dataEntry) : base(dataEntry)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (ulong handle in this.PartReferenceHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmPartRef partReference) &&
				!this.CadObject.PartReferences.Contains(partReference))
			{
				this.CadObject.PartReferences.Add(partReference);
			}
		}

		foreach (ulong handle in this.BomRowHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmBomRow row) &&
				!this.CadObject.BomRows.Contains(row))
			{
				this.CadObject.BomRows.Add(row);
			}
		}
	}
}
