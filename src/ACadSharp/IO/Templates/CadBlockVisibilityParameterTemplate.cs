using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal partial class CadBlockVisibilityParameterTemplate : CadBlock1PtParameterTemplate
	{
		public List<ulong> EntityHandles { get; } = new List<ulong>();

		public List<StateTemplate> StateTemplates { get; } = new();

		public CadBlockVisibilityParameterTemplate() : base(new BlockVisibilityParameter())
		{
		}

		public CadBlockVisibilityParameterTemplate(BlockVisibilityParameter cadObject)
			: base(cadObject)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			BlockVisibilityParameter bvp = this.CadObject as BlockVisibilityParameter;

			foreach (var handle in this.EntityHandles)
			{
				if (builder.TryGetCadObject(handle, out Entity entity))
				{
					bvp.Entities.Add(entity);
				}
				else
				{
					builder.Notify($"[{bvp.ToString()}] entity with handle {handle} not found.");
				}
			}

			foreach (var item in this.StateTemplates)
			{
				item.Build(builder, this.EntityHandles);
				bvp.AddState(item.State);
			}
		}
	}
}