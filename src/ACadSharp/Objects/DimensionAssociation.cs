using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSMath;

namespace ACadSharp.Objects
{
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
		/// Gets or sets the associated dimension object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public Dimension Dimension { get; set; }

		/// <summary>
		/// Gets or sets the associated geometry object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 331)]
		public CadObject Geometry { get; set; }

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

		//301
		//Handle(string) of Xref object
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DimensionAssociation;


		public const string OsnapPointRefClassName = "AcDbOsnapPointRef";

		/// <inheritdoc/>
		public DimensionAssociation() : base()
		{
		}

		public class OsnapPointRef
		{
			/// <summary>
			/// Gets or sets the object snap type associated with the entity.
			/// </summary>
			[DxfCodeValue(72)]
			public ObjectOsnapType ObjectOsnapType { get; set; }

			/// <summary>
			/// Gets or sets the type of the rotated dimension, indicating whether it is parallel or perpendicular.
			/// </summary>
			[DxfCodeValue(73)]
			public SubentType SubentType { get; set; } = SubentType.Unknown;

			/// <summary>
			/// Gets the name of the associated class for the dimension reference.
			/// </summary>
			[DxfCodeValue(1)]
			public string ClassName { get; internal set; } = DimensionAssociation.OsnapPointRefClassName;

			/// <summary>
			/// Gets or sets the geometry parameter used for the Near object snap (Osnap).
			/// </summary>
			[DxfCodeValue(40)]
			public double GeometryParameter { get; set; }

			/// <summary>
			/// Gets or sets the object snap (Osnap) point in world coordinate system (WCS).
			/// </summary>
			/// <remarks>The Osnap point is used to specify precise locations on geometry for object snapping
			/// operations.</remarks>
			[DxfCodeValue(10, 20, 30)]
			public XYZ OsnapPoint { get; set; }
		}

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
	}
}