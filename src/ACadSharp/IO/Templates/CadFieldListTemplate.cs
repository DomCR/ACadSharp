using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadFieldListTemplate : CadTemplate<FieldList>, ICadOwnerTemplate
{
	public HashSet<ulong> OwnedObjectsHandlers { get; } = new();

	public CadFieldListTemplate(FieldList obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (var handle in this.OwnedObjectsHandlers)
		{
			if (builder.TryGetCadObject(handle, out Field field))
			{
				this.CadObject.Fields.Add(field);
			}
			else
			{
				builder.Notify($"Field {handle} not found for FieldList {this.CadObject.Handle}", NotificationType.Warning);
			}
		}
	}
}