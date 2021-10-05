using ACadSharp.Entities;

namespace ACadSharp.Blocks
{
	/// <summary>
	/// Class to read the dwg BLOCK type.
	/// </summary>
	internal class BlockEnd : Entity
	{
		public override string ObjectName => DxfFileToken.Block;
		public override ObjectType ObjectType => ObjectType.ENDBLK;
		public string Name { get; set; }

	}
}
