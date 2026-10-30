using ACadSharp.Objects;
using ACadSharp.Objects.Mechanical;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadAcmDataEntryBlockTemplate : CadTemplate<AcmDataEntryBlock>
{
	public List<ulong> ReferenceHandles { get; } = new();

	public CadAcmDataEntryBlockTemplate(AcmDataEntryBlock dataEntry) : base(dataEntry)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (ulong handle in this.ReferenceHandles)
		{
			if (builder.TryGetCadObject(handle, out CadObject reference) &&
				!this.CadObject.References.Contains(reference))
			{
				this.CadObject.References.Add(reference);
			}
		}
	}
}
