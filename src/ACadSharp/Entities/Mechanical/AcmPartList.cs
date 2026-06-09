using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.Mechanical;

public class AcmPartList : Entity
{
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	public override string ObjectName => DxfFileToken.AcmPartList;

	public XYZ Position { get; set; }

	public ulong StandardDINHandle { get; set; }

	public ulong BOMStandardDINHandle { get; set; }

	public ulong BomHandle { get; set; }

	public ulong ItemFilterCustomHandle { get; set; }

	public List<ulong> BomRowHandles { get; set; }

	public override void ApplyTransform(Transform transform)
	{
		throw new System.NotImplementedException();
	}

	public override BoundingBox GetBoundingBox()
	{
		throw new System.NotImplementedException();
	}
}
