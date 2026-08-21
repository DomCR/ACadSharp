using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadBlockActionTemplate : CadBlockElementTemplate
{
	public BlockAction BlockAction { get { return this.CadObject as BlockAction; } }

	public HashSet<ulong> EntityHandles { get; } = new();

	public CadBlockActionTemplate(BlockAction blockAction)
		: base(blockAction)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		//The file does not restrict these to entities. Checked against AutoCAD's own DXF of a
		//production drawing: all 184 handles this used to report as "entity not found" were
		//objects - stretch actions, linear parameters, flip grips - and the writers then wrote 385
		//fewer references than the file held. Keep every object, in file order, and the entities
		//among them in the typed list as well.
		int stale = 0;
		ulong firstStale = 0;
		foreach (var handle in this.EntityHandles)
		{
			if (builder.TryGetCadObject(handle, out CadObject obj))
			{
				this.BlockAction.Elements.Add(obj);
				if (obj is Entity entity)
				{
					this.BlockAction.Entities.Add(entity);
				}
			}
			else
			{
				if (stale == 0)
				{
					firstStale = handle;
				}

				stale++;
			}
		}

		if (stale > 0)
		{
			builder.Notify(
				$"{this.BlockAction}: {stale} of {this.EntityHandles.Count} references point at objects that are not in the drawing (first: {firstStale}); dropped",
				NotificationType.Warning);
		}
	}
}