using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

public partial class DimensionAssociation
{
	public class OsnapPointRef
	{
		/// <summary>
		/// Gets or sets the geometry parameter used for the Near object snap (Osnap).
		/// </summary>
		[DxfCodeValue(40)]
		public double GeometryParameter { get; set; }

		/// <summary>
		/// Gets or sets the object snap type associated with the entity.
		/// </summary>
		[DxfCodeValue(72)]
		public ObjectOsnapType ObjectOsnapType { get; set; }

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
		[DxfCodeValue(73)]
		public SubentType SubentType { get; set; } = SubentType.Unknown;

		/// <summary>
		/// Gets or sets the graphics system marker (GsMarker) associated with the main object.
		/// </summary>
		[DxfCodeValue(91)]
		public int GsMarker { get; set; }

		//332
		//ID of intersection object (geometry)

		[DxfCodeValue(74)]
		public SubentType IntersectionSubType { get; set; }

		[DxfCodeValue(92)]
		public int IntersectionGsMarker { get; set; }

		//302
		//Handle(string) of intersection Xref object

		[DxfCodeValue(75)]
		public bool HasLastPointRef { get; set; }

		/// <summary>
		/// Gets or sets the associated geometry object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 331)]
		public CadObject Geometry { get; set; }
	}
}