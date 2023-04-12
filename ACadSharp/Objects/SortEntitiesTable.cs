using ACadSharp.Attributes;
using ACadSharp.Blocks;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="SortEntitiesTable"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectSortEntsTable"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.SortentsTable"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectSortEntsTable)]
	[DxfSubClass(DxfSubclassMarker.SortentsTable)]
	public class SortEntitiesTable : CadObject
	{
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string ObjectName => DxfFileToken.ObjectSortEntsTable;

		//330	Soft-pointer ID/handle to owner(currently only the* MODEL_SPACE or* PAPER_SPACE blocks)
		public Block BlockOwner { get; internal set; }

		//331	Soft-pointer ID/handle to an entity(zero or more entries may exist)

		//5	Sort handle(zero or more entries may exist)
	}
}
