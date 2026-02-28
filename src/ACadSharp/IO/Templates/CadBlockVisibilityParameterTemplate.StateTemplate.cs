using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.Templates
{
	internal partial class CadBlockVisibilityParameterTemplate
	{
		public class StateTemplate
		{
			public BlockVisibilityParameter.State State { get; } = new BlockVisibilityParameter.State();

			public HashSet<ulong> EntityHandles { get; } = new();

			public HashSet<ulong> ExpressionHandles { get; } = new();

			public StateTemplate()
			{
			}

			public StateTemplate(BlockVisibilityParameter.State state)
			{
				this.State = state;
			}

			public void Build(CadDocumentBuilder builder, IEnumerable<ulong> entityHandles)
			{
				this.setEntities(builder, State.Entities, EntityHandles, entityHandles);
				this.setEntities(builder, State.Expressions, ExpressionHandles, null);
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
	}
}