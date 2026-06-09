using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities.Mechanical;

public class AcmBalloon : Entity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override string ObjectName => DxfFileToken.AcmBalloon;

	public XYZ Position { get; set; }

	public ulong StandardDINHandle { get; set; }

	public ulong BOMStandardDINHandle { get; set; }

	public ulong BomRowHandle { get; set; }

	public BlockRecord Block { get; set; }

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}
