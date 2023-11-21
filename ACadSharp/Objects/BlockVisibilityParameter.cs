using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Entities;

using CSMath;


namespace ACadSharp.Objects {

	/// <summary>
	/// Represents a BLOCKVISIBILITYPARAMETER object, in AutoCAD used to
	/// control the visibility state of entities in a dynamic block.
	/// </summary>
	public class BlockVisibilityParameter : CadObject {

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockVisibilityParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityParameter;


		/// <summary>
		/// Gets the list of all <see cref="Entity"/> objects of the dynamic block
		/// this <see cref="BlockVisibilityParameter"/> is associated with.
		/// </summary>
		public IList<Entity> Entities { get; } = new List<Entity>();

		/// <summary>
		/// Gets the list of subblocks each containing a subset of the <see cref="Entity"/>
		/// objects of the dynamic block this <see cref="BlockVisibilityParameter"/>
		/// is associated with.
		/// </summary>
		public IList<SubBlock> SubBlocks { get; } = new List<SubBlock>();

		/// <summary>
		/// Gets a position presumably used to display a triangle-button in AutoCAD open
		/// a dialog to select the subblock that is to be set visible.
		/// </summary>
		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ BasePosition { get; internal set; }
		
		/// <summary>
		/// Gets a text presumably describing the purpose of this <see cref="BlockVisibilityParameter"/>.
		/// </summary>
		[DxfCodeValue(300)]
		public string ParameterType { get; internal set; }

		/// <summary>
		/// Gets a title for the dialog to select the subblock that is to be set visible.
		/// </summary>
		[DxfCodeValue(301)]
		public string Name { get; internal set; }

		/// <summary>
		/// Gets a description presumably for the dialog to select the subblock that is to be set visible.
		/// </summary>
		[DxfCodeValue(302)]
		public string Description { get; internal set; }

		/// <summary>
		/// Unknown
		/// </summary>
		[DxfCodeValue(91)]
		public int L91 { get; internal set; }

		/// <summary>
		/// Represents a named subblock containing <see cref="Entity"/> objects.
		/// The visibility of the entities of a subblock can be determined
		/// interactively in AutoCAD.
		/// </summary>
		public class SubBlock {

			/// <summary>
			/// Gets the name of the subblock.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Get the list of <see cref="Entity"/> objects in this subblock.
			/// </summary>
			public IList<Entity> Entities { get; } = new List<Entity>();
		}
	}
}