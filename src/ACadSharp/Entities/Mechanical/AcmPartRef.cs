using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities.Mechanical;

/// <summary>
/// Represents a <see cref="AcmPartRef"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.AcmPartRef"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.PartRef"/>
/// </remarks>
[DxfName(DxfFileToken.AcmPartRef)]
[DxfSubClass(DxfSubclassMarker.PartRef)]
public class AcmPartRef : Entity
{
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	public override string ObjectName => DxfFileToken.AcmPartRef;

	public override string SubclassMarker => DxfSubclassMarker.PartRef;

	public XYZ Position { get; set; }

	public ulong StandardDINHandle { get; set; }

	public ulong BOMStandardDINHandle { get; set; }

	public ulong LineResHandle { get; set; }

	public ulong DataEntryPartHandle { get; set; }

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