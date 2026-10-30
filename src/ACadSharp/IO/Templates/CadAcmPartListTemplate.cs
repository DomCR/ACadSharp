using ACadSharp.Entities.Mechanical;
using ACadSharp.Objects;
using ACadSharp.Objects.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmPartListTemplate : CadMechanicalEntityTemplate<AcmPartList>
{
	public ulong? BomHandle { get; set; }

	public ulong? ItemFilterCustomHandle { get; set; }

	public List<ulong> RowHandles { get; set; } = new List<ulong>();

	public List<ulong> RelatedHandles { get; } = new();

	public ulong? UnknownHandle1 { get; set; }

	public ulong? UnknownHandle2 { get; set; }

	public CadAcmPartListTemplate(AcmPartList partList) : base(partList)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetCadObject(this.BomHandle, out AcmBom bom))
		{
			this.CadObject.Bom = bom;
			if (!bom.PartLists.Contains(this.CadObject))
			{
				bom.PartLists.Add(this.CadObject);
			}
		}

		if (builder.TryGetCadObject(this.ItemFilterCustomHandle, out CadObject itemFilter))
		{
			this.CadObject.ItemFilterCustom = itemFilter;
		}

		foreach (ulong handle in this.RowHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmBomRow row) &&
				!this.CadObject.Rows.Contains(row))
			{
				this.CadObject.Rows.Add(row);
			}
		}

		foreach (ulong handle in this.RelatedHandles)
		{
			if (builder.TryGetCadObject(handle, out AcmBomRow row))
			{
				if (!this.CadObject.Rows.Contains(row))
				{
					this.CadObject.Rows.Add(row);
				}
			}
			else if (builder.TryGetCadObject(handle, out CadObject related) &&
				!this.CadObject.RelatedObjects.Contains(related))
			{
				this.CadObject.RelatedObjects.Add(related);
			}
		}
	}
}
