using CSMath;

namespace ACadSharp.Entities.Mechanical;

public class AcmPartList : Entity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override string ObjectName => DxfFileToken.AcmPartList;

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}
