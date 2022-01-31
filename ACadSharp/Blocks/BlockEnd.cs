using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Blocks
{
	/// <summary>
	/// Represents a <see cref="BlockEnd"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EndBlock"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockEnd"/>
	/// </remarks>
	[DxfName(DxfFileToken.EndBlock)]
	[DxfSubClass(DxfSubclassMarker.BlockEnd)]
	public class BlockEnd : Entity
	{
		public override string ObjectName => DxfFileToken.EndBlock;

		public override ObjectType ObjectType => ObjectType.ENDBLK;
	}
}
