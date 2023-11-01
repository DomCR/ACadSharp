using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;

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
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EndBlock;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ENDBLK;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockEnd;

		public BlockEnd(BlockRecord record) : base()
		{
			this.Owner = record;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			BlockEnd clone = (BlockEnd)base.Clone();
			clone.Owner = new BlockRecord((this.Owner as BlockRecord).Name);
			return clone;
		}
	}
}
