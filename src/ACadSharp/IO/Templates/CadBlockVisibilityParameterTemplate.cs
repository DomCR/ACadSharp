using System.Collections.Generic;

using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;

namespace ACadSharp.IO.Templates
{

	internal class CadEvaluationExpressionTemplate : CadTemplate<EvaluationExpression>
	{
		public CadEvaluationExpressionTemplate(EvaluationExpression cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlockElementTemplate : CadEvaluationExpressionTemplate
	{
		public BlockElement BlockElement { get { return this.CadObject as BlockElement; } }

		public CadBlockElementTemplate(BlockElement cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlockParameterTemplate : CadBlockElementTemplate
	{
		public BlockParameter BlockParameter { get { return this.CadObject as BlockParameter; } }

		public CadBlockParameterTemplate(BlockParameter cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlock1PtParameterTemplate : CadBlockParameterTemplate
	{
		public Block1PtParameter Block1PtParameter { get { return this.CadObject as Block1PtParameter; } }

		public CadBlock1PtParameterTemplate(Block1PtParameter cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlockVisibilityParameterTemplate : CadBlock1PtParameterTemplate
	{
		public IDictionary<ulong, Entity> TotalEntityHandles { get; } = new Dictionary<ulong, Entity>();

		public IDictionary<BlockVisibilityParameter.SubBlock, IList<ulong>> SubBlockHandles { get; } = new Dictionary<BlockVisibilityParameter.SubBlock, IList<ulong>>();

		public CadBlockVisibilityParameterTemplate(BlockVisibilityParameter cadObject)
			: base(cadObject)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			BlockVisibilityParameter bvp = this.CadObject as BlockVisibilityParameter;

			foreach (var cadObjectHandle in this.TotalEntityHandles)
			{
				ulong handle = cadObjectHandle.Key;
				if (builder.TryGetCadObject(handle, out Entity entity))
				{
					this.TotalEntityHandles[handle] = entity;
					bvp.Entities.Add(entity);
				}
			}

			foreach (var subGroup in bvp.SubBlocks)
			{
				if (this.SubBlockHandles.TryGetValue(subGroup, out IList<ulong> subBlockHandles))
				{
					foreach (ulong handle in subBlockHandles)
					{
						if (this.TotalEntityHandles.TryGetValue(handle, out Entity entity))
						{
							subGroup.Entities.Add(entity);
						}
						else if (builder.TryGetCadObject(handle, out Entity entityX))
						{
						}
					}
				}
			}
		}
	}
}