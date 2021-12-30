using ACadSharp.Entities;

namespace ACadSharp.Blocks
{
	/// <summary>
	/// Class to read the dwg BLOCK type
	/// </summary>
	public class BlockEnd : Entity
	{
		public override string ObjectName => DxfFileToken.EndBlock;

		public override ObjectType ObjectType => ObjectType.ENDBLK;
	}
}
