using System.Collections.Generic;

using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates {
	internal class CadBlockActionTemplate : CadBlockElementTemplate {

		public BlockAction BlockAction { get { return this.CadObject as BlockAction; } }

		public List<ulong> EntityHandles { get; } = new List<ulong>();

		public CadBlockActionTemplate(BlockAction cadObject)
			: base(cadObject)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var handle in this.EntityHandles)
			{
				if (builder.TryGetCadObject(handle, out Entity entity)) {
					BlockAction.Entities.Add(entity);
				}
				else {
					builder.Notify($"[{BlockAction.ToString()}] entity with handle {handle} not found.");
				}
			}
		}
	}
}