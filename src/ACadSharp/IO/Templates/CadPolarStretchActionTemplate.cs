using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadPolarStretchActionTemplate : CadStretchActionBaseTemplate
{
	public List<ulong> SelectionHandles { get; set; } = new();

	public CadPolarStretchActionTemplate() : base(new BlockPolarStretchAction()) { }

	public CadPolarStretchActionTemplate(BlockPolarStretchAction stretchAction)
		: base(stretchAction)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		var stretchAction = this.CadObject as BlockPolarStretchAction;

		foreach (var handle in this.SelectionHandles)
		{
			if (builder.TryGetCadObject<Entity>(handle, out var entity))
			{
				stretchAction.RotateBindings.Add(entity);
			}
			else
			{
				builder.Notify($"[{stretchAction.ToString()}] entity with handle {handle} not found.");
			}
		}
	}
}