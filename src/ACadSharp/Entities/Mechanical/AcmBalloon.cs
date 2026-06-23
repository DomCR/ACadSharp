using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities.Mechanical;

/// <summary>
/// Represents a <see cref="AcmBalloon"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.AcmBalloon"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Balloon"/>
/// </remarks>
[DxfName(DxfFileToken.AcmBalloon)]
[DxfSubClass(DxfSubclassMarker.Balloon)]
public class AcmBalloon : Entity
{
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	public override string ObjectName => DxfFileToken.AcmBalloon;

	public override string SubclassMarker => DxfSubclassMarker.Balloon;

	public XYZ Position { get; set; }

	public ulong StandardDINHandle { get; set; }

	public ulong BOMStandardDINHandle { get; set; }

	public ulong BomRowHandle { get; set; }

	public BlockRecord Block { get; set; }

	public override void ApplyTransform(Transform transform)
	{
		this.Position = transform.ApplyTransform(this.Position);

		// TODO: Would probably also require to transform proxy entity data
	}

	public override BoundingBox GetBoundingBox()
	{
		// TODO: Would probably require to get proxy entity data

		return new BoundingBox(this.Position);
	}
}
