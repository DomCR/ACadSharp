using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadBlockStretchActionTemplate : CadBlockActionTemplate
{
	public Dictionary<ulong, BlockStretchAction.StretchBind> Bindings { get; set; } = new();

	public CadBlockStretchActionTemplate(BlockStretchAction stretchAction)
	: base(stretchAction)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		BlockStretchAction stretchAction = this.CadObject as BlockStretchAction;

		foreach (var item in this.Bindings)
		{
			if(builder.TryGetCadObject<Entity>(item.Key, out var entity))
			{
				item.Value.Entity = entity;
				stretchAction.StretchBindings.Add(item.Value);
			}
			else
			{
				builder.Notify($"[{stretchAction.ToString()}] entity with handle {item.Key} not found.");
			}
		}
	}
}
