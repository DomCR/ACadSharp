using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadStretchActionBaseTemplate : CadBlockActionTemplate
{
	public Dictionary<ulong, StretchEntityBind> Bindings { get; set; } = new();

	public CadStretchActionBaseTemplate(StretchActionBase stretchAction)
	: base(stretchAction)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		var stretchAction = this.CadObject as StretchActionBase;

		foreach (var item in this.Bindings)
		{
			if (builder.TryGetCadObject<Entity>(item.Key, out var entity))
			{
				stretchAction.StretchBindings.Add(new StretchEntityBind(entity, item.Value.PointIndexes));
			}
			else
			{
				builder.Notify($"[{stretchAction.ToString()}] entity with handle {item.Key} not found.");
			}
		}
	}
}
