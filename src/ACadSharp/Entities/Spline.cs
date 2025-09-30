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
		public List<XYZ> ControlPoints { get; private set; } = new List<XYZ>();

		/// <summary>
		/// Control-point tolerance.
		/// </summary>
		[DxfCodeValue(43)]
		public double ControlPointTolerance { get; set; } = 0.0000001;

		/// <summary>
		/// Gets or sets the polynomial degree of the resulting spline.
		/// </summary>
		/// <remarks>
		/// Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.
		/// </remarks>
		[DxfCodeValue(71)]
		public int Degree { get; set; } = 3;

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
		public List<XYZ> FitPoints { get; private set; } = new List<XYZ>();

		/// <summary>
		/// Fit tolerance.
		/// </summary>
		[DxfCodeValue(44)]
		public double FitTolerance { get; set; } = 0.0000000001;

		/// <summary>
		/// Spline flags.
		/// </summary>
		[DxfCodeValue(70)]
		public SplineFlags Flags { get => _flags; set => _flags = value; }

		/// <summary>
		/// Spline flags1.
		/// </summary>
		/// <remarks>
		/// Only valid for dwg.
		/// </remarks>
		public SplineFlags1 Flags1 { get => _flags1; set => _flags1 = value; }

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
					this._flags.AddFlag(SplineFlags.Closed);
					this._flags1.AddFlag(SplineFlags1.Closed);
				}
				else
				{
					this._flags.RemoveFlag(SplineFlags.Closed);
					this._flags1.RemoveFlag(SplineFlags1.Closed);
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
		public List<double> Knots { get; private set; } = new List<double>();

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
		public List<double> Weights { get; private set; } = new List<double>();

		public const short MaxDegree = 10;

		private SplineFlags _flags;

		private SplineFlags1 _flags1;

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
		public override CadObject Clone()
		{
			Spline clone = (Spline)base.Clone();

			clone.ControlPoints = new List<XYZ>(this.ControlPoints);
			clone.FitPoints = new List<XYZ>(this.FitPoints);
			clone.Weights = new List<double>(this.Weights);
			clone.Knots = new List<double>(this.Weights);

			return clone;
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

		/// <summary>
		/// Update the Spline control points from the fit points.
		/// </summary>
		/// <remarks>
		/// The weights are set to 1 and the degree to 3 (cubic).
		/// </remarks>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void UpdateFromFitPoints()
		{
			if (this.FitPoints == null)
			{
				throw new ArgumentNullException(nameof(this.FitPoints));
			}

			XYZ[] points = this.FitPoints.ToArray();
			int numFitPoints = points.Length;
			if (numFitPoints < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(this.FitPoints), numFitPoints, "At least two fit points required.");
			}

			this.Degree = 3;

			this.ControlPoints.Clear();
			this.Weights.Clear();

			int n = numFitPoints - 1;

			XYZ firstControlPoint;
			XYZ secondControlPoint;

			if (n == 1)
			{
				// Special case: Bezier curve should be a straight line.
				firstControlPoint = points[0] + (points[1] - points[0]) / 3.0;
				secondControlPoint = points[1] + (points[0] - points[1]) / 3.0;

				this.ControlPoints.AddRange([points[0], firstControlPoint, secondControlPoint, points[1]]);
				this.Weights.AddRange([1, 1, 1, 1]);
				this.Knots.AddRange(createBezierKnotVector(this.ControlPoints.Count, this.Degree));
				return;
			}

			// Calculate first Bezier control points
			// Right hand side vector
			double[] rhs = new double[n];

			// Set right hand side X values
			for (int i = 1; i < n - 1; i++)
			{
				rhs[i] = 4.0 * points[i].X + 2.0 * points[i + 1].X;
			}
			rhs[0] = points[0].X + 2.0 * points[1].X;
			rhs[n - 1] = (8.0 * points[n - 1].X + points[n].X) / 2.0;
			// Get first control points X-values
			double[] x = getFirstControlPoints(rhs);

			// Set right hand side Y values
			for (int i = 1; i < n - 1; i++)
			{
				rhs[i] = 4.0 * points[i].Y + 2.0 * points[i + 1].Y;
			}
			rhs[0] = points[0].Y + 2.0 * points[1].Y;
			rhs[n - 1] = (8.0 * points[n - 1].Y + points[n].Y) / 2.0;
			// Get first control points Y-values
			double[] y = getFirstControlPoints(rhs);

			// Set right hand side Z values
			for (int i = 1; i < n - 1; i++)
			{
				rhs[i] = 4.0 * points[i].Z + 2.0 * points[i + 1].Z;
			}
			rhs[0] = points[0].Z + 2.0 * points[1].Z;
			rhs[n - 1] = (8.0 * points[n - 1].Z + points[n].Z) / 2.0;
			// Get first control points Z-values
			double[] z = getFirstControlPoints(rhs);

			// create the curves
			for (int i = 0; i < n; i++)
			{
				// First control point
				firstControlPoint = new XYZ(x[i], y[i], z[i]);

				// Second control point
				if (i < n - 1)
				{
					secondControlPoint = new XYZ(
						2 * points[i + 1].X - x[i + 1],
						2 * points[i + 1].Y - y[i + 1],
						2 * points[i + 1].Z - z[i + 1]);
				}
				else
				{
					secondControlPoint = new XYZ(
						(points[n].X + x[n - 1]) / 2.0,
						(points[n].Y + y[n - 1]) / 2.0,
						(points[n].Z + z[n - 1]) / 2.0);
				}

				this.ControlPoints.AddRange([points[i], firstControlPoint, secondControlPoint, points[i + 1]]);
				this.Weights.AddRange([1, 1, 1, 1]);
				this.Knots.AddRange(createBezierKnotVector(this.ControlPoints.Count, this.Degree));
			}
		}

		private static double[] createBezierKnotVector(int numControlPoints, int degree)
		{
			// create knot vector
			int numKnots = numControlPoints + degree + 1;
			double[] knots = new double[numKnots];

			int np = degree + 1;
			int nc = numKnots / np;
			double fact = 1.0 / nc;
			int index = 1;

			for (int i = 0; i < numKnots;)
			{
				double knot;

				if (i < np)
				{
					knot = 0.0;
				}
				else if (i >= numKnots - np)
				{
					knot = 1.0;
				}
				else
				{
					knot = fact * index;
					index += 1;
				}

				for (int j = 0; j < np; j++)
				{
					knots[i] = knot;
					i += 1;
				}
			}

			return knots;
		}

		private static double[] getFirstControlPoints(double[] rhs)
		{
			int n = rhs.Length;
			double[] x = new double[n]; // Solution vector.
			double[] tmp = new double[n]; // Temp workspace.

			double b = 2.0;
			x[0] = rhs[0] / b;
			for (int i = 1; i < n; i++) // Decomposition and forward substitution.
			{
				tmp[i] = 1 / b;
				b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
				x[i] = (rhs[i] - x[i - 1]) / b;
			}

			for (int i = 1; i < n; i++)
			{
				x[n - i - 1] -= tmp[n - i] * x[n - i]; // Back substitution.
			}

			return x;
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