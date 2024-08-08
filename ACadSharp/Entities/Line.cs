﻿using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Line"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityLine"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Line"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityLine)]
	[DxfSubClass(DxfSubclassMarker.Line)]
	public class Line : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LINE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLine;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Line;

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
		/// A 3D coordinate representing the start point of the object.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// A 3D coordinate representing the end point of the object.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ EndPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Line() : base() { }

		/// <summary>
		/// Constructor with the start and end
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Line(XYZ start, XYZ end) : base()
		{
			this.StartPoint = start;
			this.EndPoint = end;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			var min = new XYZ(System.Math.Min(this.StartPoint.X, this.EndPoint.X), System.Math.Min(this.StartPoint.Y, this.EndPoint.Y), System.Math.Min(this.StartPoint.Z, this.EndPoint.Z));
			var max = new XYZ(System.Math.Max(this.StartPoint.X, this.EndPoint.X), System.Math.Max(this.StartPoint.Y, this.EndPoint.Y), System.Math.Max(this.StartPoint.Z, this.EndPoint.Z));

			return new BoundingBox(min, max);
		}
	}
}
