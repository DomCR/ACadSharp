using CSMath;

namespace ACadSharp.Entities.Mechanical;

public class AcmPartRef : Entity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override string ObjectName => DxfFileToken.AcmPartRef;

	public XYZ Position { get; internal set; }

	public double Radius { get; internal set; }

	public ulong StandardDINHandle { get; set; }

	public ulong BOMStandardDINHandle { get; set; }

	public ulong LineResHandle { get; set; }

	public ulong DataEntryPartHandle { get; set; }

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}