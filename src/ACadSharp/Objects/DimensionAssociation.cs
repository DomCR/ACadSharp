using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Entities;
using System.Linq;

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
public partial class DimensionAssociation : NonGraphicalObject, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets the associativity flags that define the reference points for an entity.
	/// </summary>
	[DxfCodeValue(90)]
	public AssociativityFlags AssociativityFlags { get; set; }

	/// <summary>
	/// Gets or sets the associated dimension object (null when the owner is not a dimension, see
	/// <see cref="AssociatedEntity"/>).
	/// </summary>
	public Dimension Dimension
	{
		get { return this.AssociatedEntity as Dimension; }
		set { this.AssociatedEntity = value; }
	}

	/// <summary>
	/// The entity this association belongs to: a dimension, but AutoCAD also attaches a DIMASSOC to the
	/// arrowhead of a leader (LEADER, MULTILEADER). Typed as <see cref="Dimension"/>, those were read as null
	/// and dropped on write.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 330)]
	public Entity AssociatedEntity { get; set; }

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

	/// <summary>
	/// True when the owner entity and every object of every point reference belong to <paramref name="document"/>.
	/// An association copied from another document still points to that document's objects, whose handles mean
	/// nothing in this one: the writers skip it, as they drop reactors from other documents (<see cref="CadObject.CleanReactors"/>).
	/// </summary>
	internal bool IsBoundTo(CadDocument document)
	{
		if (this.AssociatedEntity == null || this.AssociatedEntity.Document != document)
		{
			return false;
		}

		foreach (OsnapPointRef slot in new[] { this.FirstPointRef, this.SecondPointRef, this.ThirdPointRef, this.FourthPointRef })
		{
			int depth = 0;
			for (OsnapPointRef r = slot; r != null && depth <= 8; r = r.LastPointRef, depth++)
			{
				if (r.GeometryPath.Any(o => o?.Document != document) || r.IntersectionPath.Any(o => o?.Document != document))
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <inheritdoc/>
	/// <remarks>The point references are copied (they used to be shared with the original).</remarks>
	public override CadObject Clone()
	{
		DimensionAssociation clone = (DimensionAssociation)base.Clone();
		clone.FirstPointRef = this.FirstPointRef?.Clone();
		clone.SecondPointRef = this.SecondPointRef?.Clone();
		clone.ThirdPointRef = this.ThirdPointRef?.Clone();
		clone.FourthPointRef = this.FourthPointRef?.Clone();
		return clone;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			ApplicationName = "AcDbDimAssoc",
			CppClassName = DxfSubclassMarker.DimensionAssociation,
			DwgVersion = 0,
			DxfName = DxfFileToken.ObjectDimensionAssociation,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}