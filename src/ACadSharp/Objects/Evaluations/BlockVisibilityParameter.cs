using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKVISIBILITYPARAMETER object, in AutoCAD used to
	/// control the visibility state of entities in a dynamic block.
	/// </summary>
	public class BlockVisibilityParameter : Block1PtParameter
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockVisibilityParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityParameter;

		/// <summary>
		/// Gets the list of all <see cref="Entity"/> objects of the dynamic block
		/// this <see cref="BlockVisibilityParameter"/> is associated with.
		/// </summary>
		[DxfCodeValue(331)]
		public List<Entity> Entities { get; private set; } = new List<Entity>();

		/// <summary>
		/// Gets the list of states each containing a 2 subsets of <see cref="Entity"/> <br/>
		/// Objects must belong to the dynamic <see cref="BlockVisibilityParameter"/> associated with.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 95)]
		public List<State> States { get; private set; } = new List<State>();

		/// <summary>
		/// Gets a title for the dialog to select the subblock that is to be set visible.
		/// </summary>
		[DxfCodeValue(301)]
		public string Name { get; set; }

		/// <summary>
		/// Gets a description presumably for the dialog to select the subblock that is to be set visible.
		/// </summary>
		[DxfCodeValue(302)]
		public string Description { get; set; }

		[DxfCodeValue(91)]
		internal bool Value91 { get; set; }

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

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			BlockVisibilityParameter clone = (BlockVisibilityParameter)base.Clone();

			clone.Entities = new List<Entity>();
			foreach (var item in this.Entities)
			{
				clone.Entities.Add((Entity)item.Clone());
			}

			clone.States = new List<State>();
			foreach (var item in this.States)
			{
				clone.States.Add((State)item.Clone());
			}

			return clone;
		}
	}
}