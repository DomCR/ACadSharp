using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="DimensionAssociation"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectDimensionAssociation"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.DimensionAssociation"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectDimensionAssociation)]
[DxfSubClass(DxfSubclassMarker.DimensionAssociation)]
public partial class DimensionAssociation : NonGraphicalObject
{
	/// <summary>
	/// Gets or sets the associativity flags that define the reference points for an entity.
	/// </summary>
	[DxfCodeValue(90)]
	public AssociativityFlags AssociativityFlags { get; set; }

	/// <summary>
	/// Gets or sets the associated dimension object.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 330)]
	public Dimension Dimension { get; set; }

	public OsnapPointRef FirstPointRef { get; set; }

	public OsnapPointRef FourthPointRef { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the entity is in trans-space.
	/// </summary>
	[DxfCodeValue(70)]
	public bool IsTransSpace { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectDimensionAssociation;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <summary>
	/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
	/// </summary>
	[DxfCodeValue(71)]
	public RotatedDimensionType RotatedDimensionType { get; set; } = RotatedDimensionType.Unknown;

	public OsnapPointRef SecondPointRef { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DimensionAssociation;

	public OsnapPointRef ThirdPointRef { get; set; }

	public const string OsnapPointRefClassName = "AcDbOsnapPointRef";

	/// <inheritdoc/>
	public DimensionAssociation() : base()
	{
	}
}