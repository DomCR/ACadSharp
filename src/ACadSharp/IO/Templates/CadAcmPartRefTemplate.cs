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

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		if (builder.TryGetCadObject(this.DataEntryPartHandle, out Objects.Mechanical.AcmDataEntryPart dataEntry))
		{
			this.CadObject.DataEntry = dataEntry;
			if (!dataEntry.PartReferences.Contains(this.CadObject))
			{
				dataEntry.PartReferences.Add(this.CadObject);
			}
		}
	}
}
