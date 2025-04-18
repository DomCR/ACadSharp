using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Seqend"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntitySeqend"/> <br/>
	/// </remarks>
	[DxfName(DxfFileToken.EntitySeqend)]
	public class Seqend : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SEQEND;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntitySeqend;

		/// <inheritdoc/>
		public Seqend() : base() { }

		internal Seqend(CadObject owner)
		{
			this.Owner = owner;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
		}
	}
}
