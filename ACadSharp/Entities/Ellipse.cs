using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	public class Ellipse : Entity
	{
		public override ObjectType ObjectType => ObjectType.ELLIPSE;
		public override string ObjectName => DxfFileToken.EntityEllipse;

		//100	Subclass marker (AcDbEllipse)
		/// <summary>
		/// Specifies the distance a 2D AutoCAD object is extruded above or below its elevation.
		/// </summary>
		[DxfCodeValue(DxfCode.Thickness)]
		public double Thickness { get; set; } = 0.0;
		/// <summary>
		/// Specifies the center of an arc, circle, ellipse, view, or viewport.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ Center { get; set; } = XYZ.Zero;
		/// <summary>
		/// A 3D coordinate representing the endpoint of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate1, DxfCode.YCoordinate1, DxfCode.ZCoordinate1)]
		public XYZ EndPoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// Specifies the major to minor axis ratio of an ellipse.
		/// </summary>
		[DxfCodeValue(DxfCode.Real)]
		public double RadiusRatio { get; set; } = 0.0;
		/// <summary>
		/// Specifies the start parameter for an ellipse.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(DxfCode.StartParameter)]
		public double StartParameter { get; set; } = 0.0;
		/// <summary>
		/// Specifies the end parameter for an ellipse.
		/// </summary>
		/// <value>
		/// The valid range is 0 to 2 * PI.
		/// </value>
		[DxfCodeValue(DxfCode.EndParameter)]
		public double EndParameter { get; set; } = Math.PI * 2;

		public Ellipse() : base() { }

		internal Ellipse(DxfEntityTemplate template) : base(template) { }
	}
}
