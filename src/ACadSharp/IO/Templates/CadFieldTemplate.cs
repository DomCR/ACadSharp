using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadFieldTemplate : CadTemplate<Field>
{
	public List<ulong> CadObjectsHandles { get; set; } = new();

	public List<ICadTemplate> CadValueTemplates { get; set; } = new();

	public List<ulong> ChildrenHandles { get; set; } = new();

	public CadFieldTemplate(Field obj) : base(obj)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (var handle in CadObjectsHandles)
		{
			if (builder.TryGetCadObject(handle, out CadObject cobject))
			{
				this.CadObject.CadObjects.Add(cobject);
			}
			else
			{
				builder.Notify($"[{this.CadObject.SubclassMarker}] CadObject with handle {handle} not found.", NotificationType.Warning);
			}
		}

		foreach (var handle in ChildrenHandles)
		{
			if (builder.TryGetCadObject(handle, out Field f))
			{
				this.CadObject.Children.Add(f);
			}
			else
			{
				builder.Notify($"[{this.CadObject.SubclassMarker}] CadObject with handle {handle} not found.", NotificationType.Warning);
			}
		}

		//Values of type Handle (e.g. the "ObjectPropertyId" of an AcObjProp field) are read as a handle
		//and only resolved here.
		foreach (var valueTemplate in this.CadValueTemplates)
		{
			valueTemplate.Build(builder);
		}
	}
}