using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Ellipse"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityEllipse"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ellipse"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityEllipse)]
	[DxfSubClass(DxfSubclassMarker.Ellipse)]
	public class Ellipse : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ELLIPSE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityEllipse;

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
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; } = XYZ.Zero;
		
		/// <summary>
		/// A 3D coordinate representing the endpoint of the object.
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ EndPoint { get; set; } = XYZ.Zero;
		
		/// <summary>
		/// Specifies the major to minor axis ratio of an ellipse.
		/// </summary>
		[DxfCodeValue(40)]
		public double RadiusRatio { get; set; } = 0.0;
		
		/// <summary>
		/// Specifies the start parameter for an ellipse.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(41)]
		public double StartParameter { get; set; } = 0.0;
		
		/// <summary>
		/// Specifies the end parameter for an ellipse.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(42)]
		public double EndParameter { get; set; } = Math.PI * 2;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Ellipse() : base() { }
	}
}
