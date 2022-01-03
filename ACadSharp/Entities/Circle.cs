using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Circle"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityCircle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Circle"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityCircle)]
	[DxfSubClass(DxfSubclassMarker.Circle)]
	public class Circle : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.CIRCLE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityCircle;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; } = XYZ.Zero;

		/// <summary>
		/// Specifies the radius of an arc, circle, or position marker.
		/// </summary>
		[DxfCodeValue(40)]
		public double Radius { get; set; } = 1.0;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Circle() : base() { }
	}
}
