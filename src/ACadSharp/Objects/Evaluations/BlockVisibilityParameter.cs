using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKVISIBILITYPARAMETER object, in AutoCAD used to
	/// control the visibility state of entities in a dynamic block.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockVisibilityParameter"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockVisibilityParameter"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockVisibilityParameter)]
	[DxfSubClass(DxfSubclassMarker.BlockVisibilityParameter)]
	public partial class BlockVisibilityParameter : Block1PtParameter
	{
		/// <summary>
		/// Visibility parameter description.
		/// </summary>
		[DxfCodeValue(302)]
		public string Description { get; set; }

		/// <summary>
		/// Gets the list of all <see cref="Entity"/> objects of the dynamic block
		/// this <see cref="BlockVisibilityParameter"/> is associated with.
		/// </summary>
		[DxfCollectionCodeValue(331)]
		[DxfCodeValue(DxfReferenceType.Count, 93)]
		public List<Entity> Entities { get; private set; } = new List<Entity>();

		/// <summary>
		/// Visibility parameter name.
		/// </summary>
		[DxfCodeValue(301)]
		public string Name { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockVisibilityParameter;

		/// <summary>
		/// Gets the list of states each containing a 2 subsets of <see cref="Entity"/> <br/>
		/// Objects must belong to the dynamic <see cref="BlockVisibilityParameter"/> associated with.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 92)]
		public IReadOnlyDictionary<string, State> States { get => _states; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityParameter;

		[DxfCodeValue(91)]
		public bool Value91 { get; set; }

		private Dictionary<string, State> _states = new(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// Adds a state to the collection using the state's name as the key.
		/// </summary>
		/// <param name="state">The state to add to the collection. Cannot be null. The state's name must be unique within the collection.</param>
		public void AddState(State state)
		{
			this._states.Add(state.Name, state);
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

			clone._states = new(StringComparer.InvariantCultureIgnoreCase);
			foreach (State item in this._states.Values)
			{
				var state = (State)item.Clone();
				clone._states.Add(state.Name, state);
			}

			return clone;
		}
	}
}