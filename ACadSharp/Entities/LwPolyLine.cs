﻿using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System.Collections.Generic;

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

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LwPolyline;

		/// <summary>
		/// Polyline flags
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
		/// Specifies the distance a 2D object is extruded above or below its elevation.
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

		/// <inheritdoc/>
		public bool IsClosed
		{
			get
			{
				return this.Flags.HasFlag(LwPolylineFlags.Closed);
			}
			set
			{
				if (value)
				{
					this.Flags = this.Flags.AddFlag(LwPolylineFlags.Closed);
				}
				else
				{
					this.Flags = this.Flags.RemoveFlag(LwPolylineFlags.Closed);
				}
			}
		}

		IEnumerable<IVertex> IPolyline.Vertices { get { return this.Vertices; } }

		public IEnumerable<Entity> Explode()
		{
			return Polyline.Explode(this);
		}

		public override BoundingBox GetBoundingBox()
		{
			if (this.Vertices.Count < 2)
			{
				return BoundingBox.Null;
			}

			XYZ first = (XYZ)this.Vertices[0].Location;
			XYZ second = (XYZ)this.Vertices[1].Location;

			XYZ min = new XYZ(System.Math.Min(first.X, second.X), System.Math.Min(first.Y, second.Y), System.Math.Min(first.Z, second.Z));
			XYZ max = new XYZ(System.Math.Max(first.X, second.X), System.Math.Max(first.Y, second.Y), System.Math.Max(first.Z, second.Z));

			for (int i = 2; i < this.Vertices.Count; i++)
			{
				XYZ curr = (XYZ)this.Vertices[i].Location;

				min = new XYZ(System.Math.Min(min.X, curr.X), System.Math.Min(min.Y, curr.Y), System.Math.Min(min.Z, curr.Z));
				max = new XYZ(System.Math.Max(max.X, curr.X), System.Math.Max(max.Y, curr.Y), System.Math.Max(max.Z, curr.Z));
			}

			return new BoundingBox(min, max);
		}
	}
}
