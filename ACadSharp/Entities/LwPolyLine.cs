using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="LwPolyline"/> entity
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityLwPolyline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.LwPolyline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityLwPolyline)]
	[DxfSubClass(DxfSubclassMarker.LwPolyline)]
	public partial class LwPolyline : Entity, IPolyline
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LWPOLYLINE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLwPolyline;

		/// <summary>
		/// Polyline flag (bit-coded)
		/// </summary>
		[DxfCodeValue(70)]
		public LwPolylineFlags Flags { get; set; }

		/// <summary>
		/// Constant width
		/// </summary>
		/// <remarks>
		/// Not used if variable width (codes 40 and/or 41) is set
		/// </remarks>
		[DxfCodeValue(43)]
		public double ConstantWidth { get; set; } = 0.0;

		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(38)]
		public double Elevation { get; set; } = 0.0;

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Vertices that form this LwPolyline
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 90)]
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		public bool IsClosed
		{
			get
			{
				return this.Flags.HasFlag(LwPolylineFlags.Closed);
			}
		}

		IEnumerable<IVertex> IPolyline.Vertices { get { return this.Vertices; } }

		public IEnumerable<Entity> Explode()
		{
			return Polyline.explode(this);
		}
	}
}
