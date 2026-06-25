using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.Mechanical;

/// <summary>
/// Represents a <see cref="AcmPartList"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.AcmPartList"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.PartList"/>
/// </remarks>
[DxfName(DxfFileToken.AcmPartList)]
[DxfSubClass(DxfSubclassMarker.PartList)]
public class AcmPartList : MechanicalEntity
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AcmPartList;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.PartList;

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		this.Position = transform.ApplyTransform(this.Position);

		// TODO: Would probably also require to transform proxy entity data
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		// TODO: Would probably require to get proxy entity data

		return new BoundingBox(this.Position);
	}
}