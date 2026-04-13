using CSMath;

namespace ACadSharp.Entities.Mechanical;

public class AcmPartRef : ProxyDataEntity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override string ObjectName => DxfFileToken.AcmPartRef;

	public XYZ Position { get; internal set; }

	public double Radius { get; internal set; }

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}