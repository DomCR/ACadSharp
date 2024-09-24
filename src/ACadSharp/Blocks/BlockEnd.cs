using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;

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

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		/// <inheritdoc/>
		public override void Translate(XYZ translation)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override void Rotate(double rotation, XYZ axis)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override void Scale(XYZ scale)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}
	}
}
