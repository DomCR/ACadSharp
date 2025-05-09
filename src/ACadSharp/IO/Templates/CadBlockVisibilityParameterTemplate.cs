using System.Collections.Generic;
using System.Linq;
using ACadSharp.Entities;
using ACadSharp.Objects.Evaluations;

using static ACadSharp.IO.Templates.CadBlockVisibilityParameterTemplate;

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

	internal class CadBlockFlipActionTemplate : CadBlockActionTemplate
	{
		public BlockFlipAction BlockFlipAction { get { return this.CadObject as BlockFlipAction; } }

		public CadBlockFlipActionTemplate(BlockFlipAction cadObject)
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

	internal class CadBlock2PtParameterTemplate : CadBlockParameterTemplate
	{
		public Block2PtParameter Block2PtParameter { get { return this.CadObject as Block2PtParameter; } }

		public CadBlock2PtParameterTemplate(Block2PtParameter cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlockFlipParameterTemplate : CadBlock2PtParameterTemplate
	{
		public Block2PtParameter BlockFlipParameter { get { return this.CadObject as BlockFlipParameter; } }

		public CadBlockFlipParameterTemplate(BlockFlipParameter cadObject)
			: base(cadObject)
		{
		}
	}

	internal class CadBlockVisibilityParameterTemplate : CadBlock1PtParameterTemplate
	{
		public class StateTemplate
		{
			public BlockVisibilityParameter.State State { get; } = new BlockVisibilityParameter.State();

			public List<ulong> SubSet1 { get; } = new();

			public List<ulong> SubSet2 { get; } = new();

			public void Build(CadDocumentBuilder builder, IEnumerable<ulong> entityHandles)
			{
				this.setEntities(builder, State.Entities, SubSet1, entityHandles);
				this.setEntities(builder, State.Expressions, SubSet2, null);
			}

			private void setEntities<T>(CadDocumentBuilder builder, List<T> subset, IEnumerable<ulong> handles, IEnumerable<ulong> entities = null)
				where T : CadObject
			{
				foreach (var h in handles)
				{
					if (entities != null && !entities.Contains(h))
					{
						builder.Notify($"[{State.ToString()}] parent does not contain handle {h}.");
					}

					if (builder.TryGetCadObject(h, out T obj))
					{
						subset.Add(obj);
					}
					else
					{
						builder.Notify($"[{State.ToString()}] {typeof(T).FullName} with handle {h} not found.");
					}
				}
			}
		}

		public List<ulong> EntityHandles { get; } = new List<ulong>();

		public List<StateTemplate> StateTemplates { get; } = new();

		public CadBlockVisibilityParameterTemplate(BlockVisibilityParameter cadObject)
			: base(cadObject)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

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

			foreach (var item in StateTemplates)
			{
				item.Build(builder, this.EntityHandles);
				bvp.States.Add(item.State);
			}
		}
	}
}