using ACadSharp.Attributes;
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
		/// <summary>
		/// A 3D coordinate representing the end point of the object.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ EndPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLine;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LINE;

		/// <summary>
		/// A 3D coordinate representing the start point of the object.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; } = XYZ.Zero;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Line;

		/// <summary>
		/// Specifies the distance a 2D object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Line() : base() { }

		/// <summary>
		/// Constructor with the start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Line(IVector start, IVector end) : base()
		{
			this.StartPoint = start.Convert<XYZ>();
			this.EndPoint = end.Convert<XYZ>();
		}

		/// <summary>
		/// Constructor with the start and end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Line(XYZ start, XYZ end) : base()
		{
			this.StartPoint = start;
			this.EndPoint = end;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.StartPoint = transform.ApplyTransform(this.StartPoint);
			this.EndPoint = transform.ApplyTransform(this.EndPoint);
			this.Normal = this.transformNormal(transform, this.Normal);
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