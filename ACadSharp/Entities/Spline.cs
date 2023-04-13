using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Spline"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntitySpline"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Spline"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntitySpline)]
	[DxfSubClass(DxfSubclassMarker.Spline)]
	public class Spline : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SPLINE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntitySpline;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		/// <remarks>
		/// omitted if the spline is nonplanar
		/// </remarks>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Spline flags
		/// </summary>
		[DxfCodeValue(70)]
		public SplineFlags Flags { get; set; }

		/// <summary>
		/// Degree of the spline curve
		/// </summary>
		[DxfCodeValue(71)]
		public double Degree { get; set; }

		/// <summary>
		/// Number of knots
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		[DxfCollectionCodeValue(40)]
		public List<double> Knots { get; } = new List<double>();

		/// <summary>
		/// Number of control points (in WCS)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> ControlPoints { get; } = new List<XYZ>();

		/// <summary>
		/// Number of fit points (in WCS)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 74)]
		[DxfCollectionCodeValue(11, 21, 31)]
		public List<XYZ> FitPoints { get; } = new List<XYZ>();

		/// <summary>
		/// Knot tolerance
		/// </summary>
		[DxfCodeValue(42)]
		public double KnotTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Control-point tolerance
		/// </summary>
		[DxfCodeValue(43)]
		public double ControlPointTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Fit tolerance
		/// </summary>
		[DxfCodeValue(44)]
		public double FitTolerance { get; set; } = 0.0000000001;

		/// <summary>
		/// Start tangent—may be omitted in WCS
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ StartTangent { get; set; }

		/// <summary>
		/// End tangent—may be omitted in WCS
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ EndTangent { get; set; }

		/// <summary>
		/// Weight(if not 1); with multiple group pairs, they are present if all are not 1
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 41)]
		public List<double> Weights { get; } = new List<double>();

		internal SplineFlags1 Flags1 { get; set; }

		internal KnotParameterization KnotParameterization { get; set; }

		public Spline() : base() { }

		public override object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
