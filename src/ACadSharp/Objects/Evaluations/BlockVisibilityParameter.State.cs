using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects.Evaluations
{
	public partial class BlockVisibilityParameter
	{
		/// <summary>
		/// Represents a named state containing <see cref="Entity"/> objects. <br/>
		/// The state controls the visibility of the entities assigned to it.
		/// </summary>
		public class State : ICloneable
		{
			/// <summary>
			/// Gets the name of the state.
			/// </summary>
			[DxfCodeValue(303)]
			public string Name { get; set; }

			/// <summary>
			/// Get the list of <see cref="Entity"/> objects in this state.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 94)]
			[DxfCollectionCodeValue(DxfReferenceType.Handle, 332)]
			public List<Entity> Entities { get; private set; } = new();

			/// <summary>
			/// Get the list of <see cref="EvaluationExpression"/> objects.
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 95)]
			[DxfCollectionCodeValue(DxfReferenceType.Handle, 333)]
			public List<EvaluationExpression> Expressions { get; private set; } = new();

			/// <inheritdoc/>
			public object Clone()
			{
				State clone = (State)MemberwiseClone();

				clone.Entities = new List<Entity>();
				foreach (var item in this.Entities)
				{
					clone.Entities.Add((Entity)item.Clone());
				}

				clone.Expressions = new();
				foreach (var item in this.Expressions)
				{
					clone.Expressions.Add((EvaluationExpression)item.Clone());
				}

				return clone;
			}
		}
	}
}