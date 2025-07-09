using ACadSharp.Attributes;
using CSMath;
using System;
using CSUtilities.Extensions;
using System.Collections.Generic;
using System.Linq;

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
		/// Spline flags1.
		/// </summary>
		/// <remarks>
		/// Only valid for dwg.
		/// </remarks>
		public SplineFlags1 Flags1 { get; set; }

		/// <summary>
		/// Flag whether the spline is closed.
		/// </summary>
		public bool IsClosed
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
					this.Flags1 = this.Flags1.AddFlag(SplineFlags1.Closed);
				}
				else
				{
					this.Flags = this.Flags.RemoveFlag(SplineFlags.Closed);
					this.Flags1 = this.Flags1.RemoveFlag(SplineFlags1.Closed);
				}
			}
		}

		/// <summary>
		/// Knot parameters.
		/// </summary>
		public KnotParameterization KnotParameterization { get; set; }

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

			for (int i = 0; i < this.FitPoints.Count; i++)
			{
				this.FitPoints[i] = transform.ApplyTransform(this.FitPoints[i]);
			}
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			List<XYZ> vertices = this.PolygonalVertexes(256);

			return BoundingBox.FromPoints(vertices);
		}

		/// <summary>
		/// Gets a point on the spline.
		/// </summary>
		/// <param name="t">Parametric position on spline, between 0 and 1 (not linearly related to curve length).</param>
		/// <returns>Point coordinates for the given parametric position on the spline.</returns>
		public XYZ PointOnSpline(double t)
		{
			double u = t * (this.Knots.Last() - this.Knots.First()) + this.Knots.First();
			XYZ P = new XYZ();

			// P(u) = sum( N_{i,k}(u) * P_i, i= 0 to m-1 )
			// where P_i are the control points and N_{i,k}(u) the basis functions for a spline of degree k
			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				P += this.n(i, this.Degree, u) * this.ControlPoints[i];
			}
			return P;
		}

		/// <summary>
		/// Converts the spline in a list of vertexes.
		/// </summary>
		/// <param name="precision">Number of vertexes generated.</param>
		/// <returns>A list vertexes that represents the spline expressed in object coordinate system.</returns>
		public List<XYZ> PolygonalVertexes(int precision)
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
			}

			List<XYZ> ocsVertexes = new List<XYZ>();
			for (int i = 0; i < precision; i++)
			{
				double t = (double)i / (double)(precision - 1);
				ocsVertexes.Add(this.PointOnSpline(t));
			}

			return ocsVertexes;
		}

		private double n(int i, int k, double u)
		{
			// Bspline basis function N_{i,k}(u)
			// u = parameter, ranges from lowest knot value to highest knot value
			// k is the spline degree
			// i ranges from 0 to (m-1) with m is the number of control points
			if (k == 0)
			{
				if ((this.Knots[i] <= u) && (u <= this.Knots[i + 1]))
					return 1;
				else
					return 0;
			}
			else
			{
				double memb1, memb2;
				if (this.Knots[i + k] == this.Knots[i])
					memb1 = 0;
				else
					memb1 = ((u - this.Knots[i]) / (this.Knots[i + k] - this.Knots[i])) * n(i, k - 1, u);
				if (this.Knots[i + k + 1] == this.Knots[i + 1])
					memb2 = 0;
				else
					memb2 = ((this.Knots[i + k + 1] - u) / (this.Knots[i + k + 1] - this.Knots[i + 1])) * n(i + 1, k - 1, u);
				return memb1 + memb2;
			}
		}
	}
}