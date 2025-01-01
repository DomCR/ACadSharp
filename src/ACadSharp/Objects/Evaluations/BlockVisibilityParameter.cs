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
		/// Visibility parameter name.
		/// </summary>
		[DxfCodeValue(301)]
		public string Name { get; set; }

		/// <summary>
		/// Visibility parameter description.
		/// </summary>
		[DxfCodeValue(302)]
		public string Description { get; set; }

		[DxfCodeValue(91)]
		internal bool Value91 { get; set; }

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