using System.Collections.Generic;

using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{

	internal class BlockVisibilityParameterTemplate : CadTemplate<BlockVisibilityParameter>
	{

		public BlockVisibilityParameterTemplate(BlockVisibilityParameter cadObject)
			: base(cadObject)
		{
		}

		public List<ulong> EntityHandles { get; } = new ();

		public IDictionary<BlockVisibilityParameter.SubBlock, IList<ulong>> SubBlockHandles { get; } = new Dictionary<BlockVisibilityParameter.SubBlock, IList<ulong>>();

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (var handle in this.EntityHandles)
			{
				if (builder.TryGetCadObject(handle, out Entity entity))
				{
					this.CadObject.Entities.Add(entity);
				}
			}

			foreach (var subGroup in this.CadObject.SubBlocks)
			{
				if (this.SubBlockHandles.TryGetValue(subGroup, out IList<ulong> subBlockHandles))
				{
					foreach (ulong handle in subBlockHandles)
					{
						if (builder.TryGetCadObject(handle, out Entity entity))
						{
							subGroup.Entities.Add(entity);
						}
					}
				}
			}
		}
	}
}