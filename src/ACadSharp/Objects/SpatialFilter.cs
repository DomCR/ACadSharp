using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="SpatialFilter"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectSpatialFilter"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.SpatialFilter"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectSpatialFilter)]
	[DxfSubClass(DxfSubclassMarker.SpatialFilter)]
	public class SpatialFilter : Filter
	{
		public const string SpatialFilterEntryName = "SPATIAL";

		/// <summary>
		/// Back clipping plane distance.
		/// </summary>
		[DxfCodeValue(41)]
		public double BackDistance { get; set; }

		/// <summary>
		/// Clip boundary definition point (in OCS). <br/>
		/// Always 2 or more based on an xref scale of 1.
		/// </summary>
		/// <remarks>
		/// 2 points = Rectangular clip boundary (lower-left and upper-right) <br/>
		/// greater than 2 points = Polyline clip boundary
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Count, 70)]
		[DxfCollectionCodeValue(10, 20)]
		public List<XY> BoundaryPoints { get; set; } = new List<XY>();

		/// <summary>
		/// Back clipping plane flag.
		/// </summary>
		[DxfCodeValue(73)]
		public bool ClipBackPlane { get; set; }

		/// <summary>
		/// Front clipping plane flag.
		/// </summary>
		[DxfCodeValue(72)]
		public bool ClipFrontPlane { get; set; }

		/// <summary>
		/// Clip boundary display enabled flag.
		/// </summary>
		[DxfCodeValue(71)]
		public bool DisplayBoundary { get; set; }

		/// <summary>
		/// Front clipping plane distance.
		/// </summary>
		[DxfCodeValue(40)]
		public double FrontDistance { get; set; }

		/// <summary>
		///  This matrix transforms points into the coordinate system of the clip boundary.
		/// </summary>
		public Matrix4 InsertTransform { get; set; } = Matrix4.Identity;

		/// <summary>
		/// This matrix is the inverse of the original block reference (insert entity) transformation.
		///  The original block reference transformation is the one that is applied to all entities in the block when the block reference is regenerated.
		/// </summary>
		public Matrix4 InverseInsertTransform { get; set; } = Matrix4.Identity;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectSpatialFilter;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <summary>
		/// Origin used to define the local coordinate system of the clip boundary.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Origin { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.SpatialFilter;

		/// <inheritdoc/>
		public SpatialFilter() : base()
		{
		}

		/// <inheritdoc/>
		public SpatialFilter(string name) : base(name)
		{
		}
	}
}