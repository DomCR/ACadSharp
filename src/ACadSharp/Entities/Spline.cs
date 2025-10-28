using ACadSharp.Attributes;
using CSMath;
using CSUtilities.Extensions;
using System;
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
		/// Gets or sets a value indicating whether the spline is periodic.
		/// </summary>
		/// <remarks>A periodic spline forms a closed loop, where the start and end points are connected seamlessly.
		/// Setting this property updates the internal flags to reflect the periodicity of the spline.</remarks>
		public bool IsPeriodic
		{
			get
			{
				return this.Flags.HasFlag(SplineFlags.Periodic);
			}
			set
			{
				if (value)
				{
					this._flags.AddFlag(SplineFlags.Periodic);
				}
				else
				{
					this._flags.RemoveFlag(SplineFlags.Periodic);
				}
			}
		}

		/// <summary>
		/// Knot parameters.
		/// </summary>
		public KnotParametrization KnotParametrization { get; set; }

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
			List<XYZ> vertices;
			if (!this.TryPolygonalVertexes(256, out vertices))
			{
				vertices = new List<XYZ>(this.FitPoints);
			}

			return BoundingBox.FromPoints(vertices);
		}

		/// <summary>
		/// Calculates the point on the spline at the specified parametric position.
		/// </summary>
		/// <param name="t">The parametric position along the spline, where <see langword="0"/> represents the start of the spline and <see
		/// langword="1"/> represents the end. Must be a value between <see langword="0"/> and <see langword="1"/>.</param>
		/// <returns>The point on the spline at the specified parametric position, represented as an <see cref="XYZ"/> object.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="t"/> is less than <see langword="0"/> or greater than <see langword="1"/>.</exception>
		public XYZ PointOnSpline(double t)
		{
			if (t < 0 || t > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(t), t, "The parametric position must be a value between 0 and 1.");
			}

			if (t == 1)
			{
				t -= double.Epsilon;
			}

			var knots = this.Knots.ToArray();

			this.prepare(out XYZ[] controlPts, out double[] weights);
			this.getStartAndEndKnots(knots, out double uStart, out double uEnd);

			double uDelta = (uEnd - uStart) * t;
			double u = uStart + uDelta;
			return c(controlPts, weights, knots, this.Degree, u);
		}

		/// <summary>
		/// Generates a list of vertex points representing a polygonal approximation of the curve.
		/// </summary>
		/// <remarks>The method calculates the vertex points by dividing the curve into segments based on the specified
		/// precision. If the curve is not closed or periodic, the last control point is explicitly added to the
		/// result.</remarks>
		/// <param name="precision">The number of segments used to approximate the curve. Must be equal to or greater than 2.</param>
		/// <returns>A list of <see cref="XYZ"/> objects representing the vertex points of the polygonal approximation.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="precision"/> is less than 2.</exception>
		public List<XYZ> PolygonalVertexes(int precision)
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
			}

			var knots = this.Knots.ToArray();
			this.prepare(out XYZ[] controlPts, out double[] weights);

			List<XYZ> vertexes = new List<XYZ>();
			this.getStartAndEndKnots(knots, out double uStart, out double uEnd);

			if (!this.IsClosed && !this.IsPeriodic)
			{
				precision -= 1;
			}

			double uDelta = (uEnd - uStart) / precision;

			for (int i = 0; i < precision; i++)
			{
				double u = uStart + uDelta * i;
				vertexes.Add(c(controlPts, weights, knots, this.Degree, u));
			}

			if (!(this.IsClosed || this.IsPeriodic))
			{
				vertexes.Add(controlPts[controlPts.Length - 1]);
			}

			return vertexes;
		}

		/// <summary>
		/// Attempts to calculate a point on the spline at the specified parameter value.
		/// </summary>
		/// <remarks>This method catches any exceptions that occur during the calculation and returns <see
		/// langword="false"/> in such cases. Ensure that the parameter <paramref name="t"/> is within the valid range for the
		/// spline to avoid errors.</remarks>
		/// <param name="t">The parameter value along the spline, typically in the range [0, 1].</param>
		/// <param name="point">When this method returns, contains the calculated point on the spline if the operation succeeds; otherwise, <see
		/// cref="XYZ.NaN"/>. This parameter is passed uninitialized.</param>
		/// <returns><see langword="true"/> if the point was successfully calculated; otherwise, <see langword="false"/>.</returns>
		public bool TryPointOnSpline(double t, out XYZ point)
		{
			point = XYZ.NaN;

			try
			{
				point = this.PointOnSpline(t);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Attempts to calculate the polygonal vertexes of the current object with the specified precision.
		/// </summary>
		/// <remarks>This method catches and suppresses any exceptions that occur during the calculation, returning
		/// <see langword="false"/> in such cases. The <paramref name="points"/> parameter will always be initialized, even if
		/// the operation fails.</remarks>
		/// <param name="precision">The level of precision to use when calculating the polygonal vertexes. Must be a positive integer.</param>
		/// <param name="points">When this method returns, contains a list of <see cref="XYZ"/> objects representing the calculated polygonal
		/// vertexes, if the operation succeeds; otherwise, contains an empty list.</param>
		/// <returns><see langword="true"/> if the polygonal vertexes were successfully calculated; otherwise, <see langword="false"/>.</returns>
		public bool TryPolygonalVertexes(int precision, out List<XYZ> points)
		{
			points = new List<XYZ>();

			try
			{
				points = this.PolygonalVertexes(precision);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Updates the control points, weights, and knots of a Bezier curve based on the current fit points.
		/// </summary>
		/// <remarks>This method calculates the control points, weights, and knot vector for a cubic Bezier curve
		/// using the fit points provided in the <see cref="FitPoints"/> property. The method supports both straight-line and
		/// multi-segment Bezier curves. The degree of the curve must be 3, and at least two fit points are
		/// required.</remarks>
		/// <returns><see langword="true"/> if the control points, weights, and knots were successfully updated; otherwise, <see
		/// langword="false"/> if the degree of the curve is not 3.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <see cref="FitPoints"/> property is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="FitPoints"/> property contains fewer than two points.</exception>
		public bool UpdateFromFitPoints()
		{
			if (this.Degree != 3)
			{
				return false;
			}

			XYZ[] points = this.FitPoints.ToArray();
			int numFitPoints = points.Length;
			if (numFitPoints < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(this.FitPoints), numFitPoints, "At least two fit points required.");
			}

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
				return true;
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

			return true;
		}

		private static XYZ c(XYZ[] ctrlPoints, double[] weights, double[] knots, int degree, double u)
		{
			XYZ vectorSum = XYZ.Zero;
			double denominatorSum = 0.0;

			// optimization suggested by ThVoss
			for (int i = 0; i < ctrlPoints.Length; i++)
			{
				double nurb = computeNurb(knots, i, degree, u);
				denominatorSum += nurb * weights[i];
				vectorSum += weights[i] * nurb * ctrlPoints[i];
			}

			// avoid possible divided by zero error, this should never happen
			if (Math.Abs(denominatorSum) < double.Epsilon)
			{
				return XYZ.Zero;
			}

			return (1.0 / denominatorSum) * vectorSum;
		}

		private static double computeNurb(double[] knots, int i, int p, double u)
		{
			if (p <= 0)
			{
				if (knots[i] <= u && u < knots[i + 1])
				{
					return 1;
				}

				return 0.0;
			}

			double leftCoefficient = 0.0;
			if (!(Math.Abs(knots[i + p] - knots[i]) < double.Epsilon))
			{
				leftCoefficient = (u - knots[i]) / (knots[i + p] - knots[i]);
			}

			double rightCoefficient = 0.0; // article contains error here, denominator is Knots[i + p + 1] - Knots[i + 1]
			if (!(Math.Abs(knots[i + p + 1] - knots[i + 1]) < double.Epsilon))
			{
				rightCoefficient = (knots[i + p + 1] - u) / (knots[i + p + 1] - knots[i + 1]);
			}

			return leftCoefficient * computeNurb(knots, i, p - 1, u) + rightCoefficient * computeNurb(knots, i + 1, p - 1, u);
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

		private static double[] createKnotVector(int numControlPoints, int degree, bool isPeriodic)
		{
			// create knot vector
			int numKnots;
			double[] knots;

			if (!isPeriodic)
			{
				numKnots = numControlPoints + degree + 1;
				knots = new double[numKnots];

				int i;
				for (i = 0; i <= degree; i++)
				{
					knots[i] = 0.0;
				}

				for (; i < numControlPoints; i++)
				{
					knots[i] = i - degree;
				}

				for (; i < numKnots; i++)
				{
					knots[i] = numControlPoints - degree;
				}
			}
			else
			{
				numKnots = numControlPoints + 2 * degree + 1;
				knots = new double[numKnots];

				double factor = 1.0 / (numControlPoints - degree);
				for (int i = 0; i < numKnots; i++)
				{
					knots[i] = (i - degree) * factor;
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

		private void getStartAndEndKnots(double[] knots, out double uStart, out double uEnd)
		{
			if (this.IsClosed)
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
			else if (this.IsPeriodic)
			{
				uStart = knots[this.Degree];
				uEnd = knots[knots.Length - this.Degree - 1];
			}
			else
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
		}

		[Obsolete]
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

		private void prepare(out XYZ[] controlPts, out double[] weights)
		{
			var c = this.ControlPoints.ToArray();
			var w = this.Weights.ToArray();
			var knots = this.Knots.ToArray();

			// control points
			int numCtrlPoints = c.Length;
			if (numCtrlPoints == 0)
			{
				throw new ArgumentException("A spline entity with control points is required.", nameof(c));
			}

			// weights
			if (!w.Any() || w.Length != numCtrlPoints)
			{
				// give the default 1.0 to the control points weights
				w = new double[numCtrlPoints];
				for (int i = 0; i < numCtrlPoints; i++)
				{
					w[i] = 1.0;
				}
			}

			// knots
			if (!knots.Any())
			{
				knots = createKnotVector(numCtrlPoints, this.Degree, this.IsPeriodic);
			}
			else
			{
				int numKnots;
				if (this.IsPeriodic)
				{
					numKnots = numCtrlPoints + 2 * this.Degree + 1;
				}
				else
				{
					numKnots = numCtrlPoints + this.Degree + 1;
				}

				if (knots.Length != numKnots)
				{
					throw new ArgumentException("Invalid number of knots.");
				}
			}

			if (this.IsPeriodic)
			{
				controlPts = new XYZ[numCtrlPoints + this.Degree];
				weights = new double[numCtrlPoints + this.Degree];
				for (int i = 0; i < this.Degree; i++)
				{
					int index = numCtrlPoints - this.Degree + i;
					controlPts[i] = c[index];
					weights[i] = w[index];
				}

				c.CopyTo(controlPts, this.Degree);
				w.CopyTo(weights, this.Degree);
			}
			else
			{
				controlPts = c;
				weights = w;
			}

			double uStart;
			double uEnd;
			List<XYZ> vertexes = new List<XYZ>();

			if (this.IsClosed)
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
			else if (this.IsPeriodic)
			{
				uStart = knots[this.Degree];
				uEnd = knots[knots.Length - this.Degree - 1];
			}
			else
			{
				uStart = knots[0];
				uEnd = knots[knots.Length - 1];
			}
		}
	}
}