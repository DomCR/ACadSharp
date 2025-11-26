using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects
{
	[System.Flags]
	public enum AssociativityFlags : short
	{
		None = 0,

		FirstPointReference = 1,

		SecondPointReference = 2,

		ThirdPointReference = 4,

		FourthPointReference = 8
	}

	public enum ObjectOsnapType : short
	{
		None = 0,

		Endpoint = 1,

		Midpoint = 2,

		Center = 3,

		Node = 4,

		Quadrant = 5,

		Intersection = 6,

		Insertion = 7,

		Perpendicular = 8,

		Tangent = 9,

		Nearest = 10,

		ApparentIntersection = 11,

		Parallel = 12,

		StartPoint = 13,
	}

	public enum RotatedDimensionType : short
	{
		Unknown = 0,

		Parallel = 1,

		Perpendicular = 2
	}

	public enum SubentType : short
	{
		Unknown = 0,

		Edge = 1,

		Face = 2
	}

	/// <summary>
	/// Represents a <see cref="DimensionAssociation"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDimensionAssociation"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DimensionAssociation"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDimensionAssociation)]
	[DxfSubClass(DxfSubclassMarker.DimensionAssociation)]
	public class DimensionAssociation : NonGraphicalObject
	{
		/// <summary>
		/// Gets or sets the associativity flags that define the reference points for an entity.
		/// </summary>
		[DxfCodeValue(90)]
		public AssociativityFlags AssociativityFlags { get; set; }

		/// <summary>
		/// Gets the name of the class represented by this instance.
		/// </summary>
		[DxfCodeValue(1)]
		public string ClassName { get; set; } = "AcDbOsnapPointRef";

		/// <summary>
		/// Gets or sets the associated dimension object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public Dimension Dimension { get; set; }

		/// <summary>
		/// Gets or sets the geometry parameter used for the Near object snap (Osnap).
		/// </summary>
		[DxfCodeValue(40)]
		public double GeometryParameter { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is in trans-space.
		/// </summary>
		[DxfCodeValue(70)]
		public bool IsTransSpace { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDimensionAssociation;

		/// <summary>
		/// Gets or sets the object snap type associated with the entity.
		/// </summary>
		[DxfCodeValue(72)]
		public ObjectOsnapType ObjectOsnapType { get; set; }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		//301
		//Handle(string) of Xref object
		/// <summary>
		/// Gets or sets the object snap (Osnap) point in world coordinate system (WCS).
		/// </summary>
		/// <remarks>The Osnap point is used to specify precise locations on geometry for object snapping
		/// operations.</remarks>
		[DxfCodeValue(10, 20, 30)]
		public XYZ OsnapPoint { get; set; }

		/// <summary>
		/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
		/// </summary>
		[DxfCodeValue(71)]
		public RotatedDimensionType RotatedDimensionType { get; set; } = RotatedDimensionType.Unknown;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DimensionAssociation;

		/// <summary>
		/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
		/// </summary>
		[DxfCodeValue(73)]
		public SubentType SubentType { get; set; } = SubentType.Unknown;

		//331
		//ID of main object (geometry)

		//73
		//SubentType of main object (edge, face)

		//91
		//GsMarker of main object (index)
		//332
		//ID of intersection object (geometry)

		//74
		//SubentType of intersction object (edge/face)

		//92
		//GsMarker of intersection object (index)

		//302
		//Handle(string) of intersection Xref object

		//75
		//hasLastPointRef flag(true/false)

		/// <inheritdoc/>
		public DimensionAssociation() : base()
		{
		}
	}
}