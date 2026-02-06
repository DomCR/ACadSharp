using CSMath;

namespace ACadSharp.Entities.AecObjects
{
	public class Wall : Entity
	{
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityAecWall;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AecWall;

		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}

		public override BoundingBox GetBoundingBox()
		{
			throw new System.NotImplementedException();
		}
	}
}
