using ACadSharp.Entities.Mechanical;
using ACadSharp.Objects.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmBomTemplate : CadTemplate<AcmBom>
{
	public List<ulong> RowHandles { get; } = new();

	public ulong? PartListHandle { get; set; }

	public ulong? DataEntryHandle { get; set; }

	public ulong? UnknownHandle { get; set; }

	public CadAcmBomTemplate(AcmBom bom) : base(bom)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (ulong handle in this.RowHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmBomRow row) &&
				!this.CadObject.Rows.Contains(row))
			{
				this.CadObject.Rows.Add(row);
			}
		}

		if (builder.TryGetCadObject(this.PartListHandle, out AcmPartList partList) &&
			!this.CadObject.PartLists.Contains(partList))
		{
			this.CadObject.PartLists.Add(partList);
		}

		if (builder.TryGetCadObject(this.DataEntryHandle, out AcmDataEntryBlock dataEntry))
		{
			this.CadObject.DataEntry = dataEntry;
		}
	}
}
