using CSMath;

namespace ACadSharp.Entities.Mechanical;

public class AcmBalloon : ProxyDataEntity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}
