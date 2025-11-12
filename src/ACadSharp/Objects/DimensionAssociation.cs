using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	public enum RotatedDimensionType : short
	{
		Unknown = 0,
		Parallel = 1,
		Perpendicular = 2
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
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDimensionAssociation;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DimensionAssociation;


		//330
		//ID of dimension object
		/// <summary>
		/// Gets or sets the associated dimension object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public Dimension Dimension { get; set; }

		//90
		//Associativity flag
		//1 = First point reference
		//2 = Second point reference
		//4 = Third point reference
		//8 = Fourth point reference

		//70
		//Trans-space flag(true/false)

		/// <summary>
		/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
		/// </summary>
		[DxfCodeValue(71)]
		public RotatedDimensionType RotatedDimensionType { get; set; } = RotatedDimensionType.Unknown;

		/// <summary>
		/// Gets the name of the class represented by this instance.
		/// </summary>
		[DxfCodeValue(1)]
		public string ClassName { get; set; } = "AcDbOsnapPointRef";

		/// <summary>
		/// Gets or sets the object snap type associated with the entity.
		/// </summary>
		[DxfCodeValue(72)]
		public ObjectOsnapType ObjectOsnapType { get; set; }

		//331
		//ID of main object (geometry)

		//73
		//SubentType of main object (edge, face)

		//91

		//GsMarker of main object (index)

		//301

		//Handle(string) of Xref object

		//40

		//Geometry parameter for Near Osnap

		//10

		//Osnap point in WCS; X value

		//20

		//Osnap point in WCS; Y value

		//30

		//Osnap point in WCS; Z value

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