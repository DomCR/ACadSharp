using ACadSharp.Attributes;
using ACadSharp.Objects.Mechanical;
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
public class AcmPartRef : MechanicalEntity
{
	/// <summary>
	/// Gets the Mechanical part data referenced by this entity.
	/// </summary>
	public AcmDataEntryPart DataEntry { get; internal set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.AcmPartRef;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.PartRef;

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
