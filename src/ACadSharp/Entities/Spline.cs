using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
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
		/// <summary>
		/// Flag whether the spline is closed.
		/// </summary>
		public bool Closed
		{
			get
			{
				return this.Flags.HasFlag(SplineFlags.Closed);
			}
			set
			{
				if (value)
				{
					this.Flags = this.Flags.AddFlag(SplineFlags.Closed);
				}
				else
				{
					this.Flags = this.Flags.RemoveFlag(SplineFlags.Closed);
				}
			}
		}

		/// <summary>
		/// Number of control points (in WCS).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 73)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> ControlPoints { get; } = new List<XYZ>();

		/// <summary>
		/// Control-point tolerance.
		/// </summary>
		[DxfCodeValue(43)]
		public double ControlPointTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Degree of the spline curve.
		/// </summary>
		[DxfCodeValue(71)]
		public int Degree { get; set; }

		/// <summary>
		/// End tangent—may be omitted in WCS.
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ EndTangent { get; set; }

		/// <summary>
		/// Number of fit points (in WCS).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 74)]
		[DxfCollectionCodeValue(11, 21, 31)]
		public List<XYZ> FitPoints { get; } = new List<XYZ>();

		/// <summary>
		/// Fit tolerance.
		/// </summary>
		[DxfCodeValue(44)]
		public double FitTolerance { get; set; } = 0.0000000001;

		/// <summary>
		/// Spline flags.
		/// </summary>
		[DxfCodeValue(70)]
		public SplineFlags Flags { get; set; }

		/// <summary>
		/// Number of knots.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		[DxfCollectionCodeValue(40)]
		public List<double> Knots { get; } = new List<double>();

		/// <summary>
		/// Knot tolerance.
		/// </summary>
		[DxfCodeValue(42)]
		public double KnotTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		/// <remarks>
		/// Omitted if the spline is non-planar.
		/// </remarks>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntitySpline;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SPLINE;

		/// <summary>
		/// Start tangent—may be omitted in WCS.
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ StartTangent { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Spline;

		/// <summary>
		/// Weight(if not 1); with multiple group pairs, they are present if all are not 1.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 41)]
		public List<double> Weights { get; } = new List<double>();

		internal SplineFlags1 Flags1 { get; set; }

		internal KnotParameterization KnotParameterization { get; set; }

		/// <inheritdoc/>
		public Spline() : base() { }

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Normal = this.transformNormal(transform, this.Normal);

			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				this.ControlPoints[i] = transform.ApplyTransform(this.ControlPoints[i]);
			}

			for (int i = 0; i < this.FitPoints.Capacity; i++)
			{
				this.FitPoints[i] = transform.ApplyTransform(this.FitPoints[i]);
			}
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.FromPoints(this.ControlPoints);
		}
	}
}