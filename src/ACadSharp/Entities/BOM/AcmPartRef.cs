using CSMath;

namespace ACadSharp.Entities.BOM;

public class AcmPartRef : Entity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

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
